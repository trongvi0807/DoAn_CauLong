using DoAn_CauLong.Models;
using DoAn_CauLong.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn_CauLong.Controllers
{
    public class QuanLyThongBaoController : Controller
    {
        // GET: QuanLyThongBao
        public ActionResult ThongBao()
        {
            using (var db = new QLDN_CAULONGEntities())
            {
                var data = db.Database.SqlQuery<ThongBaoViewModel>(
                    "EXEC SP_ThongBaoDanhGiaThap"
                ).ToList();

                return View(data);
            }
        }
        public ActionResult SoThongBao()
        {
            using (var data = new QLDN_CAULONGEntities())
            {
                int soThongBao = Session["SoThongBao"] != null ? (int)Session["SoThongBao"] : 0;

                return Content(soThongBao.ToString());
            }
        }

    }
}