using System;
using System.Collections.Generic;
using System.Text;

namespace SCICHRPortal.Data.Entities.Metadatas
{
    public class BulkImportResult
    {
        public int TotalInserted { get; set; }
        public int TotalFailed { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public int BatchCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
