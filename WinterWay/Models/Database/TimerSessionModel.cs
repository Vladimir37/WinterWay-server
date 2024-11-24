using System.Text.Json.Serialization;

namespace WinterWay.Models.Database
{
    public class TimerSessionModel
    {
        public int Id { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? StopDate { get; set; }
        public bool Active { get; set; }

        public int TimerId { get; set; }
        [JsonIgnore]
        public TimerModel Timer { get; set; }
    }
}
