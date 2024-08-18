namespace WinterWay.Models.Database
{
    public class TextCounterModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }

        public int TaskId { get; set; }
        public TaskModel Task { get; set; }

        public TextCounterModel CloneToNewTask()
        {
            return new TextCounterModel
            {
                Name = Name,
                Value = 0,
            };
        }
    }
}
