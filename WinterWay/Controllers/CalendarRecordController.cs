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
    public class CalendarRecordController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<UserModel> _userManager;
        private readonly CalendarService _calendarService;

        public CalendarRecordController(UserManager<UserModel> userManager, CalendarService calendarService, ApplicationContext db)
        {
            _userManager = userManager;
            _calendarService = calendarService;
            _db = db;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCalendarRecord([FromBody] CreateCalendarRecordDTO createCalendarRecordForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendar = _db.Calendars
                .Where(c => c.Id == createCalendarRecordForm.CalendarId)
                .Where(c => !c.Archived)
                .Where(c => c.UserId == user!.Id)
                .FirstOrDefault();

            if (targetCalendar == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Calendar does not exists"));
            }

            var validDay = _calendarService.ParseDate(createCalendarRecordForm.Date, out DateOnly targetDay);

            if (!validDay)
            {
                return BadRequest(new ApiError(InternalError.InvalidForm, "Invalid date format"));
            }

            var isDateAlreadyExists = _db.CalendarRecords
                .Where(cr => cr.CalendarId == targetCalendar.Id)
                .Where(cr => cr.Date == targetDay)
                .Any();

            if (isDateAlreadyExists)
            {
                return BadRequest(new ApiError(InternalError.InvalidForm, "A record for this day already exists in this calendar"));
            }

            if (!_calendarService.Validate(createCalendarRecordForm.SerializedValue, targetCalendar.Id, targetCalendar.Type))
            {
                return BadRequest(new ApiError(InternalError.InvalidForm, "Invalid value"));
            }

            var newRecord = new CalendarRecordModel(targetDay.Value, createCalendarRecordForm.Text, targetCalendar.Id, targetCalendar.Type, createCalendarRecordForm.SerializedValue);

            if (createCalendarRecordForm.FillDefaultValues && targetCalendar.SerializedDefaultValue != null)
            {
                var lastCalendarRecord = _db.CalendarRecords
                    .Where(cr => cr.CalendarId == targetCalendar.Id)
                    .OrderByDescending(cr => cr.Date)
                    .FirstOrDefault();

                if (lastCalendarRecord == null)
                {
                    return BadRequest(new ApiError(InternalError.InvalidForm, "Unable to fill a calendar with less than two records"));
                }

                List<CalendarRecordModel> daysBetween = new List<CalendarRecordModel>();

                for (var stepDay = lastCalendarRecord.Date.AddDays(1); stepDay < newRecord.Date; stepDay.AddDays(1))
                {
                    var dayBetweenRecord = new CalendarRecordModel(stepDay, null, targetCalendar.Id, targetCalendar.Type, targetCalendar.SerializedDefaultValue);
                    daysBetween.Add(dayBetweenRecord);
                }

                _db.CalendarRecords.AddRange(daysBetween);
            }

            _db.CalendarRecords.Add(newRecord);
            _db.SaveChanges();
            return Ok(newRecord);
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditCalendarRecord([FromBody] EditCalendarRecordDTO editCalendarRecordForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendarRecord = _db.CalendarRecords
                .Include(cr => cr.Calendar)
                .Where(cr => cr.Id == editCalendarRecordForm.CalendarRecordId)
                .Where(cr => cr.Calendar.UserId == user!.Id)
                .FirstOrDefault();

            if (targetCalendarRecord == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Calendar record does not exists"));
            }

            targetCalendarRecord.Text = editCalendarRecordForm.Text;
            targetCalendarRecord.SetNewValue(editCalendarRecordForm.SerializedValue, targetCalendarRecord.Calendar.Type);
            _db.SaveChanges();

            return Ok(targetCalendarRecord);
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveCalendarRecord([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendarRecord = _db.CalendarRecords
                .Include(cr => cr.Calendar)
                .Where(cr => cr.Id == idForm.Id)
                .Where(cr => cr.Calendar.UserId == user!.Id)
                .FirstOrDefault();

            if (targetCalendarRecord == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Calendar record does not exists"));
            }

            _db.CalendarRecords.Remove(targetCalendarRecord);
            _db.SaveChanges();
            return Ok("Calendar record has been deleted");
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

            if (getCalendarRecordsForm.MaxCount != null)
            {
                maxCountOfElements = getCalendarRecordsForm.MaxCount.Value;
            }

            var targetRecords = _db.CalendarRecords
                .Include(cr => cr.Calendar)
                .Where(cr => cr.Id == getCalendarRecordsForm.CalendarId)
                .Where(cr => cr.Calendar.UserId == user!.Id)
                .Where(cr => cr.Date > dateStart)
                .Where(cr => cr.Date < dateEnd)
                .OrderByDescending(cr => cr.Date)
                .Take(maxCountOfElements);

            return Ok(targetRecords);
        }
    }
}
