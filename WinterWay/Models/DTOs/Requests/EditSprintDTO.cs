using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class EditSprintDTO
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [MinLength(1)]
        public string Name { get; set; } = string.Empty;
    }
}
