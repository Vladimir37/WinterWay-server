﻿using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterWay.Enums;
using WinterWay.Models.Database.Auth;
using WinterWay.Models.DTOs.Responses.Shared;
using WinterWay.Services;

namespace WinterWay.Controllers.DataBackup
{
    [Route("api/[controller]")]
    public class BackupController : ControllerBase
    {
        private readonly BackupService _backupService;
        private readonly UserManager<UserModel> _userManager;
        private readonly IConfiguration _config;
        
        private readonly bool _importAvailable;
        
        public BackupController(BackupService backupService, UserManager<UserModel> userManager, IConfiguration config)
        {
            _backupService = backupService;
            _userManager = userManager;
            _config = config;
            
            var registrationConfig = _config.GetSection("Registration");
            _importAvailable = registrationConfig.GetValue<bool>("Import");
        }

        [HttpPost("import")]
        [AllowAnonymous]
        public async Task<IActionResult> ImportUserData([FromBody] JsonElement userRawJson)
        {
            var usersAlreadyExist = await _userManager.Users.AnyAsync();
            
            if (!_importAvailable || usersAlreadyExist)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ImportIsClosed, "Import is unavailable"));
            }
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var user = JsonSerializer.Deserialize<UserModel>(userRawJson.GetRawText(), options);

            if (
                user == null || 
                user.UserName == string.Empty || 
                user.UserName == null || 
                user.PasswordHash == string.Empty ||
                user.PasswordHash == null
            )
            {
                return BadRequest(new ApiErrorDTO(InternalError.InvalidUserData, "Invalid data format"));
            }
            
            var result = await _backupService.Import(user);

            if (!result)
            {
                return BadRequest(new ApiErrorDTO(InternalError.Other, "Import error"));
            }
            return Ok(new ApiSuccessDTO($"Imported|'{user.UserName}'"));
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportUserData()
        {
            var user = await _userManager.GetUserAsync(User);
            
            return Ok(await _backupService.Export(user!.Id));
        }
    }
}