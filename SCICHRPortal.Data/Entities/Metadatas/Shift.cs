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
        public DateTime? MondayShiftStart { get; set; }
        public DateTime? MondayShiftEnd { get; set; }
        public DateTime? TuesdayShiftStart { get; set; }
        public DateTime? TuesdayShiftEnd { get; set; }
        public DateTime? WednesdayShiftStart { get; set; }
        public DateTime? WednesdayShiftEnd { get; set; }
        public DateTime? ThursdayShiftStart { get; set; }
        public DateTime? ThursdayShiftEnd { get; set; }
        public DateTime? FridayShiftStart { get; set; }
        public DateTime? FridayShiftEnd { get; set; }
        public DateTime? SaturdayShiftStart { get; set; }
        public DateTime? SaturdayShiftEnd { get; set; }
        public DateTime? SundayShiftStart { get; set; }
        public DateTime? SundayShiftEnd { get; set; }
        public int ShiftLateMinuteGracePeriod { get; set; }
        public int ShiftLateTotalMinuteLimit { get; set; }
        public int NoTimeLogCountLimit { get; set; }
        public int NoLeaveAbsentCountLimit { get; set; }
        public string? RestDays { get; set; }
    }
}
