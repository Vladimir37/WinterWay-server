using System.Text.Json.Serialization;
using WinterWay.Models.Database.Auth;

namespace WinterWay.Models.Database.Diary
{
    public class DiaryGroupModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }
        public bool Multiple { get; set; }
        public bool CanBeEmpty { get; set; }
        public bool Archived { get; set; }
        
        public string UserId { get; set; }
        [JsonIgnore]
        public UserModel User { get; set; }
        
        public List<DiaryActivityModel> Activities { get; set; } =  new List<DiaryActivityModel>();
        [JsonIgnore]
        public List<DiaryRecordGroupModel> RecordGroups { get; set; } =  new List<DiaryRecordGroupModel>();
    }
}

