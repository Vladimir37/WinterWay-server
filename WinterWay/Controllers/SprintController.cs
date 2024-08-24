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
    public class SprintController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<UserModel> _userManager;
        private readonly RollService _rollService;

        public SprintController(UserManager<UserModel> userManager, ApplicationContext db, RollService rollService)
        {
            _userManager = userManager;
            _db = db;
            _rollService = rollService;
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditSprint([FromBody] EditSprintDTO editSprintForm)
        {
            var user = await _userManager.GetUserAsync(User);

            user = await _db.Users
                .Include(u => u.Boards)
                .FirstOrDefaultAsync(u => u.Id == user!.Id);

            var targetSprint = _db.Sprints
                .Include(s => s.Board)
                .Where(s => s.Id == editSprintForm.Id)
                .Where(s => s.Board.User.Id == user!.Id)
                .Where(s => !s.Board.IsBacklog)
                .Where(s => s.Active)
                .FirstOrDefault();

            if (targetSprint == null)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Active sprint does not exists"));
            }

            targetSprint.Name = editSprintForm.Name;
            _db.SaveChanges();
            return Ok(targetSprint);
        }

        [HttpPost("change-image")]
        public async Task<IActionResult> ChangeBackgroundOfSprint([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            user = await _db.Users
                .Include(u => u.Boards)
                .FirstOrDefaultAsync(u => u.Id == user!.Id);

            var targetSprint = _db.Sprints
                .Include(s => s.Board)
                .Where(s => s.Id == idForm.Id)
                .Where(s => s.Board.User.Id == user!.Id)
                .Where(s => !s.Board.IsBacklog)
                .Where(s => s.Active)
                .FirstOrDefault();

            if (targetSprint == null)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Active sprint does not exists"));
            }

            RollType rollType = targetSprint.Board.RollType;
            if (rollType == RollType.Day || rollType == RollType.Month)
            {
                return BadRequest(new ApiError(InnerErrors.CannotChangeFixedBackground, "Can't change fixed background"));
            }

            int newImageNum = _rollService.SelectImageForSprint(rollType, targetSprint.CreationDate, targetSprint.Image);
            targetSprint.Image = newImageNum;
            _db.SaveChanges();
            return Ok(targetSprint);
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteSprint([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            user = await _db.Users
                .Include(u => u.Boards)
                .FirstOrDefaultAsync(u => u.Id == user!.Id);

            var targetSprint = _db.Sprints
                .Include(s => s.Board)
                .Where(s => s.Id == idForm.Id)
                .Where(s => s.Board.User.Id == user!.Id)
                .Where(s => !s.Board.IsBacklog)
                .Where(s => !s.Active)
                .FirstOrDefault();

            if (targetSprint == null)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Active sprint does not exists"));
            }

            _db.Sprints.Remove(targetSprint);
            _db.SaveChanges();
            return Ok("Sprint has been removed");
        }

        [HttpGet("get-one")]
        public async Task<IActionResult> GetOneSprint([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            user = await _db.Users
                .Include(u => u.Boards)
                .FirstOrDefaultAsync(u => u.Id == user!.Id);

            var targetSprint = _db.Sprints
                .Include(s => s.Board)
                .Where(s => s.Id == idForm.Id && s.Board.User.Id == user!.Id && !s.Active)
                .Include(s => s.Tasks)
                .FirstOrDefault();

            if (targetSprint == null)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Active sprint does not exists"));
            }

            return Ok(targetSprint);
        }
    }
}
