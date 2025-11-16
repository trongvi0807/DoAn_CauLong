using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DoAn_CauLong.Filters
{
    public class CheckAdminAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // 1. Kiểm tra đã đăng nhập chưa
            if (httpContext.Session["MaTaiKhoan"] == null)
            {
                return false;
            }

            // 2. Kiểm tra MaQuyen
            try
            {
                int maQuyen = (int)httpContext.Session["MaQuyen"];
                // Cho phép MaQuyen = 1 (Admin) hoặc 2 (Nhân viên)
                if (maQuyen == 1 || maQuyen == 2)
                {
                    return true;
                }
            }
            catch { }

            return false; // Không có quyền
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Nếu chưa đăng nhập, về trang Đăng nhập
            if (filterContext.HttpContext.Session["MaTaiKhoan"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        controller = "TaiKhoan",
                        action = "DangNhap",
                        returnUrl = filterContext.HttpContext.Request.Url.GetComponents(System.UriComponents.PathAndQuery, System.UriFormat.SafeUnescaped)
                    })
                );
            }
            else // Nếu đã đăng nhập nhưng không có quyền (vd: Khách hàng)
            {
                // Về trang chủ
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        controller = "Home",
                        action = "Index"
                    })
                );
            }
        }
    }
}