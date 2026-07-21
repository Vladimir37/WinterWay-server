using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterWay.Data;
using WinterWay.Enums;
using WinterWay.Models.Database.Auth;
using WinterWay.Models.Database.Mood;
using WinterWay.Models.DTOs.Requests.Mood;
using WinterWay.Models.DTOs.Requests.Shared;
using WinterWay.Models.DTOs.Responses.Shared;

namespace WinterWay.Controllers.Mood
{
    [Route("api/[controller]")]
    public class MoodTagController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<UserModel> _userManager;

        public MoodTagController(ApplicationContext db, UserManager<UserModel> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateMoodTag([FromBody] CreateMoodTagDTO createMoodTagForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var tagsTotal = await _db.MoodTags
                .Where(t => t.UserId == user!.Id)
                .Where(t => !t.Archived)
                .CountAsync();

            var newTag = new MoodTagModel
            {
                Name = createMoodTagForm.Name,
                Color = createMoodTagForm.Color,
                SortOrder = tagsTotal,
                Archived = false,
                CreationDate = DateTime.UtcNow,
                UserId = user!.Id,
            };

            _db.MoodTags.Add(newTag);
            await _db.SaveChangesAsync();
            return Ok(newTag);
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditMoodTag([FromBody] EditMoodTagDTO editMoodTagForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTag = await _db.MoodTags
                .Where(t => t.Id == editMoodTagForm.TagId)
                .Where(t => t.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetTag == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Mood tag does not exists"));
            }

            targetTag.Name = editMoodTagForm.Name;
            targetTag.Color = editMoodTagForm.Color;
            await _db.SaveChangesAsync();
            return Ok(targetTag);
        }

        [HttpPost("change-sort-order")]
        public async Task<IActionResult> ChangeMoodTagsOrder([FromBody] ChangeElementsOrderDTO changeElementsOrderForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var tags = await _db.MoodTags
                .Where(t => changeElementsOrderForm.Elements.Contains(t.Id))
                .Where(t => t.UserId == user!.Id)
                .OrderBy(t => changeElementsOrderForm.Elements.IndexOf(t.Id))
                .ToListAsync();

            var allTagsAreActive = tags.All(t => !t.Archived);

            if (!allTagsAreActive)
            {
                return BadRequest(new ApiErrorDTO(InternalError.InvalidForm, "All mood tags must be active"));
            }

            var num = 0;
            foreach (var tag in tags)
            {
                tag.SortOrder = num;
                num++;
            }
            await _db.SaveChangesAsync();

            return Ok(tags);
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveMoodTag([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTag = await _db.MoodTags
                .Include(t => t.Records)
                .Where(t => t.Id == idForm.Id)
                .Where(t => t.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetTag == null)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ElementNotFound, "Mood tag does not exists"));
            }

            if (targetTag.Records.Count > 0)
            {
                targetTag.Archived = true;
            }
            else
            {
                _db.MoodTags.Remove(targetTag);
            }
            await _db.SaveChangesAsync();

            var otherTags = await _db.MoodTags
                .Where(t => t.UserId == user!.Id)
                .Where(t => !t.Archived)
                .OrderBy(t => t.SortOrder)
                .ToListAsync();

            var num = 0;
            foreach (var tag in otherTags)
            {
                tag.SortOrder = num;
                num++;
            }
            await _db.SaveChangesAsync();

            return Ok(new ApiSuccessDTO("MoodTagDeletion"));
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllMoodTags()
        {
            var user = await _userManager.GetUserAsync(User);

            var allTags = await _db.MoodTags
                .Where(t => t.UserId == user!.Id)
                .Where(t => !t.Archived)
                .OrderBy(t => t.SortOrder)
                .ToListAsync();

            return Ok(allTags);
        }
    }
}
