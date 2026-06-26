using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCICHRPortal.Data.XscribeTables
{
    public class XEmployee
    {
        public int Id { get; set; }
        public string? Last_Name { get; set; }
        public string? First_Name { get; set; }
        public string? Middle_Name { get; set; }
        public string? Suffix { get; set; }
        public string? Display_Name { get; set; }
        public DateTime? Birth_Date { get; set; }
        public string? Gender { get; set; }
        public string? Blood_Type { get; set; }
        public int Company_Id { get; set; }
        public int Company_Branch_Id { get; set; }
        //[Column("DepartmentId")]
        public int? Department_Id { get; set; }
        public string? Position { get; set; }
        public string? Employment_Status { get; set; }
        public bool _Deleted { get; set; }
        public string? LandLine { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? Employee_code { get; set; }
        public int Num_201_Files { get; set; }
        public string? NickName { get; set; }
        public double? Weight_Kg { get; set; }
        public double? Height_M { get; set; }
        public int? Birth_Place_City_Id { get; set; }
        public string? Marital_Status { get; set; }
        public int? Religtion_Id { get; set; }
        public int? Citizenship_Id { get; set; }
        public int? Company_Position_Id { get; set; }
        public int? Num_Employments { get; set; }
        public string? Notes {  get; set; }
        public int? SubDepartment_Id { get; set; }
        public int? Business_Unit_Id { get; set; }
        public string? Timekeeping_Device_Identifier { get; set; }
        public string? X_Point_Of_Contact { get; set; }
        public string? X_Seat_Class { get; set; }
        public int? Company_Job_Grade_Id { get; set; }
        public int? Company_Job_Rank_Id { get; set; }
        public int? Company_Job_Class_Id { get; set; }
        public int? Company_Location_Id { get; set; }
        public int? Nationality_Id { get; set; }
        public bool? Expat {  get; set; }
        public int? Default_Hr_Payroll_Record_Id { get; set; }
        public int? Employee_Group_Id { get; set; }
        public int? Employee_Classification_Id { get; set; }
        public string? Location_Address_Line1 { get; set; }
        public string? Location_Address_Line2 { get; set; }
        public int? Location_Address_Location_Building_Id { get; set; }
        public int? Location_Address_Location_Zone_Id { get; set; }
        public int? Location_Address_Location_City_Id { get; set; }
        public int? Location_Address_Location_Area_Id {  get; set; }
        public int? Location_Address_Location_Country_Id { get; set; }
        public string? Location_Address_Zip {  get; set; }
        public XDepartment? Department { get; set; }
        public Company_Branch? Company_Branch { get; set; }
    }
}
