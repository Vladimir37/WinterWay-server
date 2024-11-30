using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WinterWay.Data;
using WinterWay.Enums;
using WinterWay.Models.Database;
using WinterWay.Models.DTOs.Error;
using WinterWay.Models.DTOs.Responses;

namespace WinterWay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BackupController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<UserModel> _userManager;
        private readonly IConfiguration _config;
        
        private readonly bool _importAvailable;
        
        public BackupController(ApplicationContext db, UserManager<UserModel> userManager, IConfiguration config)
        {
            _db = db;
            _userManager = userManager;
            _config = config;
            
            var registrationConfig = _config.GetSection("Registration");
            _importAvailable = registrationConfig.GetValue<bool>("Import");
        }

        [HttpPost("import")]
        [AllowAnonymous]
        public async Task<IActionResult> Import([FromBody] JsonElement userRawJson)
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
            var user = JsonSerializer.Deserialize<UserObjectDTO>(userRawJson.GetRawText(), options);

            if (user == null)
            {
                return BadRequest(new ApiError(InternalError.InvalidUserData, "Invalid data format"));
            }

            var backlogId = user.TargetUser.BacklogSprintId;
            Dictionary<int, int?> actualSprints = new Dictionary<int, int?>();
            
            user.TargetUser.BacklogSprintId = null;
            
            await _db.Users.AddAsync(user.TargetUser);
            
            await _db.SaveChangesAsync();

            foreach (var board in user.AllBoards)
            {
                actualSprints[board.Id] = board.ActualSprintId;
                board.ActualSprintId = null;
            }

            await _db.Boards.AddRangeAsync(user.AllBoards);
            
            await _db.SaveChangesAsync();
            
            await _db.Sprints.AddRangeAsync(user.AllSprints);
            
            await _db.SaveChangesAsync();
            
            foreach (var board in user.AllBoards)
            {
                board.ActualSprintId = actualSprints[board.Id];
            }
            
            user.TargetUser.BacklogSprintId = backlogId;
            await _db.SprintResults.AddRangeAsync(user.AllSprintResults);
            await _db.Tasks.AddRangeAsync(user.AllTasks);
            
            await _db.SaveChangesAsync();
            
            await _db.Subtasks.AddRangeAsync(user.AllSubtasks);
            await _db.TextCounters.AddRangeAsync(user.AllTextCounters);
            await _db.SumCounters.AddRangeAsync(user.AllSumCounters);
            await _db.NumericCounters.AddRangeAsync(user.AllNumericCounters);
            
            await _db.SaveChangesAsync();
            
            await _db.Calendars.AddRangeAsync(user.AllCalendars);
            
            await _db.SaveChangesAsync();
            
            await _db.CalendarRecords.AddRangeAsync(user.AllCalendarRecords);
            await _db.CalendarValues.AddRangeAsync(user.AllCalendarValues);
            await _db.Timers.AddRangeAsync(user.AllTimers);
            
            await _db.SaveChangesAsync();
            
            await _db.TimerSessions.AddRangeAsync(user.AllTimerSessions);
            await _db.Notifications.AddRangeAsync(user.AllNotifications);

            await _db.SaveChangesAsync();

            return Ok(user.TargetUser.UserName);
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportUserData()
        {
            var user = await _userManager.GetUserAsync(User);
            
            var targetUser = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user!.Id);
            var allBoards = await _db.Boards.AsNoTracking().Where(b => b.UserId == user!.Id).ToListAsync();
            var allSprints = await _db.Sprints.AsNoTracking().Where(s => s.Board.UserId == user!.Id).ToListAsync();
            var allSprintResults = await _db.SprintResults.AsNoTracking().Where(sr => sr.Sprint.Board.UserId == user!.Id).ToListAsync();
            var allTasks = await _db.Tasks.AsNoTracking().Where(t => t.Board.UserId == user!.Id).ToListAsync();
            var allSubtasks = await _db.Subtasks.AsNoTracking().Where(s => s.Task.Board.UserId == user!.Id).ToListAsync();
            var allTextCounters = await _db.TextCounters.AsNoTracking().Where(tc => tc.Task.Board.UserId == user!.Id).ToListAsync();
            var allSumCounters = await _db.SumCounters.AsNoTracking().Where(sc => sc.Task.Board.UserId == user!.Id).ToListAsync();
            var allNumericCounters = await _db.NumericCounters.AsNoTracking().Where(nc => nc.Task.Board.UserId == user!.Id).ToListAsync();
            var allCalendars = await _db.Calendars.AsNoTracking().Where(c => c.UserId == user!.Id).ToListAsync(); 
            var allCalendarRecords = await _db.CalendarRecords.AsNoTracking().Where(c => c.Calendar.UserId == user!.Id).ToListAsync(); 
            var allCalendarValues = await _db.CalendarValues.AsNoTracking().Where(cv => cv.Calendar.UserId == user!.Id).ToListAsync(); 
            var allTimers = await _db.Timers.AsNoTracking().Where(t => t.UserId == user!.Id).ToListAsync(); 
            var allTimerSessions = await _db.TimerSessions.AsNoTracking().Where(ts => ts.Timer.UserId == user!.Id).ToListAsync(); 
            var allNotifications = await _db.Notifications.AsNoTracking().Where(n => n.UserId == user!.Id).ToListAsync();

            var userData = new UserObjectDTO()
            {
                TargetUser = targetUser,
                AllBoards = allBoards,
                AllSprints = allSprints,
                AllSprintResults = allSprintResults,
                AllTasks = allTasks,
                AllSubtasks = allSubtasks,
                AllTextCounters = allTextCounters,
                AllSumCounters = allSumCounters,
                AllNumericCounters = allNumericCounters,
                AllCalendars = allCalendars,
                AllCalendarRecords = allCalendarRecords,
                AllCalendarValues = allCalendarValues,
                AllTimers = allTimers,
                AllTimerSessions = allTimerSessions,
                AllNotifications = allNotifications
            };
            
            return Ok(userData);
        }
    }
}