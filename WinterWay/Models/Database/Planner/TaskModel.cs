using System.Text.Json.Serialization;
using WinterWay.Enums;

namespace WinterWay.Models.Database.Planner
{
    public class TaskModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public TaskType Type { get; set; }
        public bool IsTemplate { get; set; }
        public bool IsDone { get; set; }
        public bool AutoComplete { get; set; }
        public string Color { get; set; } = string.Empty;
        public int MaxCounter { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ClosingDate { get; set; }

        public int BoardId { get; set; }
        [JsonIgnore]
        public BoardModel Board { get; set; }
        public int? SprintId { get; set; }
        [JsonIgnore]
        public SprintModel? Sprint { get; set; }

        public List<SubtaskModel> Subtasks { get; set; } = new List<SubtaskModel>();
        public List<TextCounterModel> TextCounters { get; set; } = new List<TextCounterModel>();
        public List<SumCounterModel> SumCounters { get; set; } = new List<SumCounterModel>();
        public NumericCounterModel? NumericCounter { get; set; }

        public TaskModel CloneToNewSprint(SprintModel sprint)
        {
            return new TaskModel
            {
                Name = Name,
                Description = Description,
                Type = Type,
                IsTemplate = false,
                IsDone = false,
                AutoComplete = AutoComplete,
                Color = Color,
                MaxCounter = MaxCounter,
                CreationDate = DateTime.UtcNow,
                Board = Board,
                Sprint = sprint,

                Subtasks = Subtasks.Select(s => s.CloneToNewTask()).ToList(),
                TextCounters = TextCounters.Select(t => t.CloneToNewTask()).ToList(),
                SumCounters = SumCounters.Select(s => s.CloneToNewTask()).ToList(),
                NumericCounter = NumericCounter?.CloneToNewTask(),
            };
        }
    }
}
