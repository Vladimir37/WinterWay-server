using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests.Mood
{
    public class EditMoodTagDTO
    {
        [Required]
        public int TagId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Color { get; set; }
    }
}
