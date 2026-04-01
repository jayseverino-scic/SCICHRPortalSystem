using System.ComponentModel;

namespace SCICHRPortal.Data.Enums
{
    public enum SendOptionEnum
    {
        [Description("Send to all")]
        All = 1,
        [Description("Send all grade level")]
        AllGrade = 2,
        [Description("All teacher")]
        AllTeacher = 3,
    }
}
