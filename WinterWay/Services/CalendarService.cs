using System.Globalization;
using Microsoft.EntityFrameworkCore;
using WinterWay.Data;
using WinterWay.Enums;

namespace WinterWay.Services
{
    public class CalendarService
    {
        private readonly ApplicationContext _db;

        public CalendarService(ApplicationContext db)
        {
            _db = db;
        }

        public async Task<bool> Validate(string val, int calendarId, CalendarType calendarType)
        {
            return calendarType switch
            {
                CalendarType.Boolean => ValidateBool(val),
                CalendarType.Numeric => ValidateNumeric(val),
                CalendarType.Time => ValidateTime(val),
                CalendarType.Fixed => await ValidateFixedValue(val, calendarId),
                _ => false
            };
        }

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

        private bool ValidateBool(string val)
        {
            return bool.TryParse(val, out _);
        }

        private bool ValidateNumeric(string val)
        {
            return int.TryParse(val, out _);
        }

        private bool ValidateTime(string val)
        {
            var regex = new System.Text.RegularExpressions.Regex(@"^([01]\d|2[0-3]):([0-5]\d)$");
            return regex.IsMatch(val);
        }

        private async Task<bool> ValidateFixedValue(string val, int calendarId)
        {
            var valueIdValid = int.TryParse(val, out int valueId);

            if (!valueIdValid)
            {
                return false;
            }

            return await _db.CalendarValues
                .Where(cv => cv.CalendarId == calendarId)
                .Where(cv => cv.Id == valueId)
                .Where(cv => !cv.Archived)
                .AnyAsync();
        }
    }
}
