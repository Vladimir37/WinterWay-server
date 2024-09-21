using System.Text.Json.Serialization;
using WinterWay.Enums;

namespace WinterWay.Models.Database
{
    public class SprintModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public int Image { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? ClosingDate { get; set; }
        public int Number { get; set; }

        public int BoardId { get; set; }
        [JsonIgnore]
        public BoardModel Board { get; set; }
        public SprintResultModel? SprintResult { get; set; }
        public List<TaskModel> Tasks { get; set; } = new List<TaskModel>();
    }
}
