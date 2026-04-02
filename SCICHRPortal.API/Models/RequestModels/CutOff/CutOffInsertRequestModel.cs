using System.ComponentModel.DataAnnotations;
using SCICHRPortal.Data.Entities.Metadatas;

namespace SCICHRPortal.API.Models.RequestModels.Authenticated.CutOff
{
    public class CutOffInsertRequestModel
    {
        [Required(ErrorMessage ="Start Date is required.")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage ="End Date is required.")]
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
