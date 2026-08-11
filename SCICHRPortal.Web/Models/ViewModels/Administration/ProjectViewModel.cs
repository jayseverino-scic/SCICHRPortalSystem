using System.ComponentModel.DataAnnotations;

namespace SCICHRPortal.Web.Models.ViewModels.Administration
{
    public class ProjectViewModel
    {
        [MaxLength(100)]
        [Required(ErrorMessage ="Project Code is required.")]
        public string? Code { get; set; }
        [MaxLength(255)]
        [Required(ErrorMessage ="Project name is required.")]
        public string? Name { get; set; }
    }
}
