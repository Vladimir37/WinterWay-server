using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests.Mood
{
    public class GetMoodRecordsDTO
    {
        public string? DateStart { get; set; }
        public string? DateEnd { get; set; }
        [Range(0, int.MaxValue)]
        public int? MaxCount { get; set; }
    }
}
