using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class ChangePasswordDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string? NewPassword { get; set; }
    }
}
