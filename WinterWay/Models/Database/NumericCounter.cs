namespace WinterWay.Models.Database
{
    public class NumericCounter
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int TaskId { get; set; }
        public TaskModel Task { get; set; }

        public NumericCounter CloneToNewTask()
        {
            return new NumericCounter
            {
                Name = Name,
            };
        }
    }
}
