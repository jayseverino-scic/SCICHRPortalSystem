using System.ComponentModel.DataAnnotations;
using SCICHRPortal.Data.Entities.Metadatas;

namespace SCICHRPortal.API.Models.RequestModels.Authenticated.Administration
{
    public class EmployeeTimeLogInsertRequestModel
    {
        [Required(ErrorMessage ="Employee is required.")]
        public int EmployeeId { get; set; }
        public DateTime? DateIn { get; set; }
        public DateTime? DateOut { get; set; }
        public DateTime? TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        [Required(ErrorMessage ="Shift start is required.")]
        public DateTime ShiftStart { get; set; }
        [Required(ErrorMessage ="Shift end is required.")]
        public DateTime ShiftEnd { get; set; }
        public DateTime? BreakStart { get; set; }
        public DateTime? BreakEnd { get; set; }
        public Employee? Employee { get; set; }
    }
}
