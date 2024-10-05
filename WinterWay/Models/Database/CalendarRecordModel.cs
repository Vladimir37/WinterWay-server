namespace WinterWay.Models.Database
{
    public class CalendarRecordModel
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public string SerializedValue { get; set; }
        public string Text { get; set; }

        public int CalendarId { get; set; }
        public CalendarModel Calendar { get; set; }
        public int? FixedValueId { get; set; }
        public CalendarValueModel? FixedValue { get; set; }
    }
}
