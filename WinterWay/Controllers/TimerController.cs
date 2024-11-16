using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterWay.Data;
using WinterWay.Enums;
using WinterWay.Models.Database;
using WinterWay.Models.DTOs.Error;
using WinterWay.Models.DTOs.Requests;
using WinterWay.Services;

namespace WinterWay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimerController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<UserModel> _userManager;
        private readonly TimerService _timerService;

        public TimerController(ApplicationContext db, UserManager<UserModel> userManager, TimerService timerService)
        {
            _db = db;
            _userManager = userManager;
            _timerService = timerService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTimer([FromBody] CreateTimerDTO createTimerForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var countOfAllActiveTimers = _db.Timers
                .Where(t => !t.Archived)
                .Where(t => t.UserId == user!.Id)
                .Count();

            var currentDate = DateTime.UtcNow;

            var newTimer = new TimerModel
            {
                Name = createTimerForm.Name,
                Color = createTimerForm.Color,
                NotificationActive = true,
                Archived = false,
                SortOrder = countOfAllActiveTimers,
                CreationDate = currentDate,
                UserId = user!.Id
            };

            _db.Timers.Add(newTimer);
            _db.SaveChanges();

            _timerService.StartTimer(newTimer);

            return Ok(newTimer);
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditTimer([FromBody] EditTimerDTO editTimerForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTimer = _db.Timers
                .Where(t => t.Id == editTimerForm.TimerId)
                .Where(t => t.UserId == user!.Id)
                .FirstOrDefault();

            if (targetTimer == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Timer does not exists"));
            }

            targetTimer.Name = editTimerForm.Name;
            targetTimer.Color = editTimerForm.Color;
            targetTimer.NotificationActive = editTimerForm.NotificationActive;
            _db.SaveChanges();
            return Ok(targetTimer);
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveTimer([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTimer = _db.Timers
                .Where(t => t.Id == idForm.Id)
                .Where(t => t.UserId == user!.Id)
                .FirstOrDefault();

            if (targetTimer == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Timer does not exists"));
            }

            _db.Timers.Remove(targetTimer);
            _db.SaveChanges();
            return Ok("Timer has been removed");
        }

        [HttpPost("stop")]
        public async Task<IActionResult> StopTimer([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTimer = _db.Timers
                .Where(t => t.Id == idForm.Id)
                .Where(t => t.UserId == user!.Id)
                .FirstOrDefault();

            if (targetTimer == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Timer does not exists"));
            }

            _timerService.StopTimer(targetTimer);

            return Ok(targetTimer);
        }

        [HttpPost("restart")]
        public async Task<IActionResult> RestartTimer([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTimer = _db.Timers
                .Where(t => t.Id == idForm.Id)
                .Where(t => !t.Archived)
                .Where(t => t.UserId == user!.Id)
                .FirstOrDefault();

            if (targetTimer == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Timer does not exists"));
            }

            _timerService.StopTimer(targetTimer);
            var newTimerSession = _timerService.StartTimer(targetTimer);
            return Ok(newTimerSession);
        }

        [HttpPost("change-archive-status")]
        public async Task<IActionResult> ChangeTimerArchiveStatus([FromBody] ChangeArchiveStatusDTO changeArchiveStatusForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTimer = _db.Timers
                .Where(t => t.Id == changeArchiveStatusForm.Id)
                .Where(t => t.UserId == user!.Id)
                .FirstOrDefault();

            if (targetTimer == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Timer does not exists"));
            }

            if (targetTimer.Archived == changeArchiveStatusForm.Status)
            {
                return BadRequest(new ApiError(InternalError.InvalidForm, "The new timer status is no different from the old one"));
            }

            if (changeArchiveStatusForm.Status)
            {
                _timerService.StopTimer(targetTimer);
            }

            var countOfTimersInNewStatus = _db.Timers
                .Where(t => t.Archived == changeArchiveStatusForm.Status)
                .Where(t => t.UserId == user!.Id)
                .Count();

            targetTimer.Archived = changeArchiveStatusForm.Status;
            targetTimer.SortOrder = countOfTimersInNewStatus;
            _db.SaveChanges();

            var otherTimersInOldStatus = _db.Timers
                .Where(t => t.Archived == !changeArchiveStatusForm.Status)
                .Where(t => t.UserId == user!.Id)
                .OrderBy(t => t.SortOrder)
                .ToList();

            var num = 0;
            foreach (var timer in otherTimersInOldStatus)
            {
                timer.SortOrder = num;
                num++;
            }
            _db.SaveChanges();

            return Ok(targetTimer);
        }

        [HttpPost("change-timers-order")]
        public async Task<IActionResult> ChangeTimersOrder([FromBody] ChangeElementsOrderDTO changeTimersOrderForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var timers = _db.Timers
                .Where(t => changeTimersOrderForm.Elements.Contains(t.Id))
                .OrderBy(t => changeTimersOrderForm.Elements.IndexOf(t.Id))
                .Where(t => t.UserId == user!.Id)
                .ToList();

            var allTimersBelongToOneStatus = timers.All(s => !s.Archived);

            if (!allTimersBelongToOneStatus)
            {
                return BadRequest(new ApiError(InternalError.InvalidForm, "All timers values must be active"));
            }

            var num = 0;
            foreach (var timer in timers)
            {
                timer.SortOrder = num;
                num++;
            }
            _db.SaveChanges();

            return Ok(timers);
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllTimers()
        {
            var user = await _userManager.GetUserAsync(User);

            var timers = _db.Timers
                .Include(t => t.TimerSessions)
                .Where(t => t.UserId == user!.Id)
                .ToList();

            return Ok(timers);
        }
    }
}
