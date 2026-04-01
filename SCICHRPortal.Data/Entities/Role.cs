namespace SCICHRPortal.Data.Entities
{
    public class Role : BaseEntity
    {
        public int RoleId { get; set; }
        public string? Name { get; set; }

        public string? Description { get; set; }
    }
}
