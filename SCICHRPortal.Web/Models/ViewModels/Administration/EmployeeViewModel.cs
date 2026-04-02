using System.ComponentModel.DataAnnotations;

namespace SCICHRPortal.Web.Models.ViewModels.Administration
{
    public class EmployeeViewModel
    {
        [MaxLength(60)]
        [Required(ErrorMessage = "Last Name is required.")]
        public string? LastName { get; set; }

        [MaxLength(60)]
        [Required(ErrorMessage = "First Name is required.")]
        public string? FirstName { get; set; }

        [MaxLength(60)]
        [Required(ErrorMessage = "Midle Name is required.")]
        public string? MiddleName { get; set; }

        [MaxLength(60)]
        public string? Suffix { get; set; }

        [MaxLength(60)]
        [Required(ErrorMessage = "Employee No. is required.")]
        public string? EmployeeNo { get; set; }

        [MaxLength(2000)]
        [Required(ErrorMessage = "Address is required.")]
        public string? Address { get; set; }

        [MaxLength(100)]
        [Required(ErrorMessage = "Email is required.")]
        public string? Email { get; set; }

        [MaxLength(100)]
        [Required(ErrorMessage = "Contact Number is required.")]
        public string? ContactNumber { get; set; }

        public int? DepartmentId { get; set; }
        public int? PositionId { get; set; }
    }
}
