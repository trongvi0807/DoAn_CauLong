using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DoAn_CauLong.Models
{
    public class CheckLoginAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // Kiểm tra xem Session["MaTaiKhoan"] có tồn tại không
            return httpContext.Session["MaTaiKhoan"] != null;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Nếu không được xác thực (Session null), chuyển hướng đến trang Đăng nhập
            // Quan trọng: Truyền returnUrl để biết đường quay lại sau khi đăng nhập
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary(
                    new
                    {
                        controller = "TaiKhoan",
                        action = "DangNhap",
                        // Lấy URL mà người dùng đang cố gắng truy cập
                        returnUrl = filterContext.HttpContext.Request.Url.GetComponents(UriComponents.PathAndQuery, UriFormat.SafeUnescaped)
                    })
                );
        }
    }
}