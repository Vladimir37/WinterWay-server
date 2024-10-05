using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class ChangeCalendarsOrderDTO
    {
        [Required]
        public List<int> Calendars { get; set; } = new List<int>();
    }
}
