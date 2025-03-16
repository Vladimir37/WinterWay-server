namespace WinterWay.Models.Database.Diary
{
    public class DiaryActivityModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public int SortOrder { get; set; }
        public bool Archived { get; set; }
        
        public int DiaryGroupId { get; set; }
        public DiaryGroupModel DiaryGroup { get; set; }
        
        public List<DiaryRecordActivityModel> RecordActivities { get; set; } =  new List<DiaryRecordActivityModel>();
    }
}