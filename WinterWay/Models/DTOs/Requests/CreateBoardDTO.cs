using System.ComponentModel.DataAnnotations;
using WinterWay.Enums;
using WinterWay.Attributes;

namespace WinterWay.Models.DTOs.Requests
{
    public class CreateBoardDTO
    {
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
    }
}
