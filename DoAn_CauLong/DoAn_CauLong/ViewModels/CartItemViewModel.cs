using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAn_CauLong.ViewModels
{
    public class CartItemViewModel
    {
        // Thông tin từ GioHang Entity
        public int MaGioHang { get; set; } // ID của bản ghi trong DB
        public int MaChiTietSanPham { get; set; }
        public int SoLuong { get; set; }

        // Thông tin hiển thị (Lấy từ ChiTietSanPham Navigation Properties)
        public string TenSanPham { get; set; }
        public string HinhAnh { get; set; }
        public string TenMau { get; set; }
        public string TenSize { get; set; }
        public decimal GiaBan { get; set; } // Giá bán của biến thể

        // Thuộc tính tính toán
        public decimal ThanhTien
        {
            get { return GiaBan * SoLuong; }
        }
    }
}