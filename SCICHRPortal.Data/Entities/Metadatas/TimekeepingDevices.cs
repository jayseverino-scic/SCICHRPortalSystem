using System;
using System.Collections.Generic;
using System.Text;

namespace SCICHRPortal.Data.Entities.Metadatas
{
    public class TimekeepingDevices : BaseEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? SerialNumber {  get; set; }
        public string? Source { get; set; }
     }
}
