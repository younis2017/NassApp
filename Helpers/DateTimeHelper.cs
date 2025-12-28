using Microsoft.AspNetCore.Mvc;

namespace Nass.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime NowET()
        {
            var etZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, etZone);
        }
    }

}
