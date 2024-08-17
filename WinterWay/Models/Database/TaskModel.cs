using WinterWay.Enums;

namespace WinterWay.Models.Database
{
    public class TaskModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TaskType Type { get; set; }
        public bool IsTemplate { get; set; }
        public bool IsBacklog { get; set; }
        public bool IsDone { get; set; }
        public bool AutoComplete { get; set; }
        public string Color { get; set; }
        public int MaxCounter { get; set; }
        public DateTime CreationDate { get; set; }

        public int? BoardId { get; set; }
        public BoardModel? Board { get; set; }
        public int? SprintId { get; set; }
        public SprintModel? Sprint { get; set; }
    }
}
