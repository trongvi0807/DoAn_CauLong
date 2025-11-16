using System;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using DoAn_CauLong.Models;
using System.Web.Security;

// Controller nên nằm trong namespace của dự án
namespace DoAn_CauLong.Controllers
{
    // Controller phải kế thừa từ Controller
    public class PhanHoiController : Controller
    {
        // 💡 Khai báo DbContext bằng cách sử dụng using (Khuyến nghị)
        // hoặc giữ nó ở cấp độ Controller (cho mục đích Demo)
        private QLDN_CAULONGEntities data = new QLDN_CAULONGEntities();

        // Hàm giả định để lấy MaKhachHang từ tên đăng nhập (đã lưu trong Cookie)
        private int GetMaKhachHangFromLoggedInUser(string userName)
        {
            // Dùng TenDangNhap (userName) để truy vấn MaKhachHang từ bảng KhachHang
            var khachHang = data.KhachHangs.FirstOrDefault(k => k.TaiKhoan.TenDangNhap == userName);
            return khachHang?.MaKhachHang ?? 0; // Trả về 0 nếu không tìm thấy
        }

        // GET: hiển thị form đánh giá. Action này không cần đăng nhập
        public ActionResult ReviewForm(int maSP)
        {
            ViewBag.MaSanPham = maSP;
            // Lấy MaKH từ Session nếu có để điền vào form (tùy chọn)
            ViewBag.MaKhachHang = Session["MaKhachHang"] ?? 0;
            return View();
        }

        // POST: Xử lý gửi đánh giá
        [HttpPost]
        [Authorize] // 🛡️ Bắt buộc phải đăng nhập để gửi đánh giá
        public ActionResult SubmitReview(int maSP, int danhGia, string noiDung)
        {
            // Lấy MaKH từ Cookie (định danh Forms Authentication)
            if (!User.Identity.IsAuthenticated)
            {
                // Nếu [Authorize] không bắt được, chuyển hướng an toàn
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            int maKhachHang = GetMaKhachHangFromLoggedInUser(User.Identity.Name);

            if (maKhachHang == 0)
            {
                TempData["Error"] = "Lỗi xác thực khách hàng. Vui lòng thử đăng nhập lại.";
                return RedirectToAction("ReviewForm", new { maSP = maSP });
            }

            var parameters = new[]
            {
                // Dữ liệu lấy từ tham số Action
                new SqlParameter("@NoiDung", noiDung),
                new SqlParameter("@DanhGia", danhGia),
                
                // Dữ liệu lấy từ Server/Cookie
                new SqlParameter("@MaKH", maKhachHang),
                new SqlParameter("@MaSP", maSP)
            };

            try
            {
                // Sử dụng data (DbContext) để thực thi Stored Procedure
                data.Database.ExecuteSqlCommand(
                    "EXEC THEMPHANHOI @NoiDung, @DanhGia, @MaKH, @MaSP",
                    parameters
                );

                TempData["Message"] = "Gửi đánh giá thành công! Cảm ơn bạn đã phản hồi.";
            }
            catch (Exception ex)
            {
                // Nên log lỗi ra console hoặc file log
                TempData["Error"] = "Lỗi khi gửi đánh giá: " + ex.Message;
            }

            // Chuyển hướng về trang chi tiết sản phẩm hoặc trang form
            return RedirectToAction("ChiTietSanPham", "Home", new { id = maSP });
        }

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