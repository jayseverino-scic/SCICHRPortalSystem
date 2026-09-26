using SCICHRPortal.Data.Enums;
namespace SCICHRPortal.Data.Entities.Metadatas
{
    public class Holiday : BaseEntity
    {
        public int HolidayId { get; set; }
        public string? HolidayName { get; set; }
        public DateTime? HolidayDate { get; set; }
        public int? HolidayType { get; set; }
        // No assignments means All Projects. Assignment history is stored internally.
        public ICollection<Project> Projects { get; set; } = new List<Project>();

        public HolidayType HolidayTypes { get; set; }
    }
}
