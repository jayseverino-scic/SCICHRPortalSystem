using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using SCICHRPortal.Web.Models;

namespace SCICHRPortal.Web.Controllers
{
    [AllowAnonymous]
    public class UserController : Controller
    {

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SendEmail()
        {
            return View();
        }
    }
}