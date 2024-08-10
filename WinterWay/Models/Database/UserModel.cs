using Microsoft.AspNetCore.Identity;
using WinterWay.Enums;

namespace WinterWay.Models.Database
{
    public class UserModel : IdentityUser
    {
        public ThemeType Theme { get; set; } = ThemeType.Light;

        public List<BoardModel> Boards { get; set; } = new List<BoardModel>();
    }
}
