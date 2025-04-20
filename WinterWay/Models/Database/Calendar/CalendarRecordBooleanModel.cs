using System.Text.Json.Serialization;

namespace WinterWay.Models.Database.Calendar
{
    public class CalendarRecordBooleanModel
    {
        public int Id { get; set; }
        
        public bool Value { get; set; }
        
        public int? CalendarRecordId { get; set; }
        [JsonIgnore]
        public CalendarRecordModel? CalendarRecord { get; set; }
    }
}