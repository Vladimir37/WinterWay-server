using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class ChangeBoardsOrderDTO
    {
        [Required]
        public List<int> Boards { get; set; } = new List<int>();
    }
}
