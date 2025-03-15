﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterWay.Data;
using WinterWay.Enums;
using WinterWay.Models.Database.Auth;
using WinterWay.Models.DTOs.Error;
using WinterWay.Models.DTOs.Requests;
using WinterWay.Models.DTOs.Responses;
using WinterWay.Services;

namespace WinterWay.Controllers.Planner
{
    [Route("api/[controller]")]
    public class SprintController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<UserModel> _userManager;
        private readonly RollService _rollService;

        public SprintController(ApplicationContext db, UserManager<UserModel> userManager, RollService rollService)
        {
            _db = db;
            _userManager = userManager;
            _rollService = rollService;
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditSprint([FromBody] EditSprintDTO editSprintForm)
        {
            var user = await _userManager.GetUserAsync(User);

            user = await _db.Users
                .Include(u => u.Boards)
                .FirstOrDefaultAsync(u => u.Id == user!.Id);

            var targetSprint = await _db.Sprints
                .Include(s => s.Board)
                .Where(s => s.Id == editSprintForm.Id)
                .Where(s => s.Board.UserId == user!.Id)
                .Where(s => !s.Board.IsBacklog)
                .Where(s => s.Active)
                .FirstOrDefaultAsync();

            if (targetSprint == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Active sprint does not exists"));
            }

            targetSprint.Name = editSprintForm.Name;
            await _db.SaveChangesAsync();
            return Ok(targetSprint);
        }

        [HttpPost("change-image")]
        public async Task<IActionResult> ChangeBackgroundOfSprint([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            user = await _db.Users
                .Include(u => u.Boards)
                .FirstOrDefaultAsync(u => u.Id == user!.Id);

            var targetSprint = await _db.Sprints
                .Include(s => s.Board)
                .Where(s => s.Id == idForm.Id)
                .Where(s => s.Board.UserId == user!.Id)
                .Where(s => !s.Board.IsBacklog)
                .Where(s => s.Active)
                .FirstOrDefaultAsync();

            if (targetSprint == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Active sprint does not exists"));
            }

            RollType rollType = targetSprint.Board.RollType;
            if (rollType == RollType.Day || rollType == RollType.Month)
            {
                return BadRequest(new ApiError(InternalError.CannotChangeFixedBackground, "Can't change fixed background"));
            }

            int newImageNum = _rollService.SelectImageForSprint(rollType, targetSprint.CreationDate, targetSprint.Image);
            targetSprint.Image = newImageNum;
            await _db.SaveChangesAsync();
            return Ok(targetSprint);
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveSprint([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            user = await _db.Users
                .Include(u => u.Boards)
                .FirstOrDefaultAsync(u => u.Id == user!.Id);

            var targetSprint = await _db.Sprints
                .Include(s => s.Board)
                .Where(s => s.Id == idForm.Id)
                .Where(s => s.Board.UserId == user!.Id)
                .Where(s => !s.Board.IsBacklog)
                .Where(s => !s.Active)
                .FirstOrDefaultAsync();

            if (targetSprint == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Archive sprint does not exists"));
            }

            _db.Sprints.Remove(targetSprint);
            await _db.SaveChangesAsync();
            return Ok(new ApiSuccessDTO("SprintDeletion"));
        }

        [HttpGet("get-one")]
        public async Task<IActionResult> GetOneSprint([FromBody] IdDTO idForm)
        {
            var user = await _userManager.GetUserAsync(User);

            user = await _db.Users
                .Include(u => u.Boards)
                .FirstOrDefaultAsync(u => u.Id == user!.Id);

            var targetSprint = await _db.Sprints
                .Include(s => s.Board)
                .Include(s => s.SprintResult)
                .Where(s => s.Id == idForm.Id)
                .Where(s => s.Board.UserId == user!.Id)
                .Where(s => !s.Active)
                .Include(s => s.Tasks)
                .FirstOrDefaultAsync();

            if (targetSprint == null)
            {
                return BadRequest(new ApiError(InternalError.ElementNotFound, "Active sprint does not exists"));
            }

            return Ok(targetSprint);
        }
    }
}
