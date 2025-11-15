using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using DoAn_CauLong.Models;
using DoAn_CauLong.ViewModels;

namespace DoAn_CauLong.ViewModels
{
    public class TaiKhoanViewModel
    {
        public int MaTaiKhoan { get; set; }

        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } // Sẽ không cho sửa

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Display(Name = "Họ và Tên")]
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string HoTen { get; set; }

        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }

        [Display(Name = "Quyền")]
        [Required]
        public int MaQuyen { get; set; }

        // Chỉ dùng khi TẠO MỚI
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }
    }
}