using System.Text.Json.Serialization;

namespace WinterWay.Models.Database.Calendar
{
    public class CalendarValueModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public bool Archived { get; set; }
        public int SortOrder { get; set; }

        public int CalendarId { get; set; }
        [JsonIgnore]
        public CalendarModel Calendar { get; set; }
        [JsonIgnore]
        public List<CalendarRecordFixedModel> CalendarFixedRecords { get; set; } = new List<CalendarRecordFixedModel>();
    }
}
