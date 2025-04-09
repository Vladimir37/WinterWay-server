using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterWay.Data;
using WinterWay.Enums;
using WinterWay.Models.Database.Auth;
using WinterWay.Models.Database.Diary;
using WinterWay.Models.DTOs.Requests.Diary;
using WinterWay.Models.DTOs.Requests.Shared;
using WinterWay.Models.DTOs.Responses.Shared;

namespace WinterWay.Controllers.Diary
{
    [Route("api/[controller]")]
    public class DiaryActivityController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<UserModel> _userManager;

        public DiaryActivityController(ApplicationContext db, UserManager<UserModel> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateDiaryActivity([FromBody] CreateDiaryActivityDTO createDiaryActivityForm)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var targetDiaryGroup = await _db.DiaryGroups
                .Include(dg => dg.Activities)
                .Where(dg => dg.Id == createDiaryActivityForm.GroupId)
                .Where(dg => dg.UserId == user!.Id)
                .FirstOrDefaultAsync();
            
            if (targetDiaryGroup == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Diary group does not exists"));
            }

            var activitiesInGroupTotal = targetDiaryGroup.Activities
                .Where(a => !a.Archived)
                .Count();

            var newActivity = new DiaryActivityModel
            {
                Name = createDiaryActivityForm.Name,
                Icon = createDiaryActivityForm.Icon,
                Color = createDiaryActivityForm.Color,
                SortOrder = activitiesInGroupTotal,
                Archived = false,
                DiaryGroupId = targetDiaryGroup.Id,
            };
            _db.DiaryActivities.Add(newActivity);
            await _db.SaveChangesAsync();
            return Ok(newActivity);
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditDiaryActivity([FromBody] EditDiaryActivityDTO editDiaryActivityForm)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var targetActivity = await _db.DiaryActivities
                .Include(da => da.DiaryGroup)
                .Where(da => da.Id == editDiaryActivityForm.ActivityId)
                .Where(da => da.DiaryGroup.UserId == user!.Id)
                .FirstOrDefaultAsync();
            
            if (targetActivity == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Diary activity does not exists"));
            }
            
            targetActivity.Name = editDiaryActivityForm.Name;
            targetActivity.Icon = editDiaryActivityForm.Icon;
            targetActivity.Color = editDiaryActivityForm.Color;
            await _db.SaveChangesAsync();
            return Ok(targetActivity);
        }

        [HttpPost("change-archive-status")]
        public async Task<IActionResult> ChangeArchiveStatus([FromBody] ChangeArchiveStatusDTO changeArchiveStatusForm)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var targetActivity = await _db.DiaryActivities
                .Include(da => da.DiaryGroup)
                .Where(da => da.Id == changeArchiveStatusForm.Id)
                .Where(da => da.DiaryGroup.UserId == user!.Id)
                .FirstOrDefaultAsync();
            
            if (targetActivity == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Diary activity does not exists"));
            }
            
            if (targetActivity.Archived == changeArchiveStatusForm.Status)
            {
                return BadRequest(new ApiErrorDTO(InternalError.InvalidForm, "The new diary activity status is no different from the old one"));
            }
            
            var countOfDiaryInNewStatus = await _db.DiaryGroups
                .Where(dg => dg.Archived == changeArchiveStatusForm.Status)
                .Where(dg => dg.UserId == user!.Id)
                .CountAsync();
            
            targetActivity.Archived = changeArchiveStatusForm.Status;
            targetActivity.SortOrder = countOfDiaryInNewStatus;
            
            await _db.SaveChangesAsync();

            var otherDiaryActivitiesInOldStatus = await _db.DiaryGroups
                .Where(dg => dg.Archived != changeArchiveStatusForm.Status)
                .Where(dg => dg.UserId == user!.Id)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            var num = 0;
            foreach (var diaryActivity in otherDiaryActivitiesInOldStatus)
            {
                diaryActivity.SortOrder = num;
                num++;
            }
            await _db.SaveChangesAsync();
            return Ok(targetActivity);
        }

        [HttpPost("change-sort-order")]
        public async Task<IActionResult> ChangeDiaryActivitiesOrder([FromBody] ChangeElementsOrderDTO changeElementsOrderForm)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var diaryActivities = await _db.DiaryActivities
                .Include(da => da.DiaryGroup)
                .Where(da => changeElementsOrderForm.Elements.Contains(da.Id))
                .OrderBy(da => changeElementsOrderForm.Elements.IndexOf(da.Id))
                .Where(da => da.DiaryGroup.UserId == user!.Id)
                .ToListAsync();

            var allDiaryGroupsBelongToOneStatus = diaryActivities.All(c => !c.Archived);

            if (!allDiaryGroupsBelongToOneStatus)
            {
                return BadRequest(new ApiErrorDTO(InternalError.InvalidForm, "All diary activities must be active"));
            }
            
            var num = 0;
            foreach (var diaryGroup in diaryActivities)
            {
                diaryGroup.SortOrder = num;
                num++;
            }
            await _db.SaveChangesAsync();
            return Ok(diaryActivities);
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveDiaryActivity([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var targetActivity = await _db.DiaryActivities
                .Include(da => da.DiaryGroup)
                .Include(da => da.RecordActivities)
                .Where(da => da.DiaryGroup.UserId == user!.Id)
                .Where(da => da.Id == idForm.Id)
                .FirstOrDefaultAsync();
            
            if (targetActivity == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Diary activity does not exists"));
            }
            
            _db.DiaryRecordActivities.RemoveRange(targetActivity.RecordActivities);
            _db.DiaryActivities.Remove(targetActivity);
            await _db.SaveChangesAsync();
            return Ok(new ApiSuccessDTO("DiaryActivityDeletion"));
        }

        [HttpGet("get-all-in-group")]
        public async Task<IActionResult> GetAllActivityInTheGroup([FromQuery] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var targetDiaryGroup = await _db.DiaryGroups
                .Include(dg => dg.Activities)
                .Where(dg => dg.Id == idForm.Id)
                .Where(dg => dg.UserId == user!.Id)
                .FirstOrDefaultAsync();
            
            if (targetDiaryGroup == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Diary group does not exists"));
            }
            
            return Ok(targetDiaryGroup.Activities);
        }
    }
}