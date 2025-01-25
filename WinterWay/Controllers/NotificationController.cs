using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterWay.Data;
using WinterWay.Models.Database;
using WinterWay.Models.DTOs.Requests;
using WinterWay.Services;

namespace WinterWay.Controllers
{
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<UserModel> _userManager;
        private readonly NotificationService _notificationService;
        private readonly RateLimiterService _rateLimiterService;

        public NotificationController(ApplicationContext db, UserManager<UserModel> userManager, NotificationService notificationService, RateLimiterService rateLimiterService)
        {
            _db = db;
            _userManager = userManager;
            _notificationService = notificationService;
            _rateLimiterService = rateLimiterService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetNotifications([FromQuery] GetNotificationsDTO getNotificationsForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var notificationsCount = int.MaxValue;
            if (getNotificationsForm.Count != null && getNotificationsForm.Count > 0)
            {
                notificationsCount = getNotificationsForm.Count.Value;
            }

            var notificationsSkip = 0;
            if (getNotificationsForm.Skip != null && getNotificationsForm.Skip > 0)
            {
                notificationsSkip = getNotificationsForm.Skip.Value;
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
                .Skip(notificationsSkip)
                .Take(notificationsCount);
            
            var allNotifications = await allNotificationsQuery.ToListAsync();
            
            return Ok(allNotifications);
        }

        [HttpGet("calculate")]
        public async Task<IActionResult> Calculate()
        {
            var user = await _userManager.GetUserAsync(User);
            var requestTypeCalculate = "calculateNotifications";
            var blockPeriod = new TimeSpan(0, 30, 0);

            if (_rateLimiterService.IsRequestAvailableAgain(user!.Id, requestTypeCalculate, blockPeriod))
            {
                var newNotifications = await _notificationService.Calculate(user.Id);
                _rateLimiterService.SetLastRequestTime(user.Id, requestTypeCalculate);
                return Ok(newNotifications);
            }

            return Ok(new List<NotificationModel>());
        }

        [HttpPost("read")]
        public async Task<IActionResult> Read([FromBody] ChangeNotificationStatusDTO changeNotificationStatusForm)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var targetNotifications = await _db.Notifications
                .Where(n => changeNotificationStatusForm.Notifications.Contains(n.Id))
                .Where(n => n.UserId == user!.Id)
                .ToListAsync();

            foreach (var notification in targetNotifications)
            {
                notification.IsRead = true;
            }

            await _db.SaveChangesAsync();
            
            return Ok(targetNotifications);
        }
        
        [HttpPost("archive")]
        public async Task<IActionResult> Archive([FromBody] ChangeNotificationStatusDTO changeNotificationStatusForm)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var targetNotifications = await _db.Notifications
                .Where(n => changeNotificationStatusForm.Notifications.Contains(n.Id))
                .Where(n => n.UserId == user!.Id)
                .ToListAsync();

            foreach (var notification in targetNotifications)
            {
                notification.Archived = true;
            }

            await _db.SaveChangesAsync();
            
            return Ok(targetNotifications);
        }
    }
}
