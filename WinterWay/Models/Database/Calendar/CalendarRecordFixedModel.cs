using System.Text.Json.Serialization;

namespace WinterWay.Models.Database.Calendar
{
    public class CalendarRecordFixedModel
    {
        public int Id { get; set; }
        
        public int FixedValueId { get; set; }
        public CalendarValueModel? FixedValue { get; set; }
        
        public int? CalendarRecordId { get; set; }
        [JsonIgnore]
        public CalendarRecordModel? CalendarRecord { get; set; }
    }
}