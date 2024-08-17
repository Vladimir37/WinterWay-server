using System.ComponentModel.DataAnnotations;
using WinterWay.Enums;
using WinterWay.Attributes;

namespace WinterWay.Models.DTOs.Requests
{
    public class EditUserDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 4)]
        public string? Username { get; set; }
        [Required]
        [EnumValidation(typeof(ThemeType))]
        public ThemeType Theme { get; set; }
        [Required]
        public bool AutoCompleteTasks { get; set; }
    }
}
