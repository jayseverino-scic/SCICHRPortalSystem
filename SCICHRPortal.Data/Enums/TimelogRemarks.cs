using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SCICHRPortal.Data.Enums
{
    public enum TimelogRemarks
    {
        [Description("Biometrics")]
        Biometrics,
        [Description("Manual Add")]
        ManualAdd,
        [Description("Manual Edited")]
        ManualEdit
    }
}
