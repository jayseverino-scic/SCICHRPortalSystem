using System.ComponentModel.DataAnnotations;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.XscribeTables;

namespace SCICHRPortal.API.Models.RequestModels.Authenticated.Administration
{
    public class EmployeeTimeLogInsertRequestModel
    {
        [Required(ErrorMessage ="Employee is required.")]
        public int EmployeeId { get; set; }
        public DateTime? DateIn { get; set; }
        public DateTime? DateOut { get; set; }
        public DateTime? TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public DateTime? ShiftStart { get; set; }
        public DateTime? ShiftEnd { get; set; }

        public String? SystemRemarks { get; set; }
        public XEmployee? Employee { get; set; }
    }
}
