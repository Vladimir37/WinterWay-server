﻿using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WinterWay.Enums;
using WinterWay.Models.DTOs.Error;
using WinterWay.Models.DTOs.Requests;
using WinterWay.Models.DTOs.Responses;
using WinterWay.Models.Database;
using Microsoft.AspNetCore.Authentication;

namespace WinterWay.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly SignInManager<UserModel> _signInManager;
        private readonly IConfiguration _config;

        private readonly bool _registrationIsPossible;
        private readonly bool _registrationForOnlyFirst;

        public AuthController(UserManager<UserModel> userManager, SignInManager<UserModel> signInManager, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;

            var registrationConfig = _config.GetSection("Registration");

            _registrationIsPossible = registrationConfig.GetValue<bool>("Available");
            _registrationForOnlyFirst = registrationConfig.GetValue<bool>("OnlyFirst");
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModelDTO loginModel)
        {
            var user = await _userManager.FindByNameAsync(loginModel.Username!);
            if (user != null && await _userManager.CheckPasswordAsync(user, loginModel.Password!))
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
            return Unauthorized(new ApiError(InnerErrors.InvalidUserData, "Invalid user data"));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok("Successful logout");
        }

        [HttpPost("signup")]
        [AllowAnonymous]
        public async Task<IActionResult> Signup([FromBody] LoginModelDTO loginModel)
        {
            if (!_registrationIsPossible || (_registrationForOnlyFirst && _userManager.Users.Any()))
            {
                return StatusCode(403, new ApiError(InnerErrors.RegistrationIsClosed, "Registration is closed"));
            }
            if (await _userManager.FindByNameAsync(loginModel.Username) != null)
            {
                return BadRequest(new ApiError(InnerErrors.UsernameAlreadyExists, "Username alreay exists"));
            }
            var user = new UserModel { UserName = loginModel.Username };
            var result = await _userManager.CreateAsync(user, loginModel.Password);

            if (result.Succeeded)
            {
                return Ok("User created");
            }
            return BadRequest(new ApiError(InnerErrors.Other, "Signup error"));
        }

        [HttpPost("edit-user")]
        public async Task<IActionResult> EditUser([FromBody] EditUserDTO editUserModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new ApiError(InnerErrors.InvalidUserData, "Invalid user data"));
            }
            if (await _userManager.FindByNameAsync(editUserModel.Username!) != null && editUserModel.Username != user.UserName)
            {
                return BadRequest(new ApiError(InnerErrors.UsernameAlreadyExists, "Username alreay exists"));
            }
            
            user.Theme = editUserModel.Theme;
            var resultThemeUpdate = await _userManager.UpdateAsync(user);
            var resultUsernameUpdate = await _userManager.SetUserNameAsync(user, editUserModel.Username);
            if (resultUsernameUpdate.Succeeded && resultThemeUpdate.Succeeded)
            {
                return Ok("User has been updated");
            }
            return BadRequest(new ApiError(InnerErrors.Other, "User edit error"));
        }

        [HttpGet("access-denied")]
        [AllowAnonymous]
        public async Task<IActionResult> AccessDenied()
        {
            return Unauthorized(new ApiError(InnerErrors.NotAuthorized, "User is not authorized"));
        }

        [HttpGet("user-status")]
        public async Task<IActionResult> UserStatus()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new ApiError(InnerErrors.InvalidUserData, "Invalid user data"));
            }

            return Ok(new
            {
                id = user.Id,
                username = user.UserName,
                theme = user.Theme,
            });
        }

        [HttpGet("reg-status")]
        [AllowAnonymous]
        public async Task<IActionResult> RegStatus()
        {
            if (!_registrationIsPossible)
            {
                return Ok(new RegStatusDTO(false, false));
            }
            else if (_registrationForOnlyFirst)
            {
                return Ok(new RegStatusDTO(true, !_userManager.Users.Any()));
            }
            return Ok(new RegStatusDTO(true, true));
        }
    }
}
