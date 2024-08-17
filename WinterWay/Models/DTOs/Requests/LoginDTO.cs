using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class LoginDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 4)]
        public string? Username { get; set; }
        [Required]
        [StringLength (100, MinimumLength = 6)]
        public string? Password { get; set; }
    }
}
