using System.ComponentModel.DataAnnotations;
using WinterWay.Enums;
using WinterWay.Attributes;

namespace WinterWay.Models.DTOs.Requests
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
