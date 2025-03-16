using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterWay.Data;
using WinterWay.Enums;
using WinterWay.Models.Database.Auth;
using WinterWay.Models.Database.Calendar;
using WinterWay.Models.DTOs.Requests.Calendar;
using WinterWay.Models.DTOs.Requests.Shared;
using WinterWay.Models.DTOs.Responses.Shared;
using WinterWay.Services;

namespace WinterWay.Controllers.Calendar
{
    [Route("api/[controller]")]
    public class CalendarRecordController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<UserModel> _userManager;
        private readonly CalendarService _calendarService;

        public CalendarRecordController(ApplicationContext db, UserManager<UserModel> userManager, CalendarService calendarService)
        {
            _db = db;
            _userManager = userManager;
            _calendarService = calendarService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCalendarRecord([FromBody] CreateCalendarRecordDTO createCalendarRecordForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendar = await _db.Calendars
                .Where(c => c.Id == createCalendarRecordForm.CalendarId)
                .Where(c => !c.Archived)
                .Where(c => c.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetCalendar == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Calendar does not exists"));
            }

            var validDay = _calendarService.ParseDate(createCalendarRecordForm.Date, out DateOnly targetDay);

            if (!validDay)
            {
                return BadRequest(new ApiErrorDTO(InternalError.InvalidForm, "Invalid date format"));
            }

            var isDateAlreadyExists = await _db.CalendarRecords
                .Where(cr => cr.CalendarId == targetCalendar.Id)
                .Where(cr => cr.Date == targetDay)
                .AnyAsync();

            if (isDateAlreadyExists)
            {
                return BadRequest(new ApiErrorDTO(InternalError.InvalidForm, "A record for this day already exists in this calendar"));
            }

            if (!await _calendarService.Validate(createCalendarRecordForm.SerializedValue, targetCalendar.Id, targetCalendar.Type))
            {
                return BadRequest(new ApiErrorDTO(InternalError.InvalidForm, "Invalid value"));
            }

            var newRecord = new CalendarRecordModel(targetDay, createCalendarRecordForm.Text, targetCalendar.Id, targetCalendar.Type, createCalendarRecordForm.SerializedValue);

            if (createCalendarRecordForm.FillDefaultValues && targetCalendar.SerializedDefaultValue != null)
            {
                var lastCalendarRecord = await _db.CalendarRecords
                    .Where(cr => cr.CalendarId == targetCalendar.Id)
                    .Where(cr => cr.Date < newRecord.Date)
                    .OrderByDescending(cr => cr.Date)
                    .FirstOrDefaultAsync();

                if (lastCalendarRecord == null)
                {
                    return BadRequest(new ApiErrorDTO(InternalError.InvalidForm, "Unable to fill a calendar with less than two records"));
                }

                List<CalendarRecordModel> daysBetween = new List<CalendarRecordModel>();

                for (var stepDay = lastCalendarRecord.Date.AddDays(1); stepDay < newRecord.Date; stepDay = stepDay.AddDays(1))
                {
                    var dayBetweenRecord = new CalendarRecordModel(stepDay, null, targetCalendar.Id, targetCalendar.Type, targetCalendar.SerializedDefaultValue);
                    daysBetween.Add(dayBetweenRecord);
                }

                _db.CalendarRecords.AddRange(daysBetween);
            }

            _db.CalendarRecords.Add(newRecord);
            await _db.SaveChangesAsync();
            return Ok(newRecord);
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditCalendarRecord([FromBody] EditCalendarRecordDTO editCalendarRecordForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendarRecord = await _db.CalendarRecords
                .Include(cr => cr.Calendar)
                .Where(cr => cr.Id == editCalendarRecordForm.CalendarRecordId)
                .Where(cr => cr.Calendar.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetCalendarRecord == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Calendar record does not exists"));
            }

            targetCalendarRecord.Text = editCalendarRecordForm.Text;
            targetCalendarRecord.SetNewValue(editCalendarRecordForm.SerializedValue, targetCalendarRecord.Calendar.Type);
            await _db.SaveChangesAsync();

            return Ok(targetCalendarRecord);
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveCalendarRecord([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendarRecord = await _db.CalendarRecords
                .Include(cr => cr.Calendar)
                .Where(cr => cr.Id == idForm.Id)
                .Where(cr => cr.Calendar.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetCalendarRecord == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Calendar record does not exists"));
            }

            _db.CalendarRecords.Remove(targetCalendarRecord);
            await _db.SaveChangesAsync();
            return Ok(new ApiSuccessDTO("CalendarRecordDeletion"));
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllCalendarRecords([FromBody] GetCalendarRecordsDTO getCalendarRecordsForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var dateStart = DateOnly.MinValue;
            var dateEnd = DateOnly.MaxValue;

            if (getCalendarRecordsForm.DateStart != null && _calendarService.ParseDate(getCalendarRecordsForm.DateStart, out DateOnly startDate))
            {
                dateStart = startDate;
            }

            if (getCalendarRecordsForm.DateEnd != null && _calendarService.ParseDate(getCalendarRecordsForm.DateEnd, out DateOnly endDate))
            {
                dateEnd = endDate;
            }

            var maxCountOfElements = int.MaxValue;

            if (getCalendarRecordsForm.MaxCount != null && getCalendarRecordsForm.MaxCount > 0)
            {
                maxCountOfElements = getCalendarRecordsForm.MaxCount.Value;
            }

            var targetRecords = _db.CalendarRecords
                .Include(cr => cr.Calendar)
                .Where(cr => cr.CalendarId == getCalendarRecordsForm.CalendarId)
                .Where(cr => cr.Calendar.UserId == user!.Id)
                .Where(cr => cr.Date > dateStart)
                .Where(cr => cr.Date < dateEnd)
                .OrderByDescending(cr => cr.Date)
                .Take(maxCountOfElements);

            return Ok(targetRecords);
        }
    }
}
