using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class EditTimerDTO
    {
        [Required]
        public int TimerId { get; set; }
        [Required]
        public string Name { get; set; }
        public bool NotificationActive { get; set; }
        public string? Color { get; set; }
    }
}
