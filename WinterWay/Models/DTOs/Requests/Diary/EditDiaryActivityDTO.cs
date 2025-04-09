using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests.Diary
{
    public class EditDiaryActivityDTO
    {
        [Required]
        public int ActivityId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Icon { get; set; }
        [Required]
        public string Color { get; set; }
    }
}