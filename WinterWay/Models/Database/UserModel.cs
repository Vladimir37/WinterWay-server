﻿using Microsoft.AspNetCore.Identity;
using WinterWay.Enums;

namespace WinterWay.Models.Database
{
    public class UserModel : IdentityUser
    {
        public ThemeType Theme { get; set; } = ThemeType.Light;
        public bool AutoCompleteTasks { get; set; } = false;

        public List<BoardModel> Boards { get; set; } = new List<BoardModel>();
    }
}
