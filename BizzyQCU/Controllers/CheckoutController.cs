using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BizzyQCU.Controllers
{
    public class CheckoutController : Controller
    {
        public ActionResult Checkout()
        {
            return View("CheckoutPage");
        }

        // Backward-compatible endpoint for existing links/bookmarks.
        public ActionResult CheckoutPage()
        {
            return RedirectToAction("Checkout");
        }
    }
}
