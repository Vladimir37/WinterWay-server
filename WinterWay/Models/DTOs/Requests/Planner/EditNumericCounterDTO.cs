using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests.Planner
{
    public class EditNumericCounterDTO
    {
        [Required]
        public int NumericCounterId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
