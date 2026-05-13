using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SCICHRPortal.Data.TimekeepingTables
{
    public class ZKDevices
    {
        public Guid Id { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public bool IsDeleted { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? IPAddress { get; set; }
        public string? SerialNumber { get; set; }
        public DateTime LastUpdate { get; set; }
        public string? APIVersion { get; set; }
        public string? RegistryCode { get; set; }
        public string? FirmwareVersion { get; set; }
        public string? DeviceFunction {  get; set; }
        public bool FingerprintSupported { get; set; }
        public bool FaceSupported { get; set; }
        public bool PalmSupported { get; set; }
        public int LockOpenDuration { get; set; }
        public string? DeviceInformation {  get; set; }
        public string? TimeZone {  get; set; }
        public int AntiPassback {  get; set; }
        public bool AntiPassbackOn {  get; set; }
        public string? KeyMapping { get; set; }
        public string? SyncStatus { get; set; }
        public string? Model {  get; set; }
        public string? KeyCode { get; set; }
    }
}
