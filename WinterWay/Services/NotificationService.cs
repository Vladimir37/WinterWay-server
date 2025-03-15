using Microsoft.EntityFrameworkCore;
using WinterWay.Data;
using WinterWay.Enums;
using WinterWay.Models.Database.Notification;

namespace WinterWay.Services
{
    public class NotificationService
    {
        private readonly ApplicationContext _db;

        public NotificationService(ApplicationContext db)
        {
            _db = db;
        }

        public async Task<List<NotificationModel>> Calculate(string userId)
        {
            List<NotificationModel> newNotifications = new List<NotificationModel>();

            newNotifications.AddRange(await CalculateSprints(userId));
            newNotifications.AddRange(await CalculateCalendars(userId));
            newNotifications.AddRange(await CalculateTimers(userId));
            
            return newNotifications;
        }

        private async Task<List<NotificationModel>> CalculateSprints(string userId)
        {
            var newNotifications = new List<NotificationModel>();
            
            var allUserNotification = await _db.Notifications
                .Where(n => n.UserId == userId)
                .Where(n => n.Type == NotificationType.TimeToRoll)
                .ToListAsync();

            var targetSprints = await _db.Sprints
                .Include(s => s.Board)
                .Where(s => s.Board.UserId == userId)
                .Where(s => s.Active)
                .Where(s => s.ExpirationDate < DateTime.UtcNow)
                .Where(s => !allUserNotification
                    .Select(n => n.EntityId)
                    .Contains(s.Id)
                )
                .ToListAsync();

            foreach (var sprint in targetSprints)
            {
                var newNotification = await CreateNotification(NotificationType.TimeToRoll, sprint.Id, userId);
                if (newNotification != null)
                {
                    newNotifications.Add(newNotification);
                }
            }

            return newNotifications;
        }

        private async Task<List<NotificationModel>> CalculateCalendars(string userId)
        {
            var newNotifications = new List<NotificationModel>();
            
            var allUserNotification = await _db.Notifications
                .Where(n => n.UserId == userId)
                .Where(n => n.Type == NotificationType.CalendarRecordForToday)
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var allTodayRecords = await _db.CalendarRecords
                .Include(cr => cr.Calendar)
                .Where(cr => cr.Date == today)
                .Where(cr => cr.Calendar.UserId == userId)
                .ToListAsync();

            var targetCalendars = await _db.Calendars
                .Where(c => c.UserId == userId)
                .Where(c => !c.Archived)
                .ToListAsync();
            
            var filteredCalendars = targetCalendars
                .Where(c => allTodayRecords.All(cr => cr.CalendarId != c.Id))
                .Where(c => !allUserNotification
                    .Any(n => n.EntityId == c.Id && DateOnly.FromDateTime(n.CreationDate) == today)
                )
                .ToList(); 
            
            foreach (var calendar in filteredCalendars)
            {
                var newNotification = await CreateNotification(NotificationType.CalendarRecordForToday, calendar.Id, userId);
                if (newNotification != null)
                {
                    newNotifications.Add(newNotification);
                }
            }

            return newNotifications;
        }

        private async Task<List<NotificationModel>> CalculateTimers(string userId)
        {
            var newNotifications = new List<NotificationModel>();
            
            NotificationType[] timerTypes = [NotificationType.DayOnTimer, NotificationType.WeekOnTimer, NotificationType.MonthOnTimer, NotificationType.YearOnTimer];

            var allUserNotification = await _db.Notifications
                .Where(n => n.UserId == userId)
                .Where(n => timerTypes.Contains(n.Type))
                .ToListAsync();

            var allTimerSessions = await _db.TimerSessions
                .Include(ts => ts.Timer)
                .Where(ts => ts.Active)
                .Where(ts => ts.Timer.UserId == userId)
                .ToListAsync();

            foreach (var timerSession in allTimerSessions)
            {
                var targetNotificationType = GetMaxTimeSpan(timerSession.CreationDate);

                var notificationAlreadyExists = allUserNotification
                    .Where(n => n.Type == targetNotificationType)
                    .Where(n => n.EntityId == timerSession.TimerId)
                    .Any();
                
                if (targetNotificationType != null && !notificationAlreadyExists)
                {
                    var newNotification = await CreateNotification(targetNotificationType.Value, timerSession.TimerId, userId);
                    if (newNotification != null)
                    {
                        newNotifications.Add(newNotification);
                    }
                }
            }

            return newNotifications;
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
            
            await _db.Notifications.AddAsync(newNotification);
            await _db.SaveChangesAsync();
            return newNotification;
        }

        private NotificationType? GetMaxTimeSpan(DateTime date)
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan difference = now - date;
            
            return difference.TotalDays switch
            {
                >= 365 => NotificationType.YearOnTimer,
                >= 30 => NotificationType.MonthOnTimer,
                >= 7 => NotificationType.WeekOnTimer,
                >= 1 => NotificationType.DayOnTimer,
                _ => null
            };
        }

        private NotificationEntity GetEntityType(NotificationType type)
        {
            return type switch
            {
                NotificationType.TimeToRoll => NotificationEntity.Sprint,
                NotificationType.CalendarRecordForToday => NotificationEntity.Calendar,
                NotificationType.TaskCounterReachedMaxValue => NotificationEntity.Task,
                _ => NotificationEntity.TimerSession,
            };
        }

        private async Task<string?> GetNotificationMessage(NotificationType type, NotificationEntity entity, int entityId, string userId)
        {
            return entity switch
            {
                NotificationEntity.Sprint => await GetNotificationSprintMessage(entityId, userId),
                NotificationEntity.Calendar => await GetNotificationCalendarMessage(entityId, userId),
                NotificationEntity.TimerSession => await GetNotificationTimerMessage(type, entityId, userId),
                NotificationEntity.Task => await GetNotificationTaskMessage(entityId, userId),
                _ => null,
            };
        }

        private async Task<string?> GetNotificationSprintMessage(int entityId, string userId)
        {
            var targetSprint = await _db.Sprints
                .Include(s => s.Board)
                .Where(b => b.Id == entityId)
                .Where(b => b.Board.UserId == userId)
                .FirstOrDefaultAsync();

            if (targetSprint == null)
            {
                return null;
            }

            return $"Sprint \"{targetSprint.Name}\" in \"{targetSprint.Board.Name}\" is complete and ready for rollover";
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
        
        private async Task<string?> GetNotificationCalendarMessage(int entityId, string userId)
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
        
        private async Task<string?> GetNotificationTaskMessage(int entityId, string userId)
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
