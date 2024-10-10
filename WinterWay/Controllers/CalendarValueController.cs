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

        public CalendarValueController(UserManager<UserModel> userManager, CalendarService calendarService, ApplicationContext db)
        {
            _userManager = userManager;
            _calendarService = calendarService;
            _db = db;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCalendarValue([FromBody] CreateCalendarValueDTO createCalendarValueForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendar = _db.Calendars
                .Where(c => c.Id == createCalendarValueForm.CalendarId)
                .Where(c => c.Type == CalendarType.Fixed)
                .Where(c => !c.Archived)
                .Where(c => c.UserId == user!.Id)
                .FirstOrDefault();

            if (targetCalendar == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "No matching calendars found"));
            }

            var formattedName = createCalendarValueForm.Name.Trim();

            var isNameExists = _db.CalendarValues
                .Include(cv => cv.Calendar)
                .Where(cv => cv.Name == formattedName)
                .Where(cv => cv.CalendarId == createCalendarValueForm.CalendarId)
                .Where(cv => cv.Calendar.UserId == user!.Id)
                .Any();

            if (isNameExists)
            {
                return BadRequest(new ApiError(InternalError.InvalidForm, "A value with this name already exists in this calendar"));
            }

            var valuesCount = _db.CalendarValues
                .Include(c => c.Calendar)
                .Where(cv => cv.CalendarId == createCalendarValueForm.CalendarId)
                .Where(cv => cv.Calendar.UserId == user!.Id)
                .Count();

            var newCalendarValue = new CalendarValueModel
            {
                Name = formattedName,
                Color = createCalendarValueForm.Color,
                Archived = false,
                SortOrder = valuesCount,
                CalendarId = targetCalendar.Id,
            };

            _db.CalendarValues.Add(newCalendarValue);
            _db.SaveChanges();
            return Ok(newCalendarValue);
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditCalendarValue([FromBody] EditCalendarValueDTO editCalendarValueForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendarValue = _db.CalendarValues
                .Include(cv => cv.Calendar)
                .Where(cv => cv.Id == editCalendarValueForm.CalendarValueId)
                .Where(cv => cv.Calendar.UserId == user!.Id)
                .FirstOrDefault();

            if (targetCalendarValue == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Calendar does not exists"));
            }

            targetCalendarValue.Name = editCalendarValueForm.Name;
            targetCalendarValue.Color = editCalendarValueForm.Color;
            _db.SaveChanges();

            return Ok(targetCalendarValue);
        }

        [HttpPost("change-order")]
        public async Task<IActionResult> ChangeCalendarValuesOrder([FromBody] ChangeElementsOrderDTO changeCalendarValuesOrderForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var calendarValues = _db.CalendarValues
                .Include(cv => cv.Calendar)
                .Where(cv => changeCalendarValuesOrderForm.Elements.Contains(cv.Id))
                .OrderBy(cv => changeCalendarValuesOrderForm.Elements.IndexOf(cv.Id))
                .Where(cv => cv.Calendar.UserId == user!.Id)
                .ToList();

            var allCalendarValuesBelongToOneStatus = calendarValues.All(s => s.Archived == false);

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
            _db.SaveChanges();

            return Ok(calendarValues);
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveCalendarValue([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendarValue = _db.CalendarValues
                .Include(cv => cv.Calendar)
                .Include(cv => cv.CalendarRecords)
                .Where(cv => cv.Id == idForm.Id)
                .Where(cv => cv.Calendar.UserId == user!.Id)
                .FirstOrDefault();

            if (targetCalendarValue == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Calendar value does not exists"));
            }

            if (targetCalendarValue.CalendarRecords.Count() > 0)
            {
                return BadRequest(new ApiError(InternalError.InvalidForm, "To delete, the value must have no records"));
            }

            var calendarValueArchiveStatus = targetCalendarValue.Archived;

            _db.CalendarValues.Remove(targetCalendarValue);
            _db.SaveChanges();

            var otherCalendarValues = _db.CalendarValues
                .Include(cv => cv.Calendar)
                .Where(cv => cv.Archived == calendarValueArchiveStatus)
                .Where(cv => cv.Calendar.UserId == user!.Id)
                .OrderBy(cv => cv.SortOrder)
                .ToList();

            var num = 0;
            foreach (var calendarValue in otherCalendarValues)
            {
                calendarValue.SortOrder = num;
                num++;
            }
            _db.SaveChanges();

            return Ok("Calendar value has been deleted");
        }

        [HttpPost("change-archive-status")]
        public async Task<IActionResult> ChangeCalendarValueArchiveStatus([FromBody] ChangeArchiveStatusDTO changeArchiveStatusForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendarValue = _db.CalendarValues
                .Include(cv => cv.Calendar)
                .Where(cv => cv.Id == changeArchiveStatusForm.Id)
                .Where(cv => cv.Archived != changeArchiveStatusForm.Status)
                .Where(cv => cv.Calendar.UserId == user!.Id)
                .FirstOrDefault();

            if (targetCalendarValue == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Calendar value does not exists"));
            }

            var allCalendarValuesInNewStatus = _db.CalendarValues
                .Include(cv => cv.Calendar)
                .Where(cv => cv.Archived == changeArchiveStatusForm.Status)
                .Where(cv => cv.Calendar.UserId == user!.Id)
                .Count();

            targetCalendarValue.Archived = changeArchiveStatusForm.Status;
            targetCalendarValue.SortOrder = allCalendarValuesInNewStatus;
            _db.SaveChanges();

            var otherCalendarValuesInOldStatus = _db.CalendarValues
                .Include(cv => cv.Calendar)
                .Where(cv => cv.Archived != changeArchiveStatusForm.Status)
                .Where(cv => cv.Calendar.UserId == user!.Id)
                .OrderBy(cv => cv.SortOrder)
                .ToList();

            var num = 0;
            foreach (var calendarValue in otherCalendarValuesInOldStatus)
            {
                calendarValue.SortOrder = num;
                num++;
            }
            _db.SaveChanges();

            return Ok(targetCalendarValue);
        }
    }
}
