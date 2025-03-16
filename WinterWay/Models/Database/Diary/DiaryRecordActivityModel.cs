namespace WinterWay.Models.Database.Diary
{
    public class DiaryRecordActivityModel
    {
        public int Id { get; set; }
        
        public int DiaryGroupId { get; set; }
        public DiaryGroupModel DiaryGroup { get; set; }
        
        public int DiaryActivityId { get; set; }
        public DiaryRecordActivityModel DiaryActivity { get; set; }
    }
}