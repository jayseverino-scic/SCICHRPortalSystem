using System.Globalization;

namespace SCICHRPortal.Utility.Extensions
{
    public static class CapitalizeString
    {
        public static string ToTitleCase(this string title)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower());
        }
    }
}
