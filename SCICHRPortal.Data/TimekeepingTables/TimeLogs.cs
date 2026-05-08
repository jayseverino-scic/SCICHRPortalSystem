using System;
using System.Collections.Generic;
using System.Text;

namespace SCICHRPortal.Data.TimekeepingTables
{
    public class TimeLogs
    {
        public Guid Id { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime RecordDate {  get; set; }
        public DateTime TimeLogStamp { get; set; }
        public string? LogType { get; set; }
        public string? AccessNumber { get; set; }
        public string? DeviceSerialNumber { get; set; }
        public string? VerifyMode { get; set; }
        public string? CheckSum { get; set; }
        public string? Location {  get; set; }
    }
}
