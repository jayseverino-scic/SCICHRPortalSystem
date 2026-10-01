using System.IO.Compression;

namespace SCICHRPortal.API.Storage;

public static class TimeLogAttachmentValidation
{
    public const long MaxBytes = 10 * 1024 * 1024;
    public static async Task<string?> ContentTypeAsync(string filename, Stream stream)
    {
        var header = new byte[8];
        var count = await stream.ReadAsync(header);
        stream.Position = 0;
        var extension = Path.GetExtension(filename).ToLowerInvariant();
        if (extension == ".pdf" && count >= 5 && header.AsSpan(0, 5).SequenceEqual("%PDF-"u8)) return "application/pdf";
        if (extension is ".jpg" or ".jpeg" && count >= 3 && header[0] == 255 && header[1] == 216 && header[2] == 255) return "image/jpeg";
        if (extension == ".png" && count == 8 && header.AsSpan().SequenceEqual(new byte[] {137,80,78,71,13,10,26,10})) return "image/png";
        if (extension == ".xls" && count == 8 && header.AsSpan().SequenceEqual(new byte[] {208,207,17,224,161,177,26,225}))
            return "application/vnd.ms-excel";
        if (extension is ".docx" or ".xlsx")
        {
            try
            {
                using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
                if (archive.GetEntry("[Content_Types].xml") is null) return null;
                if (extension == ".docx" && archive.GetEntry("word/document.xml") is not null)
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                if (extension == ".xlsx" && archive.GetEntry("xl/workbook.xml") is not null)
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }
            catch (InvalidDataException) { return null; }
            finally { stream.Position = 0; }
        }
        return null;
    }
}
