using System;
using System.Collections.Generic;
using System.Text;

namespace SCICHRPortal.Data.Entities
{
    public class ClaraFormOTRequest : BaseEntity
    {
        public string? employee_overtime_summary_employee_name { get; set; }
        public string? employee_overtime_summary_employee_hris_id { get; set; }
        public DateOnly? employee_overtime_summary_overtime_date_from { get; set; }
        public TimeOnly? employee_overtime_summary_approved_start_time { get; set; }
        public DateOnly? employee_overtime_summary_overtime_date_to { get; set; }
        public TimeOnly? employee_overtime_summary_approved_end_time { get; set; }
        public double? employee_overtime_summary_filed_hours { get; set; }
        public double? employee_overtime_summary_approved_hours { get; set; }
        public string? employee_overtime_summary_request_reference { get; set; }
        public TimeSpan? employee_overtime_summary_closed_at { get; set; }
        public TimeSpan? employee_overtime_summary_date_created { get; set; }
        public Guid? employee_overtime_summary_id { get; set; }
    }
}
