using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterWay.Data;
using WinterWay.Enums;
using WinterWay.Services;
using WinterWay.Models.Database;
using WinterWay.Models.DTOs.Error;
using WinterWay.Models.DTOs.Requests;

namespace WinterWay.Controllers
{
    [Route("api/[controller]")]
    public class CounterController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<UserModel> _userManager;
        private readonly CompleteTaskService _completeTaskService;

        public CounterController(ApplicationContext db, UserManager<UserModel> userManager, CompleteTaskService completeTaskService)
        {
            _db = db;
            _userManager = userManager;
            _completeTaskService = completeTaskService;
        }

        [HttpPost("create-subtask")]
        public async Task<IActionResult> CreateSubtask([FromBody] CreateSubtaskOrTextCounterDTO createSubtaskForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTask = await _db.Tasks
                .Include(t => t.Board)
                .Include(t => t.Subtasks)
                .Where(t => t.Id == createSubtaskForm.TaskId)
                .Where(t => t.Type == TaskType.TodoList)
                .Where(t => t.Board.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetTask == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Task does not exists"));
            }

            var countOfSubtasks = await _db.Subtasks
                .Where(s => s.TaskId == targetTask.Id)
                .CountAsync();

            var newSubtask = new SubtaskModel
            {
                Text = createSubtaskForm.Text,
                IsDone = false,
                SortOrder = countOfSubtasks,
                Task = targetTask
            };

            _db.Subtasks.Add(newSubtask);
            await _db.SaveChangesAsync();
            return Ok(targetTask);
        }

        [HttpPost("edit-subtask")]
        public async Task<IActionResult> EditSubtask([FromBody] EditSubtaskOrTextCounterDTO editSubtaskForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetSubtask = await _db.Subtasks
                .Include(s => s.Task)
                .ThenInclude(t => t.Board)
                .Include(s => s.Task)
                .ThenInclude(t => t.Subtasks)
                .Where(s => s.Id == editSubtaskForm.SubtaskId)
                .Where(s => s.Task.Type == TaskType.TodoList)
                .Where(s => s.Task.Board.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetSubtask == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Subtask does not exists"));
            }

            targetSubtask.Text = editSubtaskForm.Text;
            await _db.SaveChangesAsync();
            return Ok(targetSubtask.Task);
        }

        [HttpPost("change-subtask-status")]
        public async Task<IActionResult> ChangeSubtaskStatus([FromBody] ChangeSubtaskStatusDTO changeStatusForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetSubtask = await _db.Subtasks
                .Include(s => s.Task)
                .ThenInclude(t => t.Board)
                .Include(s => s.Task)
                .ThenInclude(t => t.Subtasks)
                .Where(s => s.Id == changeStatusForm.SubtaskId)
                .Where(s => s.Task.SprintId != null)
                .Where(s => !s.Task.IsTemplate)
                .Where(s => !s.Task.Board.IsBacklog)
                .Where(s => s.Task.Type == TaskType.TodoList)
                .Where(s => s.Task.Board.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetSubtask == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Subtask in sprint does not exists"));
            }

            targetSubtask.IsDone = changeStatusForm.Status;
            await _db.SaveChangesAsync();

            if (changeStatusForm.Status)
            {
                var finalTask = await _completeTaskService.CheckAutocompleteStatus(targetSubtask.Task, user!.Id);
                return Ok(finalTask);
            }

            return Ok(targetSubtask.Task);
        }

