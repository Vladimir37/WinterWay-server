using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterWay.Attributes;
using WinterWay.Data;
using WinterWay.Enums;
using WinterWay.Models.Database;
using WinterWay.Models.DTOs.Error;
using WinterWay.Models.DTOs.Responses;
using WinterWay.Services;

namespace WinterWay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BackupController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly BackupService _backupService;
        private readonly UserManager<UserModel> _userManager;
        private readonly IConfiguration _config;
        
        private readonly bool _importAvailable;
        
        public BackupController(ApplicationContext db, BackupService backupService, UserManager<UserModel> userManager, IConfiguration config)
        {
            _db = db;
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
                return BadRequest(new ApiError(InternalError.ImportIsClosed, "Import is unavailable"));
            }
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var user = JsonSerializer.Deserialize<UserModel>(userRawJson.GetRawText(), options);

            if (user == null)
            {
                return BadRequest(new ApiError(InternalError.InvalidUserData, "Invalid data format"));
            }
            
            var result = await _backupService.Import(user);

            if (!result)
            {
                return BadRequest(new ApiError(InternalError.Other, "Import error"));
            }
            return Ok(user.UserName);
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportUserData()
        {
            var user = await _userManager.GetUserAsync(User);
            
            return Ok(await _backupService.Export(user!.Id));
        }
    }
}