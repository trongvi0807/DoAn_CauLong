using DoAn_CauLong.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn_CauLong.Controllers
{
    public class ThemPhanHoiSPController : Controller
    {
        // GET: ThemPhanHoiSP
        private QLDN_CAULONGEntities data = new QLDN_CAULONGEntities();
        public ActionResult ReviewForm(int maSP)
        {
            if (Session["MaKH"] == null)
            {
                TempData["Message"] = "Bạn cần đăng nhập để gửi phản hồi!";
                return RedirectToAction("ChiTietSanPham", "Home", new { id = maSP });
            }
            ViewBag.MaSanPham = maSP; return View();
        }
        [HttpPost]
        public ActionResult SubmitReview(int maSP, int danhGia, string noiDung)
        {
            if (Session["MaKH"] == null)
            {
                TempData["Message"] = "Bạn cần đăng nhập để gửi phản hồi!";
                return RedirectToAction("ChiTietSanPham", "Home", new { id = maSP });
            }

            int maKH = (int)Session["MaKH"]; // Lấy MaKH từ session

            try
            {
                var parameters = new[]
                {
            new SqlParameter("@NoiDung", noiDung ?? (object)DBNull.Value),
            new SqlParameter("@DanhGia", danhGia),
            new SqlParameter("@MaKH", maKH),
            new SqlParameter("@MaSP", maSP)
        };

                data.Database.ExecuteSqlCommand(
                    "EXEC THEMPHANHOI @NoiDung, @DanhGia, @MaKH, @MaSP",
                    parameters
                );

                // optional nhưng an toàn
                data.SaveChanges();

                TempData["Message"] = "Gửi đánh giá thành công!";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Lỗi khi gửi đánh giá: " + ex.Message;
            }

            return RedirectToAction("ChiTietSanPham", "Home", new { id = maSP });
        }


    }
}