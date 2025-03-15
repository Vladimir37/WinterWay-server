using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterWay.Data;
using WinterWay.Enums;
using WinterWay.Models.Database.Auth;
using WinterWay.Models.Database.Planner;
using WinterWay.Models.DTOs.Error;
using WinterWay.Models.DTOs.Requests;
using WinterWay.Models.DTOs.Responses;
using WinterWay.Services;

namespace WinterWay.Controllers.Planner
{
    [Route("api/[controller]")]
    public class BoardController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<UserModel> _userManager;
        private readonly RollService _rollService;

        public BoardController(ApplicationContext db, UserManager<UserModel> userManager, RollService rollService)
        {
            _db = db;
            _userManager = userManager;
            _rollService = rollService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBoard([FromBody] CreateBoardDTO createBoardForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var boardsTotal = await _db.Boards
                .Where(b => b.UserId == user!.Id)
                .Where(b => !b.Archived)
                .CountAsync();

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
                SortOrder = boardsTotal,
                NotificationActive = createBoardForm.RollType != RollType.None,
                CreationDate = DateTime.UtcNow,
                UserId = user!.Id,
            };
            _db.Boards.Add(newBoard);
            await _db.SaveChangesAsync();
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
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Board does not exists"));
            }

            targetBoard.Name = editBoardForm.Name;
            targetBoard.RollType = editBoardForm.RollType;
            targetBoard.RollStart = editBoardForm.RollStart;
            targetBoard.RollDays = editBoardForm.RollDays;
            targetBoard.Color = editBoardForm.Color;
            targetBoard.Favorite = editBoardForm.Favorite;
            targetBoard.NotificationActive = editBoardForm.NotificationActive;
            await _db.SaveChangesAsync();
            return Ok(targetBoard);
        }

        [HttpPost("change-archive-status")]
        public async Task<IActionResult> ChangeBoardArchiveStatus([FromBody] ChangeArchiveStatusDTO changeArchiveStatusForm)
        {
            var user = await _userManager.GetUserAsync(User);

            user = await _db.Users
                .Include(u => u.Boards)
                .ThenInclude(b => b.ActualSprint)
                .FirstOrDefaultAsync(u => u.Id == user!.Id);

            var targetBoard = user!.Boards
                .Where(b => b.Archived != changeArchiveStatusForm.Status)
                .Where(b => !b.IsBacklog)
                .Where(b => b.Id == changeArchiveStatusForm.Id)
                .FirstOrDefault();

            if (targetBoard == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Active board does not exists"));
            }

            if (changeArchiveStatusForm.Status && targetBoard.ActualSprint != null)
            {
                targetBoard.ActualSprint.Active = false;
                targetBoard.ActualSprint.ClosingDate = DateTime.UtcNow;
                await _rollService.GenerateResult(targetBoard.ActualSprint, 0, 0);
                targetBoard.ActualSprintId = null;
            }
            var countOfBoardInNewStatus = await _db.Boards
                .Where(b => b.UserId == user!.Id)
                .Where(b => b.Archived == changeArchiveStatusForm.Status)
                .CountAsync();

            targetBoard.Archived = changeArchiveStatusForm.Status;
            targetBoard.SortOrder = countOfBoardInNewStatus;
            await _db.SaveChangesAsync();

            var otherBoardsInOldStatus = await _db.Boards
                .Where(b => b.UserId == user!.Id)
                .Where(b => b.Archived != changeArchiveStatusForm.Status)
                .OrderBy(b => b.SortOrder)
                .ToListAsync();

            var num = 0;
            foreach (var board in otherBoardsInOldStatus)
            {
                board.SortOrder = num;
                num++;
            }
            await _db.SaveChangesAsync();

            return Ok(targetBoard);
        }

        [HttpPost("change-boards-order")]
        public async Task<IActionResult> ChangeBoardsOrder([FromBody] ChangeElementsOrderDTO changeBoardsOrderForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var boards = await _db.Boards
                .Where(b => b.UserId == user!.Id)
                .Where(b => changeBoardsOrderForm.Elements.Contains(b.Id))
                .OrderBy(s => changeBoardsOrderForm.Elements.IndexOf(s.Id))
                .ToListAsync();

            var allBoardsBelongToOneStatus = boards.All(s => !s.Archived);

            if (!allBoardsBelongToOneStatus)
            {
                return BadRequest(new ApiError(InternalError.InvalidForm, "All boards must be active"));
            }

            var num = 0;
            foreach (var board in boards)
            {
                board.SortOrder = num;
                num++;
            }
            await _db.SaveChangesAsync();

            return Ok(boards);
        }

        [HttpPost("roll")]
        public async Task<IActionResult> Roll([FromBody] RollDTO rollForm)
        {
            var user = await _userManager.GetUserAsync(User);

            user = await _db.Users
                .AsSplitQuery()
                .Include(u => u.Boards)
                .ThenInclude(b => b.ActualSprint)
                .ThenInclude(s => s.Tasks)
                .Include(u => u.BacklogSprint)
                .ThenInclude(s => s.Board)
                .FirstOrDefaultAsync(u => u.Id == user!.Id);

            var targetBoard = user!.Boards
                .Where(b => !b.Archived)
                .Where(b => !b.IsBacklog)
                .Where(b => b.Id == rollForm.BoardId)
                .FirstOrDefault();

            if (targetBoard == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Active board does not exists"));
            }

            var previousSprint = targetBoard.ActualSprint;

            int backlogTasks = await _rollService.MoveTasksToBacklog(targetBoard, user.BacklogSprint!, rollForm.TasksToBacklog);
            int spilloverTasks = await _rollService.RollSprint(targetBoard, rollForm.TasksSpill);
            if (previousSprint != null)
            {
                await _rollService.GenerateResult(previousSprint, spilloverTasks, backlogTasks);
            }
            return Ok(targetBoard.ActualSprint);
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveBoard([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            user = await _db.Users
                .Include(u => u.Boards)
                .FirstOrDefaultAsync(u => u.Id == user!.Id);

            var targetBoard = user!.Boards
                .Where(b => b.Archived)
                .Where(b => !b.IsBacklog)
                .Where(b => b.Id == idForm.Id)
                .FirstOrDefault();

            if (targetBoard == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Archive board does not exists"));
            }

            _db.Boards.Remove(targetBoard);
            await _db.SaveChangesAsync();

            var otherArchivedBoards = await _db.Boards
                .Where(b => b.UserId == user!.Id)
                .Where(b => b.Archived)
                .OrderBy(b => b.SortOrder)
                .ToListAsync();

            var num = 0;
            foreach (var board in otherArchivedBoards)
            {
                board.SortOrder = num;
                num++;
            }
            await _db.SaveChangesAsync();

            return Ok(new ApiSuccessDTO("BoardDeletion"));
        }

        [HttpGet("all-boards")]
        public async Task<IActionResult> GetAllBoards()
        {
            var user = await _userManager.GetUserAsync(User);

            user = await _db.Users
                .Include(u => u.Boards)
                .ThenInclude(b => b.ActualSprint)
                .ThenInclude(a => a.Tasks)
                .Include(u => u.Boards)
                .ThenInclude(b => b.AllTasks
                    .Where(t => t.IsTemplate)
                )
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
                .ThenInclude(a => a.Tasks)
                .Include(u => u.Boards)
                .ThenInclude(b => b.AllSprints)
                .ThenInclude(s => s.SprintResult)
                .Include(u => u.Boards)
                .ThenInclude(b => b.AllTasks)
                .Include(u => u.Boards)
                .ThenInclude(b => b.AllSprints)
                .ThenInclude(s => s.Tasks)
                .Include(u => u.Boards)
                .ThenInclude(b => b.AllTasks
                    .Where(t => t.IsTemplate)
                )
                .FirstOrDefaultAsync(u => u.Id == user!.Id);

            return Ok(user!.Boards.ToList());
        }
    }
}
