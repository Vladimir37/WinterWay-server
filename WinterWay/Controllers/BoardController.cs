using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterWay.Data;
using WinterWay.Models.Database;
using WinterWay.Models.DTOs.Requests;
using WinterWay.Models.DTOs.Error;
using WinterWay.Enums;

namespace WinterWay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BoardController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<UserModel> _userManager;

        public BoardController(UserManager<UserModel> userManager, ApplicationContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBoard([FromBody] CreateBoardDTO createBoardForm)
        {
            var user = await _userManager.GetUserAsync(User);
            var newBoard = new BoardModel
            {
                Name = createBoardForm.Name,
                RollType = createBoardForm.RollType,
                RollStart = createBoardForm.RollStart,
                RollDays = createBoardForm.RollDays,
                Color = createBoardForm.Color,
                Favorite = false,
                Archived = false,
                LastImage = null,
                CreationDate = DateTime.UtcNow,
                User = user!
            };
            _db.Boards.Add(newBoard);
            _db.SaveChanges();
            return Ok(newBoard);
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditBoard([FromBody] EditBoardDTO editBoardForm)
        {
            var user = await _userManager.GetUserAsync(User);

            user = await _db.Users
                .Include(u => u.Boards)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            var targetBoard = user.Boards
                .FirstOrDefault(b => b.Id == editBoardForm.Id);

            if (targetBoard == null)
            {
                return BadRequest(new ApiError(InnerErrors.ElementNotFound, "Board does not exists"));
            }

            targetBoard.Name = editBoardForm.Name;
            targetBoard.RollType = editBoardForm.RollType;
            targetBoard.RollStart = editBoardForm.RollStart;
            targetBoard.RollDays = editBoardForm.RollDays;
            targetBoard.Color = editBoardForm.Color;
            targetBoard.Favorite = editBoardForm.Favorite;
            targetBoard.Archived = editBoardForm.Archived;
            _db.SaveChanges();
            return Ok(targetBoard);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllBoards()
        {
            var user = await _userManager.GetUserAsync(User);

            user = await _db.Users
                .Include(u => u.Boards)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            return Ok(user!.Boards.ToList());
        }
    }
}
