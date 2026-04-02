using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace SCICHRPortal.Web.Controllers
{
    public class AdministrationController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.SystemSelected = "Administration";
            return View();
        }
        public IActionResult Department()
        {
            ViewBag.SystemSelected = "Administration";
            return View();
        }
        public IActionResult Shift()
        {
            ViewBag.SystemSelected = "Administration";
            return View();
        }
        public IActionResult Employee()
        {
            ViewBag.SystemSelected = "Administration";
            return View();
        }
        public IActionResult Audit()
        {
            ViewBag.SystemSelected = "Administration";
            return View();
        }
        public IActionResult EmployeeShift()
        {
            ViewBag.SystemSelected = "Administration";
            return View();
        }
        public IActionResult EmployeeTimeLog()
        {
            ViewBag.SystemSelected = "Administration";
            return View();
        }
        public IActionResult CutOff()
        {
            ViewBag.SystemSelected = "Administration";
            return View();
        }
        public IActionResult LeaveType()
        {
            ViewBag.SystemSelected = "Administration";
            return View();
        }
        public IActionResult EmployeeAttendance()
        {
            ViewBag.SystemSelected = "Administration";
            return View();
        }
        public IActionResult BiometricsLog()
        {
            ViewBag.SystemSelected = "Administration";
            return View();
        }
        public IActionResult Position()
        {
            ViewBag.SystemSelected = "Administration";
            return View();
        }
        public IActionResult TimekeepingAdminSetup()
        {
            ViewBag.SystemSelected = "Administration";
            return View();
        }
        public IActionResult EmployeeAttendanceSummary()
        {
            ViewBag.SystemSelected = "Administration";
            return View();
        }
    }
}
