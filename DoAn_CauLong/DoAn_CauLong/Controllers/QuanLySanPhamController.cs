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
            // Eager loading: Tải luôn LoaiSanPham và Hang để tránh lỗi N+1 query
            var sanPhams = data.SanPhams.Include(s => s.LoaiSanPham).Include(s => s.Hang).OrderByDescending(s => s.NgayTao).ToList();
            return View(sanPhams);
        }

        // GET: QuanLySanPham/Details/5
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
                .Include(s => s.ChiTietSanPhams) // Tải danh sách biến thể
                .Include(s => s.ChiTietSanPhams.Select(ct => ct.MauSac)) // Tải màu của biến thể
                .Include(s => s.ChiTietSanPhams.Select(ct => ct.Size)) // Tải size của biến thể
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
            // Tải dữ liệu cho các DropDownList
            ViewBag.MaLoai = new SelectList(data.LoaiSanPhams, "MaLoai", "TenLoai");
            ViewBag.MaNhaCungCap = new SelectList(data.NhaCungCaps, "MaNhaCungCap", "TenNhaCungCap");
            ViewBag.MaHang = new SelectList(data.Hangs, "MaHang", "TenHang");
            ViewBag.MaKhuyenMai = new SelectList(data.KhuyenMais, "MaKhuyenMai", "TenChuongTrinh");

            return View();
        }

        // POST: QuanLySanPham/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Cho phép nhập HTML vào (ví dụ: CKEditor cho MoTa)
        public ActionResult Create([Bind(Include = "TenSanPham,MoTa,GiaGoc,MaLoai,MaNhaCungCap,MaHang,MaKhuyenMai,CoSize,CoMau")] SanPham sanPham, HttpPostedFileBase HinhAnhUpload)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload file hình ảnh
                if (HinhAnhUpload != null && HinhAnhUpload.ContentLength > 0)
                {
                    // Lấy tên file
                    string fileName = Path.GetFileName(HinhAnhUpload.FileName);
                    // Tạo đường dẫn lưu file trên server
                    string path = Path.Combine(Server.MapPath("~/Content/Images"), fileName);
                    // Lưu file
                    HinhAnhUpload.SaveAs(path);

                    // Lưu tên file vào model
                    sanPham.HinhAnhDaiDien = fileName;
                }

                // Gán ngày tạo
                sanPham.NgayTao = DateTime.Now;

                // Lưu vào CSDL
                data.SanPhams.Add(sanPham);
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

        // GET: QuanLySanPham/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // SỬA LỖI: Đổi 'data' thành 'data'
            SanPham sanPham = data.SanPhams.Find(id);
            if (sanPham == null)
            {
                return HttpNotFound();
            }

            // Tải danh sách cho các DropDownList
            // Quan trọng: Truyền "sanPham.MaLoai" làm giá trị được chọn (selected value)
            ViewBag.MaLoai = new SelectList(data.LoaiSanPhams, "MaLoai", "TenLoai", sanPham.MaLoai);
            ViewBag.MaNhaCungCap = new SelectList(data.NhaCungCaps, "MaNhaCungCap", "TenNhaCungCap", sanPham.MaNhaCungCap);
            ViewBag.MaHang = new SelectList(data.Hangs, "MaHang", "TenHang", sanPham.MaHang);
            ViewBag.MaKhuyenMai = new SelectList(data.KhuyenMais, "MaKhuyenMai", "TenChuongTrinh", sanPham.MaKhuyenMai);

            return View(sanPham);
        }

        // POST: QuanLySanPham/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Cho phép nhập HTML
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

            // Xóa sản phẩm (CSDL sẽ tự xóa ChiTietSanPham do đã cài ON DELETE CASCADE)
            data.SanPhams.Remove(sanPham);
            data.SaveChanges();

            return RedirectToAction("Index");
        }

        // Giải phóng tài nguyên dataContext
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