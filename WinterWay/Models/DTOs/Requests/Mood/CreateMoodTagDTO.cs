using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests.Mood
{
    public class CreateMoodTagDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Color { get; set; }
    }
}
