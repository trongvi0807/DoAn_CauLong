using DoAn_CauLong.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity; 

namespace DoAn_CauLong.Controllers
{
    public class ThemPhanHoiSPController : Controller
    {
        private QLDN_CAULONGEntities data = new QLDN_CAULONGEntities();

        // Hàm hỗ trợ: Lấy MaKhachHang từ MaTaiKhoan
        private int? GetMaKhachHangFromTaiKhoan(int maTaiKhoan)
        {
            
            var khachHang = data.KhachHangs.FirstOrDefault(kh => kh.MaTaiKhoan == maTaiKhoan);
            return khachHang?.MaKhachHang;
        }

        public ActionResult ReviewForm(int maSP)
        {
            if (Session["MaTaiKhoan"] == null)
            {
                TempData["Message"] = "Bạn cần đăng nhập để gửi phản hồi!";
                
                return RedirectToAction("ChiTietSanPham", "Home", new { id = maSP });
            }
            ViewBag.MaSanPham = maSP;
            return View();
        }

        [HttpPost]
        public ActionResult SubmitReview(int maSP, int danhGia, string noiDung)
        {
            if (Session["MaTaiKhoan"] == null)
            {
                TempData["Message"] = "Bạn cần đăng nhập để gửi phản hồi!";
                return RedirectToAction("ChiTietSanPham", "Home", new { id = maSP });
            }

            int maTaiKhoan = (int)Session["MaTaiKhoan"]; 

            // 1. Lấy Mã Khách Hàng (MaKH) thực tế từ Mã Tài Khoản (MaTaiKhoan)
            int? maKHNullable = GetMaKhachHangFromTaiKhoan(maTaiKhoan);

            if (!maKHNullable.HasValue)
            {
                
                TempData["Message"] = "Lỗi: Tài khoản đăng nhập không phải là Khách hàng và không thể gửi đánh giá.";
                return RedirectToAction("ChiTietSanPham", "Home", new { id = maSP });
            }
            int maKH = maKHNullable.Value;

            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@NoiDung", noiDung ?? (object)DBNull.Value),
                    new SqlParameter("@DanhGia", danhGia),
                    new SqlParameter("@MaKH", maKH), // Dùng MaKH đã tra cứu
                    new SqlParameter("@MaSP", maSP)
                };

                // 2. Thực thi Stored Procedure
                int rowsAffected = data.Database.ExecuteSqlCommand(
                    "EXEC THEMPHANHOI @NoiDung, @DanhGia, @MaKH, @MaSP",
                    parameters
                );

                if (rowsAffected > 0)
                {
                    TempData["Message"] = "Gửi đánh giá thành công!";
                }
                else
                {
                    TempData["Message"] = "Thêm phản hồi thành công";
                }
            }
            catch (Exception ex)
            {
                
                string errorMessage = ex.Message;
                if (ex.InnerException is SqlException sqlEx)
                {
                    // Trích xuất lỗi SQL chi tiết để dễ dàng chẩn đoán
                    errorMessage = $"Lỗi SQL ({sqlEx.Number}): {sqlEx.Message}";
                }

                TempData["Message"] = "Lỗi hệ thống khi gửi đánh giá: " + errorMessage;
            }

            return RedirectToAction("ChiTietSanPham", "Home", new { id = maSP });
        }
    }
}