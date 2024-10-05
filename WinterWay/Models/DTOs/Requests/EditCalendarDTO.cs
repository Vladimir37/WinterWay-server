using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class EditCalendarDTO
    {
        [Required]
        public int CalendarId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Color { get; set; }
        [Required]
        public string SerializedDefaultValue { get; set; }
    }
}
