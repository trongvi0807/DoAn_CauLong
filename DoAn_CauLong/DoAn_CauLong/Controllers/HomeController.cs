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
using DoAn_CauLong.Filters; 

namespace DoAn_CauLong.Controllers 
{
    public class HomeController : Controller
    {
        QLDN_CAULONGEntities data = new QLDN_CAULONGEntities();

        public ActionResult Index()
        {
            var sp = data.SanPhams
                         .Include(s => s.LoaiSanPham)
                         .Include(s => s.Hang)
                         .Include(s => s.KhuyenMai)
                         .ToList();
            return View(sp);
        }

        public ActionResult Contact()
        {
            
            return View();
        }
        
        public ActionResult ChiTietSanPham(int id)
        {
           
            var sanPham = data.SanPhams
                .Include(sp => sp.Hang)
                .Include(sp => sp.KhuyenMai)
                .Include(sp => sp.KhuyenMai) 
                .Include(sp => sp.KhuyenMai)
                .Include(sp => sp.KhuyenMai)
                .Include(sp => sp.KhuyenMai)
                .Include(sp => sp.KhuyenMai)
                .Include(sp => sp.KhuyenMai)
                .Include(sp => sp.KhuyenMai)
                .Include(sp => sp.KhuyenMai)
                .Include(sp => sp.KhuyenMai)
                .Include(sp => sp.KhuyenMai)
                .Include(sp => sp.KhuyenMai)
                .Include(sp => sp.KhuyenMai)
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

           

            //thêm câu truy vấn tính trung bình đánh giá
            var TrungBinhDanhGia = data.Database.SqlQuery<decimal?>(
                "SELECT dbo.TrungBinhDanhGia(@MaSP)",
                new SqlParameter("@MaSP", id)
            ).FirstOrDefault();

            // Hàm của bạn trả về decimal(4,2)
            double XepHangTrungBinh = (double)(TrungBinhDanhGia ?? 0.0m);

            //  Lấy số lượng phản hồi 
            int reviewCount = data.PhanHois
                .Where(ph => ph.MaSanPham == id)
                .Count();

            
            var reviews = data.PhanHois
            .Where(ph => ph.MaSanPham == id)
             .OrderByDescending(ph => ph.NgayPhanHoi) // phản hồi mới nhất sẽ thêm ở đầu
            .ToList();
            // 4. Chuẩn bị ViewModel
            var viewModel = new ProductDetailViewModel
            {
                SanPham = sanPham,
                Variants = variants,
                AverageRating = XepHangTrungBinh,
                ReviewCount = reviewCount,
                AvailableColors = variants.Where(v => v.MauSac != null).Select(v => v.MauSac).Distinct().ToList(),
                AvailableSizes = variants.Where(v => v.Size != null).Select(v => v.Size).Distinct().ToList(),

               
                ThongSoVot = variants.SelectMany(v => v.ThongSoVots).FirstOrDefault(),

                  Reviews = reviews
            };

            // 5. Truyền ViewModel sang View
            return View(viewModel);
        }

        // Thêm sản phẩm vào giỏ hàng
        [HttpPost]
        [CheckLogin] 
        public ActionResult AddToCart(int chiTietId, int quantity)
        {
            try
            {
                // 1. Lấy MaKhachHang hiện tại từ Session
                int maKhachHang = GetMaKhachHangFromSession();
                if (maKhachHang == -1)
                {
                    TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                    return RedirectToAction("DangNhap", "TaiKhoan");
                }

                // 2. Kiểm tra tồn kho
                var productVariant = data.ChiTietSanPhams.Find(chiTietId);
                if (productVariant == null)
                {
                    TempData["Error"] = "Sản phẩm không tồn tại.";
                    return Redirect(Request.UrlReferrer?.ToString() ?? Url.Action("Index"));
                }

                // 3. Tìm xem mục hàng đã có trong DB chưa
                var existingCartItem = data.GioHangs
                    .SingleOrDefault(g => g.MaKhachHang == maKhachHang && g.MaChiTietSanPham == chiTietId);

                int soLuongMoi = 0;
                if (existingCartItem != null)
                {
                    // Nếu đã có: Cộng dồn số lượng
                    soLuongMoi = (existingCartItem.SoLuong ?? 0) + quantity;
                }
                else
                {
                    // Nếu chưa có: Gán số lượng mới
                    soLuongMoi = quantity;
                }

                // 4. Kiểm tra tổng số lượng có vượt tồn kho không
                if ((productVariant.SoLuongTon ?? 0) < soLuongMoi)
                {
                    TempData["Error"] = "Số lượng sản phẩm trong giỏ vượt quá tồn kho!";
                    return Redirect(Request.UrlReferrer?.ToString() ?? Url.Action("Index"));
                }

                // 5. Thêm mới hoặc Cập nhật
                if (existingCartItem == null)
                {
                    var newCartItem = new GioHang
                    {
                        MaKhachHang = maKhachHang,
                        MaChiTietSanPham = chiTietId,
                        SoLuong = soLuongMoi,
                        NgayThem = DateTime.Now
                    };
                    data.GioHangs.Add(newCartItem);
                }
                else
                {
                    existingCartItem.SoLuong = soLuongMoi;
                    existingCartItem.NgayThem = DateTime.Now;
                    data.Entry(existingCartItem).State = EntityState.Modified;
                }

                data.SaveChanges();

                // 6. Cập nhật lại Session["GioHangCount"]
                UpdateCartCountInSession(maKhachHang);

                TempData["Message"] = "Thêm vào giỏ hàng thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi: " + ex.Message;
            }

            // Quay lại trang chi tiết sản phẩm
            return Redirect(Request.UrlReferrer?.ToString() ?? Url.Action("Index"));
        }

