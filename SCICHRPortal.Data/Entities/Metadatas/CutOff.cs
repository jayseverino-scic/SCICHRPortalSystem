using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCICHRPortal.Data.Entities.Metadatas
{
    public class CutOff : BaseEntity
    {
        public int CutOffId { get; set; }
        public DateTime StartDate {  get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
