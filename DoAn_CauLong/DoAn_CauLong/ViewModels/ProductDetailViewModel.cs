using DoAn_CauLong.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace DoAn_CauLong.ViewModels
{
    public class ProductDetailViewModel
    {
        public SanPham SanPham { get; set; }
        public List<ChiTietSanPham> Variants { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public ThongSoVot ThongSoVot { get; set; }
        public List<MauSac> AvailableColors { get; set; }
        public List<Models.Size> AvailableSizes { get; set; }
        //hiển thị phản hồi
        public List<PhanHoi> Reviews { get; set; }
    }
}