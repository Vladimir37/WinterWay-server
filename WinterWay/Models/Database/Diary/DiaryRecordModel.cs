using System.Text.Json.Serialization;
using WinterWay.Models.Database.Auth;

namespace WinterWay.Models.Database.Diary
{
    public class DiaryRecordModel
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public string? Info { get; set; }
        
        public string UserId { get; set; }
        [JsonIgnore]
        public UserModel User { get; set; }
        public List<DiaryRecordGroupModel> Groups { get; set; }
    }
}