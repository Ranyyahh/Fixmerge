using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdminP.Controllers
{
    public class AdminPanelController : Controller
    {

        public ActionResult LandingAdmin()
        {
            return View();
        }

        public ActionResult AdminlandingEntrep()
        {
            return View();
        }

        public ActionResult AdminEditEntrep()
        {
            return View();
        }

        public ActionResult AdminItemListing()  
        {
            return View();
        }

        public ActionResult AdminUsers()
        {
            return View();
        }
    }
}