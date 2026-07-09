using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using Org.BouncyCastle.Asn1.Ocsp;
using SCICHRPortal.Data.TimekeepingTables;
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
    public class SPersonnelsController : ControllerBase
    {
        private ISPersonnelsService SPersonnelsService { get; }
        private IUserService UserService { get; }
        private IUserRoleService UserRoleService { get; }
        private IDepartmentService DepartmentService { get; }
        private IPositionService PositionService { get; }
        private AppSettings AppSettings { get; }
        private readonly IMailService MailService;

        public SPersonnelsController(ISPersonnelsService employeeService, IUserService userService, IUserRoleService userRoleService, IOptions<AppSettings> appSettings, IMailService mailService, IDepartmentService departmentService, IPositionService positionService)
        {
            SPersonnelsService = employeeService;
            UserService = userService;
            UserRoleService = userRoleService;
            AppSettings = appSettings.Value;
            MailService = mailService;
            DepartmentService = departmentService;
            PositionService = positionService;
        }


        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var employees = await SPersonnelsService.GetAllAsync();
            return Ok(employees);
        }
        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber, int pageSize, string? searchKeyword)
        {
            var tuple = await SPersonnelsService.FilterAsync(pageNumber, pageSize, searchKeyword!);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;
            var dateToday = DateTime.Today;

            var data = tuple.Item1.Select(d => new
            {
                d.Id,
                d.PersonnelNo,
                d.AccessNumber,
                d.LastName,
                d.FirstName,
                d.MiddleName,
                d.Email,
                d.ContactNumber,
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
