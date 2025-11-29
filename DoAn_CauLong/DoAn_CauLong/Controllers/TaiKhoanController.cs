using DoAn_CauLong.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace DoAn_CauLong.Controllers
{
    public class TaiKhoanController : Controller
    {
        QLDN_CAULONGEntities data = new QLDN_CAULONGEntities();

        
        public ActionResult DangNhap(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
     
        public ActionResult DangNhap(string TenDangNhap, string MatKhau, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = data.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == TenDangNhap && t.MatKhau == MatKhau);

                if (user != null)
                {
                    Session["MaTaiKhoan"] = user.MaTaiKhoan;
                    Session["TenDangNhap"] = user.TenDangNhap;
                    Session["MaQuyen"] = user.MaQuyen;

                    var khachHang = data.KhachHangs.FirstOrDefault(k => k.MaTaiKhoan == user.MaTaiKhoan);

                    if (khachHang != null && !string.IsNullOrEmpty(khachHang.HoTen))
                    {
                        Session["MaKH"] = khachHang.MaKhachHang; // kiểm tra mã khách hàng-vừa thêm từ Vĩ
                        Session["HoTen"] = khachHang.HoTen; // Dùng Họ Tên
                    }
                    else
                    {
                        Session["HoTen"] = user.TenDangNhap;
                    }

                    
                    int cartCount = 0;
                    if (khachHang != null)
                    {
                        // Đếm tổng số lượng sản phẩm trong giỏ của khách hàng
                        cartCount = data.GioHangs
                .Where(g => g.MaKhachHang == khachHang.MaKhachHang)
                .Sum(g => (int?)g.SoLuong) ?? 0;
                    }
                    Session["GioHangCount"] = cartCount;


                 
                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }

            // Nếu đăng nhập thất bại, gửi lại returnUrl
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        public ActionResult DangKy()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangKy(string HoTen, string TenDangNhap, string MatKhau, string Email, string XacNhanMatKhau)
        {
            if (ModelState.IsValid)
            {
                if (MatKhau != XacNhanMatKhau)
                {
                    ModelState.AddModelError("", "Xác nhận mật khẩu không khớp.");
                    return View();
                }

                // (Code kiểm tra Email và TenDangNhap của bạn đã tốt)
                var emailLower = Email.ToLower();
                var checkUser = data.TaiKhoans.Any(t => t.TenDangNhap == TenDangNhap);
                var checkEmailTK = data.TaiKhoans.Any(t => t.Email != null && t.Email.ToLower() == emailLower);
                var checkEmailKH = data.KhachHangs.Any(k => k.Email != null && k.Email.ToLower() == emailLower);

                if (checkUser)
                {
                    ModelState.AddModelError("", "Tên đăng nhập đã tồn tại.");
                    return View();
                }
                if (checkEmailTK || checkEmailKH)
                {
                    ModelState.AddModelError("", "Email đã được sử dụng.");
                    return View();
                }

                try
                {
                    data.Database.ExecuteSqlCommand("EXEC ThemTaiKhoanMoi @TenDangNhap, @MatKhau, @Email",
                        new SqlParameter("@TenDangNhap", TenDangNhap),
                        new SqlParameter("@MatKhau", MatKhau),
                        new SqlParameter("@Email", Email)
                    );

                    var newUser = data.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == TenDangNhap);
                    if (newUser == null)
                    {
                        ModelState.AddModelError("", "Lỗi khi tạo tài khoản, vui lòng thử lại.");
                        return View();
                    }

                    KhachHang newKhachHang = new KhachHang
                    {
                        HoTen = HoTen,
                        Email = Email,
                        MaTaiKhoan = newUser.MaTaiKhoan,
                        NgayTao = DateTime.Now // Thêm NgayTao
                    };
                    data.KhachHangs.Add(newKhachHang);
                    data.SaveChanges();

                    // Tự động đăng nhập
                    Session["MaTaiKhoan"] = newUser.MaTaiKhoan;
                    Session["TenDangNhap"] = newUser.TenDangNhap;
                    Session["MaQuyen"] = newUser.MaQuyen;
                    Session["HoTen"] = newKhachHang.HoTen;

                    // SỬA 5: Người dùng mới đăng ký, giỏ hàng = 0
                    Session["GioHangCount"] = 0;

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.InnerException?.Message ?? ex.Message);
                }
            }
            return View();
        }

        public ActionResult DangXuat()
        {
            Session.Clear();
            Session.Abandon(); // Đảm bảo xóa sạch Session
            return RedirectToAction("Index", "Home");
        }

        // Thêm hàm Dispose để giải phóng DbContext
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                data.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult DoiMatKhau()
        {
            return View();
        }

        [HttpPost]
        [CheckLogin]
        [ValidateAntiForgeryToken]
        public ActionResult DoiMatKhau(string matKhauCu, string matKhauMoi, string xacNhanMoi)
        {
            if (matKhauMoi != xacNhanMoi)
            {
                ModelState.AddModelError("", "Mật khẩu mới không khớp.");
                return View();
            }

            string tenDangNhap = Session["TenDangNhap"].ToString();
            int maTaiKhoan = (int)Session["MaTaiKhoan"];

         
            bool laMatKhauCuDung = data.Database
                .SqlQuery<bool>("SELECT dbo.KiemTraDangNhap(@User, @Pass)",
                    new SqlParameter("@User", tenDangNhap),
                    new SqlParameter("@Pass", matKhauCu)
                ).FirstOrDefault();

   

            if (laMatKhauCuDung)
            {
                // Mật khẩu cũ đúng -> Cập nhật mật khẩu mới
                var taiKhoan = data.TaiKhoans.Find(maTaiKhoan);
                taiKhoan.MatKhau = matKhauMoi;
                data.SaveChanges();

                TempData["Success"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Mật khẩu cũ sai
                ModelState.AddModelError("", "Mật khẩu cũ không chính xác.");
                return View();
            }
        }
        [CheckLogin]
        public ActionResult ThongTin()
        {
            int maTaiKhoan = (int)Session["MaTaiKhoan"];

            // Lấy thông tin khách hàng
            var khachHang = data.KhachHangs.FirstOrDefault(k => k.MaTaiKhoan == maTaiKhoan);

            if (khachHang == null)
            {
                // Nếu là Admin/NV, chuyển họ về trang chủ
                TempData["Error"] = "Tài khoản của bạn là tài khoản quản trị, không có thông tin khách hàng.";
                return RedirectToAction("Index", "Home");
            }

            // Gửi model KhachHang sang View
            return View(khachHang);
        }
    }
}