using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class EditSumCounterDTO
    {
        [Required]
        public int SubtaskId { get; set; }
        [Required]
        public string Text { get; set; } = string.Empty;
        [Required]
        public int Sum { get; set; }
    }
}
