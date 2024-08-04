using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class LoginModelDTO
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
