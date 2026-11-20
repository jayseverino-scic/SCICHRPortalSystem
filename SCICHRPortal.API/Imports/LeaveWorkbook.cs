using System.Drawing;
using System.Globalization;
using System.IO.Compression;
using System.Xml;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SCICHRPortal.Data.DTOs;

namespace SCICHRPortal.API.Imports
{
    public sealed record LeaveWorkbookReadResult(IReadOnlyList<LeaveImportRow> Rows, string? Error = null);

    public static class LeaveWorkbook
    {
        public const int MaximumRows = 1000;
        public const long MaximumFileBytes = 10 * 1024 * 1024;
        private static readonly string[] Headers = ["Employee No.", "Employee Name", "From Date", "From Time", "To Date", "To Time", "Type of Leave"];

        public static LeaveWorkbookReadResult Read(Stream stream)
        {
            if (!stream.CanSeek || stream.Length == 0 || stream.Length > MaximumFileBytes)
                return new([], "Select a non-empty .xlsx file no larger than 10 MiB.");
            try
            {
                stream.Position = 0;
                using var normalized = new MemoryStream();
                using (var safeArchive = new ZipArchive(normalized, ZipArchiveMode.Create, leaveOpen: true))
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true))
                {
                    const long expandedLimit = 50 * 1024 * 1024;
                    long expanded = 0;
                    var buffer = new byte[65536];
                    var names = new HashSet<string>(StringComparer.Ordinal);
                    if (archive.Entries.Count > 1000 || archive.GetEntry("xl/workbook.xml") == null)
                        return new([], "Select a valid .xlsx workbook.");
                    foreach (var entry in archive.Entries)
                    {
                        if (!names.Add(entry.FullName)) return new([], "The workbook contains duplicate ZIP entries. Save it again as .xlsx.");
                        if (entry.Length > expandedLimit - expanded)
                            return new([], "The workbook expands beyond the 50 MiB limit. Remove unused sheets and content.");
                        // Repackage only bounded bytes supplied by .NET's reader. EPPlus uses
                        // a different ZIP reader, so never pass unchecked compressed input on.
                        using var content = entry.Open();
                        var target = safeArchive.CreateEntry(entry.FullName, System.IO.Compression.CompressionLevel.Fastest);
                        long entryBytes = 0;
                        using (var destination = target.Open())
                        {
                            int read;
                            while ((read = content.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                if (read > expandedLimit - expanded)
                                    return new([], "The workbook expands beyond the 50 MiB limit. Remove unused sheets and content.");
                                destination.Write(buffer, 0, read);
                                expanded += read;
                                entryBytes += read;
                            }
                        }
                        if (entryBytes != entry.Length || target.Crc32 != entry.Crc32)
                            return new([], "The workbook ZIP contents are inconsistent. Save it again as .xlsx.");
                    }
                }
                normalized.Position = 0;
                using var package = new ExcelPackage(normalized);
                var sheet = package.Workbook.Worksheets.FirstOrDefault();
                if (sheet == null) return new([], "The workbook must contain a Leave worksheet.");
                for (var column = 1; column <= Headers.Length; column++)
                    if (!string.IsNullOrEmpty(sheet.Cells[1, column].Formula) ||
                        !string.Equals(sheet.Cells[1, column].Text.Trim(), Headers[column - 1], StringComparison.OrdinalIgnoreCase))
                        return new([], "The first worksheet must use the column headers and order from the Leave template.");
                var lastRow = 1;
                // Enumerate stored values/formulas, not the rectangle up to a distant styled
                // blank cell. DimensionByValue can omit trailing formulas without cached values.
                foreach (var cell in sheet.Cells)
                    if (cell.Start.Column <= Headers.Length && (!string.IsNullOrEmpty(cell.Formula) ||
                        cell.Value != null && !(cell.Value is string text && string.IsNullOrWhiteSpace(text))))
                        lastRow = Math.Max(lastRow, cell.Start.Row);
                if (lastRow > MaximumRows + 1)
                    return new([], "Use a maximum of 1,000 data rows after the header. Remove extra data rows before importing.");
                var rows = new List<LeaveImportRow>();
                for (var number = 2; number <= lastRow; number++)
                {
                    var cells = Enumerable.Range(1, Headers.Length).Select(column => sheet.Cells[number, column]).ToArray();
                    if (cells.All(cell => string.IsNullOrEmpty(cell.Formula) &&
                        (cell.Value == null || cell.Value is string text && string.IsNullOrWhiteSpace(text)))) continue;
                    var employeeNo = cells[0].Text.Trim();
                    var leaveType = cells[6].Text.Trim();
                    string? error = null;
                    if (cells.Any(cell => !string.IsNullOrEmpty(cell.Formula))) error = "Formulas are not supported. Paste values before importing.";
                    else if (employeeNo.Length is 0 or > 20) error = "Employee No. is required and must be at most 20 characters.";
                    else if (leaveType.Length is 0 or > 256) error = "Type of Leave is required and must be at most 256 characters.";
                    var fromDate = ReadDate(cells[2].Value, package.Workbook.Date1904);
                    var fromTime = ReadTime(cells[3].Value);
                    var toDate = ReadDate(cells[4].Value, package.Workbook.Date1904);
                    var toTime = ReadTime(cells[5].Value);
                    if (error == null && (fromDate == null || toDate == null))
                        error = "Enter valid From and To dates as Excel dates or yyyy-MM-dd text.";
                    if (error == null && (fromTime == null || toTime == null))
                        error = "Enter valid From and To times as Excel times, HH:mm, or h:mm AM/PM.";
                    var reportedNumber = employeeNo.Length > 20 ? employeeNo[..20] + "…" : employeeNo;
                    rows.Add(new(number, reportedNumber, fromDate, fromTime, toDate, toTime, leaveType, error));
                }
                return rows.Count == 0 ? new([], "The workbook contains no Leave records.") : new(rows);
            }
            catch (Exception exception) when (exception is InvalidDataException or IOException or ArgumentException or XmlException or InvalidOperationException)
            {
                return new([], "The workbook could not be read. Upload an unencrypted .xlsx file using the template.");
            }
        }

