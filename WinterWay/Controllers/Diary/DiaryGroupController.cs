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
    public class DiaryGroupController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<UserModel> _userManager;

        public DiaryGroupController(ApplicationContext db, UserManager<UserModel> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateDiaryGroup([FromBody] CreateDiaryGroupDTO createDiaryGroupForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var diaryGroupsTotal = await _db.DiaryGroups
                .Where(dg => dg.UserId == user!.Id)
                .Where(b => !b.Archived)
                .CountAsync();

            var newDiaryGroup = new DiaryGroupModel
            {
                Name = createDiaryGroupForm.Name,
                SortOrder = diaryGroupsTotal,
                Multiple = createDiaryGroupForm.Multiple,
                CanBeEmpty = createDiaryGroupForm.CanBeEmpty,
                Archived = false,
                UserId = user!.Id,
            };
            
            _db.DiaryGroups.Add(newDiaryGroup);
            await _db.SaveChangesAsync();
            return Ok(newDiaryGroup);
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditDiaryGroup([FromBody] EditDiaryGroupDTO editDiaryGroupForm)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var targetDiaryGroup = await _db.DiaryGroups
                .Where(dg => dg.Id == editDiaryGroupForm.DiaryGroupId)
                .Where(dg => dg.UserId == user!.Id)
                .FirstOrDefaultAsync();
            
            if (targetDiaryGroup == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Diary group does not exists"));
            }

            var multipleStatusChangeError = false;

            if (targetDiaryGroup.Multiple && !editDiaryGroupForm.Multiple)
            {
                multipleStatusChangeError = await _db.DiaryRecordGroups
                    .Include(drg => drg.DiaryRecord)
                    .Include(drg => drg.Activities)
                    .Where(drg => drg.DiaryGroupId == targetDiaryGroup.Id)
                    .Where(drg => drg.Activities.Count() > 1)
                    .Where(drg => drg.DiaryRecord.UserId == user!.Id)
                    .AnyAsync();
            }
            
            if (multipleStatusChangeError)
            {
                return BadRequest(new ApiErrorDTO(InternalError.UnableToChangeParameterValue, "It is impossible to make the group's values single, as it already has multiple values"));
            }
            
            targetDiaryGroup.Name = editDiaryGroupForm.Name;
            targetDiaryGroup.Multiple = editDiaryGroupForm.Multiple;
            targetDiaryGroup.CanBeEmpty = editDiaryGroupForm.CanBeEmpty;
            await _db.SaveChangesAsync();
            return Ok(targetDiaryGroup);
        }

        [HttpPost("change-archive-status")]
        public async Task<IActionResult> ChangeArchiveStatus([FromBody] ChangeArchiveStatusDTO changeArchiveStatusForm)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var targetDiaryGroup = await _db.DiaryGroups
                .Where(dg => dg.Id == changeArchiveStatusForm.Id)
                .Where(dg => dg.UserId == user!.Id)
                .FirstOrDefaultAsync();
            
            if (targetDiaryGroup == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Diary group does not exists"));
            }
            
            if (targetDiaryGroup.Archived == changeArchiveStatusForm.Status)
            {
                return BadRequest(new ApiErrorDTO(InternalError.InvalidForm, "The new diary group status is no different from the old one"));
            }
            
            var countOfDiaryGroupsInNewStatus = await _db.DiaryGroups
                .Where(dg => dg.Archived == changeArchiveStatusForm.Status)
                .Where(dg => dg.UserId == user!.Id)
                .CountAsync();
            
            targetDiaryGroup.Archived = changeArchiveStatusForm.Status;
            targetDiaryGroup.SortOrder = countOfDiaryGroupsInNewStatus;
            
            await _db.SaveChangesAsync();

            var otherDiaryGroupsInOldStatus = await _db.DiaryGroups
                .Where(dg => dg.Archived != changeArchiveStatusForm.Status)
                .Where(dg => dg.UserId == user!.Id)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            var num = 0;
            foreach (var diaryGroup in otherDiaryGroupsInOldStatus)
            {
                diaryGroup.SortOrder = num;
                num++;
            }
            await _db.SaveChangesAsync();
            return Ok(targetDiaryGroup);
        }

        [HttpPost("change-sort-order")]
        public async Task<IActionResult> ChangeDiaryGroupOrder([FromBody] ChangeElementsOrderDTO changeElementsOrderForm)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var diaryGroups = await _db.DiaryGroups
                .Where(dg => changeElementsOrderForm.Elements.Contains(dg.Id))
                .OrderBy(dg => changeElementsOrderForm.Elements.IndexOf(dg.Id))
                .Where(dg => dg.UserId == user!.Id)
                .ToListAsync();

            var allDiaryGroupsBelongToOneStatus = diaryGroups.All(c => !c.Archived);

            if (!allDiaryGroupsBelongToOneStatus)
            {
                return BadRequest(new ApiErrorDTO(InternalError.InvalidForm, "All diary groups must be active"));
            }
            
            var num = 0;
            foreach (var diaryGroup in diaryGroups)
            {
                diaryGroup.SortOrder = num;
                num++;
            }
            await _db.SaveChangesAsync();
            return Ok(diaryGroups);
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveDiaryGroup([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var targetDiaryGroup = await _db.DiaryGroups
                .Include(dg => dg.Activities)
                .Include(dg => dg.RecordGroups)
                    .ThenInclude(drg => drg.Activities)
                .Where(dg => dg.Id == idForm.Id)
                .Where(dg => dg.UserId == user!.Id)
                .FirstOrDefaultAsync();
            
            if (targetDiaryGroup == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Diary group does not exists"));
            }

            foreach (var recordGroup in targetDiaryGroup.RecordGroups)
            {
                _db.DiaryRecordActivities.RemoveRange(recordGroup.Activities);
            }
            _db.DiaryRecordGroups.RemoveRange(targetDiaryGroup.RecordGroups);
            _db.DiaryActivities.RemoveRange(targetDiaryGroup.Activities);
            _db.DiaryGroups.Remove(targetDiaryGroup);
            await _db.SaveChangesAsync();
            return Ok(new ApiSuccessDTO("DiaryGroupDeletion"));
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllDiaryGroups()
        {
            var user = await _userManager.GetUserAsync(User);
            
            var allDiaryGroups = await _db.DiaryGroups
                .Include(dg => dg.Activities)
                .Where(dg => dg.UserId == user!.Id)
                .ToListAsync();

            return Ok(allDiaryGroups);
        }
    }
}