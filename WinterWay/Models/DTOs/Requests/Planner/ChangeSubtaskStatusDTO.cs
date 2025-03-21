﻿using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests.Planner
{
    public class ChangeSubtaskStatusDTO
    {
        [Required]
        public int SubtaskId { get; set; }
        [Required]
        public bool Status { get; set; }
    }
}
