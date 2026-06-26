using System;
using System.Collections.Generic;
using System.Text;

namespace SCICHRPortal.Data.XscribeTables
{
    public class XDepartment
    {
        public int Id { get; set; }
        public int Company_Id { get; set; }
        public string? Name { get; set; }
        public bool _Deleted { get; set; }
    }
}
