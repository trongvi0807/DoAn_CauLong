using DoAn_CauLong.Models;
using DoAn_CauLong.ViewModels;
using System;
using System.Data.Entity; // Cần thiết cho EntityState.Modified
using System.Linq;
using System.Web.Mvc;
// Không cần thêm các using không dùng (như System.Data, System.Reflection)

namespace QLDN_CauLong.Controllers
{
    public class HomeController : Controller
    {
        // GIỮ NGUYÊN: Sử dụng DbContext ở cấp độ Controller
        QLDN_CAULONGEntities data = new QLDN_CAULONGEntities();

        public ActionResult Index()
        {
            var sp = data.SanPhams
                         .Include(p => p.LoaiSanPham) // Sử dụng Lambda Expression
                         .ToList();
            return View(sp);
        }

        // Action: Hiển thị chi tiết sản phẩm
        public ActionResult ChiTietSanPham(int id)
        {
            // Logic chi tiết sản phẩm không thay đổi
            // ... (Giữ nguyên ChiTietSanPham logic)
            var sanPham = data.SanPhams
                .Include(sp => sp.Hang)
                .Include(sp => sp.KhuyenMai)
                .FirstOrDefault(sp => sp.MaSanPham == id);

            if (sanPham == null)
            {
                return HttpNotFound();
            }

            var variants = data.ChiTietSanPhams
                .Where(cts => cts.MaSanPham == id)
                .Include(cts => cts.MauSac)
                .Include(cts => cts.Size)
                .Include(cts => cts.ThongSoVots)
                .ToList();

            var reviews = data.PhanHois
                .Where(ph => ph.MaSanPham == id)
                .ToList();

            double averageRating = reviews.Any() ? reviews.Average(ph => (double)ph.DanhGia) : 0;
            int reviewCount = reviews.Count();

            var viewModel = new ProductDetailViewModel
            {
                SanPham = sanPham,
                Variants = variants,
                AverageRating = averageRating,
                ReviewCount = reviewCount,
                AvailableColors = variants.Where(v => v.MauSac != null).Select(v => v.MauSac).Distinct().ToList(),
                AvailableSizes = variants.Where(v => v.Size != null).Select(v => v.Size).Distinct().ToList(),
            };

            return View(viewModel);
        }

        // Hàm helper để cập nhật số lượng Giỏ hàng trong Session
        private void UpdateCartSession(int maKhachHang)
        {
            // Cần đảm bảo rằng data (DbContext) được khai báo và có thể truy cập ở đây
            int cartCount = data.GioHangs
                                .Where(g => g.MaKhachHang == maKhachHang)
                                .Sum(g => (int?)g.SoLuong) ?? 0;

            // Cập nhật giá trị vào Session
            Session["GioHangCount"] = cartCount;
        }

        // Action: Thêm sản phẩm vào giỏ hàng
        [HttpPost]
        [Authorize]
        public ActionResult AddToCart(int chiTietId, int quantity)
        {
            // 1. Lấy MaKhachHang hiện tại (Đã xác thực)
            int maKhachHang = GetMaKhachHangFromLoggedInUser(User.Identity.Name);

            // 2. Tìm xem mục hàng đã có trong DB chưa
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

            // 3. Lưu thay đổi vào Database
            data.SaveChanges();

            // ✅ BƯỚC KHẮC PHỤC: Cập nhật Session Giỏ hàng sau khi lưu vào DB
            UpdateCartSession(maKhachHang);

            TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng!";
            return RedirectToAction("ViewCart");
        }

        // Hàm giả định để lấy MaKhachHang (Giữ nguyên)
        private int GetMaKhachHangFromLoggedInUser(string userName)
        {
            // CẦN TỰ IMPLEMENT LOGIC TRUY VẤN DB THỰC TẾ Ở ĐÂY
            // Ví dụ: return data.KhachHangs.Single(k => k.TaiKhoan.TenDangNhap == userName).MaKhachHang;
            return 1;
        }

        // Action: Xem Giỏ hàng
        [Authorize] // 🛡️ ASP.NET MVC sẽ kiểm tra Cookie. Nếu không có, tự động chuyển hướng.
        public ActionResult ViewCart()
        {
            // ❌ ĐÃ LOẠI BỎ KHỐI IF KIỂM TRA ĐĂNG NHẬP THỦ CÔNG

            int maKhachHang = GetMaKhachHangFromLoggedInUser(User.Identity.Name);

            // Tối ưu hóa truy vấn bằng Projection (.Select)
            var cartViewModels = data.GioHangs
                .Where(g => g.MaKhachHang == maKhachHang)
                .OrderByDescending(g => g.NgayThem)
                .Select(g => new CartItemViewModel
                {
                    MaGioHang = g.MaGioHang,
                    MaChiTietSanPham = g.MaChiTietSanPham.Value,
                    SoLuong = g.SoLuong.Value,

                    // Đảm bảo các Navigation Property là NOT NULL trong truy vấn
                    TenSanPham = g.ChiTietSanPham.SanPham.TenSanPham,
                    GiaBan = g.ChiTietSanPham.GiaBan ?? 0,
                    HinhAnh = g.ChiTietSanPham.HinhAnh ?? g.ChiTietSanPham.SanPham.HinhAnhDaiDien,
                    TenMau = g.ChiTietSanPham.MauSac.TenMau,
                    TenSize = g.ChiTietSanPham.Size.TenSize,
                })
                .ToList();

            int totalItems = cartViewModels.Sum(item => item.SoLuong);
            Session["GioHangCount"] = totalItems; // <-- Thêm dòng này
            return View(cartViewModels);
        }
    }
}