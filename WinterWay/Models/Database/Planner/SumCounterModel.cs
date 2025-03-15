using System.Text.Json.Serialization;

namespace WinterWay.Models.Database
{
    public class SumCounterModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int Sum { get; set; }
        public int SortOrder { get; set; }
        
        public int TaskId { get; set; }
        [JsonIgnore]
        public TaskModel Task { get; set; }

        public SumCounterModel CloneToNewTask()
        {
            return new SumCounterModel
            {
                Text = Text,
                Sum = Sum,
                SortOrder = SortOrder,
            };
        }
    }
}