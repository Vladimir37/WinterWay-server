using System.ComponentModel.DataAnnotations;
using WinterWay.Attributes;
using WinterWay.Enums;

namespace WinterWay.Models.DTOs.Requests.Calendar
{
    public class CreateCalendarDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [EnumValidation(typeof(CalendarType))]
        public CalendarType Type { get; set; }
        public string? Color { get; set; }
        public string? SerializedDefaultValue { get; set; }
    }
}
