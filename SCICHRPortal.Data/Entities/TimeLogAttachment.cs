namespace SCICHRPortal.Data.Entities;

public class TimeLogAttachment
{
    public int TimeLogAttachmentId { get; set; }
    public int TimeLogId { get; set; }
    public string Provider { get; set; } = "Local";
    public string StorageKey { get; set; } = "";
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long Size { get; set; }
    public string UploadedBy { get; set; } = "";
    public DateTime UploadedAt { get; set; }
}
