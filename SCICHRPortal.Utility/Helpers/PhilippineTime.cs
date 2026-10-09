namespace SCICHRPortal.Utility.Helpers
{
    public static class PhilippineTime
    {
        private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
        public static DateTime Now => DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zone), DateTimeKind.Unspecified);
        public static bool IsLocal(DateTime? value) => value is null || value.Value.Kind == DateTimeKind.Unspecified;
        public static DateTime SessionStart(DateTime date, DateTime? time) => DateTime.SpecifyKind(date.Date.Add(time?.TimeOfDay ?? TimeSpan.Zero), DateTimeKind.Unspecified);
    }
}
