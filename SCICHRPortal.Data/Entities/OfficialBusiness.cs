using SCICHRPortal.Data.Entities.Metadatas;

namespace SCICHRPortal.Data.Entities
{
    public class OfficialBusiness : BaseEntity
    {
        public int OfficialBusinessId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public Employee? Employee { get; set; }
    }
}
