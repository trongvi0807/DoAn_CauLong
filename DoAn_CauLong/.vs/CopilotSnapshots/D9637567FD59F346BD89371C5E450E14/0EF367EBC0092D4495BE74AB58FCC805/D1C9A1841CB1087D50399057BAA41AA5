using DoAn_CauLong.Models;
using DoAn_CauLong.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace QLDN_CauLong.Controllers
{
    public class HomeController : Controller
    {
        // GIỮ NGUYÊN: Sử dụng DbContext ở cấp độ Controller
        QLDN_CAULONGEntities data = new QLDN_CAULONGEntities();

        public ActionResult Index()
        {
            var sp = data.SanPhams
                         .Include("LoaiSanPham")
                         .ToList();
            return View(sp);
        }


        // Action: Hiển thị chi tiết sản phẩm
        public ActionResult ChiTietSanPham(int id)
        {
            // 1. Lấy dữ liệu Sản phẩm chính
            var sanPham = data.SanPhams
                .Include(sp => sp.Hang)
                .Include(sp => sp.KhuyenMai)
                .FirstOrDefault(sp => sp.MaSanPham == id);

            if (sanPham == null)
            {
                return HttpNotFound();
            }

            // 2. Lấy tất cả biến thể chi tiết
            var variants = data.ChiTietSanPhams
                .Where(cts => cts.MaSanPham == id)
                .Include(cts => cts.MauSac)
                .Include(cts => cts.Size)
                .Include(cts => cts.ThongSoVots)
                .ToList();

            // 3. Lấy thông tin Đánh giá
            var reviews = data.PhanHois
                .Where(ph => ph.MaSanPham == id)
                .ToList();

            double averageRating = reviews.Any() ? reviews.Average(ph => (double)ph.DanhGia) : 0;
            int reviewCount = reviews.Count();

            // 4. Chuẩn bị ViewModel
            var viewModel = new ProductDetailViewModel
            {
                SanPham = sanPham,
                Variants = variants,
                AverageRating = averageRating,
                ReviewCount = reviewCount,
                AvailableColors = variants.Where(v => v.MauSac != null).Select(v => v.MauSac).Distinct().ToList(),
                AvailableSizes = variants.Where(v => v.Size != null).Select(v => v.Size).Distinct().ToList(),
            };

            // 5. Truyền ViewModel sang View
            return View(viewModel);
        }

        // Action: Thêm sản phẩm vào giỏ hàng
        [HttpPost]
        public ActionResult AddToCart(int chiTietId, int quantity)
        {
            // 1. KIỂM TRA ĐĂNG NHẬP
            if (!User.Identity.IsAuthenticated)
            {
                string returnUrl = Request.Url?.ToString() ?? Url.Action("Index", "Home");
                // ĐÃ SỬA: Chuyển hướng đến TaiKhoan/DangNhap
                return RedirectToAction("DangNhap", "TaiKhoan", new { returnUrl = returnUrl });
            }

            // 2. Lấy MaKhachHang hiện tại
            int maKhachHang = GetMaKhachHangFromLoggedInUser(User.Identity.Name);

            // 3. Tìm xem mục hàng đã có trong DB chưa
            var existingCartItem = data.GioHangs
                .SingleOrDefault(g => g.MaKhachHang == maKhachHang && g.MaChiTietSanPham == chiTietId);

            if (existingCartItem == null)
            {
                // A. Mục hàng chưa tồn tại: Tạo mới
                var newCartItem = new GioHang
                {
                    MaKhachHang = maKhachHang,
                    MaChiTietSanPham = chiTietId,
                    SoLuong = quantity,
                    NgayThem = DateTime.Now
                };
                data.GioHangs.Add(newCartItem);
            }
            else
            {
                // B. Mục hàng đã tồn tại: Cập nhật số lượng
                existingCartItem.SoLuong += quantity;
                existingCartItem.NgayThem = DateTime.Now;
                data.Entry(existingCartItem).State = EntityState.Modified;
            }

            data.SaveChanges();
            TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng!";
            return RedirectToAction("ViewCart");
        }

        // Hàm giả định để lấy MaKhachHang (Giữ nguyên)
        private int GetMaKhachHangFromLoggedInUser(string userName)
        {
            // CẦN TỰ IMPLEMENT LOGIC TRUY VẤN DB THỰC TẾ Ở ĐÂY
            return 1;
        }

        // Action: Xem Giỏ hàng
        public ActionResult ViewCart()
        {
            // 1. KIỂM TRA ĐĂNG NHẬP
            if (!User.Identity.IsAuthenticated)
            {
                // ĐÃ SỬA: Chuyển hướng đến TaiKhoan/DangNhap
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            int maKhachHang = GetMaKhachHangFromLoggedInUser(User.Identity.Name);

            // 2. Tối ưu hóa truy vấn bằng Projection (.Select)
            var cartViewModels = data.GioHangs
                .Where(g => g.MaKhachHang == maKhachHang)
                .OrderByDescending(g => g.NgayThem)
                .Select(g => new CartItemViewModel
                {
                    MaGioHang = g.MaGioHang,
                    MaChiTietSanPham = g.MaChiTietSanPham.Value,
                    SoLuong = g.SoLuong.Value,

                    TenSanPham = g.ChiTietSanPham.SanPham.TenSanPham,
                    GiaBan = g.ChiTietSanPham.GiaBan ?? 0,
                    HinhAnh = g.ChiTietSanPham.HinhAnh ?? g.ChiTietSanPham.SanPham.HinhAnhDaiDien,
                    TenMau = g.ChiTietSanPham.MauSac.TenMau,
                    TenSize = g.ChiTietSanPham.Size.TenSize
                })
                .ToList();

            // 3. Hiển thị View Giỏ hàng
            return View(cartViewModels);
        }
    }
}