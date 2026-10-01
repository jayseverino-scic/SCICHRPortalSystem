using System.ComponentModel.DataAnnotations;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.XscribeTables;

namespace SCICHRPortal.API.Models.RequestModels.Authenticated.Administration
{
    public class EmployeeTimeLogUpdateRequestModel : EmployeeTimeLogInsertRequestModel
    {
        [Range(1, int.MaxValue)]
        public int TimeLogId { get; set; }
        [Required, Range(0, long.MaxValue)]
        public long? Version { get; set; }
    }
}
