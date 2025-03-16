using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests.Notification
{
    public class ChangeNotificationStatusDTO
    {
        [Required]
        public List<int> Notifications { get; set; } = new List<int>();
    }
}