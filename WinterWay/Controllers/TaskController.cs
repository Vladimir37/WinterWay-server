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

            if (createTaskForm.BoardId != null)
            {
                targetBoard = _db.Boards
                    .Where(b => b.UserId == user!.Id && b.Id == createTaskForm.BoardId && !b.Archived)
                    .FirstOrDefault();
            }

            if (targetBoard != null && createTaskForm.SprintId != null)
            {
                targetSprint = _db.Sprints
                    .Where(s => s.BoardId == targetBoard.Id && s.Id == createTaskForm.SprintId && s.Active)
                    .FirstOrDefault();
            }

            if (
                (createTaskForm.BoardId != null && targetBoard == null) ||
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

            // todo user board
            //var targetTask = _db.Tasks
            //    .
            var targetTask = _db.Tasks
                .Include(t => t.Board)
                .ThenInclude(b => b.User)
                .Where(t => t.Id == editTaskForm.Id && t.Board.UserId == user!.Id)
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
            //
        }
    }
}
