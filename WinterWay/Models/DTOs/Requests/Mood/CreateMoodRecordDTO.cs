using System.ComponentModel.DataAnnotations;
using WinterWay.Attributes;
using WinterWay.Enums;

namespace WinterWay.Models.DTOs.Requests.Mood
{
    public class CreateMoodRecordDTO
    {
        [Required]
        public string Date { get; set; }
        [Required]
        [EnumValidation(typeof(MoodType))]
        public MoodType Type { get; set; }
        [Required]
        public string Text { get; set; }
        public int? TagId { get; set; }
    }
}
