using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using Org.BouncyCastle.Asn1.Ocsp;
using SCICHRPortal.Service.Implementations;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;
using SCICHRPortal.Utility.Cryptography;
using SCICHRPortal.Utility.Settings;
using System.Globalization;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize]
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class XEmployeeController : ControllerBase
    {
        private IXEmployeeService EmployeeService { get; }
        private IUserService UserService { get; }
        private IUserRoleService UserRoleService { get; }
        private IXDepartmentService DepartmentService { get; }
        private IPositionService PositionService { get; }
        private AppSettings AppSettings { get; }
        private readonly IMailService MailService;

        public XEmployeeController(IXEmployeeService employeeService, IUserService userService, IUserRoleService userRoleService, IOptions<AppSettings> appSettings, IMailService mailService, IXDepartmentService departmentService, IPositionService positionService)
        {
            EmployeeService = employeeService;
            UserService = userService;
            UserRoleService = userRoleService;
            AppSettings = appSettings.Value;
            MailService = mailService;
            DepartmentService = departmentService;
            PositionService = positionService;
        }

        private async Task<FileStreamResult> GetEmailTemplate(string templateUrl)
        {

            using HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(AppSettings.WebUrl!)
            };

            HttpResponseMessage response = await client.GetAsync(templateUrl);

            if (response.IsSuccessStatusCode)
            {
                var content = response.Content;
                var contentStream = await content.ReadAsStreamAsync();
                return File(contentStream, "text/html", "Template.html");
            }
            else
            {
                throw new FileNotFoundException();
            }
        }

        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var employees = await EmployeeService.GetAllAsync();
            return Ok(employees);
        }
        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber, int pageSize, string? searchKeyword)
        {
            var tuple = await EmployeeService.FilterAsync(pageNumber, pageSize, searchKeyword!);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;
            var dateToday = DateTime.Today;

            var data = tuple.Item1.Select(d => new
            {
                d.Id,
                d.Department_Id,
                d.Company_Position_Id,
                d.Employee_code,
                d.Last_Name,
                d.First_Name,
                d.Middle_Name,
                d.Suffix,
                d.Location_Address_Line1,
                d.Email,
                d.Mobile,
                d.Department,
                d.Position,
                OrderNumber = orderNumber++
            });

            var dto = new
            {
                Data = data,
                Total = tuple.Item2
            };
            return Ok(dto);
        }
    }
}
