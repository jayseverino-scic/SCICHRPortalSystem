using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCICHRPortal.Data.Entities.Metadatas
{
    public class TimekeepingAdminSetup : BaseEntity
    {
        public int SetupId { get; set; }
        public string? AdminPassword { get; set; }
    }
}
