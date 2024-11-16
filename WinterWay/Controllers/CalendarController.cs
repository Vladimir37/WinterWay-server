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
    public class CalendarController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<UserModel> _userManager;
        private readonly CalendarService _calendarService;

        public CalendarController(ApplicationContext db, UserManager<UserModel> userManager, CalendarService calendarService)
        {
            _db = db;
            _userManager = userManager;
            _calendarService = calendarService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCalendar([FromBody] CreateCalendarDTO createCalendarForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var calendarsTotal = _db.Calendars
                .Where(b => b.UserId == user!.Id)
                .Where(b => !b.Archived)
                .Count();

            if (createCalendarForm.SerializedDefaultValue != null && !_calendarService.Validate(createCalendarForm.SerializedDefaultValue, -1, createCalendarForm.Type))
            {
                return BadRequest(new ApiError(InternalError.InvalidForm, "Invalid default value"));
            }

            var newCalendar = new CalendarModel
            {
                Name = createCalendarForm.Name,
                Type = createCalendarForm.Type,
                Color = createCalendarForm.Color,
                SerializedDefaultValue = createCalendarForm.SerializedDefaultValue,
                SortOrder = calendarsTotal,
                Archived = false,
                NotificationActive = true,
                CreationDate = DateTime.UtcNow,
                ArchivingDate = null,
                UserId = user!.Id
            };

            _db.Calendars.Add(newCalendar);
            _db.SaveChanges();
            return Ok(newCalendar);
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditCalendar([FromBody] EditCalendarDTO editCalendarForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendar = _db.Calendars
                .Where(c => c.Id == editCalendarForm.CalendarId)
                .Where(c => c.UserId == user!.Id)
                .FirstOrDefault();

            if (targetCalendar == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Calendar does not exists"));
            }

            if (!_calendarService.Validate(editCalendarForm.SerializedDefaultValue, targetCalendar.Id, targetCalendar.Type))
            {
                return BadRequest(new ApiError(InternalError.InvalidForm, "Invalid default value"));
            }

            targetCalendar.Name = editCalendarForm.Name;
            targetCalendar.Color = editCalendarForm.Color;
            targetCalendar.SerializedDefaultValue = editCalendarForm.SerializedDefaultValue;
            targetCalendar.NotificationActive = editCalendarForm.NotificationActive;
            _db.SaveChanges();
            return Ok(targetCalendar);
        }

        [HttpPost("change-archive-status")]
        public async Task<IActionResult> ChangeArchiveStatus([FromBody] ChangeArchiveStatusDTO changeArchiveStatusForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendar = _db.Calendars
                .Where(c => c.Id == changeArchiveStatusForm.Id)
                .Where(c => c.UserId == user!.Id)
                .FirstOrDefault();

            if (targetCalendar == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Calendar does not exists"));
            }

            if (targetCalendar.Archived == changeArchiveStatusForm.Status)
            {
                return BadRequest(new ApiError(InternalError.InvalidForm, "The new calendar status is no different from the old one"));
            }

            var countOfCalendarsInNewStatus = _db.Calendars
                .Where(c => c.Archived == changeArchiveStatusForm.Status)
                .Where(c => c.UserId == user!.Id)
                .Count();

            targetCalendar.Archived = changeArchiveStatusForm.Status;
            targetCalendar.SortOrder = countOfCalendarsInNewStatus;
            if (changeArchiveStatusForm.Status)
            {
                targetCalendar.ArchivingDate = DateTime.UtcNow;
            } 
            else
            {
                targetCalendar.ArchivingDate = null;
            }
            _db.SaveChanges();

            var otherCalendarsInOldStatus = _db.Calendars
                .Where(c => c.Archived != changeArchiveStatusForm.Status)
                .Where(c => c.UserId == user!.Id)
                .OrderBy(c => c.SortOrder)
                .ToList();

            var num = 0;
            foreach (var calendar in otherCalendarsInOldStatus)
            {
                calendar.SortOrder = num;
                num++;
            }
            _db.SaveChanges();
            return Ok(targetCalendar);
        }

        [HttpPost("change-calendars-order")]
        public async Task<IActionResult> ChangeCalendarsOrder([FromBody] ChangeElementsOrderDTO changeCalendarsOrderForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var calendars = _db.Calendars
                .Where(c => changeCalendarsOrderForm.Elements.Contains(c.Id))
                .OrderBy(c => changeCalendarsOrderForm.Elements.IndexOf(c.Id))
                .Where(c => c.UserId == user!.Id)
                .ToList();

            var allCalendarsBelongToOneStatus = calendars.All(c => !c.Archived);

            if (!allCalendarsBelongToOneStatus)
            {
                return BadRequest(new ApiError(InternalError.InvalidForm, "All calendars must be active"));
            }

            var num = 0;
            foreach (var calendar in calendars)
            {
                calendar.SortOrder = num;
                num++;
            }
            _db.SaveChanges();

            return Ok(calendars);
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveCalendar([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendar = _db.Calendars
                .Where(c => c.Id == idForm.Id)
                .Where(c => c.UserId == user!.Id)
                .FirstOrDefault();

            if (targetCalendar == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Calendar does not exists"));
            }

            var removedCalendarArchiveStatus = targetCalendar.Archived;

            _db.Calendars.Remove(targetCalendar);
            _db.SaveChanges();

            var otherCalendars = _db.Calendars
                .Where(c => c.UserId == user!.Id)
                .Where(c => c.Archived == removedCalendarArchiveStatus)
                .OrderBy(c => c.SortOrder)
                .ToList();

            var num = 0;
            foreach (var calendar in otherCalendars)
            {
                calendar.SortOrder = num;
                num++;
            }
            _db.SaveChanges();

            return Ok("Calendar has been removed");
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllCalendars()
        {
            var user = await _userManager.GetUserAsync(User);

            var allCalendars = _db.Calendars
                .Where(c => c.UserId == user!.Id)
                .ToList();

            return Ok(allCalendars);
        }

        [HttpGet("get-one")]
        public async Task<IActionResult> GetOneCalendar([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendar = _db.Calendars
                .Include(c => c.CalendarRecords)
                .Include(c => c.CalendarValues)
                .Where(c => c.Id == idForm.Id)
                .Where(c => c.UserId == user!.Id)
                .FirstOrDefault();

            return Ok(targetCalendar);
        }
    }
}
