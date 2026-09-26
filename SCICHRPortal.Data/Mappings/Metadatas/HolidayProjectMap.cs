using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCICHRPortal.Data.Entities.Metadatas;

namespace SCICHRPortal.Data.Mappings.Metadatas;

public class HolidayProjectMap
{
    public HolidayProjectMap(EntityTypeBuilder<HolidayProject> builder)
    {
        builder.HasKey(link => new { link.HolidayId, link.ProjectId });
        builder.HasQueryFilter(link => !link.Deleted);
    }
}
