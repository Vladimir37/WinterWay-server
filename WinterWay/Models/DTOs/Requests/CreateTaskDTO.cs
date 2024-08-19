using System.ComponentModel.DataAnnotations;
using WinterWay.Enums;

namespace WinterWay.Models.DTOs.Requests
{
    public class CreateTaskDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;

        public int? BoardId { get; set; }
        public int? SprintId { get; set; }
    }
}
