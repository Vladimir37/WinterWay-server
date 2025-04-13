using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterWay.Data;
using WinterWay.Enums;
using WinterWay.Models.Database.Auth;
using WinterWay.Models.DTOs.Requests.Diary;
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
    }
}

