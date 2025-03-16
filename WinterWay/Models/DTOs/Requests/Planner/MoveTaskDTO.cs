using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests.Planner
{
    public class MoveTaskDTO
    {
        [Required]
        public int TaskId { get; set; }
        [Required]
        public int BoardId { get; set; }
        [Required]
        public int SprintId { get; set; }
    }
}
