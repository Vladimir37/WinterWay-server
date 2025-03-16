using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterWay.Data;
using WinterWay.Enums;
using WinterWay.Models.Database.Auth;
using WinterWay.Models.Database.Diary;
using WinterWay.Models.DTOs.Requests.Diary;
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
                .Where(c => c.Id == editDiaryGroupForm.DiaryGroupId)
                .Where(c => c.UserId == user!.Id)
                .FirstOrDefaultAsync();
            
            if (targetDiaryGroup == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Diary group does not exists"));
            }

            var multipleStatusChangeError = false;
            var canBeEmptyStatusChangeError = false;

            if (targetDiaryGroup.Multiple && !editDiaryGroupForm.Multiple)
            {
                multipleStatusChangeError = await _db.DiaryRecordGroups
                    .Include(drg => drg.DiaryRecord)
                    .Include(drg => drg.Activities)
                    .Where(drg => drg.DiaryRecord.UserId == user!.Id)
                    .Where(drg => drg.Activities.Count() > 1)
                    .AnyAsync();
            }

            if (targetDiaryGroup.CanBeEmpty && !editDiaryGroupForm.CanBeEmpty)
            {
                canBeEmptyStatusChangeError = await _db.DiaryRecordGroups
                    .Include(drg => drg.DiaryRecord)
                    .Include(drg => drg.Activities)
                    .Where(drg => drg.DiaryRecord.UserId == user!.Id)
                    .Where(drg => !drg.Activities.Any())
                    .AnyAsync();
            }

            if (multipleStatusChangeError)
            {
                return BadRequest(new ApiErrorDTO(InternalError.UnableToChangeParameterValue, "It is impossible to make the group's values single, as it already has multiple values"));
            }
            if (canBeEmptyStatusChangeError)
            {
                return BadRequest(new ApiErrorDTO(InternalError.UnableToChangeParameterValue, "It is impossible to make the group's values required, as it already has empty values"));
            }
        }
    }
}