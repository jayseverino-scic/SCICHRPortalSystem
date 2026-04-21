namespace SCICHRPortal.Data.Entities
{
    public class Announcement : BaseEntity
    {
        public int AnnouncementId { get; set; }
        public string? Title { get; set; }
        public string? AnnouncementForm { get; set; }
    }
}
