using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests.Diary
{
    public class CreateDiaryGroupDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public bool Multiple { get; set; }
        [Required]
        public bool CanBeEmpty { get; set; }
    }
}