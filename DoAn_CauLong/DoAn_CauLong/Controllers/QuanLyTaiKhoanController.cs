using DoAn_CauLong.Filters;
using DoAn_CauLong.Models;
using DoAn_CauLong.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace DoAn_CauLong.Controllers
{
    [AuthorizeAdmin]
    public class QuanLyTaiKhoanController : Controller
    {
        // GET: QuanLyTaiKhoan
        QLDN_CAULONGEntities data = new QLDN_CAULONGEntities();
        public ActionResult Index()
        {
            var danhSach = data.TaiKhoans
                       .Include(t => t.PhanQuyen)
                       .Include(t => t.KhachHangs)
                       .OrderBy(t => t.MaQuyen)
                       .ToList();
            return View("Index", danhSach);
        }
        public ActionResult Create()
        {
            ViewBag.MaQuyen = new SelectList(data.PhanQuyens, "MaQuyen", "TenQuyen");
            return View("Create", new TaiKhoanViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TaiKhoanViewModel model)
        {
            ViewBag.MaQuyen = new SelectList(data.PhanQuyens, "MaQuyen", "TenQuyen", model.MaQuyen);

            if (string.IsNullOrEmpty(model.MatKhau))
            {
                ModelState.AddModelError("MatKhau", "Mật khẩu không được để trống khi tạo mới.");
            }

            if (ModelState.IsValid)
            {
                if (data.TaiKhoans.Any(t => t.TenDangNhap == model.TenDangNhap))
                {
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại.");
                    return View("Create", model);
                }
                if (data.TaiKhoans.Any(t => t.Email == model.Email) || data.KhachHangs.Any(k => k.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại.");
                    return View("Create", model);
                }

                using (var transaction = data.Database.BeginTransaction())
                {
                    try
                    {
                        TaiKhoan newTk = new TaiKhoan();
                        newTk.TenDangNhap = model.TenDangNhap;
                        newTk.MatKhau = model.MatKhau;
                        newTk.Email = model.Email;
                        newTk.MaQuyen = model.MaQuyen;
                        newTk.NgayTao = DateTime.Now;
                        data.TaiKhoans.Add(newTk);
                        data.SaveChanges();

                        KhachHang newKh = new KhachHang();
                        newKh.HoTen = model.HoTen;
                        newKh.Email = model.Email;
                        newKh.SoDienThoai = model.SoDienThoai;
                        newKh.DiaChi = model.DiaChi;
                        newKh.MaTaiKhoan = newTk.MaTaiKhoan;
                        newKh.NgayTao = DateTime.Now;
                        data.KhachHangs.Add(newKh);
                        data.SaveChanges();

                        transaction.Commit();
                        TempData["Message"] = "Tạo tài khoản thành công!";
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", "Lỗi khi tạo tài khoản: " + ex.Message);
                    }
                }
            }
            return View("Create", model);
        }
        // GET: QuanLyTaiKhoan/Edit/5
        public ActionResult Edit(int id)
        {
            var taiKhoan = data.TaiKhoans.Find(id);
            if (taiKhoan == null)
            {
                return HttpNotFound();
            }

            var khachHang = data.KhachHangs.FirstOrDefault(k => k.MaTaiKhoan == id);

            TaiKhoanViewModel viewModel = new TaiKhoanViewModel();
            viewModel.MaTaiKhoan = taiKhoan.MaTaiKhoan;
            viewModel.TenDangNhap = taiKhoan.TenDangNhap;
            viewModel.Email = taiKhoan.Email;
            viewModel.MaQuyen = taiKhoan.MaQuyen.Value;

            // === THÊM DÒNG NÀY ĐỂ HIỂN THỊ MK HIỆN TẠI ===
            viewModel.MatKhau = taiKhoan.MatKhau;
            // ===========================================

            if (khachHang != null)
            {
                viewModel.HoTen = khachHang.HoTen;
                viewModel.SoDienThoai = khachHang.SoDienThoai;
                viewModel.DiaChi = khachHang.DiaChi;
            }

            ViewBag.MaQuyen = new SelectList(data.PhanQuyens, "MaQuyen", "TenQuyen", viewModel.MaQuyen);
            return View("Edit", viewModel);
        }

        // POST: QuanLyTaiKhoan/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TaiKhoanViewModel model)
        {
            ViewBag.MaQuyen = new SelectList(data.PhanQuyens, "MaQuyen", "TenQuyen", model.MaQuyen);

            if (data.TaiKhoans.Any(t => t.Email == model.Email && t.MaTaiKhoan != model.MaTaiKhoan) ||
                data.KhachHangs.Any(k => k.Email == model.Email && k.MaTaiKhoan != model.MaTaiKhoan))
            {
                ModelState.AddModelError("Email", "Email này đã được tài khoản khác sử dụng.");
            }

            if (ModelState.IsValid)
            {
                using (var transaction = data.Database.BeginTransaction())
                {
                    try
                    {
                        var tkToUpdate = data.TaiKhoans.Find(model.MaTaiKhoan);
                        tkToUpdate.Email = model.Email;
                        tkToUpdate.MaQuyen = model.MaQuyen;

                        // === THÊM DÒNG NÀY ĐỂ LƯU MẬT KHẨU MỚI ===
                        // (Cảnh báo: xem giải thích bảo mật bên dưới)
                        tkToUpdate.MatKhau = model.MatKhau;
                        // ========================================

                        var khToUpdate = data.KhachHangs.FirstOrDefault(k => k.MaTaiKhoan == model.MaTaiKhoan);
                        if (khToUpdate != null)
                        {
                            khToUpdate.HoTen = model.HoTen;
                            khToUpdate.Email = model.Email;
                            khToUpdate.SoDienThoai = model.SoDienThoai;
                            khToUpdate.DiaChi = model.DiaChi;
                        }

                        data.SaveChanges();
                        transaction.Commit();

                        TempData["Message"] = "Cập nhật tài khoản thành công!";
                        return RedirectToAction("Index"); // Đã sửa từ "DanhSach"
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", "Lỗi khi cập nhật: " + ex.Message);
                    }
                }
            }
            return View("Edit", model);
        }

        // GET: QuanLyTaiKhoan/XacNhanXoa/5
        public ActionResult Delete(int id)
        {
            var taiKhoan = data.TaiKhoans
                               .Include(t => t.PhanQuyen)
                               .Include(t => t.KhachHangs) // <-- Sửa thành số nhiều
                               .FirstOrDefault(t => t.MaTaiKhoan == id);
            if (taiKhoan == null)
            {
                return HttpNotFound();
            }
            // Trả về View tên là "Delete.cshtml"
            return View("Delete", taiKhoan);
        }

        // POST: QuanLyTaiKhoan/XacNhanXoa/5
        [HttpPost, ActionName("XacNhanXoa")]
        [ValidateAntiForgeryToken]
        public ActionResult XacNhanXoa(int id)
        {
            using (var transaction = data.Database.BeginTransaction())
            {
                try
                {
                    // Phải xóa KhachHang trước để kích hoạt Trigger xóa (DonHang, GioHang...)
                    var khToDelete = data.KhachHangs.FirstOrDefault(k => k.MaTaiKhoan == id);
                    if (khToDelete != null)
                    {
                        data.KhachHangs.Remove(khToDelete);
                    }

                    var tkToDelete = data.TaiKhoans.Find(id);
                    if (tkToDelete != null)
                    {
                        data.TaiKhoans.Remove(tkToDelete);
                    }

                    data.SaveChanges();
                    transaction.Commit();

                    TempData["Message"] = "Xóa tài khoản thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["Error"] = "Lỗi khi xóa: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                data.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}