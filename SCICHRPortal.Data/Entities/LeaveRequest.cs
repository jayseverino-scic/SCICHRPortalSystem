using SCICHRPortal.Data.Entities.Metadatas;

namespace SCICHRPortal.Data.Entities
{
    public class LeaveRequest : BaseEntity
    {
        public int LeaveRequestId { get; set; }
        public int EmployeeId { get; set; }
        public int LeaveTypeId { get; set; }
        public DateTime DateRequest { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public double RequestDays { get; set; }
        public string? LeaveReason { get; set; }

        public Employee? Employee { get; set; }
        public LeaveType? LeaveType { get; set; }
    }
}
