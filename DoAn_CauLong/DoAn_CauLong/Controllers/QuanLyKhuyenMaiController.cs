using DoAn_CauLong.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using DoAn_CauLong.Filters;

namespace DoAn_CauLong.Controllers
{
    
    //[Authorize]
    [CheckAdmin] // Giữ kiểm tra quyền truy cập Admin/Nhân viên
    public class QuanLyKhuyenMaiController : Controller
    {
        private readonly QLDN_CAULONGEntities db = new QLDN_CAULONGEntities();

        public ActionResult Index()
        {
            var list = db.KhuyenMais.OrderByDescending(k => k.NgayBatDau).ToList();
            return View(list);
        }

        public ActionResult Create()
        {
            return View(new KhuyenMai());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(KhuyenMai model)
        {
            if (ModelState.IsValid)
            {
                db.KhuyenMais.Add(model);
                db.SaveChanges();
                TempData["Message"] = "Tạo khuyến mãi thành công";
                return RedirectToAction("Index");
            }
            TempData["Error"] = "Dữ liệu không hợp lệ";
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var km = db.KhuyenMais.Find(id);
            if (km == null) return HttpNotFound();
            return View(km);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(KhuyenMai model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ";
                return View(model);
            }
            var km = db.KhuyenMais.Find(model.MaKhuyenMai);
            if (km == null) return HttpNotFound();
            km.TenChuongTrinh = model.TenChuongTrinh;
            km.MoTa = model.MoTa;
            km.NgayBatDau = model.NgayBatDau;
            km.NgayKetThuc = model.NgayKetThuc;
            km.PhanTramGiam = model.PhanTramGiam;
            km.GiamToiDa = model.GiamToiDa;
            km.SoLuongSuDung = model.SoLuongSuDung;
            db.SaveChanges();
            TempData["Message"] = "Cập nhật khuyến mãi thành công";
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            var km = db.KhuyenMais.Find(id);
            if (km == null) return HttpNotFound();
            return View(km);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var km = db.KhuyenMais.Find(id);
            if (km == null) return HttpNotFound();

            // Pre-delete: tìm các sản phẩm đang tham chiếu khuyến mãi này
            var referencingProducts = db.SanPhams.Where(p => p.MaKhuyenMai == id).ToList();
            if (referencingProducts.Count > 0)
            {
                // Gỡ tham chiếu (SET NULL) để tránh lỗi ràng buộc FK khi xóa
                foreach (var sp in referencingProducts)
                {
                    sp.MaKhuyenMai = null;
                }
            }

            db.KhuyenMais.Remove(km);
            db.SaveChanges(); // EF sẽ thực hiện trong 1 transaction => an toàn tính toàn vẹn

            TempData["Message"] = referencingProducts.Count > 0
                ? $"Xóa khuyến mãi thành công (đã gỡ khỏi {referencingProducts.Count} sản phẩm)"
                : "Xóa khuyến mãi thành công";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}