using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterWay.Data;
using WinterWay.Enums;
using WinterWay.Models.Database;
using WinterWay.Models.DTOs.Error;
using WinterWay.Models.DTOs.Requests;

namespace WinterWay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<UserModel> _userManager;

        public TaskController(UserManager<UserModel> userManager, ApplicationContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateNewTask([FromBody] CreateTaskDTO createTaskForm)
        {
            var user = await _userManager.GetUserAsync(User);

            BoardModel? targetBoard = null;
            SprintModel? targetSprint = null;

            targetBoard = _db.Boards
                .Where(b => b.UserId == user!.Id)
                .Where(b => b.Id == createTaskForm.BoardId)
                .Where(b => !b.Archived)
                .FirstOrDefault();

            if (targetBoard != null && createTaskForm.SprintId != null)
            {
                targetSprint = _db.Sprints
                    .Where(s => s.BoardId == targetBoard.Id)
                    .Where(s => s.Id == createTaskForm.SprintId)
                    .Where(s => s.Active)
                    .FirstOrDefault();
            }

            if (
                targetBoard == null ||
                (createTaskForm.SprintId != null && targetSprint == null)
            )
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Board or sprint does not exists"));
            }

            var creationDate = DateTime.UtcNow;
            var newTask = new TaskModel
            {
                Name = createTaskForm.Name,
                Description = createTaskForm.Description,
                Type = TaskType.Default,
                IsTemplate = targetSprint == null,
                IsDone = false,
                AutoComplete = user!.AutoCompleteTasks,
                Color = createTaskForm.Color,
                MaxCounter = 0,
                CreationDate = creationDate,

                Board = targetBoard,
                Sprint = targetSprint,
            };

            _db.Tasks.Add(newTask);
            _db.SaveChanges();
            return Ok(newTask);
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditTask([FromBody] EditTaskDTO editTaskForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTask = _db.Tasks
                .Include(t => t.Board)
                .Where(t => t.Id == editTaskForm.Id)
                .Where(t => t.Board.UserId == user!.Id)
                .FirstOrDefault();

            if (targetTask == null)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Task does not exists"));
            }

            targetTask.Name = editTaskForm.Name;
            targetTask.Description = editTaskForm.Description;
            targetTask.Color = editTaskForm.Color;
            targetTask.AutoComplete = editTaskForm.AutoComplete;
            targetTask.MaxCounter = editTaskForm.MaxCounter;
            _db.SaveChanges();
            return Ok(targetTask);
        }

        [HttpPost("move")]
        public async Task<IActionResult> MoveTaskToOtherSprint([FromBody] MoveTaskDTO moveTaskForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTask = _db.Tasks
                .Include(t => t.Board)
                .Where(t => t.Id == moveTaskForm.TaskId)
                .Where(t => t.Board.UserId == user!.Id)
                .FirstOrDefault();

            var isBoardExists = _db.Boards
                .Where(b => b.Id == moveTaskForm.BoardId)
                .Where(b => b.UserId == user!.Id)
                .Any();
            var isSprintExists = _db.Sprints
                .Include(s => s.Board)
                .Where(s => s.Id == moveTaskForm.SprintId)
                .Where(s => s.Active)
                .Where(s => s.Board.UserId == user!.Id)
                .Any();

            if (!isBoardExists || !isSprintExists || targetTask == null)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Board, sprint or task does not exists"));
            }

            targetTask.BoardId = moveTaskForm.BoardId;
            targetTask.SprintId = moveTaskForm.SprintId;
            _db.SaveChanges();
            return Ok(targetTask);
        }

        [HttpPost("change-status")]
        public async Task<IActionResult> ChangeTaskStatus([FromBody] ChangeTaskStatusDTO changeStatusForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTask = _db.Tasks
                .Include(t => t.Board)
                .Where(t => t.Id == changeStatusForm.TaskId)
                .Where(t => t.Board.UserId == user!.Id)
                .FirstOrDefault();

            if (targetTask == null)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Task does not exists"));
            }

            targetTask.IsDone = changeStatusForm.Status;
            _db.SaveChanges();
            return Ok(targetTask);
        }

        [HttpPost("change-type")]
        public async Task<IActionResult> ChangeTaskType([FromBody] ChangeTaskTypeDTO changeTypeForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTask = _db.Tasks
                .Include(t => t.Board)
                .Where(t => t.Id == changeTypeForm.TaskId)
                .Where(t => t.Board.UserId == user!.Id)
                .FirstOrDefault();

            if (targetTask == null)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Task does not exists"));
            }

            targetTask.Type = changeTypeForm.TaskType;
            targetTask.MaxCounter = changeTypeForm.MaxValue;
            
            if (changeTypeForm.TaskType == TaskType.NumericCounter && targetTask.NumericCounter == null)
            {
                var newNumericCounter = new NumericCounterModel
                {
                    Name = "New numeric counter",
                    Task = targetTask,
                    Value = 0
                };
                targetTask.NumericCounter = newNumericCounter;
            }

            _db.SaveChanges();
            return Ok(targetTask);
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveTask([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTask = _db.Tasks
                .Include(t => t.Board)
                .Where(t => t.Id == idForm.Id)
                .Where(t => t.Board.UserId == user!.Id)
                .FirstOrDefault();

            if (targetTask == null)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Task does not exists"));
            }

            _db.Tasks.Remove(targetTask);
            return Ok("Task has been deleted");
        }

        [HttpGet("all-in-sprint")]
        public async Task<IActionResult> GetAllTasksInSprint([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetSprintExists = _db.Sprints
                .Include(s => s.Board)
                .Where(s => s.Board.UserId == user!.Id)
                .Where(s => s.Id == idForm.Id)
                .Any();

            if (!targetSprintExists)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Sprint does not exists"));
            }

            var targetTasks = _db.Tasks
                .Where(t => t.SprintId == idForm.Id)
                .Include(t => t.Subtasks)
                .Include(t => t.TextCounters)
                .Include(t => t.NumericCounter)
                .ToList();

            return Ok(targetTasks);
        }

        [HttpGet("all-on-board")]
        public async Task<IActionResult> GetAllTasksOnBoard([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetBoardExists = _db.Boards
                .Where(s => s.UserId == user!.Id)
                .Where(s => s.Id == idForm.Id)
                .Any();

            if (!targetBoardExists)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Sprint does not exists"));
            }

            var targetTasks = _db.Tasks
                .Where(t => t.BoardId == idForm.Id)
                .Where(t => t.SprintId == null)
                .Include(t => t.Subtasks)
                .Include(t => t.TextCounters)
                .Include(t => t.NumericCounter)
                .ToList();

            return Ok(targetTasks);
        }
    }
}
