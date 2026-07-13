using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Data.XscribeTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCICHRPortal.Data.Entities
{
    public class BiometricsLog : BaseEntity
    {
        public int BiometricsLogId { get; set; }
        public string? PersonnelId { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public DateTime? Date {  get; set; }
        public DateTime? Time {  get; set; }
        public string? LogType { get; set; }
        public string? DeviceName { get; set; }
        public string? ProjectName { get; set; }
        public XCompany_Branch? XCompany_Branch { get; set; }
        public SZKDevices? SZKDevices { get; set; }
    }
}