        private static DateOnly? ReadDate(object? value, bool date1904)
        {
            if (value is DateTime date) return DateOnly.FromDateTime(date);
            if (value is double or int or decimal or float)
            {
                try { return DateOnly.FromDateTime(DateTime.FromOADate(Convert.ToDouble(value, CultureInfo.InvariantCulture) + (date1904 ? 1462 : 0))); }
                catch (ArgumentException) { return null; }
            }
            return value is string text && DateOnly.TryParseExact(text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                ? parsed : null;
        }

        private static TimeOnly? ReadTime(object? value)
        {
            if (value is DateTime date) return TimeOnly.FromDateTime(date);
            if (value is double or int or decimal or float)
            {
                var number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                if (!double.IsFinite(number) || number < 0 || number >= 1) return null;
                // Allow binary arithmetic noise, but do not round actual seconds into a valid minute.
                var minutes = number * 1440;
                var rounded = Math.Round(minutes, MidpointRounding.AwayFromZero);
                return rounded < 1440 && Math.Abs(minutes - rounded) <= 0.0000001
                    ? TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(rounded)) : null;
            }
            return value is string text && TimeOnly.TryParseExact(text.Trim(), ["H:mm", "HH:mm", "h:mm tt", "hh:mm tt", "HH:mm:ss"],
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : null;
        }

        public static async Task<byte[]> CreateTemplateAsync(IReadOnlyList<LeaveTypeLookupItem> leaveTypes, CancellationToken cancellationToken)
        {
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Leave");
            for (var column = 1; column <= Headers.Length; column++) sheet.Cells[1, column].Value = Headers[column - 1];
            sheet.Cells[1, 1, 1, Headers.Length].Style.Font.Bold = true;
            sheet.Cells[1, 1, 1, Headers.Length].Style.Font.Color.SetColor(Color.White);
            sheet.Cells[1, 1, 1, Headers.Length].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[1, 1, 1, Headers.Length].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(46, 125, 50));
            sheet.Cells[1, 1, 1, Headers.Length].AutoFilter = true;
            sheet.View.FreezePanes(2, 1);
            sheet.Column(1).Width = 22;
            sheet.Column(2).Width = 30;
            sheet.Column(3).Width = sheet.Column(5).Width = 17;
            sheet.Column(4).Width = sheet.Column(6).Width = 15;
            sheet.Column(7).Width = 32;
            sheet.Cells[2, 1, MaximumRows + 1, 2].Style.Numberformat.Format = "@";
            sheet.Cells[2, 3, MaximumRows + 1, 3].Style.Numberformat.Format = "yyyy-mm-dd";
            sheet.Cells[2, 4, MaximumRows + 1, 4].Style.Numberformat.Format = "hh:mm";
            sheet.Cells[2, 5, MaximumRows + 1, 5].Style.Numberformat.Format = "yyyy-mm-dd";
            sheet.Cells[2, 6, MaximumRows + 1, 6].Style.Numberformat.Format = "hh:mm";
            sheet.Cells[2, 7, MaximumRows + 1, 7].Style.Numberformat.Format = "@";
            var typeSheet = package.Workbook.Worksheets.Add("Leave Types");
            typeSheet.Cells[1, 1].Value = "Type of Leave";
            typeSheet.Cells[1, 1].Style.Font.Bold = true;
            typeSheet.Column(1).Width = 42;
            for (var index = 0; index < leaveTypes.Count; index++)
            {
                var cell = typeSheet.Cells[index + 2, 1];
                cell.Style.Numberformat.Format = "@";
                cell.Value = leaveTypes[index].Description;
            }
            if (leaveTypes.Count > 0)
            {
                package.Workbook.Names.Add("LeaveTypeDescriptions", typeSheet.Cells[2, 1, leaveTypes.Count + 1, 1]);
                var validation = sheet.DataValidations.AddListValidation($"G2:G{MaximumRows + 1}");
                validation.Formula.ExcelFormula = "LeaveTypeDescriptions";
                validation.ShowErrorMessage = true;
                validation.Error = "Select a Type of Leave from the Leave Types sheet.";
            }
            var instructions = package.Workbook.Worksheets.Add("Instructions");
            var lines = new[]
            {
                "Fill the first worksheet, using rows 2 through 1001. Keep the headers and column order.",
                "Employee No. identifies the employee. Preserve leading zeros. Employee Name is optional and informational.",
                "Type of Leave must match one active description from the Leave Types sheet. Create missing types in Leave Type master data first.",
                "Use Excel date cells or yyyy-MM-dd text, and Excel time cells or HH:mm text. All periods are Manila local time.",
                "The end must follow the start. Whole-minute times, overnight periods, and adjacent periods are supported.",
                "Valid rows are imported independently. Invalid, ambiguous, duplicate, and overlapping rows are rejected with row numbers.",
                "Correct and upload only rejected rows. Existing records are never overwritten. Do not use formulas."
            };
            instructions.Column(1).Width = 120;
            for (var index = 0; index < lines.Length; index++) instructions.Cells[index + 1, 1].Value = lines[index];
            return await package.GetAsByteArrayAsync(cancellationToken);
        }
    }
}
