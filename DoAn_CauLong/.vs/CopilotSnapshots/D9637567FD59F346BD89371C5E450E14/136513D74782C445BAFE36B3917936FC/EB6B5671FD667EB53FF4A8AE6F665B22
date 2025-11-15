using DoAn_CauLong.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace DoAn_CauLong.Controllers
{
    public class SanPhamController : Controller
    {

        QLDN_CAULONGEntities data = new QLDN_CAULONGEntities();

        // GET: SanPham
        // optional parameters: loai (int id or short code/string), hang (brand name), ten (partial product name / model line)
        public ActionResult Index(string loai = null, string hang = null, string ten = null)
        {
            var query = data.SanPhams.AsQueryable();

            // filter by category (loai)
            if (!string.IsNullOrEmpty(loai))
            {
                // try parse numeric id
                if (int.TryParse(loai, out int maLoai))
                {
                    query = query.Where(s => s.MaLoai == maLoai);
                }
                else
                {
                    // support short codes like Vot, Giay, AoQuan, PhuKien
                    var key = loai.Trim().ToLower();
                    if (key == "vot" || key == "votc" || key == "vot-cau-long")
                    {
                        query = query.Where(s => s.LoaiSanPham.TenLoai.ToLower().Contains("vợt") || s.LoaiSanPham.TenLoai.ToLower().Contains("vot"));
                    }
                    else if (key == "giay" || key.Contains("giày"))
                    {
                        query = query.Where(s => s.LoaiSanPham.TenLoai.ToLower().Contains("giày") || s.LoaiSanPham.TenLoai.ToLower().Contains("giay"));
                    }
                    else if (key == "aoquan" || key.Contains("ao") || key.Contains("áo"))
                    {
                        query = query.Where(s => s.LoaiSanPham.TenLoai.ToLower().Contains("áo") || s.LoaiSanPham.TenLoai.ToLower().Contains("ao"));
                    }
                    else if (key == "phukien" || key.Contains("phu"))
                    {
                        query = query.Where(s => s.LoaiSanPham.TenLoai.ToLower().Contains("phụ") || s.LoaiSanPham.TenLoai.ToLower().Contains("phu"));
                    }
                    else
                    {
                        // fallback: match any LoaiSanPham containing the provided text
                        query = query.Where(s => s.LoaiSanPham.TenLoai.ToLower().Contains(key));
                    }
                }
            }

            // filter by brand (hang) - accept brand name
            if (!string.IsNullOrEmpty(hang))
            {
                var hangKey = hang.Trim();
                // find brand id by name (case-insensitive)
                var hangEntity = data.Hangs.FirstOrDefault(h => h.TenHang.Equals(hangKey, StringComparison.OrdinalIgnoreCase));
                if (hangEntity != null)
                {
                    query = query.Where(s => s.MaHang == hangEntity.MaHang);
                }
                else
                {
                    // fallback: match by name contains
                    query = query.Where(s => s.Hang.TenHang.ToLower().Contains(hangKey.ToLower()));
                }
            }

            // filter by partial product name / model line
            if (!string.IsNullOrEmpty(ten))
            {
                var tenKey = ten.Trim().ToLower();
                query = query.Where(s => s.TenSanPham.ToLower().Contains(tenKey));
            }

            // include related entities for display (LoaiSanPham, Hang, KhuyenMai)
            var result = query
                .Include(s => s.LoaiSanPham)
                .Include(s => s.Hang)
                .Include(s => s.KhuyenMai)
                .OrderByDescending(s => s.NgayTao)
                .ToList();

            return View(result);
        }

        // Action: tìm kiếm sản phẩm (Form tìm kiếm trên _Layout.cshtml gửi tới đây)
        [HttpGet]
        public ActionResult TimKiem(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                // nếu không có từ khóa, chuyển về trang danh sách chung
                return RedirectToAction("Index");
            }

            var key = keyword.Trim().ToLower();

            // Tìm sản phẩm có tên chứa từ khóa, hoặc hãng chứa từ khóa, hoặc loại chứa từ khóa
            var results = data.SanPhams
                .Where(s => s.TenSanPham.ToLower().Contains(key)
                            || s.Hang.TenHang.ToLower().Contains(key)
                            || s.LoaiSanPham.TenLoai.ToLower().Contains(key))
                .OrderByDescending(s => s.NgayTao)
                .Include(s => s.KhuyenMai)
                .ToList();

            ViewBag.Title = $"Kết quả tìm kiếm cho '{keyword}'";
            ViewBag.Keyword = keyword;
            return View("Index", results);
        }

        // GET: KhuyenMai Index - danh sách sản phẩm khuyến mãi
        public ActionResult KhuyenMai()
        {
            var today = DateTime.Now.Date;

            // Lấy các chương trình khuyến mãi đang có hiệu lực
            var activePromotions = data.KhuyenMais
                .Where(k => (!k.NgayBatDau.HasValue || k.NgayBatDau.Value <= today)
                         && (!k.NgayKetThuc.HasValue || k.NgayKetThuc.Value >= today))
                .ToList();

            if (!activePromotions.Any())
            {
                return View("~/Views/KhuyenMai/Index.cshtml", new List<SanPham>());
            }

            // Lấy tất cả sản phẩm có MaKhuyenMai trùng với activePromotions
            var promoIds = activePromotions.Select(k => k.MaKhuyenMai).ToList();

            var products = data.SanPhams
                .Include(s => s.KhuyenMai)
                .Where(s => s.MaKhuyenMai.HasValue && promoIds.Contains(s.MaKhuyenMai.Value))
                .ToList();

            return View("~/Views/KhuyenMai/Index.cshtml", products);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                data.Dispose();
            }
            base.Dispose(disposing);
        }
        // GET: SanPham
        public ActionResult ViewAllProduct(int maLoai)
        {
            // 1. Lấy tên loại sản phẩm để hiển thị trên tiêu đề trang
            var loaiSanPham = data.LoaiSanPhams
                .SingleOrDefault(l => l.MaLoai == maLoai);

            if (loaiSanPham == null)
            {
                return HttpNotFound();
            }

            // 2. Lấy tất cả sản phẩm thuộc loại này
            var sanPhams = data.SanPhams
                .Where(sp => sp.MaLoai == maLoai)
                .Include(sp => sp.LoaiSanPham) // Tùy chọn: Bao gồm thông tin loại nếu cần
                .Include(sp => sp.KhuyenMai)
                .ToList();

            // 3. Truyền danh sách sản phẩm và tên loại vào View
            ViewBag.TenLoai = loaiSanPham.TenLoai;
            return View(sanPhams);

        }
    }
}