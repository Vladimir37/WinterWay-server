using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests.Calendar
{
    public class EditCalendarDefaultValueDTO
    {
        [Required]
        public int CalendarId { get; set; }
        [Required]
        public string SerializedDefaultValue { get; set; }
    }
}