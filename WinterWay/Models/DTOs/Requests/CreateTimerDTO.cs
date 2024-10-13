using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class CreateTimerDTO
    {
        [Required]
        public string Name { get; set; }
        public string? Color { get; set; }
    }
}
