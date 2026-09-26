using System.Text.Json.Serialization;

namespace SCICHRPortal.Data.Entities.Metadatas;

public class HolidayProject : BaseEntity
{
    public int HolidayId { get; set; }
    public int ProjectId { get; set; }
    [JsonIgnore]
    public Holiday Holiday { get; set; } = null!;
    public Project? Project { get; set; }
}
