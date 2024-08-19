using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WinterWay.Data;
using WinterWay.Enums;
using WinterWay.Models.Database;
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

            // todo - return error if board/sprint sended but not found

            var creationDate = DateTime.UtcNow;
            var newTask = new TaskModel
            {
                Name = createTaskForm.Name,
                Description = createTaskForm.Description,
                Type = TaskType.Default,
                IsTemplate = createTaskForm.SprintId == null,
                IsBacklog = createTaskForm.SprintId == null && createTaskForm.BoardId == null,
                IsDone = false,
                AutoComplete = user!.AutoCompleteTasks,
                Color = createTaskForm.Color,
                MaxCounter = 0,
                CreationDate = creationDate,
            };
        }
    }
}
