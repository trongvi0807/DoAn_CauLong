using DoAn_CauLong.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace DoAn_CauLong.Controllers
{
    public class KhuyenMaiController : Controller
    {
        QLDN_CAULONGEntities data = new QLDN_CAULONGEntities();

        // GET: KhuyenMai
        // Show all promotions and their products
        public ActionResult Index()
        {
            // load all promotions
            var promotions = data.KhuyenMais
                .OrderBy(k => k.NgayBatDau)
                .ToList();

            // mapping promotionId -> list of products
            var promoProducts = new Dictionary<int, List<SanPham>>();

            foreach (var km in promotions)
            {
                var products = data.SanPhams
                    .Include(s => s.KhuyenMai)
                    .Include(s => s.Hang)
                    .Where(s => s.MaKhuyenMai == km.MaKhuyenMai)
                    .OrderByDescending(s => s.NgayTao)
                    .ToList();

                promoProducts[km.MaKhuyenMai] = products;
            }

            ViewBag.Promotions = promotions;
            ViewBag.PromoProducts = promoProducts;

            // pass promotions list as model is optional; view will read ViewBag
            return View("~/Views/KhuyenMai/Index.cshtml", new List<SanPham>());
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
