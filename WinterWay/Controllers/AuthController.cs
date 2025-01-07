using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using WinterWay.Enums;
using WinterWay.Data;
using WinterWay.Models.DTOs.Error;
using WinterWay.Models.DTOs.Requests;
using WinterWay.Models.DTOs.Responses;
using WinterWay.Models.Database;
using WinterWay.Services;

namespace WinterWay.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<UserModel> _userManager;
        private readonly SignInManager<UserModel> _signInManager;
        private readonly IConfiguration _config;
        private readonly BackgroundImageService _backgroundImageService;

        private readonly string _appName;
        private readonly string _version;
        private readonly bool _registrationIsPossible;
        private readonly bool _registrationForOnlyFirst;
        private readonly bool _importAvailable;

        public AuthController(ApplicationContext db, UserManager<UserModel> userManager, SignInManager<UserModel> signInManager, IConfiguration config, BackgroundImageService backgroundImageService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _backgroundImageService = backgroundImageService;
            _config = config;
            _db = db;

            var registrationConfig = _config.GetSection("Registration");
            var appSettings = _config.GetSection("AppSettings");

            _registrationIsPossible = registrationConfig.GetValue<bool>("Available");
            _registrationForOnlyFirst = registrationConfig.GetValue<bool>("OnlyFirst");
            _importAvailable = registrationConfig.GetValue<bool>("Import");
            _appName = appSettings.GetValue<string>("Name")!;
            _version = appSettings.GetValue<string>("Version")!;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginForm)
        {
            var user = await _userManager.FindByNameAsync(loginForm.Username!);
            if (user != null && await _userManager.CheckPasswordAsync(user, loginForm.Password!))
            {
                var authClaims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                };

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                };

                await _signInManager.SignInWithClaimsAsync(user, authProperties, authClaims);

                return Ok("Successful authorization");
            }
            return Unauthorized(new ApiError(InternalError.InvalidUserData, "Invalid user data"));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok("Successful logout");
        }

        [HttpPost("signup")]
        [AllowAnonymous]
        public async Task<IActionResult> Signup([FromBody] LoginDTO signupForm)
        {
            if (!_registrationIsPossible || (_registrationForOnlyFirst && await _userManager.Users.AnyAsync()))
            {
                return StatusCode(403, new ApiError(InternalError.RegistrationIsClosed, "Registration is closed"));
            }
            if (await _userManager.FindByNameAsync(signupForm.Username!) != null)
            {
                return BadRequest(new ApiError(InternalError.UsernameAlreadyExists, "Username alreay exists"));
            }

            var user = new UserModel { 
                UserName = signupForm.Username,
            };

            var result = await _userManager.CreateAsync(user, signupForm.Password!);

            var currentDate = DateTime.UtcNow;
            var backlogBoard = new BoardModel
            {
                Name = "Backlog",
                RollType = RollType.None,
                RollStart = RollStart.StartDate,
                RollDays = 0,
                CurrentSprintNumber = 0,
                Color = "#000000",
                IsBacklog = true,
                Favorite = false,
                Archived = false,
                SortOrder = 0,
                CreationDate = currentDate,
                User = user,
            };

            _db.Boards.Add(backlogBoard);

            var backlogSprint = new SprintModel
            {
                Name = "Backlog",
                Active = true,
                Image = 0,
                CreationDate = currentDate,
                ExpirationDate = null,
                ClosingDate = null,
                Number = 0,
                Board = backlogBoard,
            };


            _db.Sprints.Add(backlogSprint);
            user.BacklogSprint = backlogSprint;
            
            await _userManager.UpdateAsync(user);
            backlogBoard.ActualSprint = backlogSprint;

            await _db.SaveChangesAsync();

            if (result.Succeeded)
            {
                return Ok("User created");
            }
            return BadRequest(new ApiError(InternalError.Other, "Signup error"));
        }

        [HttpPost("edit-user")]
        public async Task<IActionResult> EditUser([FromBody] EditUserDTO editUserForm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new ApiError(InternalError.InvalidUserData, "Invalid user data"));
            }
            if (await _userManager.FindByNameAsync(editUserForm.Username!) != null && editUserForm.Username != user.UserName)
            {
                return BadRequest(new ApiError(InternalError.UsernameAlreadyExists, "Username alreay exists"));
            }
            
            user.Theme = editUserForm.Theme;
            user.AutoCompleteTasks = editUserForm.AutoCompleteTasks;
            var resultThemeUpdate = await _userManager.UpdateAsync(user);
            var resultUsernameUpdate = await _userManager.SetUserNameAsync(user, editUserForm.Username);
            if (resultUsernameUpdate.Succeeded && resultThemeUpdate.Succeeded)
            {
                return Ok("User has been updated");
            }
            return BadRequest(new ApiError(InternalError.Other, "User edit error"));
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO changePasswordForm)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!await _userManager.CheckPasswordAsync(user!, changePasswordForm.OldPassword!))
            {
                return BadRequest(new ApiError(InternalError.InvalidUserData, "Incorrect password"));
            }

            var result = await _userManager.ChangePasswordAsync(user!, changePasswordForm.OldPassword!, changePasswordForm.NewPassword!);
            if (result.Succeeded)
            {
                return Ok("Password has been changed");
            }
            return BadRequest(new ApiError(InternalError.Other, "Password has not been changed"));
        }

        [HttpGet("access-denied")]
        [AllowAnonymous]
        public async Task<IActionResult> AccessDenied()
        {
            return Unauthorized(new ApiError(InternalError.NotAuthorized, "User is not authorized"));
        }

        [HttpGet("user-status")]
        public async Task<IActionResult> UserStatus()
        {
            var user = await _userManager.GetUserAsync(User);

            return Ok(new UserStatusDTO(user!.Id, user.UserName!, user.Theme, user.AutoCompleteTasks));
        }

        [HttpGet("background-status")]
        public async Task<IActionResult> BackgroundStatus()
        {
            return Ok(_backgroundImageService.BackgroundData);
        }

        [HttpGet("app-status")]
        [AllowAnonymous]
        public async Task<IActionResult> AppStatus()
        {
            var usersAlreadyExist = await _userManager.Users.AnyAsync();
            var importIsAvailable = _importAvailable && !usersAlreadyExist;
                
            if (!_registrationIsPossible)
            {
                return Ok(new AppStatusDTO(false, false, false, _appName, _version));
            }
            else if (_registrationForOnlyFirst)
            {
                return Ok(new AppStatusDTO(true, !await _userManager.Users.AnyAsync(), importIsAvailable, _appName, _version));
            }
            return Ok(new AppStatusDTO(true, true, importIsAvailable, _appName, _version));
        }
    }
}
