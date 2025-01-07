using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class LoginDTO
    {
        [Required]
        [StringLength(40, MinimumLength = 6)]
        public string? Username { get; set; }
        [Required]
        [StringLength (40, MinimumLength = 6)]
        public string? Password { get; set; }
    }
}
