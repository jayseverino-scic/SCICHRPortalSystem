using System;
using System.Collections.Generic;
using System.Text;

namespace SCICHRPortal.Data.TimekeepingTables
{
    public class STimeLogs
    {
        public Guid Id { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime RecordDate {  get; set; }
        public DateTimeOffset TimeLogStamp { get; set; }
        public string? LogType { get; set; }
        public string? AccessNumber { get; set; }
        public string? DeviceSerialNumber { get; set; }
        public string? VerifyMode { get; set; }
        public string? CheckSum { get; set; }
        public string? Location {  get; set; }
    }
}
