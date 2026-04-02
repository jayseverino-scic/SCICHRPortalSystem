using System.ComponentModel.DataAnnotations;

namespace SCICHRPortal.Web.Models.ViewModels.Administration
{
    public class LeaveTypeViewModel
    {
        public int LeaveTypeId { get; set; }
        [Required(ErrorMessage ="Leave Description is required.")]
        public string? LeaveDescription { get; set; }
        [Required(ErrorMessage ="Allowed Days is required.")]
        public int AllowedDays { get; set; }
        public bool IsPaid { get; set; }
    }
}
