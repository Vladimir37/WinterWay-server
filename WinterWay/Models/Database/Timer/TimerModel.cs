using System.Text.Json.Serialization;
using WinterWay.Models.Database.Auth;

namespace WinterWay.Models.Database.Timer
{
    public class TimerModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Color { get; set; }
        public bool Archived { get; set; }
        public bool NotificationActive { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreationDate { get; set; }

        public string UserId { get; set; }
        [JsonIgnore]
        public UserModel User { get; set; }
        public List<TimerSessionModel> TimerSessions { get; set; } = new List<TimerSessionModel>();
    }
}
