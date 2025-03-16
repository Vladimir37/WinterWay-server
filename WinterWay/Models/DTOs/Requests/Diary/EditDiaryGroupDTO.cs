using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests.Diary
{
    public class EditDiaryGroupDTO
    {
        [Required]
        public int DiaryGroupId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public bool Multiple { get; set; }
        [Required]
        public bool CanBeEmpty { get; set; }
    }
}