using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SCICHRPortal.Data.XscribeTables
{
    public class Company_Branch
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Registered_Name { get; set; }
        public string? Description { get; set; }
        public string? Addr_Line1 { get; set; }
        public string? Addr_Line2 { get; set; }
        public int? Addr_City_Id { get; set; }
        public int? Addr_Area_Id { get; set; }
        public int? Addr_Country_Id { get; set; }
        public string? Addr_Zip { get; set; }
        public int Company_Id { get; set; }
        public bool _Deleted { get; set; }
        public string? LandLine {  get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public int Num_Employees { get; set; }
        public string? Tin {  get; set; }
        public bool? Vat { get; set; }
        public double? Vat_Ratio { get; set; }
        public string? Website { get; set; }
        public string? SSS_Number { get; set; }
        public string? Philhealth_Number { get; set; }
        public string? Pagibig_Number { get; set; }
        public string? Rdo_Code { get; set; }
        public DateTime?  Creation_Date { get; set; }
        public string? Company_Branch_Code { get; set; }
        public string? Code { get; set; }
        public string? Type { get; set; }
        public string? Cost_Center { get; set; }
        public string? Segment {  get; set; }
        public string? Group { get; set; }
        public string? Division { get; set; }
        public bool? Disabled { get; set;  }
        public int? Authorized_Tax_Representative_Employee_Id { get; set; }
        public string? Authorized_Tax_Representative_Identification_Number { get; set; }
        public DateTime? Authorized_Tax_Representative_Identification_Issuance_Date { get; set; }
        public DateTime? Authorized_Tax_Representative_Identification_Expiration_Date { get; set; }
    }
}
