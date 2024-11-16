using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class ChangeNotificationStatusDTO
    {
        [Required]
        public List<int> Notifications { get; set; } = new List<int>();
    }
}