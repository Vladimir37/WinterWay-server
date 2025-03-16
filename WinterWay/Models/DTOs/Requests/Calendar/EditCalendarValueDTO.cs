using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests.Calendar
{
    public class EditCalendarValueDTO
    {
        [Required]
        public int CalendarValueId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Color { get; set; }
    }
}
