using System;
using System.Collections.Generic;

namespace DoAn_CauLong.ViewModels
{
    public class AdminDashboardViewModel
    {
        // Tổng doanh thu năm hiện tại
        public decimal TongDoanhThuNam { get; set; } = 0;
        public int SoLuongDonNam { get; set; } = 0;
        public int Nam { get; set; } = DateTime.Now.Year;

        // Doanh thu theo quý
        public List<DoanhThuQuyViewModel> DoanhThuQuy { get; set; } = new List<DoanhThuQuyViewModel>();

        // Doanh thu theo năm
        public List<DoanhThuNamViewModel> DoanhThuNam { get; set; } = new List<DoanhThuNamViewModel>();

        // Doanh thu theo tháng
        public List<DoanhThuThangViewModel> DoanhThuThang { get; set; } = new List<DoanhThuThangViewModel>();

        // Sản phẩm bán chạy
        public List<SanPhamBanChayViewModel> SanPhamBanChay { get; set; } = new List<SanPhamBanChayViewModel>();
    }

    public class DoanhThuQuyViewModel
    {
        public int Quy { get; set; } = 0;
        public decimal TongDoanhThu { get; set; } = 0;
        public int SoLuongDon { get; set; } = 0;
    }

    public class DoanhThuNamViewModel
    {
        public int Nam { get; set; } = DateTime.Now.Year;
        public decimal TongDoanhThu { get; set; } = 0;
        public int SoLuongDon { get; set; } = 0;
    }

    public class DoanhThuThangViewModel
    {
        public int Thang { get; set; } = 1;
        public decimal TongDoanhThu { get; set; } = 0;
    }

    public class SanPhamBanChayViewModel
    {
        public string TenSanPham { get; set; } = string.Empty;
        public int TongSoLuongBan { get; set; } = 0;
    }
}
