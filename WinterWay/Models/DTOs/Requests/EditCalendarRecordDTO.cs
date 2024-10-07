using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class EditCalendarRecordDTO
    {
        [Required]
        public int CalendarRecordId { get; set; }
        public string? Text { get; set; }
        [Required]
        public string SerializedValue { get; set; }

    }
}
