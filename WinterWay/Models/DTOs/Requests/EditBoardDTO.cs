using System.ComponentModel.DataAnnotations;
using WinterWay.Attributes;
using WinterWay.Enums;

namespace WinterWay.Models.DTOs.Requests
{
    public class EditBoardDTO
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [EnumValidation(typeof(RollType))]
        public RollType RollType { get; set; }
        [Required]
        [EnumValidation(typeof(RollStart))]
        public RollStart RollStart { get; set; }
        public string Color { get; set; } = string.Empty;
        [Required]
        public int RollDays { get; set; }
        public bool Favorite { get; set; }
        public bool NotificationActive { get; set; }
    }
}
