using DoAn_CauLong.Models;
using DoAn_CauLong.ViewModels;
using DoAn_CauLong.Filters;
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

       
        private decimal TinhGiaBanThucTe(ChiTietSanPham item)
        {
            // 1. Lấy giá gốc (ưu tiên giá biến thể, nếu null thì lấy giá sản phẩm cha)
            decimal giaGoc = item.GiaBan ?? item.SanPham.GiaGoc ?? 0;
            decimal giaBanHienTai = giaGoc;

            // 2. Lấy thông tin khuyến mãi từ sản phẩm cha
            var khuyenMai = item.SanPham.KhuyenMai;

            // 3. Kiểm tra logic khuyến mãi
            if (khuyenMai != null)
            {
                DateTime now = DateTime.Now;

                // XỬ LÝ NGÀY KẾT THÚC: Nếu có ngày kết thúc, ta cho phép khuyến mãi đến hết giây cuối cùng của ngày đó (23:59:59)
                // Nếu NgayKetThuc trong DB là 00:00:00, ta cộng thêm 1 ngày rồi trừ 1 tick để thành cuối ngày.
                DateTime? ngayBatDau = khuyenMai.NgayBatDau;
                DateTime? ngayKetThuc = khuyenMai.NgayKetThuc;

                if (ngayKetThuc.HasValue && ngayKetThuc.Value.TimeOfDay == TimeSpan.Zero)
                {
                    ngayKetThuc = ngayKetThuc.Value.Date.AddDays(1).AddTicks(-1);
                }

                // Kiểm tra ngày bắt đầu và kết thúc
                bool isActive = (ngayBatDau == null || ngayBatDau <= now) &&
                                (ngayKetThuc == null || ngayKetThuc >= now);

                if (isActive)
                {
                    decimal phanTram = khuyenMai.PhanTramGiam ?? 0;
                    decimal soTienGiam = giaGoc * (phanTram / 100);

                    // Kiểm tra mức giảm tối đa (nếu có)
                    if (khuyenMai.GiamToiDa.HasValue && soTienGiam > khuyenMai.GiamToiDa.Value)
                    {
                        soTienGiam = khuyenMai.GiamToiDa.Value;
                    }

                    giaBanHienTai = giaGoc - soTienGiam;
                }
            }

            return giaBanHienTai;
        }

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
            return -1;
        }

       
        // HIỂN THỊ TRANG THANH TOÁN
      
        [CheckLogin]
        public ActionResult Checkout()
        {
            int maKhachHang = GetMaKhachHangFromSession();
            if (maKhachHang == -1)
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var khachHang = data.KhachHangs.Find(maKhachHang);

            // Lấy giỏ hàng kèm theo thông tin Khuyến Mãi để tính toán
            var cartRawItems = data.GioHangs
                .Where(g => g.MaKhachHang == maKhachHang)
                .Include(g => g.ChiTietSanPham)
                .Include(g => g.ChiTietSanPham.SanPham)
                .Include(g => g.ChiTietSanPham.SanPham.KhuyenMai) // <--- Include bảng Khuyến Mãi
                .Include(g => g.ChiTietSanPham.MauSac)
                .Include(g => g.ChiTietSanPham.Size)
                .ToList();

            if (!cartRawItems.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn trống. Không thể thanh toán.";
                return RedirectToAction("ViewCart", "Home");
            }

            // Chuyển đổi sang ViewModel và tính giá đã giảm
            var cartViewModels = new List<CartItemViewModel>();
            foreach (var item in cartRawItems)
            {
                decimal giaThucTe = TinhGiaBanThucTe(item.ChiTietSanPham);

                cartViewModels.Add(new CartItemViewModel
                {
                    MaChiTietSanPham = item.MaChiTietSanPham ?? 0,
                    SoLuong = item.SoLuong ?? 0,
                    TenSanPham = item.ChiTietSanPham.SanPham.TenSanPham,
                    // GÁN GIÁ ĐÃ GIẢM VÀO ĐÂY
                    GiaBan = giaThucTe,
                    HinhAnh = item.ChiTietSanPham.HinhAnh ?? item.ChiTietSanPham.SanPham.HinhAnhDaiDien,
                    TenMau = item.ChiTietSanPham.MauSac != null ? item.ChiTietSanPham.MauSac.TenMau : "N/A",
                    TenSize = item.ChiTietSanPham.Size != null ? item.ChiTietSanPham.Size.TenSize : "N/A"
                });
            }

            var viewModel = new CheckoutViewModel
            {
                CustomerInfo = khachHang,
                CartItems = cartViewModels,
                // Tổng tiền sẽ được tính dựa trên giá đã giảm
                Total = cartViewModels.Sum(item => item.ThanhTien)
            };

            return View(viewModel);
        }

        
        // XỬ LÝ ĐẶT HÀNG VÀ LƯU DATABASE
        
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

            // Lấy lại giỏ hàng kèm thông tin Khuyến Mãi
            var cartItems = data.GioHangs
                                .Where(g => g.MaKhachHang == maKhachHang)
                                .Include(g => g.ChiTietSanPham)
                                .Include(g => g.ChiTietSanPham.SanPham)
                                .Include(g => g.ChiTietSanPham.SanPham.KhuyenMai) 
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
                    // 1. TẠO ĐƠN HÀNG
                    var maDHParam = new System.Data.SqlClient.SqlParameter("@MaDonHang_OUT", System.Data.SqlDbType.Int)
                    {
                        Direction = System.Data.ParameterDirection.Output
                    };

                    data.Database.ExecuteSqlCommand(
                        "EXEC ThemDonHang @MaKhachHang, @DiaChi, @SoDienThoai, @GhiChu, @MaDonHang_OUT OUTPUT",
                        new System.Data.SqlClient.SqlParameter("@MaKhachHang", maKhachHang),
                        new System.Data.SqlClient.SqlParameter("@DiaChi", DiaChiGiao),
                        new System.Data.SqlClient.SqlParameter("@SoDienThoai", SoDienThoaiNhan),
                        new System.Data.SqlClient.SqlParameter("@GhiChu", GhiChu),
                        maDHParam
                    );

                    int newMaDonHang = (int)maDHParam.Value;
                    decimal tongTienDonHang = 0;

                    // 2. LƯU CHI TIẾT ĐƠN HÀNG VỚI GIÁ ĐÃ GIẢM
                    foreach (var item in cartItems)
                    {
                        // Gọi hàm tính giá đã giảm
                        decimal giaDaGiam = TinhGiaBanThucTe(item.ChiTietSanPham);

                        var chiTietDH = new ChiTietDonHang
                        {
                            MaDonHang = newMaDonHang,
                            MaChiTietSanPham = item.MaChiTietSanPham,
                            SoLuong = item.SoLuong,
                            // Lưu giá thực tế (đã giảm) vào Database
                            DonGia = giaDaGiam,
                            ThanhTien = (item.SoLuong ?? 0) * giaDaGiam
                        };
                        data.ChiTietDonHangs.Add(chiTietDH);
                        tongTienDonHang += chiTietDH.ThanhTien ?? 0;
                    }

                    // 3. CẬP NHẬT TỔNG TIỀN CHO ĐƠN HÀNG
                    var newOrder = data.DonHangs.Find(newMaDonHang);
                    if (newOrder != null)
                    {
                        newOrder.TongTien = tongTienDonHang;
                        newOrder.TongTienSauGiam = tongTienDonHang;
                        data.Entry(newOrder).State = EntityState.Modified;
                    }

                    // 4. Xóa giỏ hàng
                    data.GioHangs.RemoveRange(cartItems);

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

        
        [CheckLogin]
        public ActionResult Index()
        {
            int maKhachHang = GetMaKhachHangFromSession();
            if (maKhachHang == -1)
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var orders = data.DonHangs
                             .Include(d => d.KhachHang)
                             .Where(d => d.MaKhachHang == maKhachHang)
                             .OrderByDescending(d => d.NgayDat)
                             .ToList();

            return View(orders);
        }

        [CheckLogin]
        public ActionResult Details(int id)
        {
            int maKhachHang = GetMaKhachHangFromSession();
            if (maKhachHang == -1)
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var order = data.DonHangs
                .Include(d => d.KhachHang)
                .Include(d => d.ChiTietDonHangs.Select(ct => ct.ChiTietSanPham.SanPham))
                .Include(d => d.ChiTietDonHangs.Select(ct => ct.ChiTietSanPham.MauSac))
                .Include(d => d.ChiTietDonHangs.Select(ct => ct.ChiTietSanPham.Size))
                .FirstOrDefault(d => d.MaDonHang == id && d.MaKhachHang == maKhachHang);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index");
            }

            return View(order);
        }

        [HttpPost]
        [CheckLogin]
        [ValidateAntiForgeryToken]
        public ActionResult HuyDonHang(int id)
        {
            int maKhachHang = GetMaKhachHangFromSession();
            if (maKhachHang == -1) return RedirectToAction("DangNhap", "TaiKhoan");

            using (var transaction = data.Database.BeginTransaction())
            {
                try
                {
                    var order = data.DonHangs.FirstOrDefault(d => d.MaDonHang == id && d.MaKhachHang == maKhachHang);
                    if (order == null)
                    {
                        TempData["Error"] = "Không tìm thấy đơn hàng.";
                        return RedirectToAction("Index");
                    }

                    if (order.TrangThai != "Chờ xác nhận")
                    {
                        TempData["Error"] = "Đơn hàng đang xử lý, không thể hủy.";
                        return RedirectToAction("Index");
                    }

                    order.TrangThai = "Đã hủy";
                    data.Entry(order).State = EntityState.Modified;
                    data.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] = "Đã hủy đơn hàng #" + id;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["Error"] = "Lỗi: " + ex.Message;
                }
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) data.Dispose();
            base.Dispose(disposing);
        }
    }
}