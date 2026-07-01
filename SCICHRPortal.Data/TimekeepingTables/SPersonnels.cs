using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCICHRPortal.Data.TimekeepingTables
{
    public class SPersonnels
    {
        public Guid Id { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsDeleted { get; set; }
        public string? PersonnelNo { get; set; }
        public string? AccessNumber { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? ContactNumber { get; set; }
        public string? Email {  get; set; }
        public string? Password { get; set; }
        public DateTime BirthDate {  get; set; }
        public string? PhotoId { get; set; }
        public Guid UserRoleId { get; set; }
        public string? SmsContactNumber { get; set; }
        public bool SmsNotification {  get; set; }
        public bool IsActive { get; set; }
        public string? PasswordSettings { get; set; }
        public DateTime DateHired { get; set; }
        public DateTime SeparationDate {  get; set; }
        public bool EnableOTP { get; set; }
    }
}
