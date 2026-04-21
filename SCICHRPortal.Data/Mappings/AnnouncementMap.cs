using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCICHRPortal.Data.Entities;

namespace SCICHRPortal.Data.Mappings
{
    public class AnnouncementMap
    {
        public AnnouncementMap(EntityTypeBuilder<Announcement> entityBuilder)
        {
            entityBuilder.HasKey(t => t.AnnouncementId);

            entityBuilder.Property(t => t.Title)
                .HasMaxLength(100)
                .IsRequired();
            entityBuilder.Property(t => t.CreatedBy).HasMaxLength(120);
            entityBuilder.Property(t => t.UpdatedBy).HasMaxLength(120);

            entityBuilder.Property(t => t.AnnouncementForm)
                .IsRequired();
        }
    }
}
