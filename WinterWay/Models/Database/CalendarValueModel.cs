namespace WinterWay.Models.Database
{
    public class CalendarValueModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public bool Archived { get; set; }
        public int SortOrder { get; set; }

        public int CalendarId { get; set; }
        public CalendarModel Calendar { get; set; }
        public List<CalendarRecordModel> CalendarRecords { get; set; } = new List<CalendarRecordModel>();
    }
}
