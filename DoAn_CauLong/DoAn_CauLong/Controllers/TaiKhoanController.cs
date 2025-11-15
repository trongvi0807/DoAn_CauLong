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
        public ActionResult DangNhap()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangNhap(string TenDangNhap, string MatKhau)
        {
            if (ModelState.IsValid)
            {
                var user = data.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == TenDangNhap && t.MatKhau == MatKhau);

                if (user != null)
                {

                    Session["MaTaiKhoan"] = user.MaTaiKhoan;
                    Session["TenDangNhap"] = user.TenDangNhap;
                    Session["MaQuyen"] = user.MaQuyen;

                    // Tìm Họ Tên từ bảng KhachHang
                    var khachHang = data.KhachHangs.FirstOrDefault(k => k.MaTaiKhoan == user.MaTaiKhoan);

                    if (khachHang != null && !string.IsNullOrEmpty(khachHang.HoTen))
                    {
                        Session["MaKH"] = khachHang.MaKhachHang; // kiểm tra mã khách hàng-vừa thêm từ Vĩ

                        Session["HoTen"] = khachHang.HoTen; // Dùng Họ Tên
                    }
                    else
                    {
                        Session["HoTen"] = user.TenDangNhap; // Dùng Tên đăng nhập nếu không có
                    }

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }
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

                // === SỬA LỖI KIỂM TRA EMAIL ===
                // 1. Chuẩn hóa email về chữ thường để kiểm tra không phân biệt hoa/thường
                var emailLower = Email.ToLower();

                // 2. Kiểm tra TenDangNhap (tên đăng nhập nên phân biệt hoa/thường)
                var checkUser = data.TaiKhoans.Any(t => t.TenDangNhap == TenDangNhap);

                // 3. Kiểm tra Email ở cả 2 bảng (TaiKhoan và KhachHang)
                // Thêm 't.Email != null' để tránh lỗi nếu CSDL có email là NULL
                var checkEmailTK = data.TaiKhoans.Any(t => t.Email != null && t.Email.ToLower() == emailLower);
                var checkEmailKH = data.KhachHangs.Any(k => k.Email != null && k.Email.ToLower() == emailLower);

                if (checkUser)
                {
                    ModelState.AddModelError("", "Tên đăng nhập đã tồn tại.");
                    return View();
                }

                // 4. Gộp 2 kết quả kiểm tra email
                if (checkEmailTK || checkEmailKH)
                {
                    ModelState.AddModelError("", "Email đã được sử dụng.");
                    return View();
                }
                // === KẾT THÚC SỬA ===

                try
                {
                    // 1. Tạo TÀI KHOẢN (dùng Procedure)
                    data.Database.ExecuteSqlCommand("EXEC ThemTaiKhoanMoi @TenDangNhap, @MatKhau, @Email",
                        new SqlParameter("@TenDangNhap", TenDangNhap),
                        new SqlParameter("@MatKhau", MatKhau),
                        new SqlParameter("@Email", Email) // Truyền Email gốc (chưa ToLower())
                    );

                    // 2. Lấy lại Tài khoản vừa tạo để có MaTaiKhoan
                    var newUser = data.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == TenDangNhap);
                    if (newUser == null)
                    {
                        ModelState.AddModelError("", "Lỗi khi tạo tài khoản, vui lòng thử lại.");
                        return View();
                    }

                    // 3. Tạo KHACHHANG mới liên kết với Tài khoản
                    KhachHang newKhachHang = new KhachHang
                    {
                        HoTen = HoTen,
                        Email = Email, // Lưu Email gốc
                        MaTaiKhoan = newUser.MaTaiKhoan
                    };
                    data.KhachHangs.Add(newKhachHang);
                    data.SaveChanges(); // Lưu KhachHang vào CSDL

                    // 4. Tự động đăng nhập
                    Session["MaTaiKhoan"] = newUser.MaTaiKhoan;
                    Session["TenDangNhap"] = newUser.TenDangNhap;
                    Session["MaQuyen"] = newUser.MaQuyen;
                    Session["HoTen"] = newKhachHang.HoTen;

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    // Lỗi (có thể do trigger hoặc lỗi kết nối)
                    // Nếu lỗi báo "Email này đã tồn tại!" tức là trigger SQL đã hoạt động
                    ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.InnerException?.Message ?? ex.Message);
                }
            }
            return View();
        }

        public ActionResult DangXuat()
        {
            // Xóa tất cả Session
            Session.Clear();
            Session.Remove("MaTaiKhoan");
            Session.Remove("TenDangNhap");
            Session.Remove("HoTen");
            Session.Remove("MaQuyen");

            Session.Abandon();

            return RedirectToAction("Index", "Home");
        }
    }
}