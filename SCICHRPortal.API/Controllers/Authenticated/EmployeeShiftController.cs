using iText.Kernel.Geom;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using SCICHRPortal.API.Models.RequestModels.Authenticated.Administration;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
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
        public EmployeeShiftController(IEmployeeShiftService employeeShiftService, IEmployeeService employeeService, IShiftService shiftService, IDepartmentService departmentService)
        {
            EmployeeShiftService = employeeShiftService;
            EmployeeService = employeeService;
            ShiftService = shiftService;
            DepartmentService = departmentService;
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
                        DepartmentId = (int)employee.DepartmentId,
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
                d.ShiftStart,
                d.ShiftEnd,
                d.BreakStart,
                d.BreakEnd,
                d.IsFlexibleBreak,
                d.IsFlexibleShift,
                d.IsNoBreak,
                d.IsNoShift,
                d.EmployeeId,
                EmployeeName = $"{d.Employee!.LastName}, {d.Employee!.FirstName}",
                d.DepartmentId,
                DepartmentName = d.Department?.DepartmentName,
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
        public async Task<IActionResult> EmployeeShiftFilterAsync(int departmentId, int shiftId, string filterType)
        {           
            IEnumerable<EmployeeShift>? employeeShiftsList = null;
            List<EmployeeShiftUpdateRequestModel> listToDisplay = new List<EmployeeShiftUpdateRequestModel>();
            IEnumerable<Employee>? employees = null;
            if (filterType == "Assigned")
            {
                employeeShiftsList = await EmployeeShiftService.EmployeeShiftFilter(departmentId, shiftId);
                foreach(EmployeeShift employeeShift in employeeShiftsList.ToList())
                {
                    EmployeeShiftUpdateRequestModel employeeShiftUpdateRequestModel = new EmployeeShiftUpdateRequestModel
                    {
                        AssignedShiftId = employeeShift.AssignedShiftId,
                        ShiftId = employeeShift.ShiftId,
                        EmployeeId = employeeShift.EmployeeId,
                        DepartmentId = employeeShift.DepartmentId,
                        ShiftDate = employeeShift.ShiftDate,
                        ShiftStart = employeeShift.ShiftStart,
                        ShiftEnd = employeeShift.ShiftEnd,
                        BreakEnd = employeeShift.BreakEnd,
                        BreakStart = employeeShift.BreakStart,
                        IsFlexibleShift = employeeShift.IsFlexibleShift,
                        IsFlexibleBreak = employeeShift.IsFlexibleBreak,
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
                employeeShiftsList = await EmployeeShiftService.EmployeeShiftFilter(departmentId, shiftId);

                int[] assignedIds = employeeShiftsList.Select(x => x.EmployeeId).ToArray();

                List<EmployeeShift> employeeShifts = new List<EmployeeShift>();
                IEnumerable<Department> departmentList = await DepartmentService.GetAllAsync();
                List<Department> departments = departmentList.ToList();
                employees = await EmployeeService.GetAllAsync();
                employees = employees.Where(item => !assignedIds.Any(x => x == item.EmployeeId)).ToList();
                if (departmentId != 0)
                {
                    employees = employees.Where(e => e.DepartmentId == departmentId);
                }
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
                        LastName = x.Left.LastName,
                        FirstName = x.Left.FirstName
                    }
                ).ToList();

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
                                DepartmentId = (int)employee.DepartmentId!,
                                Employee = employee,
                                Department = departments.Where(d => d.DepartmentId == employee.DepartmentId).SingleOrDefault(),
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
                        ShiftDate = employeeShift.ShiftDate,
                        ShiftStart = employeeShift.ShiftStart,
                        ShiftEnd = employeeShift.ShiftEnd,
                        BreakStart = employeeShift.BreakStart,
                        BreakEnd = employeeShift.BreakEnd,
                        IsFlexibleBreak = employeeShift.IsFlexibleBreak,
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
                d.ShiftStart,
                d.ShiftEnd,
                d.BreakStart,
                d.BreakEnd,
                d.IsFlexibleBreak,
                d.IsFlexibleShift,
                d.IsNoShift,
                d.IsNoBreak,
                d.EmployeeId,
                EmployeeName = $"{d.Employee?.LastName}, {d.Employee?.FirstName}",
                d.DepartmentId,
                DepartmentName = d.Department?.DepartmentName,
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
            DateTime? shiftStart = shift.ShiftStart;
            DateTime? shiftEnd = shift.ShiftEnd;
            DateTime? breakStart = shift.BreakStart;
            DateTime? breakEnd = shift.BreakEnd;

            foreach (var item in employeeShift)
            {
                if (item.IsAssigned == true && item.AssignedShiftId != 0)
                {
                    EmployeeShift shiftAssignment = new EmployeeShift
                    {
                        AssignedShiftId = item.AssignedShiftId,
                        ShiftId = shiftId,
                        EmployeeId = item.EmployeeId,
                        DepartmentId = item.DepartmentId,
                        ShiftDate = DateTime.Now,
                        ShiftStart = shiftStart,
                        ShiftEnd = shiftEnd,
                        BreakEnd = item.IsNoBreak ? null : breakEnd,
                        BreakStart = item.IsNoBreak ? null : breakStart,
                        IsFlexibleBreak = item.IsFlexibleBreak,
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
                        DepartmentId = item.DepartmentId,
                        ShiftDate = DateTime.Now,
                        ShiftStart = shiftStart,
                        ShiftEnd = shiftEnd,
                        BreakEnd = item.IsNoBreak ? null : breakEnd,
                        BreakStart = item.IsNoBreak ? null : breakStart,
                        IsFlexibleShift = item.IsFlexibleShift,
                        IsFlexibleBreak = item.IsFlexibleBreak,
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
