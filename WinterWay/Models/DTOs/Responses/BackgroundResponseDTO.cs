namespace WinterWay.Models.DTOs.Responses
{
    public class BackgroundResponseDTO
    {
        public string Name { get; set; }
        public string Dir { get; set; }
        public string Extension { get; set; }
        public BackgroundCountsDTO Count { get; set; }
    }
}