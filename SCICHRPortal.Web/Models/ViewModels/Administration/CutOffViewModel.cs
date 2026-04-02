using System.ComponentModel.DataAnnotations;

namespace SCICHRPortal.Web.Models.ViewModels.Administration
{
    public class CutOffViewModel
    {
        public int CutOffId { get; set; }
        [Required(ErrorMessage ="Start Date is required.")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage ="End Date is required.")]
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