        [HttpPost("change-subtasks-order")]
        public async Task<IActionResult> ChangeSubtasksOrder([FromBody] ChangeElementsOrderDTO changeOrderForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetSubtasks = await _db.Subtasks
                .Include(s => s.Task)
                .ThenInclude(t => t.Board)
                .Where(s => changeOrderForm.Elements.Contains(s.Id))
                .Where(s => s.Task.Board.UserId == user!.Id)
                .OrderBy(s => changeOrderForm.Elements.IndexOf(s.Id))
                .ToListAsync();

            bool allSubtasksBelongToOneTask = targetSubtasks.All(s => s.TaskId == targetSubtasks.First().TaskId);

            if (!allSubtasksBelongToOneTask)
            {
                return BadRequest(new ApiError(InternalError.InvalidForm, "Subtasks belong to different tasks"));
            }

            var num = 0;
            foreach (var subtask in targetSubtasks)
            {
                subtask.SortOrder = num;
                num++;
            }
            await _db.SaveChangesAsync();
            return Ok(targetSubtasks);
        }

        [HttpPost("remove-subtask")]
        public async Task<IActionResult> RemoveSubtask([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetSubtask = await _db.Subtasks
                .Include(s => s.Task)
                .ThenInclude (t => t.Board)
                .Include(s => s.Task)
                .ThenInclude(t => t.Subtasks)
                .Where(s => s.Id == idForm.Id)
                .Where(s => s.Task.Type == TaskType.TodoList)
                .Where(s => s.Task.Board.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetSubtask == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Subtask does not exists"));
            }

            _db.Subtasks.Remove(targetSubtask);

            await _db.SaveChangesAsync();
            
            var allOtherSubtasks = await _db.Subtasks
                .Where(s => s.TaskId == targetSubtask.TaskId)
                .OrderBy(s => s.SortOrder)
                .ToListAsync();

            var num = 0;
            foreach (var subtask in allOtherSubtasks)
            {
                subtask.SortOrder = num;
                num++;
            }

            await _db.SaveChangesAsync();
            return Ok(targetSubtask.Task);
        }

        [HttpPost("create-text-counter")]
        public async Task<IActionResult> CreateTextCounter([FromBody] CreateSubtaskOrTextCounterDTO createTextCounterForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTask = await _db.Tasks
                .Include(t => t.Board)
                .Include(t => t.TextCounters)
                .Where(t => t.Id == createTextCounterForm.TaskId)
                .Where(t => t.Type == TaskType.TextCounter)
                .Where(t => !t.IsTemplate)
                .Where(t => t.SprintId != null)
                .Where(t => !t.Board.IsBacklog)
                .Where(t => t.Board.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetTask == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Task does not exists"));
            }

            var countOfTextCounters = await _db.TextCounters
                .Where(s => s.TaskId == targetTask.Id)
                .CountAsync();

            var newTextCounter = new TextCounterModel
            {
                Text = createTextCounterForm.Text,
                SortOrder = countOfTextCounters,
                Task = targetTask
            };

            _db.TextCounters.Add(newTextCounter);
            await _db.SaveChangesAsync();

            var finalTask = await _completeTaskService.CheckAutocompleteStatus(targetTask, user!.Id);

            return Ok(finalTask);
        }

        [HttpPost("edit-text-counter")]
        public async Task<IActionResult> EditTextCounter([FromBody] EditSubtaskOrTextCounterDTO editTextCounterForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTextCounter = await _db.TextCounters
                .Include(t => t.Task)
                .ThenInclude(t => t.Board)
                .Include(t => t.Task)
                .ThenInclude(t => t.TextCounters)
                .Where(t => t.Id == editTextCounterForm.SubtaskId)
                .Where(t => t.Task.Type == TaskType.TextCounter)
                .Where(t => t.Task.Board.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetTextCounter == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Text counter does not exists"));
            }

            targetTextCounter.Text = editTextCounterForm.Text;
            await _db.SaveChangesAsync();

            return Ok(targetTextCounter.Task);
        }

        [HttpPost("change-text-counters-order")]
        public async Task<IActionResult> ChangeTextCountersOrder([FromBody] ChangeElementsOrderDTO changeOrderForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTextCounters = await _db.TextCounters
                .Include(t => t.Task)
                .ThenInclude(t => t.Board)
                .Include(t => t.Task)
                .ThenInclude(t => t.TextCounters)
                .Where(t => changeOrderForm.Elements.Contains(t.Id))
                .Where(t => t.Task.Board.UserId == user!.Id)
                .OrderBy(t => changeOrderForm.Elements.IndexOf(t.Id))
                .ToListAsync(); 

            bool allTextCountersBelongToOneTask = targetTextCounters.All(s => s.TaskId == targetTextCounters.First().TaskId);

            if (!allTextCountersBelongToOneTask)
            {
                return BadRequest(new ApiError(InternalError.InvalidForm, "Text counters belong to different tasks"));
            }

            var num = 0;
            foreach (var textCounter in targetTextCounters)
            {
                textCounter.SortOrder = num;
                num++;
            }
            await _db.SaveChangesAsync();
            return Ok(targetTextCounters);
        }

        [HttpPost("remove-text-counter")]
        public async Task<IActionResult> RemoveTextCounter([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTextCounter = await _db.TextCounters
                .Include(t => t.Task)
                .ThenInclude(t => t.Board)
                .Include(t => t.Task)
                .ThenInclude(t => t.TextCounters)
                .Where(t => t.Id == idForm.Id)
                .Where(t => t.Task.Type == TaskType.TextCounter)
                .Where(t => t.Task.Board.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetTextCounter == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Text counter does not exists"));
            }

            _db.TextCounters.Remove(targetTextCounter);
            await _db.SaveChangesAsync();

            var allOtherTextCounters = await _db.TextCounters
                .Where(t => t.TaskId == targetTextCounter.TaskId)
                .OrderBy(t => t.SortOrder)
                .ToListAsync();

            var num = 0;
            foreach (var textCounter in allOtherTextCounters)
            {
                textCounter.SortOrder = num;
                num++;
            }
            await _db.SaveChangesAsync();
            return Ok(targetTextCounter.Task);
        }

        [HttpPost("create-sum-counter")]
        public async Task<IActionResult> CreateSumCounter([FromBody] CreateSumCounterDTO createSumCounterForm)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var targetTask = await _db.Tasks
                .Include(t => t.Board)
                .Include(t => t.SumCounters)
                .Where(t => t.Id == createSumCounterForm.TaskId)
                .Where(t => t.Type == TaskType.SumCounter)
                .Where(t => !t.IsTemplate)
                .Where(t => t.SprintId != null)
                .Where(t => !t.Board.IsBacklog)
                .Where(t => t.Board.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetTask == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Task does not exists"));
            }

            var countOfSumCounters = await _db.SumCounters
                .Where(s => s.TaskId == targetTask.Id)
                .CountAsync();

            var newSumCounter = new SumCounterModel
            {
                Text = createSumCounterForm.Text,
                Sum = createSumCounterForm.Sum,
                SortOrder = countOfSumCounters,
                Task = targetTask
            };

            _db.SumCounters.Add(newSumCounter);
            await _db.SaveChangesAsync();

            var finalTask = await _completeTaskService.CheckAutocompleteStatus(targetTask, user!.Id);

            return Ok(finalTask);
        }

        [HttpPost("edit-sum-counter")]
        public async Task<IActionResult> EditSumCounter([FromBody] EditSumCounterDTO editSumCounterForm)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var targetSumCounter = await _db.SumCounters
                .Include(t => t.Task)
                .ThenInclude(t => t.Board)
                .Include(t => t.Task)
                .ThenInclude(t => t.SumCounters)
                .Where(sc => sc.Id == editSumCounterForm.SubtaskId)
                .Where(sc => sc.Task.Type == TaskType.SumCounter)
                .Where(sc => sc.Task.Board.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetSumCounter == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Sum counter does not exists"));
            }

            targetSumCounter.Text = editSumCounterForm.Text;
            targetSumCounter.Sum = editSumCounterForm.Sum;
            await _db.SaveChangesAsync();

            return Ok(targetSumCounter.Task);
        }
        
