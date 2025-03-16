using WinterWay.Models.Database.Notification;

namespace WinterWay.Models.DTOs.Responses.Notification
{
    public class NotificationsResponseDTO
    {
        public int UnreadCount { get; set; }
        public List<NotificationModel> Notifications { get; set; }

        public NotificationsResponseDTO(int unreadCount, List<NotificationModel> notifications)
        {
            UnreadCount = unreadCount;
            Notifications = notifications;
        }
    }
}

