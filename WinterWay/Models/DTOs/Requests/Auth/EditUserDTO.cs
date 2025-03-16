using System.ComponentModel.DataAnnotations;
using WinterWay.Attributes;
using WinterWay.Enums;

namespace WinterWay.Models.DTOs.Requests.Auth
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
