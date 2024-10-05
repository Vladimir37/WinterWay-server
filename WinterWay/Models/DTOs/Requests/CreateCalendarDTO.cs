using System.ComponentModel.DataAnnotations;
using WinterWay.Enums;
using WinterWay.Attributes;

namespace WinterWay.Models.DTOs.Requests
{
    public class CreateCalendarDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [EnumValidation(typeof(CalendarType))]
        public CalendarType Type { get; set; }
        public string Color { get; set; }
        [Required]
        public string DefaultValue { get; set; }
    }
}
