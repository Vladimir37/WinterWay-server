using System.Text.Json.Serialization;
using WinterWay.Enums;

namespace WinterWay.Models.Database
{
    public class BoardModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public RollType RollType { get; set; }
        public RollStart RollStart { get; set; }
        public int RollDays { get; set; }
        public string? Color { get; set; }
        public bool Favorite { get; set; }
        public bool Archived { get; set; }
        public int? LastImage { get; set; }
        public DateTime CreationDate { get; set; }

        public string UserId { get; set; }
        [JsonIgnore]
        public UserModel User { get; set; }

    }
}
