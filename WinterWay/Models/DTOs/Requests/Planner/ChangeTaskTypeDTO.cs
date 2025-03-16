using System.ComponentModel.DataAnnotations;
using WinterWay.Attributes;
using WinterWay.Enums;

namespace WinterWay.Models.DTOs.Requests.Planner
{
    public class ChangeTaskTypeDTO
    {
        [Required]
        public int TaskId { get; set; }
        [Required]
        [EnumValidation(typeof(TaskType))]
        public TaskType TaskType { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public int MaxCounter { get; set; }
    }
}
