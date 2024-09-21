﻿using Microsoft.AspNetCore.Identity;
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
    [ApiController]
    [Route("api/[controller]")]
    public class CounterController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<UserModel> _userManager;
        private readonly CompleteTaskService _completeTaskService;

        public CounterController(UserManager<UserModel> userManager, ApplicationContext db, CompleteTaskService completeTaskService)
        {
            _userManager = userManager;
            _db = db;
            _completeTaskService = completeTaskService;
        }

        [HttpPost("create-subtask")]
        public async Task<IActionResult> CreateSubtask([FromBody] CreateSubtaskOrTextCounterDTO createSubtaskForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTask = _db.Tasks
                .Include(t => t.Board)
                .Where(t => t.Id == createSubtaskForm.TaskId)
                .Where(t => t.Board.UserId == user!.Id)
                .FirstOrDefault();

            if (targetTask == null)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Task does not exists"));
            }

            var newSubtask = new SubtaskModel
            {
                Text = createSubtaskForm.Text,
                IsDone = false,
                SortOrder = createSubtaskForm.SortOrder,
                Task = targetTask
            };

            _db.Subtasks.Add(newSubtask);
            _db.SaveChanges();
            return Ok(targetTask);
        }

        [HttpPost("edit-subtask")]
        public async Task<IActionResult> EditSubtask([FromBody] EditSubtaskOrTextCounterDTO editSubtaskForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetSubtask = _db.Subtasks
                .Include(s => s.Task)
                .ThenInclude(t => t.Board)
                .Where(s => s.Id == editSubtaskForm.SubtaskId)
                .Where(s => s.Task.Board.UserId == user!.Id)
                .FirstOrDefault();

            if (targetSubtask == null)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Subtask does not exists"));
            }

            targetSubtask.Text = editSubtaskForm.Text;
            _db.SaveChanges();
            return Ok(targetSubtask.Task);
        }

        [HttpPost("change-subtask-status")]
        public async Task<IActionResult> ChangeSubtaskStatus([FromBody] ChangeSubtaskStatusDTO changeStatusForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetSubtask = _db.Subtasks
                .Include(s => s.Task)
                .ThenInclude(t => t.Board)
                .Where(s => s.Id == changeStatusForm.SubtaskId)
                .Where(s => s.Task.Board.UserId == user!.Id)
                .FirstOrDefault();

            if (targetSubtask == null)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Subtask does not exists"));
            }

            targetSubtask.IsDone = changeStatusForm.Status;
            _db.SaveChanges();

            if (changeStatusForm.Status)
            {
                var finalTask = _completeTaskService.CheckAutocompleteStatus(targetSubtask.Task);
                return Ok(finalTask);
            }

            return Ok(targetSubtask.Task);
        }

        [HttpPost("change-subtasks-order")]
        public async Task<IActionResult> ChangeSubtasksOrder([FromBody] ChangeSubtaskOrTextCounterOrderDTO changeOrderForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var subtasks = _db.Subtasks
                .Include(s => s.Task)
                .ThenInclude(t => t.Board)
                .Where(s => changeOrderForm.Subtasks.Contains(s.Id))
                .Where(s => s.Task.Board.UserId == user!.Id)
                .OrderBy(s => changeOrderForm.Subtasks.IndexOf(s.Id))
                .ToList();

            var num = 0;
            foreach (var subtask in subtasks)
            {
                subtask.SortOrder = num;
                num++;
            }
            _db.SaveChanges();
            return Ok(subtasks);
        }

        [HttpPost("remove-subtask")]
        public async Task<IActionResult> RemoveSubtask([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetSubtask = _db.Subtasks
                .Include(s => s.Task)
                .ThenInclude (t => t.Board)
                .Where(s => s.Id == idForm.Id)
                .Where(s => s.Task.Board.UserId == user!.Id)
                .FirstOrDefault();

            if (targetSubtask == null)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Subtask does not exists"));
            }

            _db.Subtasks.Remove(targetSubtask);
            _db.SaveChanges();
            return Ok(targetSubtask.Task);
        }

        [HttpPost("create-text-counter")]
        public async Task<IActionResult> CreateTextCounter([FromBody] CreateSubtaskOrTextCounterDTO createTextCounterForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTask = _db.Tasks
                .Include(t => t.Board)
                .Where(t => t.Id == createTextCounterForm.TaskId)
                .Where(t => t.Board.UserId == user!.Id)
                .FirstOrDefault();

            if (targetTask == null)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Task does not exists"));
            }

            var newTextCounter = new TextCounterModel
            {
                Text = createTextCounterForm.Text,
                SortOrder = createTextCounterForm.SortOrder,
                Task = targetTask
            };

            _db.TextCounters.Add(newTextCounter);
            _db.SaveChanges();
            return Ok(targetTask);
        }

        [HttpPost("edit-text-counter")]
        public async Task<IActionResult> EditTextCounter([FromBody] EditSubtaskOrTextCounterDTO editTextCounterForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTextCounter = _db.TextCounters
                .Include(t => t.Task)
                .ThenInclude(t => t.Board)
                .Where(t => t.Id == editTextCounterForm.SubtaskId)
                .Where(t => t.Task.Board.UserId == user!.Id)
                .FirstOrDefault();

            if (targetTextCounter == null)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Text counter does not exists"));
            }

            targetTextCounter.Text = editTextCounterForm.Text;
            _db.SaveChanges();

            var finalTask = _completeTaskService.CheckAutocompleteStatus(targetTextCounter.Task);

            return Ok(finalTask);
        }

        [HttpPost("change-text-counters-order")]
        public async Task<IActionResult> ChangeTextCountersOrder([FromBody] ChangeSubtaskOrTextCounterOrderDTO changeOrderForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var textCounters = _db.TextCounters
                .Include(t => t.Task)
                .ThenInclude(t => t.Board)
                .Where(t => changeOrderForm.Subtasks.Contains(t.Id))
                .Where(t => t.Task.Board.UserId == user!.Id)
                .OrderBy(t => changeOrderForm.Subtasks.IndexOf(t.Id))
                .ToList();

            var num = 0;
            foreach (var textCounter in textCounters)
            {
                textCounter.SortOrder = num;
                num++;
            }
            _db.SaveChanges();
            return Ok(textCounters);
        }

        [HttpPost("remove-text-counter")]
        public async Task<IActionResult> RemoveTextCounter([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetTextCounter = _db.TextCounters
                .Include(t => t.Task)
                .ThenInclude(t => t.Board)
                .Where(t => t.Id == idForm.Id)
                .Where(t => t.Task.Board.UserId == user!.Id)
                .FirstOrDefault();

            if (targetTextCounter == null)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Text counter does not exists"));
            }

            _db.TextCounters.Remove(targetTextCounter);
            _db.SaveChanges();
            return Ok(targetTextCounter.Task);
        }

        [HttpPost("edit-numeric-counter")]
        public async Task<IActionResult> EditNumericCounter([FromBody] EditNumericCounterDTO editNumericCounterForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetNumericCounter = _db.NumericCounters
                .Include(s => s.Task)
                .ThenInclude (t => t.Board)
                .Where(n => n.Id == editNumericCounterForm.NumericCounterId)
                .Where(n => n.Task.Board.UserId == user!.Id)
                .FirstOrDefault();

            if (targetNumericCounter == null)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Numeric counter does not exists"));
            }

            targetNumericCounter.Name = editNumericCounterForm.Name;
            _db.SaveChanges();
            return Ok(targetNumericCounter.Task);
        }

        [HttpPost("change-numeric-counter-value")]
        public async Task<IActionResult> ChangeNumericCounterValue([FromBody] ChangeNumericCounterValueDTO changeNumericCounterValueForm)
        {
            var user = await _userManager.GetUserAsync(User);

            var targetNumericCounter = _db.NumericCounters
                .Include(s => s.Task)
                .ThenInclude(t => t.Board)
                .Where(n => n.Id == changeNumericCounterValueForm.NumericCounterId)
                .Where(n => n.Task.Board.UserId == user!.Id)
                .FirstOrDefault();

            if (targetNumericCounter == null)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Numeric counter does not exists"));
            }

            targetNumericCounter.Value = changeNumericCounterValueForm.Value;
            _db.SaveChanges();
            var finalTask = _completeTaskService.CheckAutocompleteStatus(targetNumericCounter.Task);
            return Ok(finalTask);
        }
    }
}