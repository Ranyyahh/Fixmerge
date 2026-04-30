using System.Web.Mvc;

namespace BizzyQCU.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }

        public ActionResult RegisterStudent()
        {
            return View();
        }

        public ActionResult RegisterEnterprise()
        {
            return View();
        }
    }
}

        public ActionResult Tracking()
        {
            return View();
        }

        public ActionResult History()
        {
            return View();
        }

        public ActionResult Homepage()
        {
            ViewBag.Message = "Your Homepage.";

            return View();
        }

      
        public ActionResult ProductList()
        {
            ViewBag.Message = "Product List page.";

            return View();
        }

        public ActionResult ViewEnterprise()
        {
            ViewBag.Message = "Your Enterprises.";
            return View();
        }


    }
}
