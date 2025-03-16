namespace WinterWay.Models.DTOs.Responses.Background
{
    public class BackgroundCountsDTO
    {
        public int Backlog { get; set; }
        public int Days { get; set; }
        public int Months { get; set; }
        public int None { get; set; }
        public int Other { get; set; }
        public int Empty { get; set; }
    }
}