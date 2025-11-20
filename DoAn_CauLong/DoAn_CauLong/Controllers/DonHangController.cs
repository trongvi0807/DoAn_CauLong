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
                else
                {
                    // Tự động tạo thông tin nếu chưa có
                    var taiKhoan = data.TaiKhoans.Find(maTaiKhoan);
                    if (taiKhoan != null)
                    {
                        KhachHang newKh = new KhachHang();
                        newKh.MaTaiKhoan = maTaiKhoan;


                        newKh.HoTen = taiKhoan.TenDangNhap;

                        newKh.Email = taiKhoan.Email;
                        newKh.SoDienThoai = "0000000000";
                        newKh.DiaChi = "Tại cửa hàng";
                        newKh.NgayTao = DateTime.Now;

                        data.KhachHangs.Add(newKh);
                        data.SaveChanges();

                        Session["HoTen"] = newKh.HoTen;
                        return newKh.MaKhachHang;
                    }
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

            using (var transaction = data.Database.BeginTransaction())
            {
                try
                {
                    // 1. TẠO ĐƠN HÀNG BẰNG STORED PROCEDURE
                    var maDHParam = new System.Data.SqlClient.SqlParameter("@MaDonHang_OUT", System.Data.SqlDbType.Int)
                    {
                        Direction = System.Data.ParameterDirection.Output // Thiết lập là tham số đầu ra
                    };

                    // Thực thi SP và nhận lại MaDonHang
                    data.Database.ExecuteSqlCommand(
                        "EXEC ThemDonHang @MaKhachHang, @DiaChi, @SoDienThoai, @GhiChu, @MaDonHang_OUT OUTPUT",
                        new System.Data.SqlClient.SqlParameter("@MaKhachHang", maKhachHang),
                        new System.Data.SqlClient.SqlParameter("@DiaChi", DiaChiGiao),
                        new System.Data.SqlClient.SqlParameter("@SoDienThoai", SoDienThoaiNhan),
                        new System.Data.SqlClient.SqlParameter("@GhiChu", GhiChu),
                        maDHParam // Tham số output
                    );

                    // Lấy MaDonHang vừa tạo
                    int newMaDonHang = (int)maDHParam.Value;
                    decimal tongTienDonHang = 0;

                    // 2. CHUYỂN SANG CHI TIẾT ĐƠN HÀNG
                    foreach (var item in cartItems)
                    {
                        decimal donGia = item.ChiTietSanPham.GiaBan ?? 0;

                        var chiTietDH = new ChiTietDonHang
                        {
                            MaDonHang = newMaDonHang, // Dùng ID lấy từ SP
                            MaChiTietSanPham = item.MaChiTietSanPham,
                            SoLuong = item.SoLuong,
                            DonGia = donGia,
                            ThanhTien = (item.SoLuong ?? 0) * donGia
                        };
                        data.ChiTietDonHangs.Add(chiTietDH);
                        tongTienDonHang += chiTietDH.ThanhTien ?? 0;
                    }

                    // 3. CẬP NHẬT TỔNG TIỀN (Tìm lại đơn hàng và update)
                    var newOrder = data.DonHangs.Find(newMaDonHang);
                    if (newOrder != null)
                    {
                        newOrder.TongTien = tongTienDonHang;
                        newOrder.TongTienSauGiam = tongTienDonHang; // Giả sử không có giảm giá
                        data.Entry(newOrder).State = EntityState.Modified;
                    }

                    // 4. Xóa các mục khỏi Giỏ Hàng
                    data.GioHangs.RemoveRange(cartItems);

                    // 5. Lưu và Hoàn tất
                    data.SaveChanges();
                    transaction.Commit();

                    Session["GioHangCount"] = 0;
                    TempData["Success"] = "Đặt hàng thành công! Đơn hàng #" + newMaDonHang;
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["Error"] = "Đã xảy ra lỗi khi đặt hàng: " + ex.Message;
                    return RedirectToAction("Checkout");
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

        [HttpPost] // Dùng POST để bảo mật
        [CheckLogin]
        [ValidateAntiForgeryToken]
        public ActionResult HuyDonHang(int id) // id là MaDonHang
        {
            int maKhachHang = GetMaKhachHangFromSession();
            if (maKhachHang == -1)
            {
                // Chưa đăng nhập
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            // Dùng transaction để đảm bảo an toàn dữ liệu
            using (var transaction = data.Database.BeginTransaction())
            {
                try
                {
                    // 1. Tìm đơn hàng
                    var order = data.DonHangs.FirstOrDefault(d => d.MaDonHang == id && d.MaKhachHang == maKhachHang);

                    if (order == null)
                    {
                        TempData["Error"] = "Không tìm thấy đơn hàng.";
                        return RedirectToAction("Index");
                    }

                    // 2. Chỉ cho phép hủy nếu là "Chờ xác nhận"
                    if (order.TrangThai != "Chờ xác nhận")
                    {
                        TempData["Error"] = "Đơn hàng này đang được xử lý hoặc đã hoàn tất, không thể hủy.";
                        return RedirectToAction("Index");
                    }

                    // 3. Cập nhật trạng thái
                    order.TrangThai = "Đã hủy";
                    data.Entry(order).State = EntityState.Modified;

                    // ======================================================
                    // ✨ SỬA Ở ĐÂY: Vô hiệu hóa khối code này
                    // ======================================================
                    // 4. (TẮT HOÀN TRẢ KHO) - Vì kho chưa bị trừ ở bước "Chờ xác nhận"
                    /*
                    var chiTietItems = data.ChiTietDonHangs.Where(ct => ct.MaDonHang == id).ToList();
                    foreach (var item in chiTietItems)
                    {
                        var sanPhamTonKho = data.ChiTietSanPhams.Find(item.MaChiTietSanPham);
                        if (sanPhamTonKho != null)
                        {
                            sanPhamTonKho.SoLuongTon = (sanPhamTonKho.SoLuongTon ?? 0) + (item.SoLuong ?? 0);
                            data.Entry(sanPhamTonKho).State = EntityState.Modified;
                        }
                    }
                    */
                    // ======================================================

                    // 5. Lưu tất cả
                    data.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] = "Đã hủy đơn hàng #" + id + " thành công.";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["Error"] = "Lỗi khi hủy đơn hàng: " + ex.Message;
                }
            }

            return RedirectToAction("Index");
        }
    }
}