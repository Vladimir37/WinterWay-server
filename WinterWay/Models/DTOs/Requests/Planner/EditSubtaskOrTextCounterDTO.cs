using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests.Planner
{
    public class EditSubtaskOrTextCounterDTO
    {
        [Required]
        public int SubtaskId { get; set; }
        [Required]
        public string Text { get; set; } = string.Empty;
    }
}
