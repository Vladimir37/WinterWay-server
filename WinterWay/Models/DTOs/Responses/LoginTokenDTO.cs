using System.IdentityModel.Tokens.Jwt;

namespace WinterWay.Models.DTOs.Responses
{
    public class LoginTokenDTO
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public LoginTokenDTO(JwtSecurityToken token) 
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token);
            Expiration = token.ValidTo;
        }
    }
}
