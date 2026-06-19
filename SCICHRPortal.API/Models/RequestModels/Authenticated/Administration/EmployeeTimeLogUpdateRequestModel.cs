using System.ComponentModel.DataAnnotations;
using SCICHRPortal.Data.Entities.Metadatas;

namespace SCICHRPortal.API.Models.RequestModels.Authenticated.Administration
{
    public class EmployeeTimeLogUpdateRequestModel
    {
        public int TimeLogId { get; set; }
        [Required(ErrorMessage = "Employee is required.")]
        public int EmployeeId { get; set; }
        public DateTime? DateIn { get; set; }
        public DateTime? DateOut { get; set; }
        public DateTime? TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public DateTime? ShiftStart { get; set; }
        public DateTime? ShiftEnd { get; set; }
        public String? SystemRemarks { get; set; }
        public Employee? Employee { get; set; }
    }
}
