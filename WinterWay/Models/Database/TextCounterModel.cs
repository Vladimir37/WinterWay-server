namespace WinterWay.Models.Database
{
    public class TextCounterModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int SortOrder { get; set; }

        public int TaskId { get; set; }
        public TaskModel Task { get; set; }

        public TextCounterModel CloneToNewTask()
        {
            return new TextCounterModel
            {
                Text = Text,
                SortOrder = SortOrder,
            };
        }
    }
}
