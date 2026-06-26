using System;
using System.Collections.Generic;
using System.Text;

namespace SCICHRPortal.Data.XscribeTables
{
    public class XCompany_Position
    {
        public int Id { get; set; }
        public int Company_Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public int? Rank { get; set; }
    }
}
