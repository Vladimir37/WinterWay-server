namespace WinterWay.Models.DTOs.Responses
{
    public class BackgroundResponseDTO
    {
        public string AppName { get; set; }
        public string Dir { get; set; }
        public string Extension { get; set; }
        public BackgroundCountsDTO Count { get; set; }
    }
}