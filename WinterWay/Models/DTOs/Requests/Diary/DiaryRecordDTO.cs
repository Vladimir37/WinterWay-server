using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests.Diary
{
    public class DiaryRecordDTO
    {
        [Required]
        public string Date { get; set; }
        public string Info { get; set; } = String.Empty;
        [Required]
        public Dictionary<int, List<int>> Activities { get; set; }
    }
}