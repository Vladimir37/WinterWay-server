using WinterWay.Enums;

namespace WinterWay.Models.Database
{
    public class CalendarModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public CalendarType Type { get; set; }
        public string Color { get; set; }
        public string? SerializedDefaultValue { get; set; }
        public int SortOrder { get; set; }
        public bool Archived { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ArchivingDate { get; set; }

        public string UserId { get; set; }
        public UserModel User { get; set; }
        public List<CalendarRecordModel> CalendarRecords { get; set; }
        public List<CalendarValueModel> CalendarValues { get; set; }
    }
}
