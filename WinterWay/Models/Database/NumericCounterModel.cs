namespace WinterWay.Models.Database
{
    public class NumericCounterModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int TaskId { get; set; }
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
