using System.ComponentModel.DataAnnotations;

namespace SCICHRPortal.Web.Models.ViewModels.Administration
{
    public class OfficialBusinessViewModel
    {
        public int OfficialBusinessId { get; set; }
        [Required, Range(1, int.MaxValue)]
        public int? EmployeeId { get; set; }
        [Display(Name = "Employee No.")]
        public string? EmployeeNo { get; set; }
        [Display(Name = "Employee Name")]
        public string? EmployeeName { get; set; }
        [Required, DataType(DataType.Date), Display(Name = "From Date OB")]
        public DateOnly? FromDate { get; set; }
        [Required, DataType(DataType.Date), Display(Name = "To Date OB")]
        public DateOnly? ToDate { get; set; }
        [Required, DataType(DataType.Time), Display(Name = "From Time OB")]
        public TimeOnly? FromTime { get; set; }
        [Required, DataType(DataType.Time), Display(Name = "To Time OB")]
        public TimeOnly? ToTime { get; set; }
    }
}
