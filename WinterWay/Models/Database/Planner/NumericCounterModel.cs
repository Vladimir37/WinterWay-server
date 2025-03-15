using System.Text.Json.Serialization;

namespace WinterWay.Models.Database.Planner
{
    public class NumericCounterModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }

        public int TaskId { get; set; }
        [JsonIgnore]
        public TaskModel Task { get; set; }

        public NumericCounterModel CloneToNewTask()
        {
            return new NumericCounterModel
            {
                Name = Name,
            };
        }
    }
}
