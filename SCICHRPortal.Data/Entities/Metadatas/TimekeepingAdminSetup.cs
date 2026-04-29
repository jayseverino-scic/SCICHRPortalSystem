using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCICHRPortal.Data.Entities.Metadatas
{
    public class TimekeepingAdminSetup : BaseEntity
    {
        public int SetupId { get; set; }
        public int ShiftLateMinuteGracePeriod { get; set; }
        public int BreakLateMinuteGracePeriod { get; set; }
        public int ShiftLateTotalMinuteLimit {  get; set; }
        public int BreakLateTotalMinuteLimit { get; set; }
        public int NoTimeLogCountLimit { get; set; }
        public int NoLeaveAbsentCountLimit { get; set; }
        public string? RestDays { get; set; }
    }
}
