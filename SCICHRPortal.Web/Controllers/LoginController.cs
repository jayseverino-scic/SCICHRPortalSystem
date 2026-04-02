using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using SCICHRPortal.Web.Models;

namespace SCICHRPortal.Web.Controllers
{
    public class LoginController : Controller
    {

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            var loggedIn = User.Identity!.IsAuthenticated;

            if (loggedIn)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
    }
}