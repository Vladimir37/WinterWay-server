using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using WinterWay.Data;
using WinterWay.Enums;
using WinterWay.Models.Database.Auth;
using WinterWay.Models.Database.Calendar;
using WinterWay.Models.Database.Planner;
using WinterWay.Models.DTOs.Responses.Shared;

namespace WinterWay.Services
{
    public class BackupService
    {
        private readonly ApplicationContext _db;
        private readonly IDataProtector _protector;

        public BackupService(ApplicationContext db, IDataProtectionProvider protector)
        {
            _db = db;
            _protector = protector.CreateProtector("BackupService");
        }

        public bool Import(string userEncrypted, out bool formatError, out string username)
        {
            UserModel? user = null;
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var json = _protector.Unprotect(userEncrypted);
                user = JsonSerializer.Deserialize<UserModel>(json, options);
            }
            catch (CryptographicException)
            {
                formatError = true;
                username = String.Empty;
                return false;
            }

            if (
                user == null || 
                user.UserName == string.Empty || 
                user.UserName == null || 
                user.PasswordHash == string.Empty ||
                user.PasswordHash == null
            )
            {
                formatError = true;
                username = String.Empty;
                return false;
            }
            
            int? backlogSprintId = user.BacklogSprintId;
            Dictionary<int, int?> actualSprints = new Dictionary<int, int?>();
            Dictionary<int, int?> defaultRecordIds = new Dictionary<int, int?>();
            Dictionary<int, CalendarRecordModel?> defaultRecords = new Dictionary<int, CalendarRecordModel?>();
            List<CalendarRecordFixedModel> fixedRecords = new List<CalendarRecordFixedModel>();
            
            try
            {
                user.BacklogSprintId = null;
                user.BacklogSprint = null;
                foreach (var board in user.Boards)
                {
                    actualSprints[board.Id] = board.ActualSprintId;
                    board.ActualSprintId = null;
                    board.ActualSprint = null;
                    foreach (var sprint in board.AllSprints)
                    {
                        sprint.Tasks = new List<TaskModel>();
                    }
                }

                foreach (var calendar in user.Calendars)
                {
                    defaultRecordIds[calendar.Id] = calendar.DefaultRecordId;
                    defaultRecords[calendar.Id] = calendar.DefaultRecord;
                    calendar.DefaultRecordId = null;
                    calendar.DefaultRecord = null;
                    foreach (var record in calendar.CalendarRecords)
                    {
                        if (record.FixedVal != null)
                        {
                            record.FixedVal.FixedValue = null;
                            fixedRecords.Add(record.FixedVal);
                        }
                        record.FixedVal = null;
                    }
                }
                
                _db.Users.Add(user);
                _db.SaveChanges();
                
                user.BacklogSprintId = backlogSprintId;
                foreach (var board in user.Boards)
                {
                    board.ActualSprintId = actualSprints[board.Id];
                }
                foreach (var calendar in user.Calendars)
                {
                    calendar.DefaultRecordId = defaultRecordIds[calendar.Id];
                    calendar.DefaultRecord = defaultRecords[calendar.Id];
                }
                _db.CalendarRecordFixeds.AddRange(fixedRecords);
                _db.SaveChanges();
                
                formatError = false;
                username = user.UserName;
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                formatError = false;
                username = String.Empty;
                return false;
            }
        }

        public async Task<string> Export(string userId)
        {
            var user = await _db.Users
                // Boards, sprints and sprint results
                .Include(u => u.Boards)
                    .ThenInclude(b => b.AllSprints)
                        .ThenInclude(s => s.SprintResult)
                // Subtasks, text, sum and numeric counters
                .Include(u => u.Boards)
                    .ThenInclude(b => b.AllTasks)
                        .ThenInclude(t => t.Subtasks)
                .Include(u => u.Boards)
                    .ThenInclude(b => b.AllTasks)
                        .ThenInclude(t => t.TextCounters)
                .Include(u => u.Boards)
                    .ThenInclude(b => b.AllTasks)
                        .ThenInclude(t => t.SumCounters)
                .Include(u => u.Boards)
                    .ThenInclude(b => b.AllTasks)
                        .ThenInclude(t => t.NumericCounter)
                // Calendars
                .Include(u => u.Calendars)
                    .ThenInclude(c => c.CalendarValues)
                .Include(u => u.Calendars)
                    .ThenInclude(c => c.CalendarRecords)
                        .ThenInclude(cr => cr.BooleanVal)
                .Include(u => u.Calendars)
                    .ThenInclude(c => c.CalendarRecords)
                       .ThenInclude(cr => cr.NumericVal)
                .Include(u => u.Calendars)
                    .ThenInclude(c => c.CalendarRecords)
                     .ThenInclude(cr => cr.TimeVal)
                .Include(u => u.Calendars)
                    .ThenInclude(c => c.CalendarRecords)
                        .ThenInclude(cr => cr.FixedVal)
                // Timers
                .Include(u => u.Timers)
                    .ThenInclude(t => t.TimerSessions)
                // Notifications
                .Include(u => u.Notifications)
                // Diary
                .Include(u => u.DiaryGroups)
                    .ThenInclude(dg => dg.Activities)
                .Include(u => u.DiaryRecords)
                    .ThenInclude(dr => dr.Groups)
                        .ThenInclude(dg => dg.Activities)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var userJson = JsonSerializer.Serialize(user);
            var protectedUserJson = _protector.Protect(userJson);
            return protectedUserJson;
        }
    }
}