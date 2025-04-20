using System.Text.Json.Serialization;
using WinterWay.Enums;
using WinterWay.Models.Database.Auth;

namespace WinterWay.Models.Database.Calendar
{
    public class CalendarModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public CalendarType Type { get; set; }
        public string? Color { get; set; }
        public int SortOrder { get; set; }
        public bool Archived { get; set; }
        public bool NotificationActive { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ArchivingDate { get; set; }

        public string UserId { get; set; }
        [JsonIgnore]
        public UserModel User { get; set; }
        public int? DefaultRecordId { get; set; }
        public CalendarRecordModel? DefaultRecord { get; set; }
        public List<CalendarRecordModel> CalendarRecords { get; set; }
        public List<CalendarValueModel> CalendarValues { get; set; }
    }
}
