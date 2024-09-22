using System.Text.Json.Serialization;

namespace WinterWay.Models.Database
{
    public class SubtaskModel
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsDone { get; set; }
        public int SortOrder { get; set; }

        public int TaskId { get; set; }
        [JsonIgnore]
        public TaskModel Task { get; set; }

        public SubtaskModel CloneToNewTask()
        {
            return new SubtaskModel
            {
                Text = Text,
                IsDone = false,
                SortOrder = SortOrder,
            };
        }
    }
}
