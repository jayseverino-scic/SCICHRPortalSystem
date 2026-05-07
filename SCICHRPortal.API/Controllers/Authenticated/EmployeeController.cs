using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using Org.BouncyCastle.Asn1.Ocsp;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
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
    public class EmployeeController : ControllerBase
    {
        private IEmployeeService EmployeeService { get; }
        private IUserService UserService { get; }
        private IUserRoleService UserRoleService { get; }
        private IDepartmentService DepartmentService { get; }
        private IPositionService PositionService { get; }
        private AppSettings AppSettings { get; }
        private readonly IMailService MailService;

        public EmployeeController(IEmployeeService employeeService, IUserService userService, IUserRoleService userRoleService, IOptions<AppSettings> appSettings, IMailService mailService, IDepartmentService departmentService, IPositionService positionService)
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
                d.EmployeeId,
                d.DepartmentId,
                d.PositionId,
                d.EmployeeNo,
                d.LastName,
                d.FirstName,
                d.MiddleName,
                d.Suffix,
                d.Address,
                d.Email,
                d.ContactNumber,
                d.CreatedAt,
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

        [HttpPost()]
        public async Task<IActionResult> InsertAsync(Employee employee)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var hasDuplicate = await EmployeeService.HasDuplicateName(employee);
            if (hasDuplicate.IsDuplicated)
                return Conflict(hasDuplicate);

            var salt = Salt.Create();

            string randomPassword = Guid.NewGuid().ToString("N").ToLower()
                      .Replace("1", "").Replace("o", "").Replace("0", "")
            .Substring(0, 10);

            User user = new()
            {
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                MiddleName = employee.MiddleName,
                Email = employee.Email,
                Salt = salt,
                Password = Hash.Create(randomPassword, salt),
                ContactNumber = employee.ContactNumber,
                IsPasswordChanged = false,
                IsApproved = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = employee.CreatedBy,
            };


            var duplicateEmail = await UserService.GetDuplicateEmailAsync(user);

            if (duplicateEmail is not null)
                return Conflict(new { Message = "Email Duplicated" });

            await UserService.InsertAsync(user);

            employee.UserId = user.UserId;
            employee.CreatedAt = DateTime.UtcNow;
            employee.CreatedBy = "manuel";
            await EmployeeService.InsertAsync(employee);
            UserRole userRole = new()
            {
                UserId = user.UserId,
                RoleId = 1,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "manuel"
            };

            await UserRoleService.InsertAsync(userRole);
            //try
            //{
            //    var emailTemplate = await GetEmailTemplate(AppSettings.WebUrl + "html/templates/NewUserTemplate.html");
            //    await MailService.SendForgotPasswordEmailAsync(user.Email!, $"{user.LastName}, {user.FirstName}", randomPassword, emailTemplate);
            //}
            //catch (Exception ex)
            //{
            //    return BadRequest(ex.Message);
            //}

            return StatusCode(201, employee.EmployeeId);
        }

        [HttpPut()]
        public async Task<IActionResult> UpdateAsync(Employee employee)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");
            employee.UpdatedAt = DateTime.UtcNow;
            employee.UpdatedBy = "manuel";
            var updated = await EmployeeService.UpdateAsync(employee);
            if (!updated)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }

        [HttpDelete("{employeeId}")]
        public async Task<IActionResult> DeleteAsync(int employeeId)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var deleted = await EmployeeService.DeleteAsync(employeeId);
            if (!deleted)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }
        [HttpPost("Import")]
        [Consumes("multipart/form-data")]
        [Authorize]
        public async Task<ActionResult> UploadFileAsync(IFormFile file)
        {
            if (file == null)
                return BadRequest(ResponseMessage.BadRequest);

            var extension = Path.GetExtension(file.FileName);
            if (extension != ".xlsx")
            {
                return StatusCode(415, ResponseMessage.FileNotSupported);
            }
            var employee = new List<Employee>();
            IEnumerable<Department> departments = await DepartmentService.GetAllAsync();
            IEnumerable<Position> positions = await PositionService.GetAllAsync();
            IEnumerable<User> users = await UserService.GetAllAsync();
            Department department = departments.FirstOrDefault();
            Position position = positions.FirstOrDefault();
            User user = users.FirstOrDefault();
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using var package = new ExcelPackage(stream);
                ExcelWorksheet workSheet = package.Workbook.Worksheets[0];
                var rowCount = workSheet.Dimension.Rows;
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        //var personnelId = workSheet.Cells[row, 1].Value?.ToString()?.Trim() ?? "";
                        var lastName = workSheet.Cells[row, 2].Value?.ToString()?.Trim() ?? "";
                        var firstNameName = workSheet.Cells[row, 3].Value?.ToString()?.Trim() ?? "";
                        var contactNumber = workSheet.Cells[row, 5].Value?.ToString()?.Trim() ?? "";
                        var email = workSheet.Cells[row, 7].Value?.ToString()?.Trim() ?? "";
                        var employeeNo = workSheet.Cells[row, 9].Value?.ToString()?.Trim() ?? "";
                        //if (string.IsNullOrWhiteSpace(employeeNo) || string.IsNullOrWhiteSpace(employeeName) || string.IsNullOrWhiteSpace(inOut)
                        //    || string.IsNullOrWhiteSpace(dateTimeLog))
                        //    return StatusCode(422, $"One or more fields invalid at row {row}");
                        Employee Employee = new()
                        {
                            EmployeeNo = employeeNo,
                            LastName = lastName,
                            FirstName = firstNameName,
                            ContactNumber = contactNumber,
                            Email = email,
                            PositionId = position!.PositionId,
                            DepartmentId = department!.DepartmentId,
                            MiddleName = " ",
                            Suffix = " ",
                            Address = " ",
                            UserId = user!.UserId,
                            CreatedAt = DateTime.Now,
                            CreatedBy = "Manuel"
                        };
                        employee.Add(Employee);
                        await EmployeeService.InsertAsync(Employee);
                    }
                    catch (Exception)
                    {
                        return StatusCode(422, $"One or more fields invalid at row {row}");
                    }
                }
            }
            var dto = new
            {
                Data = employee,
                Total = employee.Count()
            };
            return Ok(dto);
        }
    }
}
