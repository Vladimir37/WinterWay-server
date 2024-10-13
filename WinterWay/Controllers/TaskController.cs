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
    public class TaskController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<UserModel> _userManager;
        private readonly CompleteTaskService _completeTaskService;

        public TaskController(UserManager<UserModel> userManager, CompleteTaskService completeTaskService, ApplicationContext db)
        {
            _userManager = userManager;
            _completeTaskService = completeTaskService;
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
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Board or sprint does not exists"));
            }

            var otherTasksCount = _db.Tasks
                .Include(t => t.Board)
                .Where(t => t.Board.UserId == user!.Id)
                .Where(t => !t.IsDone)
                .Where(t => t.BoardId == createTaskForm.BoardId)
                .Where(t => t.SprintId == createTaskForm.SprintId)
                .Count();

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
                SortOrder = otherTasksCount,
                CreationDate = creationDate,
                ClosingDate = null,

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
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Task does not exists"));
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
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Board, sprint or task does not exists"));
            }

            if (targetTask.SprintId == moveTaskForm.SprintId)
            {
                return BadRequest(new ApiError(InternalError.InvalidForm, "The old and new sprints are the same"));
            }

            var tasksInSprintInStatusCount = _db.Tasks
                .Include(t => t.Board)
                .Where(t => t.IsDone == targetTask.IsDone)
                .Where(t => t.SprintId == moveTaskForm.SprintId)
                .Where(t => t.Board.UserId == user!.Id)
                .Count();

            var oldSprintId = targetTask.SprintId;

            targetTask.BoardId = moveTaskForm.BoardId;
            targetTask.SprintId = moveTaskForm.SprintId;
            targetTask.SortOrder = tasksInSprintInStatusCount;
            _db.SaveChanges();

            var otherTasksInOldSprint = _db.Tasks
                .Include(t => t.Board)
                .Where(t => t.IsDone == targetTask.IsDone)
                .Where(t => t.SprintId == oldSprintId)
                .Where(t => t.Board.UserId == user!.Id)
                .ToList();

            _completeTaskService.SortAllTasks(otherTasksInOldSprint);

            return Ok(targetTask);
        }

        [HttpPost("change-status")]
        public async Task<IActionResult> ChangeTaskStatus([FromBody] ChangeTaskStatusDTO changeStatusForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTask = _db.Tasks
                .Include(t => t.Board)
                .Where(t => t.Id == changeStatusForm.TaskId)
                .Where(t => t.SprintId != null)
                .Where(t => !t.IsTemplate)
                .Where(t => !t.Board.IsBacklog)
                .Where(t => t.Board.UserId == user!.Id)
                .FirstOrDefault();

            if (targetTask == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Available task does not exists"));
            }

            if (targetTask.IsDone == changeStatusForm.Status)
            {
                return BadRequest(new ApiError(InternalError.InvalidForm, "The status has not changed"));
            }

            var oldTaskStatus = targetTask.IsDone;

            _completeTaskService.ChangeStatus(targetTask, changeStatusForm.Status);

            var otherTasksWithOldStatus = _db.Tasks
                .Include(t => t.Board)
                .Where(t => t.IsDone == oldTaskStatus)
                .Where(t => t.SprintId == targetTask.SprintId)
                .Where(t => t.Board.UserId == user!.Id)
                .ToList();

            _completeTaskService.SortAllTasks(otherTasksWithOldStatus);

            return Ok(targetTask);
        }

        [HttpPost("change-type")]
        public async Task<IActionResult> ChangeTaskType([FromBody] ChangeTaskTypeDTO changeTypeForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTask = _db.Tasks
                .Include(t => t.Board)
                .Include(t => t.NumericCounter)
                .Where(t => t.Id == changeTypeForm.TaskId)
                .Where(t => t.Board.UserId == user!.Id)
                .FirstOrDefault();

            if (targetTask == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Task does not exists"));
            }

            targetTask.Type = changeTypeForm.TaskType;
            targetTask.MaxCounter = changeTypeForm.MaxCounter;
            
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

        [HttpPost("change-tasks-order")]
        public async Task<IActionResult> ChangeTasksOrder([FromBody] ChangeElementsOrderDTO changeTasksOrderForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var tasks = _db.Tasks
                .Include(t => t.Board)
                .Where(t => changeTasksOrderForm.Elements.Contains(t.Id))
                .Where(t => t.Board.UserId == user!.Id)
                .OrderBy(t => changeTasksOrderForm.Elements.IndexOf(t.Id))
                .ToList();

            bool allTasksBelongToOneSprint = tasks.All(s => s.SprintId == tasks.First().SprintId);
            bool allTasksBelongToOneBoard = tasks.All(s => s.BoardId == tasks.First().BoardId);
            bool allTasksHaveTheSameStatus = tasks.All(s => s.IsDone == tasks.First().IsDone);

            if (!allTasksBelongToOneSprint || !allTasksBelongToOneBoard || !allTasksHaveTheSameStatus)
            {
                return BadRequest(new ApiError(InternalError.InvalidForm, "Tasks have different boards, sprints or statuses"));
            }

            var num = 0;
            foreach (var task in tasks)
            {
                task.SortOrder = num;
                num++;
            }
            _db.SaveChanges();

            return Ok(tasks);
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
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Task does not exists"));
            }

            var oldBoardId = targetTask.BoardId;
            var oldSprintId = targetTask.SprintId;
            var oldTaskStatus = targetTask.IsDone;

            _db.Tasks.Remove(targetTask);
            _db.SaveChanges();

            List<TaskModel> otherTasksInSprintAndStatus = new List<TaskModel>();

            if (oldSprintId != null)
            {
                otherTasksInSprintAndStatus = _db.Tasks
                    .Include(t => t.Board)
                    .Where(t => t.IsDone == oldTaskStatus)
                    .Where(t => t.SprintId == oldSprintId)
                    .Where(t => t.BoardId == oldBoardId)
                    .Where(t => t.Board.UserId == user!.Id)
                    .ToList();
            } 
            else
            {
                otherTasksInSprintAndStatus = _db.Tasks
                    .Include(t => t.Board)
                    .Where(t => t.IsDone == oldTaskStatus)
                    .Where(t => t.SprintId == null)
                    .Where(t => t.BoardId == oldBoardId)
                    .Where(t => t.Board.UserId == user!.Id)
                    .ToList();
            }

            _completeTaskService.SortAllTasks(otherTasksInSprintAndStatus);

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
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Sprint does not exists"));
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
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Board does not exists"));
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
