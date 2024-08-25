using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class ChangeSubtaskOrTextCounterOrderDTO
    {
        [Required]
        public List<int> Subtasks { get; set; } = new List<int>();
    }
}
