using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class GetCalendarRecordsDTO
    {
        [Required]
        public int CalendarId { get; set; }
        public string? DateStart { get; set; }
        public string? DateEnd { get; set; }
        [Range(0, int.MaxValue)]
        public int? MaxCount { get; set; }
    }
}
