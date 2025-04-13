using System.Globalization;

namespace WinterWay.Services
{
    public class DateTimeService
    {
        public bool ParseDate(string val, out DateOnly parsedDate)
        {
            var culture = CultureInfo.InvariantCulture;
            var style = DateTimeStyles.None;

            if (DateOnly.TryParseExact(val, "yyyy-MM-dd", culture, style, out DateOnly resultDate))
            {
                parsedDate = resultDate;
                return true;

            }
            parsedDate = DateOnly.MinValue;
            return false;
        }
    }
}

