using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class ChangeNumericCounterValueDTO
    {
        [Required]
        public int NumericCounterId { get; set; }
        [Required]
        public int Value { get; set; }
    }
}
