using System.Text.Json.Serialization;
using WinterWay.Enums;
using WinterWay.Models.Database.Auth;

namespace WinterWay.Models.Database.Mood
{
    public class MoodRecordModel
    {
        public int Id { get; set; }
        public MoodType Type { get; set; }
        public string Text { get; set; }
        public DateOnly Date { get; set; }
        public DateTime CreationDate { get; set; }

        public string UserId { get; set; }
        [JsonIgnore]
        public UserModel User { get; set; }

        public int? TagId { get; set; }
        public MoodTagModel? Tag { get; set; }
    }
}
