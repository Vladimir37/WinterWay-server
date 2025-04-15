using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests.Diary
{
    public class GetDiaryRecordsDTO
    {
        public string? DateStart { get; set; }
        public string? DateEnd { get; set; }
        [Range(0, int.MaxValue)]
        public int? MaxCount { get; set; }
    }
}