using System.ComponentModel.DataAnnotations;

namespace SCICHRPortal.Web.Models.ViewModels.Administration
{
    public class PositionViewModel
    {
        public int PositionId { get; set; }
        [Required(ErrorMessage ="Position name is required.")]
        public string? PositionName { get; set; }
    }
}
