using DoAn_CauLong.Models;
using DoAn_CauLong.Filters; // << Thêm 'using' này
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System;

namespace DoAn_CauLong.Controllers
{
    [CheckAdmin] 
    public class QuanLyDonHangController : Controller
    {
        QLDN_CAULONGEntities data = new QLDN_CAULONGEntities();

        // GET: /QuanLyDonHang/Index
        
        public ActionResult Index()
        {
            var orders = data.DonHangs
                             .Include(d => d.KhachHang) 
                             .OrderByDescending(d => d.NgayDat)
                             .ToList();
            return View(orders);
        }

       
        public ActionResult Details(int id) 
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


        
        // Action để cập nhật trạng thái đơn hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetTrangThai(int id, string trangThaiMoi)
        {
            // Lấy đơn hàng VÀ các chi tiết của nó
            var order = data.DonHangs.Include(d => d.ChiTietDonHangs).FirstOrDefault(d => d.MaDonHang == id);
            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index");
            }

            string trangThaiCu = order.TrangThai;

           
            // CASE 1: XÁC NHẬN ĐƠN (Chờ xác nhận -> Đang xử lý)
           
            if (trangThaiCu == "Chờ xác nhận" && trangThaiMoi == "Đang xử lý")
            {
                using (var transaction = data.Database.BeginTransaction())
                {
                    try
                    {
                        // BƯỚC 1: KIỂM TRA TỒN KHO (Check all items first)
                        foreach (var item in order.ChiTietDonHangs)
                        {
                            var ctsp = data.ChiTietSanPhams.Find(item.MaChiTietSanPham);
                            if (ctsp == null || (ctsp.SoLuongTon ?? 0) < (item.SoLuong ?? 0))
                            {
                                // Nếu thiếu hàng, báo lỗi và dừng lại
                                TempData["Error"] = $"KHÔNG THỂ XÁC NHẬN: Không đủ tồn kho cho sản phẩm (ID: {item.MaChiTietSanPham}).";
                                transaction.Rollback();
                                return RedirectToAction("Details", new { id = id });
                            }
                        }

                        // BƯỚC 2: TRỪ TỒN KHO (Sau khi đã kiểm tra tất cả)
                        foreach (var item in order.ChiTietDonHangs)
                        {
                            var ctsp = data.ChiTietSanPhams.Find(item.MaChiTietSanPham);
                            if (ctsp != null)
                            {
                                ctsp.SoLuongTon = Math.Max(0, (ctsp.SoLuongTon ?? 0) - (item.SoLuong ?? 0));
                                data.Entry(ctsp).State = EntityState.Modified;
                            }
                        }

                        // BƯỚC 3: Cập nhật trạng thái đơn
                        order.TrangThai = trangThaiMoi;
                        data.Entry(order).State = EntityState.Modified;

                        data.SaveChanges();
                        transaction.Commit();
                        TempData["Success"] = "Đã xác nhận đơn hàng và cập nhật tồn kho.";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        TempData["Error"] = "Lỗi khi xác nhận đơn hàng: " + ex.Message;
                    }
                }
            }
           
            // CASE 2: ADMIN HỦY ĐƠN (Hủy đơn đã được xử lý)
          
            else if ((trangThaiCu == "Đang xử lý" || trangThaiCu == "Đang giao hàng") && trangThaiMoi == "Đã hủy")
            {
                using (var transaction = data.Database.BeginTransaction())
                {
                    try
                    {
                        // Hoàn trả tồn kho
                        foreach (var item in order.ChiTietDonHangs)
                        {
                            var ctsp = data.ChiTietSanPhams.Find(item.MaChiTietSanPham);
                            if (ctsp != null)
                            {
                                ctsp.SoLuongTon = (ctsp.SoLuongTon ?? 0) + (item.SoLuong ?? 0);
                                data.Entry(ctsp).State = EntityState.Modified;
                            }
                        }

                        // Cập nhật trạng thái đơn
                        order.TrangThai = trangThaiMoi;
                        data.Entry(order).State = EntityState.Modified;

                        data.SaveChanges();
                        transaction.Commit();
                        TempData["Success"] = "Đã hủy đơn hàng và hoàn trả tồn kho.";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        TempData["Error"] = "Lỗi khi hủy đơn hàng: " + ex.Message;
                    }
                }
            }
            
            // CASE 3: CÁC THAY ĐỔI KHÁC (Không ảnh hưởng tồn kho)
            // (VD: Đang xử lý -> Đang giao hàng, Chờ xác nhận -> Đã hủy)
           
            else
            {
                try
                {
                    order.TrangThai = trangThaiMoi;
                    data.Entry(order).State = EntityState.Modified;
                    data.SaveChanges();
                    TempData["Success"] = "Cập nhật trạng thái thành công!";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi: " + ex.Message;
                }
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