using System.Text.Json.Serialization;
using WinterWay.Models.Database.Auth;

namespace WinterWay.Models.Database.Diary
{
    public class DiaryRecordGroupModel
    {
        public int Id { get; set; }
        
        public int DiaryRecordId { get; set; }
        [JsonIgnore]
        public DiaryRecordModel DiaryRecord { get; set; }
        
        public int DiaryGroupId { get; set; }
        [JsonIgnore]
        public DiaryGroupModel DiaryGroup { get; set; }
        
        public List<DiaryRecordActivityModel> Activities { get; set; }
    }
}