using DoAn_CauLong.Filters;
using DoAn_CauLong.Models;
using DoAn_CauLong.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO; 
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace DoAn_CauLong.Controllers
{
    [AuthorizeAdmin]
    public class QuanLySanPhamController : Controller
    {
        QLDN_CAULONGEntities data = new QLDN_CAULONGEntities();

        // GET: QuanLySanPham
        public ActionResult Index()
        {
            
            var sanPhams = data.SanPhams.Include(s => s.LoaiSanPham).Include(s => s.Hang).OrderByDescending(s => s.NgayTao).ToList();
            return View(sanPhams);
        }

        
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Tải tất cả dữ liệu liên quan mà View Details cần
            SanPham sanPham = data.SanPhams
                .Include(s => s.Hang)
                .Include(s => s.LoaiSanPham)
                .Include(s => s.NhaCungCap)
                .Include(s => s.KhuyenMai)
                .Include(s => s.ChiTietSanPhams)
                .Include(s => s.ChiTietSanPhams.Select(ct => ct.MauSac)) 
                .Include(s => s.ChiTietSanPhams.Select(ct => ct.Size)) 
                .FirstOrDefault(s => s.MaSanPham == id);

            if (sanPham == null)
            {
                return HttpNotFound();
            }

            return View(sanPham);
        }

        // GET: QuanLySanPham/Create
        public ActionResult Create()
        {
            
            ViewBag.MaLoai = new SelectList(data.LoaiSanPhams, "MaLoai", "TenLoai");
            ViewBag.MaNhaCungCap = new SelectList(data.NhaCungCaps, "MaNhaCungCap", "TenNhaCungCap");
            ViewBag.MaHang = new SelectList(data.Hangs, "MaHang", "TenHang");
            ViewBag.MaKhuyenMai = new SelectList(data.KhuyenMais, "MaKhuyenMai", "TenChuongTrinh");

            return View();
        }

        // POST: QuanLySanPham/Create
        // POST: QuanLySanPham/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create([Bind(Include = "TenSanPham,MoTa,GiaGoc,MaLoai,MaNhaCungCap,MaHang,MaKhuyenMai,CoSize,CoMau")] SanPham sanPham, HttpPostedFileBase HinhAnhUpload)
        {
            if (ModelState.IsValid)
            {
                // 1. Xử lý upload file hình ảnh 
                string fileName = ""; // Mặc định rỗng hoặc ảnh default
                if (HinhAnhUpload != null && HinhAnhUpload.ContentLength > 0)
                {
                    fileName = Path.GetFileName(HinhAnhUpload.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/Images"), fileName);
                    HinhAnhUpload.SaveAs(path);
                }
                sanPham.HinhAnhDaiDien = fileName;

                // 2. THAY THẾ: Gọi Stored Procedure thay vì Add() và SaveChanges()
                object maKhuyenMaiParam = sanPham.MaKhuyenMai.HasValue ? (object)sanPham.MaKhuyenMai.Value : DBNull.Value;

                // Dùng ExecuteSqlCommand để gọi Proc
                data.Database.ExecuteSqlCommand(
                    "EXEC ThemSanPhamKhuyenMai @TenSanPham, @MoTa, @GiaGoc, @HinhAnhDaiDien, @MaLoai, @MaNhaCungCap, @MaHang, @MaKhuyenMai, @CoSize, @CoMau",
                    new SqlParameter("@TenSanPham", sanPham.TenSanPham ?? (object)DBNull.Value),
                    new SqlParameter("@MoTa", sanPham.MoTa ?? (object)DBNull.Value),
                    new SqlParameter("@GiaGoc", sanPham.GiaGoc ?? 0),
                    new SqlParameter("@HinhAnhDaiDien", sanPham.HinhAnhDaiDien ?? (object)DBNull.Value),
                    new SqlParameter("@MaLoai", sanPham.MaLoai ?? (object)DBNull.Value),
                    new SqlParameter("@MaNhaCungCap", sanPham.MaNhaCungCap ?? (object)DBNull.Value),
                    new SqlParameter("@MaHang", sanPham.MaHang ?? (object)DBNull.Value),
                    new SqlParameter("@MaKhuyenMai", maKhuyenMaiParam),
                    new SqlParameter("@CoSize", sanPham.CoSize ?? false),
                    new SqlParameter("@CoMau", sanPham.CoMau ?? false)
                );

                return RedirectToAction("Index");
            }

            ViewBag.MaLoai = new SelectList(data.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
            ViewBag.MaNhaCungCap = new SelectList(data.NhaCungCaps, "MaNhaCungCap", "TenNhaCungCap", sanPham.MaNhaCungCap);
            ViewBag.MaHang = new SelectList(data.Hangs, "MaHang", "TenHang", sanPham.MaHang);
            ViewBag.MaKhuyenMai = new SelectList(data.KhuyenMais, "MaKhuyenMai", "TenChuongTrinh", sanPham.MaKhuyenMai);

            return View(sanPham);
        }

        // GET: QuanLySanPham/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            
            SanPham sanPham = data.SanPhams.Find(id);
            if (sanPham == null)
            {
                return HttpNotFound();
            }

           
            ViewBag.MaLoai = new SelectList(data.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
            ViewBag.MaNhaCungCap = new SelectList(data.NhaCungCaps, "MaNhaCungCap", "TenNhaCungCap", sanPham.MaNhaCungCap);
            ViewBag.MaHang = new SelectList(data.Hangs, "MaHang", "TenHang", sanPham.MaHang);
            ViewBag.MaKhuyenMai = new SelectList(data.KhuyenMais, "MaKhuyenMai", "TenChuongTrinh", sanPham.MaKhuyenMai);

            return View(sanPham);
        }

        // POST: QuanLySanPham/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] 
        public ActionResult Edit([Bind(Include = "MaSanPham,TenSanPham,MoTa,GiaGoc,HinhAnhDaiDien,MaLoai,MaNhaCungCap,MaHang,MaKhuyenMai,CoSize,CoMau,NgayTao")] SanPham sanPham, HttpPostedFileBase HinhAnhUpload)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload file (nếu có file mới)
                if (HinhAnhUpload != null && HinhAnhUpload.ContentLength > 0)
                {
                    // (Tùy chọn: Xóa file ảnh cũ nếu tồn tại)
                    if (!string.IsNullOrEmpty(sanPham.HinhAnhDaiDien))
                    {
                        string oldPath = Path.Combine(Server.MapPath("~/Content/Images"), sanPham.HinhAnhDaiDien);
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    // Lưu file ảnh mới
                    string fileName = Path.GetFileName(HinhAnhUpload.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/Images"), fileName);
                    HinhAnhUpload.SaveAs(path);

                    // Cập nhật tên file mới vào model
                    sanPham.HinhAnhDaiDien = fileName;
                }

                // Đánh dấu đối tượng là đã bị sửa
                data.Entry(sanPham).State = EntityState.Modified;
                data.SaveChanges();

                return RedirectToAction("Index");
            }

            // Nếu ModelState không hợp lệ, tải lại DropDownList cho View
            ViewBag.MaLoai = new SelectList(data.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
            ViewBag.MaNhaCungCap = new SelectList(data.NhaCungCaps, "MaNhaCungCap", "TenNhaCungCap", sanPham.MaNhaCungCap);
            ViewBag.MaHang = new SelectList(data.Hangs, "MaHang", "TenHang", sanPham.MaHang);
            ViewBag.MaKhuyenMai = new SelectList(data.KhuyenMais, "MaKhuyenMai", "TenChuongTrinh", sanPham.MaKhuyenMai);

            return View(sanPham);
        }

        // GET: QuanLySanPham/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Tải thông tin liên quan để hiển thị trên trang xác nhận xóa
            SanPham sanPham = data.SanPhams
                .Include(s => s.LoaiSanPham)
                .Include(s => s.Hang)
                .Include(s => s.NhaCungCap)
                .FirstOrDefault(s => s.MaSanPham == id);

            if (sanPham == null)
            {
                return HttpNotFound();
            }

            return View(sanPham);
        }

        // POST: QuanLySanPham/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SanPham sanPham = data.SanPhams.Find(id);

            // (Quan trọng: Xóa file ảnh trên server trước khi xóa record trong data)
            if (!string.IsNullOrEmpty(sanPham.HinhAnhDaiDien))
            {
                string path = Path.Combine(Server.MapPath("~/Content/Images"), sanPham.HinhAnhDaiDien);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }

            
            data.SanPhams.Remove(sanPham);
            data.SaveChanges();

            return RedirectToAction("Index");
        }

       
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                data.Dispose();
            }
            base.Dispose(disposing);
        }




        public ActionResult Dashboard(int? nam)
        {
            int namHienTai = nam ?? DateTime.Now.Year;

            // 1. Doanh thu theo năm (trả về 1 record)
            var doanhThuNam = data.Database.SqlQuery<DoanhThuNamViewModel>(
                "EXEC ThongKeDoanhThuNam @Nam",
                new SqlParameter("@Nam", namHienTai)
            ).FirstOrDefault();

            // List chứa đúng 1 dòng
            var lstDoanhThuNam = new List<DoanhThuNamViewModel>();
            if (doanhThuNam != null)
                lstDoanhThuNam.Add(doanhThuNam);

            // 2. Doanh thu theo quý
            var doanhThuQuy = data.Database.SqlQuery<DoanhThuQuyViewModel>(
                "EXEC ThongKeDoanhThu_Quy @Nam",
                new SqlParameter("@Nam", namHienTai)
            ).ToList();

            // 3. Doanh thu theo tháng
            var doanhThuThang = data.Database.SqlQuery<DoanhThuThangViewModel>(
                "EXEC ThongKeDoanhThuTheoThang @Nam",
                new SqlParameter("@Nam", namHienTai)
            ).ToList();

            // 4. Sản phẩm bán chạy
            var sanPhamBanChay = new List<SanPhamBanChayViewModel>();
            for (int maSP = 1; maSP <= 10; maSP++)
            {
                var sp = data.Database.SqlQuery<SanPhamBanChayViewModel>(
                    "SELECT * FROM dbo.Tong_SPDaBan(@MaSP)",
                    new SqlParameter("@MaSP", maSP)
                ).FirstOrDefault();

                if (sp != null)
                    sanPhamBanChay.Add(sp);
            }

            // Ghép vào ViewModel
            var model = new AdminDashboardViewModel
            {
                TongDoanhThuNam = doanhThuNam?.TongDoanhThu ?? 0,
                SoLuongDonNam = doanhThuNam?.SoLuongDon ?? 0,
                Nam = namHienTai,
                DoanhThuNam = lstDoanhThuNam,          // <-- BẮT BUỘC PHẢI CÓ
                DoanhThuQuy = doanhThuQuy,
                DoanhThuThang = doanhThuThang,
                SanPhamBanChay = sanPhamBanChay
            };

            return View(model);
        }


    }

}