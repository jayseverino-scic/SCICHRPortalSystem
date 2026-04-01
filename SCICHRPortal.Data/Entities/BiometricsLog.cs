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
        public int TMNo { get; set; }
        public string? EmployeeNo { get; set; }
        public string? EmployeeName { get; set; }
        public int GMNo { get; set; }
        public string? Mode { get; set; }
        public string? InOut { get; set; }
        public int AntiPass { get; set; }
        public int ProxyWork { get; set; }
        public DateTime DateTimeLog { get; set; }
    }
}
