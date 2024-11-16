using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterWay.Data;
using WinterWay.Models.Database;
using WinterWay.Models.DTOs.Requests;
using WinterWay.Services;

namespace WinterWay.Controllers
{
    public class NotificationController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<UserModel> _userManager;
        private readonly NotificationService _notificationService;

        public NotificationController(ApplicationContext db, UserManager<UserModel> userManager, NotificationService notificationService)
        {
            _db = db;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetNotifications([FromBody] GetNotificationsDTO getNotificationsForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var notificationsCount = int.MaxValue;
            if (getNotificationsForm.Count != null && getNotificationsForm.Count > 0)
            {
                notificationsCount = getNotificationsForm.Count.Value;
            }

            IQueryable<NotificationModel> allNotificationsQuery = _db.Notifications;

            if (getNotificationsForm.Read != null)
            {
                allNotificationsQuery = allNotificationsQuery.Where(n => n.IsRead == getNotificationsForm.Read);
            }

            allNotificationsQuery = allNotificationsQuery
                .Where(n => !n.Archived)
                .Where(n => n.UserId == user!.Id)
                .OrderByDescending(n => n.CreationDate)
                .Take(notificationsCount);
            
            var allNotifications = await allNotificationsQuery.ToListAsync();
            
            return Ok(allNotifications);
        }

        public async Task<IActionResult> Read([FromBody] ChangeNotificationStatusDTO changeNotificationStatusForm)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var targetNotifications = _db.Notifications
                .Where(n => changeNotificationStatusForm.Notifications.Contains(n.Id))
                .Where(n => n.UserId == user!.Id)
                .ToList();

            foreach (var notification in targetNotifications)
            {
                notification.IsRead = true;
            }

            _db.SaveChanges();
            
            return Ok(targetNotifications);
        }
        
        public async Task<IActionResult> Archive([FromBody] ChangeNotificationStatusDTO changeNotificationStatusForm)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var targetNotifications = _db.Notifications
                .Where(n => changeNotificationStatusForm.Notifications.Contains(n.Id))
                .Where(n => n.UserId == user!.Id)
                .ToList();

            foreach (var notification in targetNotifications)
            {
                notification.Archived = true;
            }

            _db.SaveChanges();
            
            return Ok(targetNotifications);
        }
    }
}
