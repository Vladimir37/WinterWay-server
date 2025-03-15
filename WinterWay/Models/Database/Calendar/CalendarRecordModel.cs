using System.Text.Json.Serialization;
using WinterWay.Enums;

namespace WinterWay.Models.Database
{
    public class CalendarRecordModel
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public string? SerializedValue { get; set; }
        public string? Text { get; set; }

        public int CalendarId { get; set; }
        [JsonIgnore]
        public CalendarModel Calendar { get; set; }
        public int? FixedValueId { get; set; }
        public CalendarValueModel? FixedValue { get; set; }

        public CalendarRecordModel() { }
        public CalendarRecordModel(DateOnly date, string? text, int calendarId, CalendarType type, string serializedDefaultValue)
        {
            Date = date;
            Text = text;
            CalendarId = calendarId;

            SetNewValue(serializedDefaultValue, type);
        }

        public void SetNewValue(string newValue, CalendarType type)
        {
            if (type == CalendarType.Fixed)
            {
                int.TryParse(newValue, out int targetValue);
                SerializedValue = null;
                FixedValueId = targetValue;
            }
            else
            {
                SerializedValue = newValue;
                FixedValueId = null;
            }
        }
    }
}
