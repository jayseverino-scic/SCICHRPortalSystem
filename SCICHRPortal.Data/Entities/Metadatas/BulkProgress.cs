using System;
using System.Collections.Generic;
using System.Text;

namespace SCICHRPortal.Data.Entities.Metadatas
{
    public class BulkProgress
    {
        public int ProcessedRows { get; set; }
        public int TotalRows { get; set; }
        public int PercentageComplete => TotalRows > 0 ? (int)((double)ProcessedRows / TotalRows * 100) : 0;
        public string Status { get; set; } = string.Empty;
    }
}
