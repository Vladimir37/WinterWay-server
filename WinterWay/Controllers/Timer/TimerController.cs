using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterWay.Data;
using WinterWay.Enums;
using WinterWay.Models.Database.Auth;
using WinterWay.Models.Database.Timer;
using WinterWay.Models.DTOs.Requests.Shared;
using WinterWay.Models.DTOs.Requests.Timer;
using WinterWay.Models.DTOs.Responses.Shared;
using WinterWay.Services;

namespace WinterWay.Controllers.Timer
{
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

            var countOfAllActiveTimers = await _db.Timers
                .Where(t => !t.Archived)
                .Where(t => t.UserId == user!.Id)
                .CountAsync();

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
            await _db.SaveChangesAsync();

            await _timerService.StartTimer(newTimer);

            return Ok(newTimer);
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditTimer([FromBody] EditTimerDTO editTimerForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTimer = await _db.Timers
                .Include(t => t.TimerSessions)
                .Where(t => t.Id == editTimerForm.TimerId)
                .Where(t => t.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetTimer == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Timer does not exists"));
            }

            targetTimer.Name = editTimerForm.Name;
            targetTimer.Color = editTimerForm.Color;
            targetTimer.NotificationActive = editTimerForm.NotificationActive;
            await _db.SaveChangesAsync();
            return Ok(targetTimer);
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveTimer([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTimer = await _db.Timers
                .Where(t => t.Id == idForm.Id)
                .Where(t => t.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetTimer == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Timer does not exists"));
            }

            _db.Timers.Remove(targetTimer);
            await _db.SaveChangesAsync();
            return Ok(new ApiSuccessDTO("TimerDeletion"));
        }

        [HttpPost("stop")]
        public async Task<IActionResult> StopTimer([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTimer = await _db.Timers
                .Where(t => t.Id == idForm.Id)
                .Where(t => t.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetTimer == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Timer does not exists"));
            }

            await _timerService.StopTimer(targetTimer);

            return Ok(targetTimer);
        }

        [HttpPost("restart")]
        public async Task<IActionResult> RestartTimer([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTimer = await _db.Timers
                .Where(t => t.Id == idForm.Id)
                .Where(t => !t.Archived)
                .Where(t => t.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetTimer == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Timer does not exists"));
            }

            await _timerService.StopTimer(targetTimer);
            var newTimerSession = await _timerService.StartTimer(targetTimer);
            return Ok(newTimerSession);
        }

        [HttpPost("change-archive-status")]
        public async Task<IActionResult> ChangeTimerArchiveStatus([FromBody] ChangeArchiveStatusDTO changeArchiveStatusForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTimer = await _db.Timers
                .Where(t => t.Id == changeArchiveStatusForm.Id)
                .Where(t => t.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetTimer == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Timer does not exists"));
            }

            if (targetTimer.Archived == changeArchiveStatusForm.Status)
            {
                return BadRequest(new ApiErrorDTO(InternalError.InvalidForm, "The new timer status is no different from the old one"));
            }

            if (changeArchiveStatusForm.Status)
            {
                await _timerService.StopTimer(targetTimer);
            }

            var countOfTimersInNewStatus = await _db.Timers
                .Where(t => t.Archived == changeArchiveStatusForm.Status)
                .Where(t => t.UserId == user!.Id)
                .CountAsync();

            targetTimer.Archived = changeArchiveStatusForm.Status;
            targetTimer.SortOrder = countOfTimersInNewStatus;
            await _db.SaveChangesAsync();

            var otherTimersInOldStatus = await _db.Timers
                .Where(t => t.Archived == !changeArchiveStatusForm.Status)
                .Where(t => t.UserId == user!.Id)
                .OrderBy(t => t.SortOrder)
                .ToListAsync();

            var num = 0;
            foreach (var timer in otherTimersInOldStatus)
            {
                timer.SortOrder = num;
                num++;
            }
            await _db.SaveChangesAsync();

            return Ok(targetTimer);
        }

        [HttpPost("change-sort-order")]
        public async Task<IActionResult> ChangeTimersOrder([FromBody] ChangeElementsOrderDTO changeTimersOrderForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var timers = await _db.Timers
                .Where(t => changeTimersOrderForm.Elements.Contains(t.Id))
                .OrderBy(t => changeTimersOrderForm.Elements.IndexOf(t.Id))
                .Where(t => t.UserId == user!.Id)
                .ToListAsync();

            var allTimersBelongToOneStatus = timers.All(s => !s.Archived);

            if (!allTimersBelongToOneStatus)
            {
                return BadRequest(new ApiErrorDTO(InternalError.InvalidForm, "All timers values must be active"));
            }

            var num = 0;
            foreach (var timer in timers)
            {
                timer.SortOrder = num;
                num++;
            }
            await _db.SaveChangesAsync();

            return Ok(timers);
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllTimers()
        {
            var user = await _userManager.GetUserAsync(User);

            var timers = await _db.Timers
                .Include(t => t.TimerSessions)
                .Where(t => t.UserId == user!.Id)
                .ToListAsync();

            return Ok(timers);
        }
    }
}