        // Action: Xem Giỏ hàng
        [CheckLogin]
        public ActionResult ViewCart()
        {
            int maKhachHang = GetMaKhachHangFromSession();
            if (maKhachHang == -1)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("Index");
            }

            // 1. Lấy dữ liệu thô từ Database
            var cartItems = data.GioHangs
                .Include(g => g.ChiTietSanPham)
                .Include(g => g.ChiTietSanPham.SanPham)
                .Include(g => g.ChiTietSanPham.SanPham.KhuyenMai) // Dòng này cực kỳ quan trọng
                .Include(g => g.ChiTietSanPham.MauSac)
                .Include(g => g.ChiTietSanPham.Size)
                .Where(g => g.MaKhachHang == maKhachHang)
                .OrderByDescending(g => g.NgayThem)
                .ToList();

            var cartViewModels = new List<CartItemViewModel>();

            foreach (var item in cartItems)
            {
                // --- FIX: Tải thủ công Khuyến Mãi nếu Include thất bại (Phòng hờ) ---
                if (item.ChiTietSanPham.SanPham.KhuyenMai == null && item.ChiTietSanPham.SanPham.MaKhuyenMai != null)
                {
                    // Load trực tiếp từ DB nếu object KhuyenMai chưa có
                    var kmDb = data.KhuyenMais.Find(item.ChiTietSanPham.SanPham.MaKhuyenMai);
                    item.ChiTietSanPham.SanPham.KhuyenMai = kmDb;
                }

                // Tính giá
                decimal giaDaGiam = TinhGiaBanThucTe(item.ChiTietSanPham);

                var viewModel = new CartItemViewModel
                {
                    MaGioHang = item.MaGioHang,
                    MaChiTietSanPham = item.MaChiTietSanPham ?? 0,
                    SoLuong = item.SoLuong ?? 0,
                    TenSanPham = item.ChiTietSanPham.SanPham.TenSanPham,
                    GiaBan = giaDaGiam, // Giá đã tính toán
                    HinhAnh = item.ChiTietSanPham.HinhAnh ?? item.ChiTietSanPham.SanPham.HinhAnhDaiDien,
                    TenMau = item.ChiTietSanPham.MauSac != null ? item.ChiTietSanPham.MauSac.TenMau : "N/A",
                    TenSize = item.ChiTietSanPham.Size != null ? item.ChiTietSanPham.Size.TenSize : "N/A"
                };

                cartViewModels.Add(viewModel);
            }

            return View(cartViewModels);
        }

    
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

        // Hàm này cập nhật số lượng trong Session
        private void UpdateCartCountInSession(int maKhachHang)
        {
            int totalItems = data.GioHangs
                         .Where(g => g.MaKhachHang == maKhachHang)
                         .Sum(g => (int?)g.SoLuong) ?? 0;

            Session["GioHangCount"] = totalItems;
        }

       
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                data.Dispose();
            }
            base.Dispose(disposing);
        }
        [CheckLogin]
        public ActionResult RemoveFromCart(int maGioHang)
        {
            try
            {
                // 1. Tìm mục giỏ hàng bằng MaGioHang (Primary Key)
                var cartItem = data.GioHangs.Find(maGioHang);
                int maKhachHang = -1;

                if (cartItem != null)
                {
                    // 2. Lấy MaKhachHang của item này TRƯỚC KHI XÓA
                    //    để biết cần cập nhật lại Session cho ai.
                    maKhachHang = cartItem.MaKhachHang ?? -1;

                    // 3. Xóa item
                    data.GioHangs.Remove(cartItem);
                    data.SaveChanges();

                    // 4. Cập nhật lại số lượng trong Session
                    if (maKhachHang != -1)
                    {
                        UpdateCartCountInSession(maKhachHang);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi xóa sản phẩm: " + ex.Message;
            }

            // 5. Quay lại trang giỏ hàng
            return RedirectToAction("ViewCart");
        }

        // HÀM HỖ TRỢ: TÍNH GIÁ BÁN THỰC TẾ (CÓ TRỪ KHUYẾN MÃI)
        private decimal TinhGiaBanThucTe(ChiTietSanPham item)
        {
            // 1. Lấy giá gốc (ưu tiên giá biến thể, nếu null thì lấy giá sản phẩm cha)
            decimal giaGoc = item.SanPham.GiaGoc ?? item.GiaBan ?? 0;
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

    }
}