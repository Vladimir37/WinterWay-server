using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class IdDTO
    {
        [Required]
        public int Id { get; set; }
    }
}
