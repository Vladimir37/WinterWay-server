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
    public class CalendarValueController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<UserModel> _userManager;
        private readonly CalendarService _calendarService;

        public CalendarValueController(ApplicationContext db, UserManager<UserModel> userManager, CalendarService calendarService)
        {
            _db = db;
            _userManager = userManager;
            _calendarService = calendarService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCalendarValue([FromBody] CreateCalendarValueDTO createCalendarValueForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendar = await _db.Calendars
                .Where(c => c.Id == createCalendarValueForm.CalendarId)
                .Where(c => c.Type == CalendarType.Fixed)
                .Where(c => !c.Archived)
                .Where(c => c.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetCalendar == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "No matching calendars found"));
            }

            var formattedName = createCalendarValueForm.Name.Trim();

            var isNameExists = await _db.CalendarValues
                .Include(cv => cv.Calendar)
                .Where(cv => cv.Name == formattedName)
                .Where(cv => cv.CalendarId == createCalendarValueForm.CalendarId)
                .Where(cv => cv.Calendar.UserId == user!.Id)
                .AnyAsync();

            if (isNameExists)
            {
                return BadRequest(new ApiError(InternalError.InvalidForm, "A value with this name already exists in this calendar"));
            }

            var valuesCount = await _db.CalendarValues
                .Include(c => c.Calendar)
                .Where(cv => cv.CalendarId == createCalendarValueForm.CalendarId)
                .Where(cv => cv.Calendar.UserId == user!.Id)
                .CountAsync();

            var newCalendarValue = new CalendarValueModel
            {
                Name = formattedName,
                Color = createCalendarValueForm.Color,
                Archived = false,
                SortOrder = valuesCount,
                CalendarId = targetCalendar.Id,
            };

            _db.CalendarValues.Add(newCalendarValue);
            await _db.SaveChangesAsync();
            return Ok(newCalendarValue);
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditCalendarValue([FromBody] EditCalendarValueDTO editCalendarValueForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendarValue = await _db.CalendarValues
                .Include(cv => cv.Calendar)
                .Where(cv => cv.Id == editCalendarValueForm.CalendarValueId)
                .Where(cv => cv.Calendar.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetCalendarValue == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Calendar does not exists"));
            }

            targetCalendarValue.Name = editCalendarValueForm.Name;
            targetCalendarValue.Color = editCalendarValueForm.Color;
            await _db.SaveChangesAsync();

            return Ok(targetCalendarValue);
        }

        [HttpPost("change-order")]
        public async Task<IActionResult> ChangeCalendarValuesOrder([FromBody] ChangeElementsOrderDTO changeCalendarValuesOrderForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var calendarValues = await _db.CalendarValues
                .Include(cv => cv.Calendar)
                .Where(cv => changeCalendarValuesOrderForm.Elements.Contains(cv.Id))
                .OrderBy(cv => changeCalendarValuesOrderForm.Elements.IndexOf(cv.Id))
                .Where(cv => cv.Calendar.UserId == user!.Id)
                .ToListAsync();

            var allCalendarValuesBelongToOneStatus = calendarValues.All(s => !s.Archived);

            if (!allCalendarValuesBelongToOneStatus)
            {
                return BadRequest(new ApiError(InternalError.InvalidForm, "All calendar values must be active"));
            }

            var num = 0;
            foreach (var calendarValue in calendarValues)
            {
                calendarValue.SortOrder = num;
                num++;
            }
            await _db.SaveChangesAsync();

            return Ok(calendarValues);
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveCalendarValue([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendarValue = await _db.CalendarValues
                .Include(cv => cv.Calendar)
                .Include(cv => cv.CalendarRecords)
                .Where(cv => cv.Id == idForm.Id)
                .Where(cv => cv.Calendar.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetCalendarValue == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Calendar value does not exists"));
            }

            if (targetCalendarValue.CalendarRecords.Any())
            {
                return BadRequest(new ApiError(InternalError.InvalidForm, "To delete, the value must have no records"));
            }

            var calendarValueArchiveStatus = targetCalendarValue.Archived;

            _db.CalendarValues.Remove(targetCalendarValue);
            await _db.SaveChangesAsync();

            var otherCalendarValues = await _db.CalendarValues
                .Include(cv => cv.Calendar)
                .Where(cv => cv.Archived == calendarValueArchiveStatus)
                .Where(cv => cv.Calendar.UserId == user!.Id)
                .OrderBy(cv => cv.SortOrder)
                .ToListAsync();

            var num = 0;
            foreach (var calendarValue in otherCalendarValues)
            {
                calendarValue.SortOrder = num;
                num++;
            }
            await _db.SaveChangesAsync();

            return Ok("Calendar value has been deleted");
        }

        [HttpPost("change-archive-status")]
        public async Task<IActionResult> ChangeCalendarValueArchiveStatus([FromBody] ChangeArchiveStatusDTO changeArchiveStatusForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendarValue = await _db.CalendarValues
                .Include(cv => cv.Calendar)
                .Where(cv => cv.Id == changeArchiveStatusForm.Id)
                .Where(cv => cv.Archived != changeArchiveStatusForm.Status)
                .Where(cv => cv.Calendar.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetCalendarValue == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Calendar value does not exists"));
            }

            var allCalendarValuesInNewStatus = await _db.CalendarValues
                .Include(cv => cv.Calendar)
                .Where(cv => cv.Archived == changeArchiveStatusForm.Status)
                .Where(cv => cv.Calendar.UserId == user!.Id)
                .CountAsync();

            targetCalendarValue.Archived = changeArchiveStatusForm.Status;
            targetCalendarValue.SortOrder = allCalendarValuesInNewStatus;
            await _db.SaveChangesAsync();

            var otherCalendarValuesInOldStatus = await _db.CalendarValues
                .Include(cv => cv.Calendar)
                .Where(cv => cv.Archived != changeArchiveStatusForm.Status)
                .Where(cv => cv.Calendar.UserId == user!.Id)
                .OrderBy(cv => cv.SortOrder)
                .ToListAsync();

            var num = 0;
            foreach (var calendarValue in otherCalendarValuesInOldStatus)
            {
                calendarValue.SortOrder = num;
                num++;
            }
            await _db.SaveChangesAsync();

            return Ok(targetCalendarValue);
        }
    }
}
