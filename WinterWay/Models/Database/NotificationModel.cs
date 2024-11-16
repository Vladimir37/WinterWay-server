using WinterWay.Enums;

namespace WinterWay.Models.Database
{
    public class NotificationModel
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public bool Archived { get; set; }
        public NotificationEntity Entity { get; set; }
        public int EntityId { get; set; }
        public NotificationType Type { get; set; }
        public DateTime CreationDate { get; set; }

        public string UserId { get; set; }
        public UserModel User { get; set; }
    }
}
