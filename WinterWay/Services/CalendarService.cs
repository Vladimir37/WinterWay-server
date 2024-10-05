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

        public bool Validate(string val, int calendarId, CalendarType calendarType)
        {
            return calendarType switch
            {
                CalendarType.Boolean => ValidateBool(val),
                CalendarType.Numeric => ValidateNumeric(val),
                CalendarType.Time => ValidateTime(val),
                CalendarType.Fixed => ValidateFixedValue(val, calendarId),
                _ => false
            };
        }

        private bool ValidateBool(string val)
        {
            return bool.TryParse(val, out bool result);
        }

        private bool ValidateNumeric(string val)
        {
            return int.TryParse(val, out int result);
        }

        private bool ValidateTime(string val)
        {
            var regex = new System.Text.RegularExpressions.Regex(@"^([01]\d|2[0-3]):([0-5]\d)$");
            return regex.IsMatch(val);
        }

        private bool ValidateFixedValue(string val, int calendarId)
        {
            var valueIdValid = int.TryParse(val, out int valueId);

            if (!valueIdValid)
            {
                return false;
            }

            return _db.CalendarValues
                .Where(cv => cv.CalendarId == calendarId)
                .Where(cv => cv.Id == valueId)
                .Any();
        }
    }
}
