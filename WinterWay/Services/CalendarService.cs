using System.Globalization;
using Microsoft.EntityFrameworkCore;
using WinterWay.Data;
using WinterWay.Enums;
using WinterWay.Models.Database.Calendar;

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

        public CalendarRecordModel? GetCalendarRecord(
            int calendarId, 
            bool isDefault,
            DateOnly? date, 
            string text,
            CalendarType type,
            string value
        )
        {
            var newRecord = new CalendarRecordModel
            {
                Date = date,
                Text = text,
                IsDefault = isDefault,
                CalendarId = calendarId,
            };

            if (type == CalendarType.Boolean)
            {
                bool.TryParse(value, out var valBool);
                var boolModel = new CalendarRecordBooleanModel
                {
                    Value = valBool,
                };
                newRecord.BooleanVal = boolModel;
            } 
            else if (type == CalendarType.Numeric)
            {
                int.TryParse(value, out var valNumeric);
                var numModel = new CalendarRecordNumericModel
                {
                    Value = valNumeric,
                };
                newRecord.NumericVal = numModel;
            }
            else if (type == CalendarType.Time)
            {
                TimeSpan.TryParse(value, out var valTime);
                var timeModel = new CalendarRecordTimeModel
                {
                    Value = valTime,
                };
                newRecord.TimeVal = timeModel;
            }
            else if (type == CalendarType.Fixed)
            {
                int.TryParse(value, out var valFixed);
                var fixedModel = new CalendarRecordFixedModel
                {
                    FixedValueId = valFixed,
                };
                newRecord.FixedVal = fixedModel;
            }
            else
            {
                return null;
            }

            return newRecord;
        }

        public CalendarRecordModel GetRecordCopy(CalendarRecordModel record, DateOnly date, CalendarType type)
        {
            var newRecord = new CalendarRecordModel
            {
                Date = date,
                IsDefault = false,
                Text = String.Empty,
                CalendarId = record.CalendarId,
            };
            
            if (type == CalendarType.Boolean)
            {
                newRecord.BooleanVal = new CalendarRecordBooleanModel
                {
                    Value = record.BooleanVal!.Value,
                };
            } 
            else if (type == CalendarType.Numeric)
            {
                newRecord.NumericVal = new CalendarRecordNumericModel
                {
                    Value = record.NumericVal!.Value,
                };
            }
            else if (type == CalendarType.Time)
            {
                newRecord.TimeVal = new CalendarRecordTimeModel
                {
                    Value = record.TimeVal!.Value,
                };
            }
            else if (type == CalendarType.Fixed)
            {
                Console.WriteLine(record);
                newRecord.FixedVal = new CalendarRecordFixedModel
                {
                    FixedValueId = record.FixedVal!.FixedValueId,
                };
            }

            return newRecord;
        }
    }
}
