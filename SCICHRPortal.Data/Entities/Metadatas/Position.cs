using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCICHRPortal.Data.Entities.Metadatas
{
    public class Position : BaseEntity
    {
        public int PositionId { get; set; }
        public string? PositionName { get; set; }
    }
}
