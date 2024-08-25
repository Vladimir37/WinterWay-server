using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class CreateSubtaskOrTextCounterDTO
    {
        [Required]
        public string Text { get; set; }
        [Required]
        public int SortOrder { get; set; }
        [Required]
        public int TaskId { get; set; }
    }
}
