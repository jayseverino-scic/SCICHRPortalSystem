using System.ComponentModel.DataAnnotations;

namespace SCICHRPortal.Web.Models.ViewModels.Administration
{
    public class DeviceViewModel
    {
        [MaxLength(255)]
        [Required(ErrorMessage ="Device Name is required.")]
        public string? Name { get; set; }
        [MaxLength(255)]
        [Required(ErrorMessage ="Serial Number is required.")]
        public string? SerialNumber { get; set; }
    }
}
