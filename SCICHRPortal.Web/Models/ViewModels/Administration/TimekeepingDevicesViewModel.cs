using System.ComponentModel.DataAnnotations;

namespace SCICHRPortal.Web.Models.ViewModels.Administration
{
    public class TimekeepingDevicesViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="This field is required.")]
        public string? Name { get; set; }
        [Required(ErrorMessage ="This field is required.")]
        public string? SerialNumber { get; set; }
        public string? Source { get; set; }
    }
}
