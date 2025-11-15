using DoAn_CauLong.Models;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DoAn_CauLong.Filters
{
    public class AuthorizeAdmin : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var session = httpContext.Session["MaQuyen"];
            if (session == null)
            {
                return false; // Chưa đăng nhập
            }

            try
            {
                int maQuyen = (int)session;
                // Chỉ cho phép MaQuyen = 1 (Admin) hoặc 2 (Nhân viên)
                if (maQuyen == 1 || maQuyen == 2)
                {
                    return true; // OK, cho phép truy cập
                }
            }
            catch { }

            return false; // Không có quyền
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Session["MaQuyen"] == null)
            {
                // Chuyển về trang Đăng nhập
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "TaiKhoan", action = "DangNhap" })
                );
            }
            else
            {
                // Chuyển về trang chủ nếu không có quyền
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Home", action = "Index" })
                );
            }
        }
    }
}