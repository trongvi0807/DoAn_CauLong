using DoAn_CauLong.Models;
using DoAn_CauLong.Filters; // << Thêm 'using' này
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace DoAn_CauLong.Controllers
{
    [CheckAdmin] // << BẢO VỆ TOÀN BỘ CONTROLLER
    public class QuanLyDonHangController : Controller
    {
        QLDN_CAULONGEntities data = new QLDN_CAULONGEntities();

        // GET: /QuanLyDonHang/Index
        // Hiển thị danh sách TẤT CẢ đơn hàng
        public ActionResult Index()
        {
            var orders = data.DonHangs
                             .Include(d => d.KhachHang) // Lấy tên Khách hàng
                             .OrderByDescending(d => d.NgayDat)
                             .ToList();
            return View(orders);
        }

        // GET: /QuanLyDonHang/Details/5
        // Hiển thị chi tiết đơn hàng (giống trang của khách)
        public ActionResult Details(int id) // id là MaDonHang
        {
            var order = data.DonHangs
                .Include(d => d.KhachHang)
                .Include(d => d.ChiTietDonHangs.Select(ct => ct.ChiTietSanPham.SanPham))
                .Include(d => d.ChiTietDonHangs.Select(ct => ct.ChiTietSanPham.MauSac))
                .Include(d => d.ChiTietDonHangs.Select(ct => ct.ChiTietSanPham.Size))
                .FirstOrDefault(d => d.MaDonHang == id);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index");
            }

            return View(order);
        }

        // POST: /QuanLyDonHang/SetTrangThai
        // Action để cập nhật trạng thái đơn hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetTrangThai(int id, string trangThaiMoi)
        {
            var order = data.DonHangs.Find(id);
            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index");
            }

            try
            {
                order.TrangThai = trangThaiMoi;
                data.Entry(order).State = EntityState.Modified;
                data.SaveChanges();
                TempData["Success"] = "Cập nhật trạng thái thành công!";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
            }

            // Quay lại trang chi tiết vừa cập nhật
            return RedirectToAction("Details", new { id = id });
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