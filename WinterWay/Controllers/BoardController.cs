using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterWay.Data;
using WinterWay.Models.Database;
using WinterWay.Models.DTOs.Requests;
using WinterWay.Models.DTOs.Error;
using WinterWay.Enums;
using WinterWay.Services;

namespace WinterWay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BoardController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<UserModel> _userManager;
        private readonly RollService _rollService;

        public BoardController(UserManager<UserModel> userManager, ApplicationContext db, RollService rollService)
        {
            _userManager = userManager;
            _db = db;
            _rollService = rollService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBoard([FromBody] CreateBoardDTO createBoardForm)
        {
            var user = await _userManager.GetUserAsync(User);
            var newBoard = new BoardModel
            {
                Name = createBoardForm.Name,
                RollType = createBoardForm.RollType,
                RollStart = createBoardForm.RollStart,
                RollDays = createBoardForm.RollDays,
                CurrentSprintNumber = 0,
                Color = createBoardForm.Color,
                IsBacklog = false,
                Favorite = false,
                Archived = false,
                CreationDate = DateTime.UtcNow,
                UserId = user!.Id,
            };
            _db.Boards.Add(newBoard);
            _db.SaveChanges();
            return Ok(newBoard);
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditBoard([FromBody] EditBoardDTO editBoardForm)
        {
            var user = await _userManager.GetUserAsync(User);

            user = await _db.Users
                .Include(u => u.Boards)
                .FirstOrDefaultAsync(u => u.Id == user!.Id);

            var targetBoard = user!.Boards
                .Where(b => b.Id == editBoardForm.Id)
                .Where(b => !b.IsBacklog)
                .FirstOrDefault();

            if (targetBoard == null)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Board does not exists"));
            }

            targetBoard.Name = editBoardForm.Name;
            targetBoard.RollType = editBoardForm.RollType;
            targetBoard.RollStart = editBoardForm.RollStart;
            targetBoard.RollDays = editBoardForm.RollDays;
            targetBoard.Color = editBoardForm.Color;
            targetBoard.Favorite = editBoardForm.Favorite;
            _db.SaveChanges();
            return Ok(targetBoard);
        }

        [HttpPost("archive")]
        public async Task<IActionResult> ArchiveBoard([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            user = await _db.Users
                .Include(u => u.Boards)
                .FirstOrDefaultAsync(u => u.Id == user!.Id);

            var targetBoard = user!.Boards
                .Where(b => !b.Archived)
                .Where(b => !b.IsBacklog)
                .FirstOrDefault(b => b.Id == idForm.Id);

            if (targetBoard == null)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Active board does not exists"));
            }

            if (targetBoard.ActualSprint != null)
            {
                targetBoard.ActualSprint.Active = false;
                targetBoard.ActualSprint.ClosingDate = DateTime.UtcNow;
                _rollService.GenerateResult(targetBoard.ActualSprint, 0, 0);
                targetBoard.ActualSprintId = null;
            }
            targetBoard.Archived = true;
            _db.SaveChanges();
            return Ok(targetBoard);
        }

        [HttpPost("unarchive")]
        public async Task<IActionResult> UnarchiveBoard([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            user = await _db.Users
                .Include(u => u.Boards)
                .FirstOrDefaultAsync(u => u.Id == user!.Id);

            var targetBoard = user.Boards
                .Where(b => b.Archived)
                .Where(b => !b.IsBacklog)
                .FirstOrDefault(b => b.Id == idForm.Id);

            if (targetBoard == null)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Archive board does not exists"));
            }

            targetBoard.Archived = false;
            _db.SaveChanges();
            return Ok(targetBoard);
        }

        [HttpPost("roll")]
        public async Task<IActionResult> Roll(RollDTO rollForm)
        {
            var user = await _userManager.GetUserAsync(User);

            user = await _db.Users
                .Include(u => u.Boards)
                .Include(u => u.BacklogSprint)
                .ThenInclude(s => s.Board)
                .FirstOrDefaultAsync(u => u.Id == user!.Id);

            var targetBoard = user!.Boards
                .Where(b => b.Archived)
                .Where(b => !b.IsBacklog)
                .FirstOrDefault(b => b.Id == rollForm.BoardId);

            if (targetBoard == null)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Active board does not exists"));
            }

            var previousSprint = targetBoard.ActualSprint;

            int backlogTasks = _rollService.MoveTasksToBacklog(targetBoard, user.BacklogSprint, rollForm.TasksToBacklog);
            int spilloverTasks = _rollService.RollSprint(targetBoard, rollForm.TasksSpill);
            if (previousSprint != null)
            {
                _rollService.GenerateResult(previousSprint, spilloverTasks, backlogTasks);
            }
            return Ok(targetBoard.ActualSprint);
        }

        [HttpPost("delete")]
        public async Task<IActionResult> RemoveBoard([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            user = await _db.Users
                .Include(u => u.Boards)
                .FirstOrDefaultAsync(u => u.Id == user!.Id);

            var targetBoard = user!.Boards
                .Where(b => b.Archived)
                .Where(b => !b.IsBacklog)
                .FirstOrDefault(b => b.Id == idForm.Id);

            if (targetBoard == null)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Archive board does not exists"));
            }

            _db.Boards.Remove(targetBoard);
            _db.SaveChanges();
            return Ok("Board has been deleted");
        }

        [HttpGet("all-boards")]
        public async Task<IActionResult> GetAllBoards()
        {
            var user = await _userManager.GetUserAsync(User);

            user = await _db.Users
                .Include(u => u.Boards)
                .ThenInclude(b => b.ActualSprint)
                .FirstOrDefaultAsync(u => u.Id == user!.Id);

            return Ok(user!.Boards.ToList());
        }

        [HttpGet("all-sprints")]
        public async Task<IActionResult> GetAllSprintsInBoard([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            user = await _db.Users
                .Include(u => u.Boards.Where(b => b.Id == idForm.Id))
                .ThenInclude(b => b.ActualSprint)
                .Include(u => u.Boards)
                .ThenInclude(b => b.AllSprints)
                .ThenInclude(s => s.SprintResult)
                .FirstOrDefaultAsync(u => u.Id == user!.Id);

            return Ok(user!.Boards.ToList());
        }
    }
}
