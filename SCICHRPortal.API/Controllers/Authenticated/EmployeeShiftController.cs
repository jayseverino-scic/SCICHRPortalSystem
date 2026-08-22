using iText.Kernel.Geom;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using SCICHRPortal.API.Models.RequestModels.Authenticated.Administration;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize]
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class EmployeeShiftController : ControllerBase
    {
        private IEmployeeShiftService EmployeeShiftService { get; }
        private IEmployeeService EmployeeService { get; }
        private IShiftService ShiftService { get; }
        private IDepartmentService DepartmentService { get; }
        private IProjectService ProjectService { get; }
        public EmployeeShiftController(IEmployeeShiftService employeeShiftService, IEmployeeService employeeService, IShiftService shiftService, IDepartmentService departmentService, IProjectService projectService)
        {
            EmployeeShiftService = employeeShiftService;
            EmployeeService = employeeService;
            ShiftService = shiftService;
            DepartmentService = departmentService;
            ProjectService = projectService;
        }

        [Authorize] 
        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var employeeShifts = await EmployeeShiftService.GetAllAsync();
            return Ok(employeeShifts);
        }

        [Authorize]
        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber, int pageSize, string? searchKeyword)
        {
            var tuple = await EmployeeShiftService.FilterAsync(pageNumber, pageSize, searchKeyword!);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;
            IEnumerable<EmployeeShift> employeeShiftsList = await EmployeeShiftService.GetAllAsync();
            List<EmployeeShift> employeeShifts = employeeShiftsList.ToList();
            IEnumerable<Employee> employees = await EmployeeService.GetAllAsync();
            IEnumerable<Department> departments = await DepartmentService.GetAllAsync();
            IEnumerable<Project> companies = await ProjectService.GetAllAsync(); 
            IEnumerable<Shift> shifts = await ShiftService.GetAllAsync();

            List<Employee> mergedList = employees
            .GroupJoin(
                tuple.Item1, left => left.EmployeeId, right => right.EmployeeId,
                (x, y) => new { Left = x, Rights = y }
            )
            .SelectMany(
                x => x.Rights.DefaultIfEmpty(),
                (x, y) => new Employee
                {
                    EmployeeId = x.Left.EmployeeId,
                    DepartmentId = x.Left.DepartmentId,
                    ProjectId = x.Left.ProjectId,
                    LastName = x.Left.LastName,
                    FirstName = x.Left.FirstName
                }
            ).ToList();

            if (mergedList != null)
            {
                foreach(Employee employee in mergedList)
                {
                    EmployeeShift employeeShift = new EmployeeShift
                    {
                        AssignedShiftId = 0,
                        ShiftId = 0,
                        EmployeeId = employee.EmployeeId,
                        DepartmentId = (int)employee.DepartmentId!,
                        ProjectId = employee.ProjectId!,
                        Employee = employee,
                        Department = departments.Where(d => d.DepartmentId == employee.DepartmentId).SingleOrDefault()
                    };
                    employeeShifts.Add(employeeShift);
                }
            }
            employeeShifts.AddRange(tuple.Item1);
            var data = employeeShifts.Select(d => new
            {
                d.AssignedShiftId,
                d.ShiftDate,
                d.MondayShiftStart,
                d.MondayShiftEnd,
                d.TuesdayShiftStart,
                d.TuesdayShiftEnd,
                d.WednesdayShiftStart,
                d.WednesdayShiftEnd,
                d.ThursdayShiftStart,
                d.ThursdayShiftEnd,
                d.FridayShiftStart,
                d.FridayShiftEnd,
                d.SaturdayShiftStart,
                d.SaturdayShiftEnd,
                d.SundayShiftStart,
                d.SundayShiftEnd,
                d.IsFlexibleShift,
                d.IsNoBreak,
                d.IsNoShift,
                d.EmployeeId,
                EmployeeName = $"{d.Employee!.LastName}, {d.Employee!.FirstName}",
                d.DepartmentId,
                DepartmentName = d.Department?.DepartmentName,
                d.ProjectId,
                CompanyBranchName = d.Project?.Name,
                d.ShiftId,
                ShiftName = d.Shift?.ShiftName,
                OrderNumber = orderNumber++
            });
            var dto = new
            {
                Data = data,
                Total = employeeShifts.Count()
            };

            return Ok(dto);
        }

        [Authorize]
        [HttpGet("ShiftFilter")]
        public async Task<IActionResult> EmployeeShiftFilterAsync(int projectId, int shiftId, string filterType)
        {
            IEnumerable<Employee>? employees = await EmployeeService.GetEmployeeByProject(projectId);
            if (employees == null)
                return BadRequest();
            IEnumerable<EmployeeShift>? employeeShiftsList = await EmployeeShiftService.EmployeeShiftFilterPerProject(projectId, shiftId); 
            List<EmployeeShiftUpdateRequestModel> listToDisplay = new List<EmployeeShiftUpdateRequestModel>();
            int[] assignedIds = employeeShiftsList.Select(x => x.EmployeeId).ToArray();
            List<EmployeeShift> employeeShifts = new List<EmployeeShift>();
            IEnumerable<Department> departmentList = await DepartmentService.GetAllAsync();
            List<Department> departments = departmentList.ToList();
            IEnumerable<Project> companyList = await ProjectService.GetAllAsync();
            List<Project> companies = companyList.ToList();
            employees = employees.Where(item => !assignedIds.Any(x => x == item.EmployeeId)).ToList();

            List<Employee> mergedList = employees
            .GroupJoin(
                employeeShifts, left => left.EmployeeId, right => right.EmployeeId,
                (x, y) => new { Left = x, Rights = y }
            )
            .SelectMany(
                x => x.Rights.DefaultIfEmpty(),
                (x, y) => new Employee
                {
                    EmployeeId = x.Left.EmployeeId,
                    DepartmentId = x.Left.DepartmentId,
                    ProjectId = x.Left.ProjectId,
                    LastName = x.Left.LastName,
                    FirstName = x.Left.FirstName
                }
            ).ToList();
            if (filterType == "Assigned")
            {
                employeeShiftsList = await EmployeeShiftService.EmployeeShiftFilterPerProject(projectId, shiftId);
                foreach(EmployeeShift employeeShift in employeeShiftsList.ToList())
                {
                    EmployeeShiftUpdateRequestModel employeeShiftUpdateRequestModel = new EmployeeShiftUpdateRequestModel
                    {
                        AssignedShiftId = employeeShift.AssignedShiftId,
                        ShiftId = employeeShift.ShiftId,
                        EmployeeId = employeeShift.EmployeeId,
                        DepartmentId = employeeShift.DepartmentId,
                        ProjectId = employeeShift.ProjectId,
                        ShiftDate = employeeShift.ShiftDate,
                        MondayShiftStart = employeeShift.MondayShiftStart,
                        MondayShiftEnd = employeeShift.MondayShiftEnd,
                        TuesdayShiftStart = employeeShift.TuesdayShiftStart,
                        TuesdayShiftEnd = employeeShift.TuesdayShiftEnd,
                        WednesdayShiftStart = employeeShift.WednesdayShiftStart,
                        WednesdayShiftEnd = employeeShift.WednesdayShiftEnd,
                        ThursdayShiftStart = employeeShift.ThursdayShiftStart,
                        ThursdayShiftEnd = employeeShift.ThursdayShiftEnd,
                        FridayShiftStart = employeeShift.FridayShiftStart,
                        FridayShiftEnd = employeeShift.FridayShiftEnd,
                        SaturdayShiftStart = employeeShift.SaturdayShiftStart,
                        SaturdayShiftEnd = employeeShift.SaturdayShiftEnd,
                        SundayShiftStart = employeeShift.SundayShiftStart,
                        SundayShiftEnd = employeeShift.SundayShiftEnd,
                        IsFlexibleShift = employeeShift.IsFlexibleShift,
                        IsNoShift = employeeShift.IsNoShift,
                        IsNoBreak = employeeShift.IsNoBreak,
                        IsAssigned = true,
                        Employee = employeeShift.Employee,
                        Department = employeeShift.Department,
                        Shift = employeeShift.Shift
                    };
                    listToDisplay.Add(employeeShiftUpdateRequestModel);
                }
            }
            else
            {
                if (mergedList != null)
                {
                    foreach (Employee employee in mergedList)
                    {
                        EmployeeShift employeeWithShift = await EmployeeShiftService.GetByEmployee(employee.EmployeeId);
                        if (employeeWithShift == null)
                        {
                            EmployeeShift employeeShift = new EmployeeShift
                            {
                                AssignedShiftId = 0,
                                ShiftId = 0,
                                EmployeeId = employee.EmployeeId,
                                DepartmentId = employee.DepartmentId == null ? 0 : employee.DepartmentId,
                                ProjectId = employee.ProjectId,
                                Employee = employee,
                                Department = departments.Where(d => d.DepartmentId == employee.DepartmentId).SingleOrDefault(),
                                Project = companies.Where(d => d.Id == employee.ProjectId).SingleOrDefault()
                            };
                            employeeShifts.Add(employeeShift);
                        }
                    }
                }
                if (filterType == "All")
                    employeeShifts.AddRange(employeeShiftsList);
                foreach (EmployeeShift employeeShift in employeeShifts.ToList())
                {
                    EmployeeShiftUpdateRequestModel employeeShiftUpdateRequestModel = new EmployeeShiftUpdateRequestModel
                    {
                        AssignedShiftId = employeeShift.AssignedShiftId,
                        ShiftId = employeeShift.ShiftId,
                        EmployeeId = employeeShift.EmployeeId,
                        DepartmentId = employeeShift.DepartmentId,
                        ProjectId = employeeShift.ProjectId,
                        ShiftDate = employeeShift.ShiftDate,
                        MondayShiftStart = employeeShift.MondayShiftStart,
                        MondayShiftEnd = employeeShift.MondayShiftEnd,
                        TuesdayShiftStart = employeeShift.TuesdayShiftStart,
                        TuesdayShiftEnd = employeeShift.TuesdayShiftEnd,
                        WednesdayShiftStart = employeeShift.WednesdayShiftStart,
                        WednesdayShiftEnd = employeeShift.WednesdayShiftEnd,
                        ThursdayShiftStart = employeeShift.ThursdayShiftStart,
                        ThursdayShiftEnd = employeeShift.ThursdayShiftEnd,
                        FridayShiftStart = employeeShift.FridayShiftStart,
                        FridayShiftEnd = employeeShift.FridayShiftEnd,
                        SaturdayShiftStart = employeeShift.SaturdayShiftStart,
                        SaturdayShiftEnd = employeeShift.SaturdayShiftEnd,
                        SundayShiftStart = employeeShift.SundayShiftStart,
                        SundayShiftEnd = employeeShift.SundayShiftEnd,
                        IsFlexibleShift = employeeShift.IsFlexibleShift,
                        IsNoShift = employeeShift.IsNoShift,
                        IsNoBreak = employeeShift.IsNoBreak,
                        IsAssigned = employeeShift.ShiftId == 0? false:true,
                        Employee = employeeShift.Employee,
                        Department = employeeShift.Department,
                        Shift = employeeShift.Shift
                    };
                    listToDisplay.Add(employeeShiftUpdateRequestModel);
                }
            }
          
            var data = listToDisplay.Select(d => new
            {
                d.AssignedShiftId,
                d.ShiftDate,
                d.MondayShiftStart,
                d.MondayShiftEnd,
                d.TuesdayShiftStart,
                d.TuesdayShiftEnd,
                d.WednesdayShiftStart,
                d.WednesdayShiftEnd,
                d.ThursdayShiftStart,
                d.ThursdayShiftEnd,
                d.FridayShiftStart,
                d.FridayShiftEnd,
                d.SaturdayShiftStart,
                d.SaturdayShiftEnd,
                d.SundayShiftStart,
                d.SundayShiftEnd,
                d.IsFlexibleShift,
                d.IsNoShift,
                d.IsNoBreak,
                d.EmployeeId,
                EmployeeName = $"{d.Employee?.LastName}, {d.Employee?.FirstName}",
                d.DepartmentId,
                DepartmentName = d.Department?.DepartmentName,
                d.ProjectId,
                CompanyName = d.Project?.Name,
                d.ShiftId,
                ShiftName = d.Shift?.ShiftName,
                d.IsAssigned 
            });

            return Ok(data);
        }

        [Authorize]
        [HttpPost()]
        public async Task<IActionResult> UpdateShiftAssignmentAsync(List<EmployeeShiftUpdateRequestModel> employeeShift, int shiftId)
        {
            if (shiftId == 0)
                return BadRequest();
            Shift shift = await ShiftService.GetAsync(shiftId);
            DateTime? mondayShiftStart = shift.MondayShiftStart;
            DateTime? mondayShiftEnd = shift.MondayShiftEnd;
            DateTime? tuesdayShiftStart = shift.TuesdayShiftStart;
            DateTime? tuesdayShiftEnd = shift.TuesdayShiftEnd;
            DateTime? wednesdayShiftStart = shift.WednesdayShiftStart;
            DateTime? wednesdayShiftEnd = shift.WednesdayShiftEnd;
            DateTime? thursdayShiftStart = shift.ThursdayShiftStart;
            DateTime? thursdayShiftEnd = shift.ThursdayShiftEnd;
            DateTime? fridayShiftStart = shift.FridayShiftStart;
            DateTime? fridayShiftEnd = shift.FridayShiftEnd;
            DateTime? saturdayShiftStart = shift.SaturdayShiftStart;
            DateTime? saturdayShiftEnd = shift.SaturdayShiftEnd;
            DateTime? sundayShiftStart = shift.SundayShiftStart;
            DateTime? sundayShiftEnd = shift.SundayShiftEnd;

            foreach (var item in employeeShift)
            {
                if (item.IsAssigned == true && item.AssignedShiftId != 0)
                {
                    EmployeeShift shiftAssignment = new EmployeeShift
                    {
                        AssignedShiftId = item.AssignedShiftId,
                        ShiftId = shiftId,
                        EmployeeId = item.EmployeeId,
                        DepartmentId = item.DepartmentId == 0 ? null : item.DepartmentId,
                        ProjectId = item.ProjectId == 0 ? null : item.ProjectId,
                        ShiftDate = DateTime.Now,
                        MondayShiftStart = mondayShiftStart,
                        MondayShiftEnd = mondayShiftEnd,
                        TuesdayShiftStart = tuesdayShiftStart,
                        TuesdayShiftEnd = tuesdayShiftEnd,
                        WednesdayShiftStart = wednesdayShiftStart,
                        WednesdayShiftEnd = wednesdayShiftEnd,
                        ThursdayShiftStart = thursdayShiftStart,
                        ThursdayShiftEnd = thursdayShiftEnd,
                        FridayShiftStart = fridayShiftStart,
                        FridayShiftEnd = fridayShiftEnd,
                        SaturdayShiftStart = saturdayShiftStart,
                        SaturdayShiftEnd = saturdayShiftEnd,
                        SundayShiftStart = sundayShiftStart,
                        SundayShiftEnd = sundayShiftEnd,
                        IsFlexibleShift = item.IsFlexibleShift,
                        IsNoBreak = item.IsNoBreak,
                        IsNoShift = item.IsNoShift,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "manuel"
                    };
                    await EmployeeShiftService.UpdateAsync(shiftAssignment);
                }
                if (item.IsAssigned == true && item.AssignedShiftId == 0)
                {
                    EmployeeShift shiftAssignment = new EmployeeShift
                    {
                        AssignedShiftId = item.AssignedShiftId,
                        ShiftId = shiftId,
                        EmployeeId = item.EmployeeId,
                        DepartmentId = item.DepartmentId == 0 ? null : item.DepartmentId,
                        ProjectId = item.ProjectId == 0 ? null : item.ProjectId,
                        ShiftDate = DateTime.Now,
                        MondayShiftStart = mondayShiftStart,
                        MondayShiftEnd = mondayShiftEnd,
                        TuesdayShiftStart = tuesdayShiftStart,
                        TuesdayShiftEnd = tuesdayShiftEnd,
                        WednesdayShiftStart = wednesdayShiftStart,
                        WednesdayShiftEnd = wednesdayShiftEnd,
                        ThursdayShiftStart = thursdayShiftStart,
                        ThursdayShiftEnd = thursdayShiftEnd,
                        FridayShiftStart = fridayShiftStart,
                        FridayShiftEnd = fridayShiftEnd,
                        SaturdayShiftStart = saturdayShiftStart,
                        SaturdayShiftEnd = saturdayShiftEnd,
                        SundayShiftStart = sundayShiftStart,
                        SundayShiftEnd = sundayShiftEnd,
                        IsFlexibleShift = item.IsFlexibleShift,
                        IsNoBreak = item.IsNoBreak,
                        IsNoShift = item.IsNoShift,
                        CreatedBy = "manuel",
                        CreatedAt = DateTime.UtcNow
                    };
                    await EmployeeShiftService.InsertAsync(shiftAssignment);
                }
                if (item.IsAssigned == false && item.AssignedShiftId != 0)
                {
                    await EmployeeShiftService.DeleteAsync(item.AssignedShiftId);
                }
            }

            return StatusCode(201, employeeShift);
        }

        [Authorize]
        [HttpPut()]
        public async Task<IActionResult> UpdateAsync(EmployeeShift employeeShift)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");
            employeeShift.UpdatedAt = DateTime.UtcNow;
            employeeShift.UpdatedBy = "manuel";
            await EmployeeShiftService.UpdateAsync(employeeShift);

            return Ok();
        }

        [Authorize]
        [HttpDelete("{employeeShiftId}")]
        public async Task<IActionResult> RemoveAsync(int employeeShiftId)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var deleted = await EmployeeShiftService.DeleteAsync(employeeShiftId);

            if (!deleted)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }
    }
}
