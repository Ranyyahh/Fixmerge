using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BizzyQCU.Controllers
{
    public class EnterpriseDashboardController : Controller
    {
        public ActionResult Index()
        {
            return View("EnterpriseDashboard");
        }
    }
}