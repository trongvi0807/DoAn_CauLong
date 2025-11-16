using DoAn_CauLong.Models;
using DoAn_CauLong.ViewModels;
using DoAn_CauLong.Filters; // << QUAN TRỌNG: Đảm bảo có 'using' này
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace DoAn_CauLong.Controllers
{
    public class DonHangController : Controller
    {
        QLDN_CAULONGEntities data = new QLDN_CAULONGEntities();

        // Hàm này lấy MaKhachHang dựa trên MaTaiKhoan trong Session
        private int GetMaKhachHangFromSession()
        {
            if (Session["MaTaiKhoan"] != null)
            {
                int maTaiKhoan = (int)Session["MaTaiKhoan"];
                var khachHang = data.KhachHangs.FirstOrDefault(kh => kh.MaTaiKhoan == maTaiKhoan);
                if (khachHang != null)
                {
                    return khachHang.MaKhachHang;
                }
            }
            return -1; // Trả về -1 nếu không tìm thấy
        }

        // ===================================================================
        // ACTION (GET): HIỂN THỊ TRANG THANH TOÁN (Bạn đã có)
        // ===================================================================
        [CheckLogin] // Bắt buộc đăng nhập
        public ActionResult Checkout()
        {
            int maKhachHang = GetMaKhachHangFromSession();
            if (maKhachHang == -1)
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            // 1. Lấy thông tin khách hàng (để điền sẵn vào form)
            var khachHang = data.KhachHangs.Find(maKhachHang);

            // 2. Lấy giỏ hàng
            var cartItems = data.GioHangs
                .Where(g => g.MaKhachHang == maKhachHang)
                .Select(g => new CartItemViewModel
                {
                    MaChiTietSanPham = g.MaChiTietSanPham ?? 0,
                    SoLuong = g.SoLuong ?? 0,
                    TenSanPham = g.ChiTietSanPham.SanPham.TenSanPham,
                    GiaBan = g.ChiTietSanPham.GiaBan ?? 0,
                    HinhAnh = g.ChiTietSanPham.HinhAnh ?? g.ChiTietSanPham.SanPham.HinhAnhDaiDien,
                    TenMau = g.ChiTietSanPham.MauSac != null ? g.ChiTietSanPham.MauSac.TenMau : "N/A",
                    TenSize = g.ChiTietSanPham.Size != null ? g.ChiTietSanPham.Size.TenSize : "N/A"
                })
                .ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn trống. Không thể thanh toán.";
                return RedirectToAction("ViewCart", "Home");
            }

            // 3. Tạo một ViewModel để gửi 2 loại dữ liệu sang View
            var viewModel = new CheckoutViewModel
            {
                CustomerInfo = khachHang,
                CartItems = cartItems,
                Total = cartItems.Sum(item => item.ThanhTien)
            };

            return View(viewModel);
        }

        // ===================================================================
        // ACTION (POST): XỬ LÝ ĐẶT HÀNG (Đây là code mới)
        // ===================================================================
        [HttpPost]
        [CheckLogin]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(string HoTenNhan, string SoDienThoaiNhan, string DiaChiGiao, string GhiChu)
        {
            int maKhachHang = GetMaKhachHangFromSession();
            if (maKhachHang == -1)
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            // Lấy lại giỏ hàng từ DB
            var cartItems = data.GioHangs
                                .Where(g => g.MaKhachHang == maKhachHang)
                                .Include(g => g.ChiTietSanPham) // Lấy ChiTietSanPham
                                .ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn trống.";
                return RedirectToAction("ViewCart", "Home");
            }

            // Dùng transaction để đảm bảo tất cả cùng thành công hoặc thất bại
            using (var transaction = data.Database.BeginTransaction())
            {
                try
                {
                    // 1. Tạo Đơn Hàng
                    DonHang newOrder = new DonHang
                    {
                        MaKhachHang = maKhachHang,
                        NgayDat = DateTime.Now,
                        TrangThai = "Chờ xác nhận", // Trạng thái mặc định
                        DiaChiGiaoHang = DiaChiGiao,
                        SoDienThoaiNhanHang = SoDienThoaiNhan,
                        GhiChu = GhiChu,
                        TongTien = 0 // Sẽ cập nhật sau
                    };
                    data.DonHangs.Add(newOrder);
                    data.SaveChanges(); // Lưu để lấy MaDonHang

                    decimal tongTienDonHang = 0;

                    // 2. Chuyển sản phẩm từ Giỏ Hàng sang Chi Tiết Đơn Hàng
                    foreach (var item in cartItems)
                    {
                        // Lấy giá bán (và khuyến mãi nếu có)
                        decimal donGia = item.ChiTietSanPham.GiaBan ?? 0;

                        // (Bạn có thể thêm logic tính lại KM phức tạp ở đây nếu cần)

                        var chiTietDH = new ChiTietDonHang
                        {
                            MaDonHang = newOrder.MaDonHang,
                            MaChiTietSanPham = item.MaChiTietSanPham,
                            SoLuong = item.SoLuong,
                            DonGia = donGia,
                            ThanhTien = (item.SoLuong ?? 0) * donGia
                        };
                        data.ChiTietDonHangs.Add(chiTietDH);
                        tongTienDonHang += chiTietDH.ThanhTien ?? 0;

                        // (Tùy chọn: Giảm số lượng tồn kho)
                        // var ctsp = data.ChiTietSanPhams.Find(item.MaChiTietSanPham);
                        // if(ctsp != null) ctsp.SoLuongTon -= item.SoLuong;
                    }

                    // 3. Cập nhật tổng tiền cho Đơn Hàng
                    newOrder.TongTien = tongTienDonHang;
                    data.Entry(newOrder).State = EntityState.Modified;

                    // 4. Xóa các mục khỏi Giỏ Hàng
                    data.GioHangs.RemoveRange(cartItems);

                    // 5. Lưu tất cả thay đổi
                    data.SaveChanges();
                    transaction.Commit(); // Hoàn tất giao dịch

                    // 6. Cập nhật giỏ hàng trên layout về 0
                    Session["GioHangCount"] = 0;

                    TempData["Success"] = "Đặt hàng thành công! Cảm ơn bạn đã mua sắm.";
                    return RedirectToAction("Index"); // Chuyển đến trang Lịch sử đơn hàng
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // Hoàn tác nếu có lỗi
                    TempData["Error"] = "Đã xảy ra lỗi khi đặt hàng: " + ex.Message;
                    return RedirectToAction("Checkout"); // Quay lại trang thanh toán
                }
            }
        }

        // ===================================================================
        // ACTION (GET): HIỂN THỊ DANH SÁCH ĐƠN HÀNG (Mục "ĐƠN HÀNG")
        // ===================================================================
        [CheckLogin]
        public ActionResult Index()
        {
            int maKhachHang = GetMaKhachHangFromSession();
            if (maKhachHang == -1)
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            // Lấy tất cả đơn hàng của khách, sắp xếp mới nhất lên đầu
            var orders = data.DonHangs
                             .Include(d => d.KhachHang) // << ✨ THÊM DÒNG NÀY
                             .Where(d => d.MaKhachHang == maKhachHang)
                             .OrderByDescending(d => d.NgayDat)
                             .ToList();

            return View(orders);
        }

        // Thêm hàm Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                data.Dispose();
            }
            base.Dispose(disposing);
        }
        [CheckLogin]
        public ActionResult Details(int id) // 'id' này là MaDonHang
        {
            // 1. Lấy MaKhachHang từ session để bảo mật,
            // đảm bảo người này CHỈ xem được đơn hàng của mình.
            int maKhachHang = GetMaKhachHangFromSession();
            if (maKhachHang == -1)
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            // 2. Lấy đơn hàng VÀ tất cả chi tiết liên quan
            var order = data.DonHangs
                .Include(d => d.KhachHang) // Lấy info Khách hàng
                .Include(d => d.ChiTietDonHangs.Select(ct => ct.ChiTietSanPham.SanPham)) // Lấy info Sản phẩm
                .Include(d => d.ChiTietDonHangs.Select(ct => ct.ChiTietSanPham.MauSac))  // Lấy info Màu
                .Include(d => d.ChiTietDonHangs.Select(ct => ct.ChiTietSanPham.Size))    // Lấy info Size
                .FirstOrDefault(d => d.MaDonHang == id && d.MaKhachHang == maKhachHang); // Quan trọng: chỉ lấy đơn của khách này

            // 3. Kiểm tra
            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền xem đơn hàng này.";
                return RedirectToAction("Index");
            }

            // 4. Trả về View
            return View(order); // Gửi TOÀN BỘ đối tượng DonHang (đã .Include()) sang View
        }
    }
}