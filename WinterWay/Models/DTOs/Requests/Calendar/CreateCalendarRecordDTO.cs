using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests.Calendar
{
    public class CreateCalendarRecordDTO
    {
        [Required]
        public int CalendarId { get; set; }
        [Required]
        public string Date { get; set; }
        public string? Text { get; set; }
        [Required]
        public string SerializedValue { get; set; }
        [Required]
        public bool FillDefaultValues { get; set; }
    }
}
