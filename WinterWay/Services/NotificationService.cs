using Microsoft.EntityFrameworkCore;
using WinterWay.Data;
using WinterWay.Enums;
using WinterWay.Models.Database;

namespace WinterWay.Services
{
    public class NotificationService
    {
        private readonly ApplicationContext _db;

        public NotificationService(ApplicationContext db)
        {
            _db = db;
        }

        public async Task<NotificationModel?> CreateNotification(NotificationType type, int entityId, string userId)
        {
            var entityType = GetEntityType(type);
            var message = await GetNotificationMessage(type, entityType, entityId, userId);

            if (message == null)
            {
                return null;
            }

            var newNotification = new NotificationModel
            {
                Message = message,
                IsRead = false,
                Archived = false,
                Entity = entityType,
                EntityId = entityId,
                Type = type,
                CreationDate = DateTime.UtcNow,
                UserId = userId
            };
            
            _db.Notifications.Add(newNotification);
            await _db.SaveChangesAsync();
            return newNotification;
        }

        private NotificationEntity GetEntityType(NotificationType type)
        {
            return type switch
            {
                NotificationType.TimeToRoll => NotificationEntity.Board,
                NotificationType.CalendarRecordForToday => NotificationEntity.Calendar,
                NotificationType.TaskCounterReachedMaxValue => NotificationEntity.Task,
                _ => NotificationEntity.TimerSession,
            };
        }

        private async Task<string?> GetNotificationMessage(NotificationType type, NotificationEntity entity, int entityId, string userId)
        {
            return entity switch
            {
                NotificationEntity.Board => await GetNotificationBoardMessage(type, entityId, userId),
                NotificationEntity.Calendar => await GetNotificationCalendarMessage(type, entityId, userId),
                NotificationEntity.TimerSession => await GetNotificationTimerMessage(type, entityId, userId),
                NotificationEntity.Task => await GetNotificationTaskMessage(type, entityId, userId),
                _ => null,
            };
        }

        private async Task<string?> GetNotificationBoardMessage(NotificationType type, int entityId, string userId)
        {
            var targetBoard = await _db.Boards
                .Where(b => b.Id == entityId)
                .Where(b => b.UserId == userId)
                .FirstOrDefaultAsync();

            if (targetBoard == null)
            {
                return null;
            }

            return $"Sprint \"{targetBoard.Name}\" is complete and ready for rollover";
        }
        
        private async Task<string?> GetNotificationTimerMessage(NotificationType type, int entityId, string userId)
        {
            var targetTimer = await _db.Timers
                .Where(c => c.Id == entityId)
                .Where(c => c.UserId == userId)
                .FirstOrDefaultAsync();

            if (targetTimer == null)
            {
                return null;
            }

            var period = type switch
            {
                NotificationType.DayOnTimer => "day",
                NotificationType.WeekOnTimer => "week",
                NotificationType.MonthOnTimer => "month",
                NotificationType.YearOnTimer => "year",
                _ => "UNDEFINED"
            };

            return $"The \"{targetTimer.Name}\" timer has reached one {period}";
        }
        
        private async Task<string?> GetNotificationCalendarMessage(NotificationType type, int entityId, string userId)
        {
            var targetCalendar = await _db.Calendars
                .Where(c => c.Id == entityId)
                .Where(c => c.UserId == userId)
                .FirstOrDefaultAsync();

            if (targetCalendar == null)
            {
                return null;
            }

            return $"You haven’t entered any data for today in the \"{targetCalendar.Name}\" calendar";
        }
        
        private async Task<string?> GetNotificationTaskMessage(NotificationType type, int entityId, string userId)
        {
            var targetTask = await _db.Tasks
                .Include(t => t.Board)
                .Where(t => t.Id == entityId)
                .Where(t => t.Board.UserId == userId)
                .FirstOrDefaultAsync();

            if (targetTask == null)
            {
                return null;
            }

            return $"The counter in the task \"{targetTask.Name}\" has reached the required value, and the task was automatically closed";
        }
    }
}
