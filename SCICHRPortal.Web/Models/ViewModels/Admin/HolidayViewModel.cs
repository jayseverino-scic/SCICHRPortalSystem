using System.ComponentModel.DataAnnotations;

namespace SCICHRPortal.Web.Models.ViewModels.Admin
{
    public class HolidayViewModel
    {
        [MaxLength(256)]
        [Required(ErrorMessage ="Holiday Name is required.")]
        public string? HolidayName { get; set; }
        [MaxLength(100)]
        [Required(ErrorMessage ="Holiday Type is required.")]
        public string? HolidayType { get; set; }
        [Required(ErrorMessage ="Holiday Date is required.")]
        public DateTime? HolidayDate { get; set; }

    }
}
