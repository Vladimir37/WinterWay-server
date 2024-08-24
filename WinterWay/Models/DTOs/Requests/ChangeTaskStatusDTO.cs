using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class ChangeTaskStatusDTO
    {
        [Required]
        public int TaskId { get; set; }
        [Required]
        public bool Status { get; set; }
    }
}
