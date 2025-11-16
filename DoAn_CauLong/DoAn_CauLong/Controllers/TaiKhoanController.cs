using DoAn_CauLong.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security; // Đã thêm

namespace DoAn_CauLong.Controllers
{
    public class TaiKhoanController : Controller
    {
        QLDN_CAULONGEntities data = new QLDN_CAULONGEntities();

        public ActionResult DangNhap(string returnUrl)
        {
            // Kiểm tra trạng thái đã đăng nhập (Dùng Cookie)
            if (User.Identity.IsAuthenticated)
            {
                // Nếu đã đăng nhập, chuyển hướng người dùng ngay lập tức
                if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }
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
                    // Đảm bảo TenDangNhap không rỗng/null trước khi tạo Cookie
                    if (string.IsNullOrEmpty(user.TenDangNhap))
                    {
                        ModelState.AddModelError("", "Tài khoản bị lỗi định danh.");
                        return View();
                    }

                    // === 1. TẠO AUTHENTICATION COOKIE ===
                    FormsAuthentication.SetAuthCookie(user.TenDangNhap, false);

                    // === 2. THIẾT LẬP SESSION (Giữ lại cho hiển thị UI) ===
                    Session["MaTaiKhoan"] = user.MaTaiKhoan;
                    Session["TenDangNhap"] = user.TenDangNhap;
                    Session["MaQuyen"] = user.MaQuyen;

                    var khachHang = data.KhachHangs.FirstOrDefault(k => k.MaTaiKhoan == user.MaTaiKhoan);
                    if (khachHang != null)
                    {
                        Session["MaKhachHang"] = khachHang.MaKhachHang;
                        Session["HoTen"] = string.IsNullOrEmpty(khachHang.HoTen) ? user.TenDangNhap : khachHang.HoTen;
                    }
                    else
                    {
                        Session["HoTen"] = user.TenDangNhap;
                    }

                    // === 3. CHUYỂN HƯỚNG SAU ĐĂNG NHẬP ===
                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // Action Đăng ký giữ nguyên, đảm bảo FormsAuthentication.SetAuthCookie được gọi

        public ActionResult DangXuat()
        {
            // 1. Xóa tất cả Session
            Session.Clear();
            Session.RemoveAll();
            Session.Abandon();

            // 2. XÓA AUTHENTICATION COOKIE (Bắt buộc)
            FormsAuthentication.SignOut();

            // Cố gắng chuyển hướng người dùng về trang chủ
            return RedirectToAction("Index", "Home");
        }

        // Thêm Dispose (Giữ nguyên)
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