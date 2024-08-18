using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WinterWay.Data;
using WinterWay.Models.Database;
using WinterWay.Models.DTOs.Requests;

namespace WinterWay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SprintController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<UserModel> _userManager;

        public SprintController(UserManager<UserModel> userManager, ApplicationContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditSprint([FromBody] EditSprintDTO editSprintForm)
        {
            //
        }
    }
}
