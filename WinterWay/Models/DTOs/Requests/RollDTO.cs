using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class RollDTO
    {
        [Required]
        public int BoardId { get; set; }
        public List<int> TasksSpill { get; set; } = new List<int>();
        public List<int> TasksToBacklog { get; set; } = new List<int>();
    }
}
