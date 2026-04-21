using System.ComponentModel.DataAnnotations;

namespace SCICHRPortal.Web.Models.ViewModels.Admin
{
    public class AnnouncementViewModel
    {
        [MaxLength(4000)]
        [Required(ErrorMessage = "Announcement is required.")]
        public string? AnnouncementForm { get; set; }
        public DateTime? CreatedAt { get; set; }

        [MaxLength(100)]
        [Required(ErrorMessage = "Title is required.")]
        public string? Title { get; set; }
    }
}
