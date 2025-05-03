using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterWay.Enums;
using WinterWay.Models.Database.Auth;
using WinterWay.Models.DTOs.Requests.DataBackup;
using WinterWay.Models.DTOs.Responses.DataBackup;
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
        public async Task<IActionResult> ImportUserData([FromBody] ImportUserDTO userRaw)
        {
            var usersAlreadyExist = await _userManager.Users.AnyAsync();
            
            if (!_importAvailable || usersAlreadyExist)
            {
                return BadRequest(new ApiErrorDTO(InternalError.ImportIsClosed, "Import is unavailable"));
            }
            
            var result = _backupService.Import(userRaw.User, out bool formatError, out string username);

            if (!result && !formatError)
            {
                return BadRequest(new ApiErrorDTO(InternalError.Other, "Import error"));
            } 
            else if (!result && formatError)
            {
                return BadRequest(new ApiErrorDTO(InternalError.InvalidUserData, "Invalid data format"));
            }
            
            return Ok(new ApiSuccessDTO("Import", username));
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportUserData()
        {
            var user = await _userManager.GetUserAsync(User);
            
            return Ok(new ExportUserDTO(await _backupService.Export(user!.Id)));
        }
    }
}