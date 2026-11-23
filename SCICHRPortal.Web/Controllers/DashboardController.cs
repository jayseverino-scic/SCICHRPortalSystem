using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SCICHRPortal.Web.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Audit()
        {
            ViewBag.SystemSelected = "Enrolment";
            return View();
        }

        [Authorize]
        public IActionResult Index()
        {
            ClaimsIdentity? identity = HttpContext.User.Identity as ClaimsIdentity;
            ViewBag.SystemSelected = "Enrolment";
            if (identity != null)
            {
                IEnumerable<Claim> claims = identity.Claims;
                var role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)!.Value;
                const string ADMIN_ROLE = "Administrator";
                const string ADMISSION_ROLE = "Admission";

                if (role == ADMIN_ROLE || role == ADMISSION_ROLE)
                {
                    return View();
                }
                else
                {
                    return this.RedirectToAction("Index", "Register");
                }
            }
             return this.RedirectToAction("Index", "Home");
        }
        public IActionResult Announcement()
        {
            ViewBag.SystemSelected = "Enrolment";
            return View();
        }
        [Authorize(Roles = "Administrator")]
        public IActionResult RegisterUser() => RedirectToAction("Users", "Administration");

        [Authorize(Roles = "Administrator")]
        public IActionResult UserRole()
        {
            ViewBag.SystemSelected = "Administration";
            return View();
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult PendingUser()
        {
            return RedirectToAction("Users", "Administration", new { approval = "pending" });
        }

        [Authorize]
        public IActionResult UserDetail()
        {
            ViewBag.SystemSelected = "Administration";
            return View(new Models.ViewModels.Administration.UserProfileViewModel());
        }

        [Authorize]
        public IActionResult Password()
        {
            ViewBag.SystemSelected = "Administration";
            return View(new Models.ViewModels.Administration.UserPasswordViewModel());
        }

        public IActionResult Holiday()
        {
            ViewBag.SystemSelected = "Enrolment";
            return View();
        }

        public IActionResult DashboardReport()
        {
            ViewBag.SystemSelected = "Enrolment";
            return View();
        }     
    }
}
