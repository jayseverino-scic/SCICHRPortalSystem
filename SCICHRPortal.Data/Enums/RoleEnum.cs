using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCICHRPortal.Data.Enums
{
    public enum RoleEnum
    {
        [Description("Administrator")]
        Administrator = 1,
        [Description("Registrar")]
        Registrar = 2,
        [Description("Parent")]
        Parent = 3,
    }
}
