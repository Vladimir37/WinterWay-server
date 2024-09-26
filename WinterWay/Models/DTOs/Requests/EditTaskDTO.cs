using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class EditTaskDTO
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public bool AutoComplete { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public int MaxCounter { get; set; }
    }
}
