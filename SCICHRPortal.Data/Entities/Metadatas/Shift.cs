using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCICHRPortal.Data.Entities.Metadatas
{
    public class Shift : BaseEntity
    {
        public int ShiftId { get; set; }
        public string? ShiftName { get; set; }
        public DateTime? ShiftStart { get; set; }
        public DateTime? ShiftEnd { get; set; }
        public DateTime? BreakStart { get; set; }
        public DateTime? BreakEnd { get; set; }
        public int ShiftLateMinuteGracePeriod { get; set; }
        public int BreakLateMinuteGracePeriod { get; set; }
        public int ShiftLateTotalMinuteLimit { get; set; }
        public int BreakLateTotalMinuteLimit { get; set; }
        public int NoTimeLogCountLimit { get; set; }
        public int NoLeaveAbsentCountLimit { get; set; }
        public string? RestDays { get; set; }
    }
}
