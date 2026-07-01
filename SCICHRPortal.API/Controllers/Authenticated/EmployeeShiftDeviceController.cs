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
    public class EmployeeShiftDeviceController : ControllerBase
    {
        private IEmployeeShiftDeviceService EmployeeShiftDeviceService { get; }
        private IXEmployeeService EmployeeService { get; }
        private IShiftService ShiftService { get; }
        private IXDepartmentService DepartmentService { get; }
        public EmployeeShiftDeviceController(IEmployeeShiftDeviceService employeeShiftService, IXEmployeeService employeeService, IShiftService shiftService, IXDepartmentService departmentService)
        {
            EmployeeShiftDeviceService = employeeShiftService;
            EmployeeService = employeeService;
            ShiftService = shiftService;
            DepartmentService = departmentService;
        }

        [Authorize] 
        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var employeeShifts = await EmployeeShiftDeviceService.GetAllAsync();
            return Ok(employeeShifts);
        }

        [Authorize]
        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber, int pageSize, string? searchKeyword)
        {
            var tuple = await EmployeeShiftDeviceService.FilterAsync(pageNumber, pageSize, searchKeyword!);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;
            IEnumerable<EmployeeShiftDevice> employeeShiftsList = await EmployeeShiftDeviceService.GetAllAsync();
            List<EmployeeShiftDevice> employeeShifts = employeeShiftsList.ToList();
            IEnumerable<XEmployee> employees = await EmployeeService.GetAllAsync();
            IEnumerable<XDepartment> departments = await DepartmentService.GetAllAsync();
            IEnumerable<Shift> shifts = await ShiftService.GetAllAsync();

            List<XEmployee> mergedList = employees
            .GroupJoin(
                tuple.Item1, left => left.Id, right => right.EmployeeId,
                (x, y) => new { Left = x, Rights = y }
            )
            .SelectMany(
                x => x.Rights.DefaultIfEmpty(),
                (x, y) => new XEmployee
                {
                    Id = x.Left.Id,
                    Department_Id = x.Left.Department_Id,
                    Last_Name = x.Left.Last_Name,
                    First_Name = x.Left.First_Name
                }
            ).ToList();

            if (mergedList != null)
            {
                foreach(XEmployee employee in mergedList)
                {
                    EmployeeShiftDevice employeeShift = new EmployeeShiftDevice
                    {
                        AssignedShiftId = 0,
                        ShiftId = 0,
                        EmployeeId = employee.Id,
                        //DepartmentId = (int)employee.DepartmentId,
                        Employee = employee,
                        //Department = departments.Where(d => d.DepartmentId == employee.DepartmentId).SingleOrDefault()
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
                EmployeeName = $"{d.Employee!.Last_Name}, {d.Employee!.First_Name}",
                d.Devicename,
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
        public async Task<IActionResult> EmployeeShiftDeviceFilterAsync(string? deviceName, int shiftId, string filterType)
        {           
            IEnumerable<EmployeeShiftDevice>? employeeShiftsList = null;
            List<EmployeeShiftDeviceUpdateRequestModel> listToDisplay = new List<EmployeeShiftDeviceUpdateRequestModel>();
            IEnumerable<XEmployee>? employees = null;
            if (filterType == "Assigned")
            {
                employeeShiftsList = await EmployeeShiftDeviceService.EmployeeShiftDeviceFilter(deviceName, shiftId);
                foreach(EmployeeShiftDevice employeeShift in employeeShiftsList.ToList())
                {
                    EmployeeShiftDeviceUpdateRequestModel employeeShiftUpdateRequestModel = new EmployeeShiftDeviceUpdateRequestModel
                    {
                        AssignedShiftId = employeeShift.AssignedShiftId,
                        ShiftId = employeeShift.ShiftId,
                        EmployeeId = employeeShift.EmployeeId,
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
                        DeviceName = employeeShift.Devicename,
                        Shift = employeeShift.Shift
                    };
                    listToDisplay.Add(employeeShiftUpdateRequestModel);
                }
            }
            else
            {
                employeeShiftsList = await EmployeeShiftDeviceService.EmployeeShiftDeviceFilter(deviceName, shiftId);

                int[] assignedIds = employeeShiftsList.Select(x => x.EmployeeId).ToArray();

                List<EmployeeShiftDevice> employeeShifts = new List<EmployeeShiftDevice>();
                IEnumerable<XDepartment> departmentList = await DepartmentService.GetAllAsync();
                List<XDepartment> departments = departmentList.ToList();
                employees = await EmployeeService.GetAllAsync();
                employees = employees.Where(item => !assignedIds.Any(x => x == item.Id)).ToList();
                //if (deviceName != "")
                //{
                //    employees = employees.Where(e => e.DepartmentId == departmentId);
                //}
                List<XEmployee> mergedList = employees
                .GroupJoin(
                    employeeShifts, left => left.Id, right => right.EmployeeId,
                    (x, y) => new { Left = x, Rights = y }
                )
                .SelectMany(
                    x => x.Rights.DefaultIfEmpty(),
                    (x, y) => new XEmployee
                    {
                        Id = x.Left.Id,
                        Department_Id = x.Left.Department_Id,
                        Last_Name = x.Left.Last_Name,
                        First_Name = x.Left.First_Name
                    }
                ).ToList();

                if (mergedList != null)
                {
                    foreach (XEmployee employee in mergedList)
                    {
                        EmployeeShiftDevice employeeWithShift = await EmployeeShiftDeviceService.GetByEmployeeDevice(employee.Id);
                        if (employeeWithShift == null)
                        {
                            EmployeeShiftDevice employeeShift = new EmployeeShiftDevice
                            {
                                AssignedShiftId = 0,
                                ShiftId = 0,
                                EmployeeId = employee.Id,
                                //DepartmentId = (int)employee.DepartmentId!,
                                Employee = employee,
                                //Department = departments.Where(d => d.DepartmentId == employee.DepartmentId).SingleOrDefault(),
                            };
                            employeeShifts.Add(employeeShift);
                        }
                    }
                }
                if (filterType == "All")
                    employeeShifts.AddRange(employeeShiftsList);
                foreach (EmployeeShiftDevice employeeShift in employeeShifts.ToList())
                {
                    EmployeeShiftDeviceUpdateRequestModel employeeShiftUpdateRequestModel = new EmployeeShiftDeviceUpdateRequestModel
                    {
                        AssignedShiftId = employeeShift.AssignedShiftId,
                        ShiftId = employeeShift.ShiftId,
                        EmployeeId = employeeShift.EmployeeId,
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
                        DeviceName = employeeShift.Devicename,
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
                EmployeeName = $"{d.Employee?.Last_Name}, {d.Employee?.First_Name}",
                d.DeviceName,
                d.ShiftId,
                ShiftName = d.Shift?.ShiftName,
                d.IsAssigned 
            });

            return Ok(data);
        }

        [Authorize]
        [HttpPost()]
        public async Task<IActionResult> UpdateShiftAssignmentAsync(List<EmployeeShiftDeviceUpdateRequestModel> employeeShift, int shiftId)
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
                    EmployeeShiftDevice shiftAssignment = new EmployeeShiftDevice
                    {
                        AssignedShiftId = item.AssignedShiftId,
                        ShiftId = shiftId,
                        EmployeeId = item.EmployeeId,
                        Devicename = item.DeviceName,
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
                    await EmployeeShiftDeviceService.UpdateAsync(shiftAssignment);
                }
                if (item.IsAssigned == true && item.AssignedShiftId == 0)
                {
                    EmployeeShiftDevice shiftAssignment = new EmployeeShiftDevice
                    {
                        AssignedShiftId = item.AssignedShiftId,
                        ShiftId = shiftId,
                        EmployeeId = item.EmployeeId,
                        Devicename= item.DeviceName,
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
                    await EmployeeShiftDeviceService.InsertAsync(shiftAssignment);
                }
                if (item.IsAssigned == false && item.AssignedShiftId != 0)
                {
                    await EmployeeShiftDeviceService.DeleteAsync(item.AssignedShiftId);
                }
            }

            return StatusCode(201, employeeShift);
        }

        [Authorize]
        [HttpPut()]
        public async Task<IActionResult> UpdateAsync(EmployeeShiftDevice employeeShift)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");
            employeeShift.UpdatedAt = DateTime.UtcNow;
            employeeShift.UpdatedBy = "manuel";
            await EmployeeShiftDeviceService.UpdateAsync(employeeShift);

            return Ok();
        }

        [Authorize]
        [HttpDelete("{employeeShiftId}")]
        public async Task<IActionResult> RemoveAsync(int employeeShiftId)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var deleted = await EmployeeShiftDeviceService.DeleteAsync(employeeShiftId);

            if (!deleted)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }
    }
}
