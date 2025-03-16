using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests.Planner
{
    public class ChangeNumericCounterValueDTO
    {
        [Required]
        public int NumericCounterId { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public int Value { get; set; }
    }
}
