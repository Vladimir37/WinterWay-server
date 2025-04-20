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
        private readonly DateTimeService _dateTimeService;

        public CalendarRecordController(ApplicationContext db, UserManager<UserModel> userManager, CalendarService calendarService, DateTimeService dateTimeService)
        {
            _db = db;
            _userManager = userManager;
            _calendarService = calendarService;
            _dateTimeService = dateTimeService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCalendarRecord([FromBody] CreateCalendarRecordDTO createCalendarRecordForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendar = await _db.Calendars
                .Include(c => c.DefaultRecord)
                    .ThenInclude(dr => dr.BooleanVal)
                .Include(c => c.DefaultRecord)
                    .ThenInclude(dr => dr.NumericVal)
                .Include(c => c.DefaultRecord)
                    .ThenInclude(dr => dr.TimeVal)
                .Include(c => c.DefaultRecord)
                    .ThenInclude(dr => dr.FixedVal)
                .Where(c => c.Id == createCalendarRecordForm.CalendarId)
                .Where(c => !c.Archived)
                .Where(c => c.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetCalendar == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Calendar does not exists"));
            }

            var validDay = _dateTimeService.ParseDate(createCalendarRecordForm.Date, out DateOnly targetDay);

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

            var newRecord = _calendarService.GetCalendarRecord(
                createCalendarRecordForm.CalendarId,
                false,
                targetDay,
                createCalendarRecordForm.Text,
                targetCalendar.Type,
                createCalendarRecordForm.SerializedValue
            );
            if (newRecord == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.InvalidForm, "Invalid type of default value"));
            }

            if (createCalendarRecordForm.FillDefaultValues && targetCalendar.DefaultRecord != null)
            {
                var lastCalendarRecord = await _db.CalendarRecords
                    .Where(cr => cr.CalendarId == targetCalendar.Id)
                    .Where(cr => cr.Date != null && !cr.IsDefault)
                    .Where(cr => cr.Date < newRecord.Date)
                    .OrderByDescending(cr => cr.Date)
                    .FirstOrDefaultAsync();

                if (lastCalendarRecord == null)
                {
                    return BadRequest(new ApiErrorDTO(InternalError.InvalidForm, "Unable to fill a calendar with less than two records"));
                }

                List<CalendarRecordModel> daysBetween = new List<CalendarRecordModel>();

                for (var stepDay = lastCalendarRecord.Date!.Value.AddDays(1); stepDay < newRecord.Date; stepDay = stepDay.AddDays(1))
                {
                    var dayBetweenRecord = _calendarService.GetRecordCopy(targetCalendar.DefaultRecord, stepDay, targetCalendar.Type);
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
                .Include(cr => cr.BooleanVal)
                .Include(cr => cr.NumericVal)
                .Include(cr => cr.TimeVal)
                .Include(cr => cr.FixedVal)
                .Where(cr => cr.Id == editCalendarRecordForm.CalendarRecordId)
                .Where(cr => cr.Calendar.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetCalendarRecord == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Calendar record does not exists"));
            }
            
            if (!await _calendarService.Validate(editCalendarRecordForm.SerializedValue, targetCalendarRecord.CalendarId, targetCalendarRecord.Calendar.Type))
            {
                return BadRequest(new ApiErrorDTO(InternalError.InvalidForm, "Invalid value"));
            }

            var editedRecord = _calendarService.GetCalendarRecord(
                targetCalendarRecord.CalendarId,
                false,
                targetCalendarRecord.Date,
                editCalendarRecordForm.Text,
                targetCalendarRecord.Calendar.Type,
                editCalendarRecordForm.SerializedValue
            );
            if (editedRecord == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.InvalidForm, "Invalid type of default value"));
            }
            
            if (targetCalendarRecord.BooleanVal != null)
            {
                _db.CalendarRecordBooleans.Remove(targetCalendarRecord.BooleanVal);
            }

            if (targetCalendarRecord.NumericVal != null)
            {
                _db.CalendarRecordNumerics.Remove(targetCalendarRecord.NumericVal);
            }

            if (targetCalendarRecord.TimeVal != null)
            {
                _db.CalendarRecordTimes.Remove(targetCalendarRecord.TimeVal);
            }

            if (targetCalendarRecord.FixedVal != null)
            {
                _db.CalendarRecordFixeds.Remove(targetCalendarRecord.FixedVal);
            }
            _db.CalendarRecords.Remove(targetCalendarRecord);
            await _db.CalendarRecords.AddAsync(editedRecord);
            await _db.SaveChangesAsync();
            return Ok(editedRecord);
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveCalendarRecord([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendarRecord = await _db.CalendarRecords
                .Include(cr => cr.Calendar)
                .Include(cr => cr.BooleanVal)
                .Include(cr => cr.NumericVal)
                .Include(cr => cr.TimeVal)
                .Include(cr => cr.FixedVal)
                .Where(cr => cr.Id == idForm.Id)
                .Where(cr => cr.Calendar.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetCalendarRecord == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Calendar record does not exists"));
            }

            if (targetCalendarRecord.BooleanVal != null)
            {
                _db.CalendarRecordBooleans.Remove(targetCalendarRecord.BooleanVal);
            }

            if (targetCalendarRecord.NumericVal != null)
            {
                _db.CalendarRecordNumerics.Remove(targetCalendarRecord.NumericVal);
            }

            if (targetCalendarRecord.TimeVal != null)
            {
                _db.CalendarRecordTimes.Remove(targetCalendarRecord.TimeVal);
            }

            if (targetCalendarRecord.FixedVal != null)
            {
                _db.CalendarRecordFixeds.Remove(targetCalendarRecord.FixedVal);
            }
            _db.CalendarRecords.Remove(targetCalendarRecord);
            await _db.SaveChangesAsync();
            return Ok(new ApiSuccessDTO("CalendarRecordDeletion"));
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllCalendarRecords([FromQuery] GetCalendarRecordsDTO getCalendarRecordsForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var dateStart = DateOnly.MinValue;
            var dateEnd = DateOnly.MaxValue;

            if (getCalendarRecordsForm.DateStart != null && _dateTimeService.ParseDate(getCalendarRecordsForm.DateStart, out DateOnly startDate))
            {
                dateStart = startDate;
            }

            if (getCalendarRecordsForm.DateEnd != null && _dateTimeService.ParseDate(getCalendarRecordsForm.DateEnd, out DateOnly endDate))
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
                .Include(cr => cr.BooleanVal)
                .Include(cr => cr.NumericVal)
                .Include(cr => cr.TimeVal)
                .Include(cr => cr.FixedVal)
                    .ThenInclude(fv => fv.FixedValue)
                .Where(cr => cr.CalendarId == getCalendarRecordsForm.CalendarId)
                .Where(cr => cr.Calendar.UserId == user!.Id)
                .Where(cr => !cr.IsDefault)
                .Where(cr => cr.Date != null)
                .Where(cr => cr.Date > dateStart)
                .Where(cr => cr.Date < dateEnd)
                .OrderByDescending(cr => cr.Date)
                .Take(maxCountOfElements)
                .ToList();
            
            return Ok(targetRecords);
        }
    }
}
