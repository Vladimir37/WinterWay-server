using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests.Shared
{
    public class IdDTO
    {
        [Required]
        public int Id { get; set; }
    }
}
