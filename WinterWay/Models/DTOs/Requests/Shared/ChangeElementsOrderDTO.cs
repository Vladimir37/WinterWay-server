using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests.Shared
{
    public class ChangeElementsOrderDTO
    {
        [Required]
        public List<int> Elements { get; set; } = new List<int>();
    }
}
