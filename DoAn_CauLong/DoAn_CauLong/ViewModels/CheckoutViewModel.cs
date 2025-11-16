using DoAn_CauLong.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAn_CauLong.ViewModels
{
    public class CheckoutViewModel
    {
        public KhachHang CustomerInfo { get; set; }
        public List<CartItemViewModel> CartItems { get; set; }
        public decimal Total { get; set; }


        public string ShippingAddress { get; set; }
        public string ShippingPhone { get; set; }
    }
}