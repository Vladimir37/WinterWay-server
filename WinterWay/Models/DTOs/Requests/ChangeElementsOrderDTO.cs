using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class ChangeElementsOrderDTO
    {
        [Required]
        public List<int> Elements { get; set; } = new List<int>();
    }
}
