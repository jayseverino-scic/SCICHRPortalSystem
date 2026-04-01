using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCICHRPortal.Data.Entities.Metadatas
{
    public class Employee : BaseEntity
    {
        public int EmployeeId { get; set; }
        public string? EmployeeNo { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? Suffix { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? ContactNumber { get; set; }
        public int? DepartmentId { get; set; }
        public int? PositionId { get; set; }
        public int? UserId { get; set; }
        public virtual Department? Department { get; set; }
        public virtual User? User { get; set; }
        public virtual Position? Position { get; set; }
    }
}
