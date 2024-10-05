using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class ChangeArchiveStatusDTO
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public bool Status { get; set; }
    }
}
