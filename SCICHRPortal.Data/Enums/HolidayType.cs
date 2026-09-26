using System.ComponentModel;

namespace SCICHRPortal.Data.Enums
{
    public enum HolidayType
    {
        [Description("Regular")]
        Regular = 1,
        [Description("Special Non-Working")]
        SpecialNonWorking = 2,
        [Description("Local")]
        Local = 3
    }
}
