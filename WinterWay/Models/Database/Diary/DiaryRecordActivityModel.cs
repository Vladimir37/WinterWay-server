using System.Text.Json.Serialization;

namespace WinterWay.Models.Database.Diary
{
    public class DiaryRecordActivityModel
    {
        public int Id { get; set; }
        
        public int DiaryRecordGroupId { get; set; }
        [JsonIgnore]
        public DiaryRecordGroupModel DiaryRecordGroup { get; set; }
        
        public int DiaryActivityId { get; set; }
        [JsonIgnore]
        public DiaryActivityModel DiaryActivity { get; set; }
    }
}