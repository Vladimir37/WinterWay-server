using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using WinterWay.Enums;
using WinterWay.Models.DTOs.Error;
using WinterWay.Models.DTOs.Requests;
using WinterWay.Models.DTOs.Responses;
using WinterWay.Models.Database;

namespace WinterWay.Controllers
{
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
            var user = await _userManager.FindByNameAsync(loginModel.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                var authClaims = new[]
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                };

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    expires: DateTime.Now.AddDays(Convert.ToDouble(_config["Jwt:DurationInDays"])),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                return Ok(new LoginTokenDTO(token));
            }
            return Unauthorized(new ApiError(InnerErrors.InvalidUserData, "Invalid user data"));
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
            Console.WriteLine(result.Errors);
            foreach (var error in result.Errors)
            {
                Console.WriteLine(error.Description);
            }
            return BadRequest(new ApiError(InnerErrors.Other, "Signup error"));
        }

        [HttpGet("user-status")]
        public async Task<IActionResult> UserStatus()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;

            return Ok(new
            {
                userId,
                userName,
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
