using System.ComponentModel.DataAnnotations;

namespace SCICHRPortal.Web.Models.ViewModels.Administration
{
    public class LeaveViewModel
    {
        [Required]
        [Range(1, int.MaxValue)]
        [Display(Name = "Type of Leave")]
        public int? LeaveTypeId { get; set; }

        [Display(Name = "Employee No.")]
        public string? EmployeeNo { get; set; }

        [Display(Name = "Employee Name")]
        public string? EmployeeName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "From Date")]
        public DateOnly? FromDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "To Date")]
        public DateOnly? ToDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "From Time")]
        public TimeOnly? FromTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "To Time")]
        public TimeOnly? ToTime { get; set; }
    }
}
