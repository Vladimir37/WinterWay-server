using System.Text.Json.Serialization;

namespace WinterWay.Models.Database.Calendar
{
    public class CalendarRecordTimeModel
    {
        public int Id { get; set; }
        
        public TimeSpan Value { get; set; }
        
        public int? CalendarRecordId { get; set; }
        [JsonIgnore]
        public CalendarRecordModel? CalendarRecord { get; set; }
    }
}