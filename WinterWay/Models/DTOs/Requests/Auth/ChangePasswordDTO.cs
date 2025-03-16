﻿using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests.Auth
{
    public class ChangePasswordDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string? OldPassword { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string? NewPassword { get; set; }
    }
}
