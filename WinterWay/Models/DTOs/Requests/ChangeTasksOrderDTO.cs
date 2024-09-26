using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class ChangeTasksOrderDTO
    {
        [Required]
        public List<int> Tasks { get; set; } = new List<int>();
    }
}