        [HttpPost("change-sum-counters-order")]
        public async Task<IActionResult> ChangeSumCountersOrder([FromBody] ChangeElementsOrderDTO changeOrderForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetSumCounters = await _db.SumCounters
                .Include(t => t.Task)
                .ThenInclude(t => t.Board)
                .Include(t => t.Task)
                .ThenInclude(t => t.SumCounters)
                .Where(t => changeOrderForm.Elements.Contains(t.Id))
                .Where(t => t.Task.Board.UserId == user!.Id)
                .OrderBy(t => changeOrderForm.Elements.IndexOf(t.Id))
                .ToListAsync(); 

            bool allTextCountersBelongToOneTask = targetSumCounters.All(s => s.TaskId == targetSumCounters.First().TaskId);

            if (!allTextCountersBelongToOneTask)
            {
                return BadRequest(new ApiError(InternalError.InvalidForm, "Sum counters belong to different tasks"));
            }

            var num = 0;
            foreach (var sumCounter in targetSumCounters)
            {
                sumCounter.SortOrder = num;
                num++;
            }
            await _db.SaveChangesAsync();
            return Ok(targetSumCounters);
        }
        
        [HttpPost("remove-sum-counter")]
        public async Task<IActionResult> RemoveSumCounter([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetSumCounter = await _db.SumCounters
                .Include(t => t.Task)
                .ThenInclude(t => t.Board)
                .Include(t => t.Task)
                .ThenInclude(t => t.SumCounters)
                .Where(t => t.Id == idForm.Id)
                .Where(t => t.Task.Type == TaskType.SumCounter)
                .Where(t => t.Task.Board.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetSumCounter == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Sum counter does not exists"));
            }

            _db.SumCounters.Remove(targetSumCounter);
            await _db.SaveChangesAsync();

            var allOtherSumCounters = await _db.TextCounters
                .Where(t => t.TaskId == targetSumCounter.TaskId)
                .OrderBy(t => t.SortOrder)
                .ToListAsync();

            var num = 0;
            foreach (var sumCounter in allOtherSumCounters)
            {
                sumCounter.SortOrder = num;
                num++;
            }
            await _db.SaveChangesAsync();
            return Ok(targetSumCounter.Task);
        }

        [HttpPost("edit-numeric-counter")]
        public async Task<IActionResult> EditNumericCounter([FromBody] EditNumericCounterDTO editNumericCounterForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetNumericCounter = await _db.NumericCounters
                .Include(s => s.Task)
                .ThenInclude (t => t.Board)
                .Where(n => n.Id == editNumericCounterForm.NumericCounterId)
                .Where(n => n.Task.Type == TaskType.NumericCounter)
                .Where(n => n.Task.Board.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetNumericCounter == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Numeric counter does not exists"));
            }

            targetNumericCounter.Name = editNumericCounterForm.Name;
            await _db.SaveChangesAsync();
            return Ok(targetNumericCounter.Task);
        }

        [HttpPost("change-numeric-counter-value")]
        public async Task<IActionResult> ChangeNumericCounterValue([FromBody] ChangeNumericCounterValueDTO changeNumericCounterValueForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetNumericCounter = await _db.NumericCounters
                .Include(s => s.Task)
                .ThenInclude(t => t.Board)
                .Where(n => n.Id == changeNumericCounterValueForm.NumericCounterId)
                .Where(n => !n.Task.IsTemplate)
                .Where(n => n.Task.SprintId != null)
                .Where(n => !n.Task.Board.IsBacklog)
                .Where(n => n.Task.Type == TaskType.NumericCounter)
                .Where(n => n.Task.Board.UserId == user!.Id)
                .FirstOrDefaultAsync();

            if (targetNumericCounter == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Numeric counter does not exists"));
            }

            targetNumericCounter.Value = changeNumericCounterValueForm.Value;
            await _db.SaveChangesAsync();
            var finalTask = await _completeTaskService.CheckAutocompleteStatus(targetNumericCounter.Task, user!.Id);
            return Ok(finalTask);
        }
    }
}
