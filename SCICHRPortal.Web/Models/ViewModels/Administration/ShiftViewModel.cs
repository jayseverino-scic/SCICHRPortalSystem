using System.ComponentModel.DataAnnotations;

namespace SCICHRPortal.Web.Models.ViewModels.Administration
{
    public class ShiftViewModel
    {
        [MaxLength(100)]
        [Required(ErrorMessage = "Shift Name required.")]
        public string? ShiftName { get; set; }
        [Required(ErrorMessage = "Shift Start is required.")]
        public DateTime? ShiftStart { get; set; }
        [Required(ErrorMessage = "Shift End is required.")]
        public DateTime? ShiftEnd { get; set; }
        public DateTime? BreakStart { get; set; }
        public DateTime? BreakEnd { get; set; }
    }
}
