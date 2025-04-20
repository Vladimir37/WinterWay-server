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

            var calendarsTotal = await _db.Calendars
                .Where(b => b.UserId == user!.Id)
                .Where(b => !b.Archived)
                .CountAsync();
            
            var newCalendar = new CalendarModel
            {
                Name = createCalendarForm.Name,
                Type = createCalendarForm.Type,
                Color = createCalendarForm.Color,
                SortOrder = calendarsTotal,
                Archived = false,
                NotificationActive = true,
                CreationDate = DateTime.UtcNow,
                ArchivingDate = null,
                UserId = user!.Id
            };

            _db.Calendars.Add(newCalendar);
            await _db.SaveChangesAsync();
            return Ok(newCalendar);
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditCalendar([FromBody] EditCalendarDTO editCalendarForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendar = await _db.Calendars
                .Where(c => c.Id == editCalendarForm.CalendarId)
                .Where(c => c.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetCalendar == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Calendar does not exists"));
            }

            targetCalendar.Name = editCalendarForm.Name;
            targetCalendar.Color = editCalendarForm.Color;
            targetCalendar.NotificationActive = editCalendarForm.NotificationActive;
            await _db.SaveChangesAsync();
            return Ok(targetCalendar);
        }

        [HttpPost("edit-default-value")]
        public async Task<IActionResult> EditCalendarDefaultValue([FromBody] EditCalendarDefaultValueDTO editCalendarDefaultValueForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendar = await _db.Calendars
                .Include(c => c.DefaultRecord)
                    .ThenInclude(cr => cr.BooleanVal)
                .Include(c => c.DefaultRecord)
                    .ThenInclude(cr => cr.NumericVal)
                .Include(c => c.DefaultRecord)
                    .ThenInclude(cr => cr.TimeVal)
                .Include(c => c.DefaultRecord)
                    .ThenInclude(cr => cr.FixedVal)
                .Where(c => c.Id == editCalendarDefaultValueForm.CalendarId)
                .Where(c => c.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetCalendar == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Calendar does not exists"));
            }
            
            if (!await _calendarService.Validate(editCalendarDefaultValueForm.SerializedDefaultValue, targetCalendar.Id, targetCalendar.Type))
            {
                return BadRequest(new ApiErrorDTO(InternalError.InvalidForm, "Invalid default value"));
            }
            
            var newDefaultRecord = _calendarService.GetCalendarRecord(
                editCalendarDefaultValueForm.CalendarId, 
                true,
                null,
                String.Empty, 
                targetCalendar.Type,
                editCalendarDefaultValueForm.SerializedDefaultValue
            );

            if (newDefaultRecord == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.InvalidForm, "Invalid type of default value"));
            }

            if (targetCalendar.DefaultRecordId != null && targetCalendar.DefaultRecord != null)
            {
                if (targetCalendar.DefaultRecord.BooleanVal != null)
                {
                    _db.CalendarRecordBooleans.Remove(targetCalendar.DefaultRecord.BooleanVal);
                }

                if (targetCalendar.DefaultRecord.NumericVal != null)
                {
                    _db.CalendarRecordNumerics.Remove(targetCalendar.DefaultRecord.NumericVal);
                }

                if (targetCalendar.DefaultRecord.TimeVal != null)
                {
                    _db.CalendarRecordTimes.Remove(targetCalendar.DefaultRecord.TimeVal);
                }

                if (targetCalendar.DefaultRecord.FixedVal != null)
                {
                    _db.CalendarRecordFixeds.Remove(targetCalendar.DefaultRecord.FixedVal);
                }
                _db.CalendarRecords.Remove(targetCalendar.DefaultRecord);
                await _db.SaveChangesAsync();
            }
            
            targetCalendar.DefaultRecord = newDefaultRecord;
            await _db.SaveChangesAsync();
            return Ok(targetCalendar);
        }

        [HttpPost("remove-default-value")]
        public async Task<IActionResult> RemoveCalendarDefaultValue([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendar = await _db.Calendars
                .Include(c => c.DefaultRecord)
                    .ThenInclude(cr => cr.BooleanVal)
                .Include(c => c.DefaultRecord)
                    .ThenInclude(cr => cr.NumericVal)
                .Include(c => c.DefaultRecord)
                    .ThenInclude(cr => cr.TimeVal)
                .Include(c => c.DefaultRecord)
                    .ThenInclude(cr => cr.FixedVal)
                .Where(c => c.Id == idForm.Id)
                .Where(c => c.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetCalendar == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Calendar does not exists"));
            }
            
            if (targetCalendar.DefaultRecordId != null && targetCalendar.DefaultRecord != null)
            {
                if (targetCalendar.DefaultRecord.BooleanVal != null)
                {
                    _db.CalendarRecordBooleans.Remove(targetCalendar.DefaultRecord.BooleanVal);
                }

                if (targetCalendar.DefaultRecord.NumericVal != null)
                {
                    _db.CalendarRecordNumerics.Remove(targetCalendar.DefaultRecord.NumericVal);
                }

                if (targetCalendar.DefaultRecord.TimeVal != null)
                {
                    _db.CalendarRecordTimes.Remove(targetCalendar.DefaultRecord.TimeVal);
                }

                if (targetCalendar.DefaultRecord.FixedVal != null)
                {
                    _db.CalendarRecordFixeds.Remove(targetCalendar.DefaultRecord.FixedVal);
                }
                _db.CalendarRecords.Remove(targetCalendar.DefaultRecord);
            }
            
            await _db.SaveChangesAsync();
            return Ok(new ApiSuccessDTO("CalendarDefaultValueDeletion"));
        }

        [HttpPost("change-archive-status")]
        public async Task<IActionResult> ChangeArchiveStatus([FromBody] ChangeArchiveStatusDTO changeArchiveStatusForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendar = await _db.Calendars
                .Where(c => c.Id == changeArchiveStatusForm.Id)
                .Where(c => c.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetCalendar == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Calendar does not exists"));
            }

            if (targetCalendar.Archived == changeArchiveStatusForm.Status)
            {
                return BadRequest(new ApiErrorDTO(InternalError.InvalidForm, "The new calendar status is no different from the old one"));
            }

            var countOfCalendarsInNewStatus = await _db.Calendars
                .Where(c => c.Archived == changeArchiveStatusForm.Status)
                .Where(c => c.UserId == user!.Id)
                .CountAsync();

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
            await _db.SaveChangesAsync();

            var otherCalendarsInOldStatus = await _db.Calendars
                .Where(c => c.Archived != changeArchiveStatusForm.Status)
                .Where(c => c.UserId == user!.Id)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            var num = 0;
            foreach (var calendar in otherCalendarsInOldStatus)
            {
                calendar.SortOrder = num;
                num++;
            }
            await _db.SaveChangesAsync();
            return Ok(targetCalendar);
        }

        [HttpPost("change-sort-order")]
        public async Task<IActionResult> ChangeCalendarsOrder([FromBody] ChangeElementsOrderDTO changeCalendarsOrderForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var calendars = await _db.Calendars
                .Where(c => changeCalendarsOrderForm.Elements.Contains(c.Id))
                .OrderBy(c => changeCalendarsOrderForm.Elements.IndexOf(c.Id))
                .Where(c => c.UserId == user!.Id)
                .ToListAsync();

            var allCalendarsBelongToOneStatus = calendars.All(c => !c.Archived);

            if (!allCalendarsBelongToOneStatus)
            {
                return BadRequest(new ApiErrorDTO(InternalError.InvalidForm, "All calendars must be active"));
            }

            var num = 0;
            foreach (var calendar in calendars)
            {
                calendar.SortOrder = num;
                num++;
            }
            await _db.SaveChangesAsync();

            return Ok(calendars);
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveCalendar([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendar = await _db.Calendars
                .Include(c => c.CalendarValues)
                .Include(c => c.CalendarRecords)
                    .ThenInclude(cr => cr.BooleanVal)
                .Include(c => c.CalendarRecords)
                    .ThenInclude(cr => cr.NumericVal)
                .Include(c => c.CalendarRecords)
                    .ThenInclude(cr => cr.TimeVal)
                .Include(c => c.CalendarRecords)
                    .ThenInclude(cr => cr.FixedVal)
                .Where(c => c.Id == idForm.Id)
                .Where(c => c.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetCalendar == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Calendar does not exists"));
            }

            var removedCalendarArchiveStatus = targetCalendar.Archived;

            foreach (var record in targetCalendar.CalendarRecords)
            {
                if (record.BooleanVal != null)
                {
                    _db.CalendarRecordBooleans.Remove(record.BooleanVal);
                }

                if (record.NumericVal != null)
                {
                    _db.CalendarRecordNumerics.Remove(record.NumericVal);
                }

                if (record.TimeVal != null)
                {
                    _db.CalendarRecordTimes.Remove(record.TimeVal);
                }

                if (record.FixedVal != null)
                {
                    _db.CalendarRecordFixeds.Remove(record.FixedVal);
                }
                _db.CalendarRecords.Remove(record);
            }

            foreach (var value in targetCalendar.CalendarValues)
            {
                _db.CalendarValues.Remove(value);
            }

            _db.Calendars.Remove(targetCalendar);
            await _db.SaveChangesAsync();

            var otherCalendars = await _db.Calendars
                .Where(c => c.UserId == user!.Id)
                .Where(c => c.Archived == removedCalendarArchiveStatus)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            var num = 0;
            foreach (var calendar in otherCalendars)
            {
                calendar.SortOrder = num;
                num++;
            }
            await _db.SaveChangesAsync();

            return Ok(new ApiSuccessDTO("CalendarDeletion"));
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllCalendars()
        {
            var user = await _userManager.GetUserAsync(User);

            var allCalendars = await _db.Calendars
                .Include(c => c.CalendarRecords.Where(cr => cr.Date == DateOnly.FromDateTime(DateTime.UtcNow)))
                .Include(c => c.DefaultRecord)
                    .ThenInclude(cr => cr.BooleanVal)
                .Include(c => c.DefaultRecord)
                    .ThenInclude(cr => cr.NumericVal)
                .Include(c => c.DefaultRecord)
                    .ThenInclude(cr => cr.TimeVal)
                .Include(c => c.DefaultRecord)
                    .ThenInclude(cr => cr.FixedVal)
                        .ThenInclude(crf => crf.FixedValue)
                .Where(c => c.UserId == user!.Id)
                .ToListAsync();

            return Ok(allCalendars);
        }

        [HttpGet("get-one")]
        public async Task<IActionResult> GetOneCalendar([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetCalendar = await _db.Calendars
                .Include(c => c.CalendarRecords)
                .Include(c => c.CalendarValues)
                .Include(c => c.DefaultRecord)
                    .ThenInclude(cr => cr.BooleanVal)
                .Include(c => c.DefaultRecord)
                    .ThenInclude(cr => cr.NumericVal)
                .Include(c => c.DefaultRecord)
                    .ThenInclude(cr => cr.TimeVal)
                .Include(c => c.DefaultRecord)
                    .ThenInclude(cr => cr.FixedVal)
                .Where(c => c.Id == idForm.Id)
                .Where(c => c.UserId == user!.Id)
                .FirstOrDefaultAsync();

            return Ok(targetCalendar);
        }
    }
}
