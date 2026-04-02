using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace SCICHRPortal.Repository
{
    public static class DateTimeInitializer
    {
        [ModuleInitializer]
        public static void Initialize()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
    }
}
