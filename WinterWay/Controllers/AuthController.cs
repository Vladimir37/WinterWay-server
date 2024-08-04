using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using WinterWay.Data;
using WinterWay.Models.DTOs.Requests;
using WinterWay.Models.Database;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WinterWay.Controllers
{
    [Route("/api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly SignInManager<UserModel> _signInManager;
        private readonly IConfiguration _config;

        public AuthController(UserManager<UserModel> userManager, SignInManager<UserModel> signInManager, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModelDTO loginModel)
        {
            var user = await _userManager.FindByNameAsync(loginModel.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                var authClaims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    expires: DateTime.Now.AddDays(Convert.ToDouble(_config["Jwt:DurationInDays"])),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                return Ok(token);
            }
            return Unauthorized("Incorrect user data");
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] LoginModelDTO loginModel)
        {
            var user = new UserModel { UserName = loginModel.Username };
            var result = await _userManager.CreateAsync(user, loginModel.Password);

            if (result.Succeeded)
            {
                return Ok("User created");
            }
            return BadRequest("Incorrect data");
        }
    }
}
