using System.Text.Json.Serialization;
using WinterWay.Enums;

namespace WinterWay.Models.Database.Calendar
{
    public class CalendarRecordModel
    {
        public int Id { get; set; }
        public DateOnly? Date { get; set; }
        public bool IsDefault { get; set; }
        public string? Text { get; set; }
        
        public CalendarRecordBooleanModel? BooleanVal { get; set; }
        public CalendarRecordNumericModel? NumericVal { get; set; }
        public CalendarRecordTimeModel? TimeVal { get; set; }
        public CalendarRecordFixedModel? FixedVal { get; set; }

        public int CalendarId { get; set; }
        [JsonIgnore]
        public CalendarModel Calendar { get; set; }
    }
}
