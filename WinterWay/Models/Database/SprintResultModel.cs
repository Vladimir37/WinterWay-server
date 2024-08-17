namespace WinterWay.Models.Database
{
    public class SprintResultModel
    {
        public int Id { get; set; }
        public int Days { get; set; }
        public int TasksDone { get; set; }
        public int TasksSpill { get; set; }
        public int TasksClosed { get; set; }
        public int TasksToBacklog { get; set; }

        public int SprintId { get; set; }
        public SprintModel Sprint { get; set; }
    }
}
