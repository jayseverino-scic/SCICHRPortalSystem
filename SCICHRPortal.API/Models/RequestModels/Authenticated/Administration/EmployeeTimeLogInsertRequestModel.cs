using System.ComponentModel.DataAnnotations;

namespace SCICHRPortal.API.Models.RequestModels.Authenticated.Administration
{
    public class EmployeeTimeLogInsertRequestModel
    {
        [Required(ErrorMessage ="Employee is required.")]
        public int EmployeeId { get; set; }
        public bool IsOB { get; set; }
        public DateTime? DateIn { get; set; }
        public DateTime? DateOut { get; set; }
        public DateTime? TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public string? ProjectTimeIn { get; set; }
        public string? ProjectTimeOut { get; set; }
        public string? DeviceTimeIn { get; set; }
        public string? DeviceTimeOut { get; set; }
    }
}
