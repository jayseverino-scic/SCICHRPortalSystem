using System.ComponentModel.DataAnnotations;

namespace SCICHRPortal.Web.Models.ViewModels.Administration
{
    public class DepartmentViewModel
    {
        [MaxLength(10)]
        [Required(ErrorMessage = "Department Code is required.")]
        public string? DeptCode { get; set; }
        [MaxLength(50)]
        [Required(ErrorMessage = "Department Name is required.")]
        public string? DepartmentName { get; set; }
    }
}
