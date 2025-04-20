using System.Text.Json.Serialization;

namespace WinterWay.Models.Database.Calendar
{
    public class CalendarRecordNumericModel
    {
        public int Id { get; set; }
        
        public int Value { get; set; }
        
        public int? CalendarRecordId { get; set; }
        [JsonIgnore]
        public CalendarRecordModel? CalendarRecord { get; set; }
    }
}