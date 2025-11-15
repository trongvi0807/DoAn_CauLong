using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Mvc;
using DoAn_CauLong.Models; // namespace của DbContext

public class PhanHoiController : Controller
{
   QLDN_CAULONGEntities 
    public PhanHoiController()
    {
        _context = new ApplicationDbContext();
    }

    // GET: hiển thị form đánh giá
    public ActionResult ReviewForm(int maSP)
    {
        ViewBag.MaSanPham = maSP;
        return View();
    }

    public ActionResult SubmitReview(int maKH, int maSP, int danhGia, string noiDung)
    {
        var parameters = new[]
        {
        new SqlParameter("@NoiDung", noiDung),
        new SqlParameter("@DanhGia", danhGia),
        new SqlParameter("@MaKH", maKH),
        new SqlParameter("@MaSP", maSP)
    };

        try
        {
            data.Database.ExecuteSqlCommand(
                "EXEC THEMPHANHOI @NoiDung, @DanhGia, @MaKH, @MaSP",
                parameters
            );

            TempData["Message"] = "Gửi đánh giá thành công!";
        }
        catch (Exception ex)
        {
            TempData["Message"] = "Lỗi khi gửi đánh giá: " + ex.Message;
        }

        return RedirectToAction("ReviewForm", new { maSP = maSP });
    }

}
