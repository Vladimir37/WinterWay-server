using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterWay.Data;
using WinterWay.Enums;
using WinterWay.Models.Database.Auth;
using WinterWay.Models.Database.Mood;
using WinterWay.Models.DTOs.Requests.Mood;
using WinterWay.Models.DTOs.Requests.Shared;
using WinterWay.Models.DTOs.Responses.Mood;
using WinterWay.Models.DTOs.Responses.Shared;
using WinterWay.Services;

namespace WinterWay.Controllers.Mood
{
    [Route("api/[controller]")]
    public class MoodRecordController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<UserModel> _userManager;
        private readonly DateTimeService _dateTimeService;

        public MoodRecordController(ApplicationContext db, UserManager<UserModel> userManager, DateTimeService dateTimeService)
        {
            _db = db;
            _userManager = userManager;
            _dateTimeService = dateTimeService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateMoodRecord([FromBody] CreateMoodRecordDTO createMoodRecordForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var validDay = _dateTimeService.ParseDate(createMoodRecordForm.Date, out DateOnly targetDay);

            if (!validDay)
            {
                return BadRequest(new ApiErrorDTO(InternalError.InvalidForm, "Invalid date format"));
            }

            MoodTagModel? targetTag = null;
            if (createMoodRecordForm.TagId != null)
            {
                targetTag = await _db.MoodTags
                    .Where(t => t.Id == createMoodRecordForm.TagId)
                    .Where(t => t.UserId == user!.Id)
                    .Where(t => !t.Archived)
                    .FirstOrDefaultAsync();

                if (targetTag == null)
                {
                    return BadRequest(new ApiErrorDTO(InternalError.InvalidForm, "Available mood tag does not exists"));
                }
            }

            var newRecord = new MoodRecordModel
            {
                Type = createMoodRecordForm.Type,
                Text = createMoodRecordForm.Text,
                Date = targetDay,
                CreationDate = DateTime.UtcNow,
                UserId = user!.Id,
                Tag = targetTag,
            };

            _db.MoodRecords.Add(newRecord);
            await _db.SaveChangesAsync();
            return Ok(newRecord);
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditMoodRecord([FromBody] EditMoodRecordDTO editMoodRecordForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetRecord = await _db.MoodRecords
                .Include(r => r.Tag)
                .Where(r => r.Id == editMoodRecordForm.RecordId)
                .Where(r => r.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetRecord == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Mood record does not exists"));
            }

            if (editMoodRecordForm.TagId != targetRecord.TagId)
            {
                MoodTagModel? targetTag = null;
                if (editMoodRecordForm.TagId != null)
                {
                    targetTag = await _db.MoodTags
                        .Where(t => t.Id == editMoodRecordForm.TagId)
                        .Where(t => t.UserId == user!.Id)
                        .Where(t => !t.Archived)
                        .FirstOrDefaultAsync();

                    if (targetTag == null)
                    {
                        return BadRequest(new ApiErrorDTO(InternalError.InvalidForm, "Available mood tag does not exists"));
                    }
                }
                targetRecord.Tag = targetTag;
            }

            targetRecord.Type = editMoodRecordForm.Type;
            targetRecord.Text = editMoodRecordForm.Text;
            await _db.SaveChangesAsync();
            return Ok(targetRecord);
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveMoodRecord([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetRecord = await _db.MoodRecords
                .Where(r => r.Id == idForm.Id)
                .Where(r => r.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetRecord == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Mood record does not exists"));
            }

            _db.MoodRecords.Remove(targetRecord);
            await _db.SaveChangesAsync();
            return Ok(new ApiSuccessDTO("MoodRecordDeletion"));
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllMoodRecords([FromQuery] GetMoodRecordsDTO getMoodRecordsForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var dateStart = DateOnly.MinValue;
            var dateEnd = DateOnly.MaxValue;

            if (getMoodRecordsForm.DateStart != null && _dateTimeService.ParseDate(getMoodRecordsForm.DateStart, out DateOnly startDate))
            {
                dateStart = startDate;
            }

            if (getMoodRecordsForm.DateEnd != null && _dateTimeService.ParseDate(getMoodRecordsForm.DateEnd, out DateOnly endDate))
            {
                dateEnd = endDate;
            }

            var maxCountOfElements = int.MaxValue;

            if (getMoodRecordsForm.MaxCount != null && getMoodRecordsForm.MaxCount > 0)
            {
                maxCountOfElements = getMoodRecordsForm.MaxCount.Value;
            }

            var targetRecords = await _db.MoodRecords
                .Include(r => r.Tag)
                .Where(r => r.UserId == user!.Id)
                .Where(r => r.Date > dateStart)
                .Where(r => r.Date < dateEnd)
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            var groupedByDay = targetRecords
                .GroupBy(r => r.Date)
                .OrderByDescending(g => g.Key)
                .Take(maxCountOfElements)
                .Select(g => new MoodDayDTO(
                    g.Key,
                    g.Where(r => r.Type == MoodType.Good).ToList(),
                    g.Where(r => r.Type == MoodType.Bad).ToList()
                ))
                .ToList();

            return Ok(groupedByDay);
        }
    }
}
