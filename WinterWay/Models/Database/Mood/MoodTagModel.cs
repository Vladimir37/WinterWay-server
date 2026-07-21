using System.Text.Json.Serialization;
using WinterWay.Models.Database.Auth;

namespace WinterWay.Models.Database.Mood
{
    public class MoodTagModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public int SortOrder { get; set; }
        public bool Archived { get; set; }
        public DateTime CreationDate { get; set; }

        public string UserId { get; set; }
        [JsonIgnore]
        public UserModel User { get; set; }

        [JsonIgnore]
        public List<MoodRecordModel> Records { get; set; } = new List<MoodRecordModel>();
    }
}
