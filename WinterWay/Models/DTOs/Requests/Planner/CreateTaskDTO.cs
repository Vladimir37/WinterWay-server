using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests.Planner
{
    public class CreateTaskDTO
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;

        public int BoardId { get; set; }
        public int? SprintId { get; set; }
    }
}
