using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using Org.BouncyCastle.Asn1.Ocsp;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository;
using SCICHRPortal.Service.Implementations;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;
using SCICHRPortal.Utility.Cryptography;
using SCICHRPortal.Utility.Settings;
using System.Data;
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
        private IProjectService ProjectService { get; }
        private IXEmployeeService XEmployeeService { get; }
        private AppSettings AppSettings { get; }
        private readonly IMailService MailService;
        private readonly ApplicationContext Context;
        public EmployeeController(IEmployeeService employeeService, IUserService userService, IUserRoleService userRoleService, IOptions<AppSettings> appSettings, IMailService mailService, IDepartmentService departmentService, IXEmployeeService xemployeeService, IProjectService projectService, ApplicationContext context)
        {
            EmployeeService = employeeService;
            UserService = userService;
            UserRoleService = userRoleService;
            AppSettings = appSettings.Value;
            MailService = mailService;
            DepartmentService = departmentService;
            XEmployeeService = xemployeeService;
            ProjectService = projectService;
            Context = context;
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
                d.ProjectId,
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
                d.Project,
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
            IEnumerable<Project> projects = await ProjectService.GetAllAsync();
            //IEnumerable<Position> positions = await PositionService.GetAllAsync();
            IEnumerable<User> users = await UserService.GetAllAsync();
            Department department = departments.FirstOrDefault();
            //Position position = positions.FirstOrDefault();
            Project project = projects.FirstOrDefault();
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
                            DepartmentId = department!.DepartmentId,
                            ProjectId = project!.Id,
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
        [HttpGet("ImportDb")]
        [Authorize]
        public async Task<ActionResult> ImportDb()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // 1. Get all data efficiently using Dapper (raw SQL)
                var xEmployees = await XEmployeeService.GetAllAsync();
                if (xEmployees == null || !xEmployees.Any())
                    return Ok(new { Data = new List<Employee>(), Total = 0 });

                // 2. Get existing employees as dictionary
                var existingEmployees = await EmployeeService.GetAllAsync();
                var existingEmployeeDict = new Dictionary<string, Employee>(StringComparer.OrdinalIgnoreCase);
                foreach (var emp in existingEmployees)
                {
                    if (!string.IsNullOrEmpty(emp.EmployeeNo) && !existingEmployeeDict.ContainsKey(emp.EmployeeNo))
                    {
                        existingEmployeeDict[emp.EmployeeNo] = emp;
                    }
                }

                // 3. Get all departments and projects with caching
                var allDepartments = await DepartmentService.GetAllAsync();
                var deptDict = allDepartments
                    .Where(d => !string.IsNullOrEmpty(d.DeptCode))
                    .GroupBy(d => d.DeptCode, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToDictionary(d => d.DeptCode, d => d.DepartmentId, StringComparer.OrdinalIgnoreCase);

                var allProjects = await ProjectService.GetAllAsync();
                var projectDict = allProjects
                    .Where(p => !string.IsNullOrEmpty(p.Code))
                    .GroupBy(p => p.Code, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToDictionary(p => p.Code, p => p.Id, StringComparer.OrdinalIgnoreCase);

                // 4. Prepare data for bulk insert using DataTable (fastest)
                var dataTable = new DataTable();
                dataTable.Columns.Add("EmployeeNo", typeof(string));
                dataTable.Columns.Add("FirstName", typeof(string));
                dataTable.Columns.Add("LastName", typeof(string));
                dataTable.Columns.Add("MiddleName", typeof(string));
                dataTable.Columns.Add("Suffix", typeof(string));
                dataTable.Columns.Add("Email", typeof(string));
                dataTable.Columns.Add("ContactNumber", typeof(string));
                dataTable.Columns.Add("ProjectId", typeof(int));
                dataTable.Columns.Add("DepartmentId", typeof(int));
                dataTable.Columns.Add("CreatedAt", typeof(DateTime));
                dataTable.Columns.Add("CreatedBy", typeof(string));
                dataTable.Columns.Add("UpdatedAt", typeof(DateTime));
                dataTable.Columns.Add("UpdatedBy", typeof(string));

                var updateDataTable = new DataTable();
                updateDataTable.Columns.Add("Id", typeof(int));
                updateDataTable.Columns.Add("EmployeeNo", typeof(string));
                updateDataTable.Columns.Add("FirstName", typeof(string));
                updateDataTable.Columns.Add("LastName", typeof(string));
                updateDataTable.Columns.Add("MiddleName", typeof(string));
                updateDataTable.Columns.Add("Suffix", typeof(string));
                updateDataTable.Columns.Add("Email", typeof(string));
                updateDataTable.Columns.Add("ContactNumber", typeof(string));
                updateDataTable.Columns.Add("ProjectId", typeof(int));
                updateDataTable.Columns.Add("DepartmentId", typeof(int));
                updateDataTable.Columns.Add("UpdatedAt", typeof(DateTime));
                updateDataTable.Columns.Add("UpdatedBy", typeof(string));

                int processedCount = 0;
                int totalRecords = xEmployees.Count();
                int batchSize = 5000; // Larger batch size for better performance

                foreach (var employee in xEmployees)
                {
                    processedCount++;

                    // Get IDs with caching
                    int deptId = GetDepartmentIdFast(employee.Department_Id?.ToString(), deptDict);
                    int projectId = GetProjectIdFast(employee.Company_Branch_Id?.ToString(), projectDict);

                    var employeeKey = employee.Id.ToString();
                    var existingEmployee = string.IsNullOrEmpty(employeeKey) ? null :
                        existingEmployeeDict.GetValueOrDefault(employeeKey);

                    if (existingEmployee == null)
                    {
                        // Add to insert DataTable
                        dataTable.Rows.Add(
                            employee.Id.ToString() ?? string.Empty,
                            employee.First_Name ?? string.Empty,
                            employee.Last_Name ?? string.Empty,
                            employee.Middle_Name ?? string.Empty,
                            employee.Suffix ?? string.Empty,
                            employee.Email ?? string.Empty,
                            employee.Mobile ?? string.Empty,
                            projectId,
                            deptId,
                            DateTime.UtcNow,
                            "manuel",
                            DateTime.UtcNow,
                            "manuel"
                        );
                    }
                    else
                    {
                        // Add to update DataTable
                        updateDataTable.Rows.Add(
                            existingEmployee.EmployeeId,
                            employee.Id.ToString() ?? string.Empty,
                            employee.First_Name ?? string.Empty,
                            employee.Last_Name ?? string.Empty,
                            employee.Middle_Name ?? string.Empty,
                            employee.Suffix ?? string.Empty,
                            employee.Email ?? string.Empty,
                            employee.Mobile ?? string.Empty,
                            projectId,
                            deptId,
                            DateTime.UtcNow,
                            "manuel"
                        );
                    }

                    // Process in batches
                    if (dataTable.Rows.Count >= batchSize)
                    {
                        await BulkInsertDataTableAsync(dataTable, "Employees");
                        dataTable.Clear();
                    }

                    if (updateDataTable.Rows.Count >= batchSize)
                    {
                        await BulkUpdateDataTableAsync(updateDataTable, "Employees");
                        updateDataTable.Clear();
                    }

                    // Log progress
                    if (processedCount % 10000 == 0)
                    {
                        Console.WriteLine($"Processed {processedCount}/{totalRecords} records");
                        Console.WriteLine($"Memory usage: {GC.GetTotalMemory(false) / 1024 / 1024} MB");
                    }
                }

                // Insert/Update remaining records
                if (dataTable.Rows.Count > 0)
                    await BulkInsertDataTableAsync(dataTable, "Employees");

                if (updateDataTable.Rows.Count > 0)
                    await BulkUpdateDataTableAsync(updateDataTable, "Employees");

                stopwatch.Stop();
                Console.WriteLine($"Import completed in {stopwatch.Elapsed.TotalMinutes:F2} minutes");

                return Ok(new
                {
                    TotalProcessed = totalRecords,
                    Inserted = dataTable.Rows.Count,
                    Updated = updateDataTable.Rows.Count,
                    ElapsedMinutes = stopwatch.Elapsed.TotalMinutes,
                    Message = "Import completed successfully with optimized bulk operations"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during import: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                return StatusCode(500, new
                {
                    Error = "Import failed",
                    Message = ex.Message,
                    Details = ex.InnerException?.Message
                });
            }
        }

        // Optimized helper methods with inline logic
        private int GetDepartmentIdFast(string? deptCode, Dictionary<string, int> deptDict)
        {
            if (string.IsNullOrEmpty(deptCode))
                return 1;
            return deptDict.TryGetValue(deptCode, out int deptId) ? deptId : 1;
        }

        private int GetProjectIdFast(string? projectCode, Dictionary<string, int> projectDict)
        {
            if (string.IsNullOrEmpty(projectCode))
                return 1;
            return projectDict.TryGetValue(projectCode, out int projectId) ? projectId : 1;
        }

        // High-performance bulk insert using SqlBulkCopy
        private async Task BulkInsertDataTableAsync(DataTable dataTable, string tableName)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
                return;

            try
            {
                var connectionString = Context.Database.GetConnectionString();

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var bulkCopy = new SqlBulkCopy(connection))
                    {
                        bulkCopy.DestinationTableName = tableName;
                        bulkCopy.BatchSize = dataTable.Rows.Count;
                        bulkCopy.BulkCopyTimeout = 600; // 10 minutes timeout
                        bulkCopy.EnableStreaming = true; // Better for large datasets

                        // Map columns
                        foreach (DataColumn column in dataTable.Columns)
                        {
                            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                        }

                        await bulkCopy.WriteToServerAsync(dataTable);
                    }
                }

                Console.WriteLine($"Bulk inserted {dataTable.Rows.Count} records into {tableName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bulk insert failed for {dataTable.Rows.Count} records: {ex.Message}");
                // Fallback to individual inserts
                await FallbackInsertDataTableAsync(dataTable, tableName);
            }
        }

        // High-performance bulk update using SqlBulkCopy with MERGE or temp table
        private async Task BulkUpdateDataTableAsync(DataTable dataTable, string tableName)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
                return;

            try
            {
                var connectionString = Context.Database.GetConnectionString();

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Create temp table
                    string tempTableName = $"#{tableName}_Temp_{Guid.NewGuid():N}";

                    string createTempTable = $@"
                CREATE TABLE {tempTableName} (
                    Id INT,
                    EmployeeNo NVARCHAR(50),
                    FirstName NVARCHAR(100),
                    LastName NVARCHAR(100),
                    MiddleName NVARCHAR(100),
                    Suffix NVARCHAR(50),
                    Email NVARCHAR(200),
                    ContactNumber NVARCHAR(50),
                    ProjectId INT,
                    DepartmentId INT,
                    UpdatedAt DATETIME2,
                    UpdatedBy NVARCHAR(100)
                )";

                    using (var createCmd = new SqlCommand(createTempTable, connection))
                    {
                        await createCmd.ExecuteNonQueryAsync();
                    }

                    // Bulk insert into temp table
                    using (var bulkCopy = new SqlBulkCopy(connection))
                    {
                        bulkCopy.DestinationTableName = tempTableName;
                        bulkCopy.BatchSize = dataTable.Rows.Count;
                        bulkCopy.BulkCopyTimeout = 600;
                        bulkCopy.EnableStreaming = true;

                        foreach (DataColumn column in dataTable.Columns)
                        {
                            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                        }

                        await bulkCopy.WriteToServerAsync(dataTable);
                    }

                    // Update using MERGE
                    string mergeSql = $@"
                MERGE {tableName} AS target
                USING {tempTableName} AS source
                ON target.Id = source.Id
                WHEN MATCHED THEN
                    UPDATE SET 
                        target.EmployeeNo = source.EmployeeNo,
                        target.FirstName = source.FirstName,
                        target.LastName = source.LastName,
                        target.MiddleName = source.MiddleName,
                        target.Suffix = source.Suffix,
                        target.Email = source.Email,
                        target.ContactNumber = source.ContactNumber,
                        target.ProjectId = source.ProjectId,
                        target.DepartmentId = source.DepartmentId,
                        target.UpdatedAt = source.UpdatedAt,
                        target.UpdatedBy = source.UpdatedBy;";

                    using (var mergeCmd = new SqlCommand(mergeSql, connection))
                    {
                        int rowsAffected = await mergeCmd.ExecuteNonQueryAsync();
                        Console.WriteLine($"Updated {rowsAffected} records in {tableName}");
                    }

                    // Drop temp table
                    string dropTable = $"DROP TABLE {tempTableName}";
                    using (var dropCmd = new SqlCommand(dropTable, connection))
                    {
                        await dropCmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bulk update failed for {dataTable.Rows.Count} records: {ex.Message}");
                await FallbackUpdateDataTableAsync(dataTable, tableName);
            }
        }

        // Fallback methods for individual operations
        private async Task FallbackInsertDataTableAsync(DataTable dataTable, string tableName)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                try
                {
                    var employee = new Employee
                    {
                        EmployeeNo = row["EmployeeNo"].ToString(),
                        FirstName = row["FirstName"].ToString(),
                        LastName = row["LastName"].ToString(),
                        MiddleName = row["MiddleName"].ToString(),
                        Suffix = row["Suffix"].ToString(),
                        Email = row["Email"].ToString(),
                        ContactNumber = row["ContactNumber"].ToString(),
                        ProjectId = (int)row["ProjectId"],
                        DepartmentId = (int)row["DepartmentId"],
                        CreatedAt = (DateTime)row["CreatedAt"],
                        CreatedBy = row["CreatedBy"].ToString(),
                        UpdatedAt = (DateTime)row["UpdatedAt"],
                        UpdatedBy = row["UpdatedBy"].ToString()
                    };
                    await EmployeeService.InsertAsync(employee);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to insert record: {ex.Message}");
                }
            }
        }

        private async Task FallbackUpdateDataTableAsync(DataTable dataTable, string tableName)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                try
                {
                    var employee = new Employee
                    {
                        EmployeeId = (int)row["Id"],
                        EmployeeNo = row["EmployeeNo"].ToString(),
                        FirstName = row["FirstName"].ToString(),
                        LastName = row["LastName"].ToString(),
                        MiddleName = row["MiddleName"].ToString(),
                        Suffix = row["Suffix"].ToString(),
                        Email = row["Email"].ToString(),
                        ContactNumber = row["ContactNumber"].ToString(),
                        ProjectId = (int)row["ProjectId"],
                        DepartmentId = (int)row["DepartmentId"],
                        UpdatedAt = (DateTime)row["UpdatedAt"],
                        UpdatedBy = row["UpdatedBy"].ToString()
                    };
                    await EmployeeService.UpdateAsync(employee);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to update record: {ex.Message}");
                }
            }
        }
    }
}
