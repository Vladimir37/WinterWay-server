using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterWay.Data;
using WinterWay.Enums;
using WinterWay.Models.Database.Auth;
using WinterWay.Models.DTOs.Requests.Diary;
using WinterWay.Models.DTOs.Requests.Shared;
using WinterWay.Models.DTOs.Responses.Shared;
using WinterWay.Services;

namespace WinterWay.Controllers.Diary
{
    [Route("api/[controller]")]
    public class DiaryRecordController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<UserModel> _userManager;
        private readonly DiaryService _diaryService;
        private readonly DateTimeService _dateTimeService;

        public DiaryRecordController(ApplicationContext db, UserManager<UserModel> userManager, DiaryService diaryService, DateTimeService dateTimeService)
        {
            _db = db;
            _userManager = userManager;
            _diaryService = diaryService;
            _dateTimeService = dateTimeService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateDiaryRecord([FromBody] DiaryRecordDTO diaryRecordForm)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var success = _diaryService.Transform(user!.Id, diaryRecordForm, true, out var error, out var recordModel);

            if (!success)
            {
                return BadRequest(error);
            }
            
            await _db.DiaryRecords.AddAsync(recordModel!);
            await _db.SaveChangesAsync();
            return Ok(recordModel);
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditDiaryRecord([FromBody] DiaryRecordDTO diaryRecordForm)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var success = _diaryService.Transform(user!.Id, diaryRecordForm, false, out var error, out var recordModel);

            if (!success)
            {
                return BadRequest(error);
            }
            
            _dateTimeService.ParseDate(diaryRecordForm.Date, out DateOnly targetDay);
            var oldDiary = await _db.DiaryRecords
                .Include(dr => dr.Groups)
                    .ThenInclude(dr => dr.Activities)
                .Where(dr => dr.Date == targetDay)
                .Where(dr => dr.UserId == user!.Id)
                .FirstOrDefaultAsync();

            foreach (var group in oldDiary!.Groups)
            {
                _db.DiaryRecordActivities.RemoveRange(group.Activities);
            }
            _db.DiaryRecordGroups.RemoveRange(oldDiary.Groups);
            _db.DiaryRecords.Remove(oldDiary);
            await _db.DiaryRecords.AddAsync(recordModel!);
            await _db.SaveChangesAsync();
            return Ok(recordModel);
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveDiaryRecord([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var targetDiaryRecord = _db.DiaryRecords
                .Include(dr => dr.Groups)
                    .ThenInclude(dr => dr.Activities)
                .Where(dr => dr.Id == idForm.Id)
                .Where(dr => dr.UserId == user!.Id)
                .FirstOrDefault();
            
            if (targetDiaryRecord == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Diary record does not exists"));
            }
            
            foreach (var group in targetDiaryRecord.Groups)
            {
                _db.DiaryRecordActivities.RemoveRange(group.Activities);
            }
            _db.DiaryRecordGroups.RemoveRange(targetDiaryRecord.Groups);
            _db.DiaryRecords.Remove(targetDiaryRecord);
            await _db.SaveChangesAsync();
            return Ok(new ApiSuccessDTO("DiaryRecordDeletion"));
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllDiaryRecords([FromQuery] GetDiaryRecordsDTO getDiaryRecordsForm)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var dateStart = DateOnly.MinValue;
            var dateEnd = DateOnly.MaxValue;

            if (getDiaryRecordsForm.DateStart != null && _dateTimeService.ParseDate(getDiaryRecordsForm.DateStart, out DateOnly startDate))
            {
                dateStart = startDate;
            }

            if (getDiaryRecordsForm.DateEnd != null && _dateTimeService.ParseDate(getDiaryRecordsForm.DateEnd, out DateOnly endDate))
            {
                dateEnd = endDate;
            }

            var maxCountOfElements = int.MaxValue;

            if (getDiaryRecordsForm.MaxCount != null && getDiaryRecordsForm.MaxCount > 0)
            {
                maxCountOfElements = getDiaryRecordsForm.MaxCount.Value;
            }
            
            var targetRecords = _db.DiaryRecords
                .Include(dr => dr.Groups)
                    .ThenInclude(dg => dg.Activities)
                .Where(dr => dr.UserId == user!.Id)
                .Where(dr => dr.Date > dateStart)
                .Where(dr => dr.Date < dateEnd)
                .OrderByDescending(dr => dr.Date)
                .Take(maxCountOfElements);

            return Ok(targetRecords);
        }
    }
}

