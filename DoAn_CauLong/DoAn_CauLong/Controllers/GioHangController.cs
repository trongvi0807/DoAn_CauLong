using DoAn_CauLong.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using DoAn_CauLong.Filters; // Cần dùng [CheckLogin]

namespace DoAn_CauLong.Controllers
{
    public class GioHangController : Controller
    {
        QLDN_CAULONGEntities data = new QLDN_CAULONGEntities();

        // Action này được gọi bằng AJAX từ _Layout.cshtml
        // Nó chỉ đọc Session, rất nhanh.
        public ActionResult GetCartCount()
        {
            int count = (Session["GioHangCount"] != null) ? (int)Session["GioHangCount"] : 0;
            return Content(count.ToString());
        }

        // Bạn cũng nên đặt Action Xóa khỏi giỏ hàng ở đây
        [CheckLogin]
        public ActionResult RemoveFromCart(int chiTietId)
        {
            // 1. Lấy MaKhachHang
            int maTaiKhoan = (int)Session["MaTaiKhoan"];
            var khachHang = data.KhachHangs.FirstOrDefault(kh => kh.MaTaiKhoan == maTaiKhoan);

            if (khachHang != null)
            {
                int maKhachHang = khachHang.MaKhachHang;
                var cartItem = data.GioHangs.FirstOrDefault(g =>
                    g.MaKhachHang == maKhachHang && g.MaChiTietSanPham == chiTietId);

                if (cartItem != null)
                {
                    data.GioHangs.Remove(cartItem);
                    data.SaveChanges();

                    // 3. Cập nhật lại Session
                    int totalItems = data.GioHangs
                                       .Where(g => g.MaKhachHang == maKhachHang)
                                       .Sum(g => (int?)g.SoLuong) ?? 0;
                    Session["GioHangCount"] = totalItems;
                }
            }

            return RedirectToAction("ViewCart", "Home");
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