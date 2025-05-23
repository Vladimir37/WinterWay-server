﻿using System.Globalization;
using WinterWay.Data;
using WinterWay.Enums;
using Microsoft.EntityFrameworkCore;
using WinterWay.Models.Database.Planner;

namespace WinterWay.Services
{
    public class RollService
    {
        private readonly ApplicationContext _db;
        private readonly BackgroundImageService _backgroundImageService;
        private readonly CultureInfo _culture;
        
        public RollService(ApplicationContext db, IConfiguration config, BackgroundImageService backgroundImageService)
        {
            _db = db;
            _backgroundImageService = backgroundImageService;
            _culture = new CultureInfo("en-US");
        }

        public async Task<int> RollSprint(BoardModel board, List<int>? spilloverTasks)
        {
            int lastImage = -1;
            if (board.ActualSprint != null)
            {
                var currentSprint = board.ActualSprint;
                currentSprint.Active = false;
                currentSprint.ClosingDate = DateTime.UtcNow;
                lastImage = currentSprint.Image;
            }
            board.CurrentSprintNumber++;
            var creationDate = DateTime.UtcNow;
            var expirationDate = GetExpirationDate(creationDate, board.RollDays, board.RollStart, board.RollType);
            var sprintName = GenerateName(board.RollType, board.Name, creationDate, board.CurrentSprintNumber);
            var backgroundImageNum = SelectImageForSprint(board.RollType, creationDate, lastImage);
            var newSprint = new SprintModel
            {
                Name = sprintName,
                Active = true,
                Image = backgroundImageNum,
                CreationDate = creationDate,
                ExpirationDate = expirationDate,
                ClosingDate = null,
                Number = board.CurrentSprintNumber,
                Board = board,
            };
            _db.Sprints.Add(newSprint);
            board.ActualSprint = newSprint;
            await _db.SaveChangesAsync();

            var templateTasks = await _db.Tasks
                .Where(t => t.BoardId == board.Id && t.Sprint == null && t.IsTemplate)
                .Include(t => t.NumericCounter)
                .Include(t => t.TextCounters)
                .Include(t => t.Subtasks)
                .ToListAsync();

            var clonedTask = templateTasks.Select(t => t.CloneToNewSprint(newSprint)).ToList();

            _db.Tasks.AddRange(clonedTask);

            await _db.SaveChangesAsync();

            return await MoveTasksToNewSprint(board, newSprint, spilloverTasks);
        }

        public async Task GenerateResult(SprintModel sprint, int tasksSpill, int tasksToBacklog)
        {
            TimeSpan? difference = sprint.ClosingDate - sprint.CreationDate;
            int sprintDuration = (int)Math.Ceiling(difference!.Value.TotalDays);
            int taskDone = sprint.Tasks.Where(t => t.IsDone).ToList().Count;
            int taskClosed = sprint.Tasks.Where(t => !t.IsDone).ToList().Count;
            var result = new SprintResultModel
            {
                Days = sprintDuration,
                TasksDone = taskDone,
                TasksClosed = taskClosed,
                TasksSpill = tasksSpill,
                TasksToBacklog = tasksToBacklog
            };
            _db.SprintResults.Add(result);
            sprint.SprintResult = result;
            await _db.SaveChangesAsync();
        }

        public async Task<int> MoveTasksToBacklog(BoardModel board, SprintModel backlogSprint, List<int>? tasksToBacklog)
        {
            if (tasksToBacklog != null && tasksToBacklog.Count > 0)
            {
                var tasks = await _db.Tasks
                    .Where(t => t.Board == board && tasksToBacklog.Contains(t.Id))
                    .ToListAsync();

                var tasksInBacklogCount = await _db.Tasks
                    .Include(t => t.Board)
                    .Where(t => t.Board.UserId == board.UserId)
                    .Where(t => t.SprintId == backlogSprint.Id)
                    .Where(t => !t.IsDone)
                    .CountAsync();

                foreach (var task in tasks)
                {
                    task.Sprint = backlogSprint;
                    task.Board = backlogSprint.Board;
                    task.IsDone = false;
                    task.SortOrder = tasksInBacklogCount;
                    tasksInBacklogCount++;
                }
                await _db.SaveChangesAsync();
                return tasks.Count;
            }
            return 0;
        }

        private async Task<int> MoveTasksToNewSprint(BoardModel board, SprintModel sprint, List<int>? tasksToNewSprint)
        {
            if (tasksToNewSprint != null && tasksToNewSprint.Count > 0)
            {
                var tasks = await _db.Tasks
                    .Where(t => t.Board == board && tasksToNewSprint.Contains(t.Id))
                    .ToListAsync();

                var tasksInSprintCount = await _db.Tasks
                    .Include(t => t.Board)
                    .Where(t => t.Board.UserId == board.UserId)
                    .Where(t => t.SprintId == sprint.Id)
                    .Where(t => !t.IsDone)
                    .CountAsync();

                foreach (var task in tasks)
                {
                    task.Sprint = sprint;
                    task.IsDone = false;
                    task.SortOrder = tasksInSprintCount;
                    tasksInSprintCount++;
                }

                await _db.SaveChangesAsync();
                return tasks.Count;
            }
            return 0;
        }

        private DateTime? GetExpirationDate(DateTime creationDate, int rollDays, RollStart rollStart, RollType rollType)
        {
            DateTime? expirationDate = null;
            if (rollStart == RollStart.StartDate)
            {
                int daysUntilNextMonday = ((int)DayOfWeek.Monday - (int)creationDate.DayOfWeek + 7) % 7;
                expirationDate = rollType switch
                {
                    RollType.None => null,
                    RollType.Day => creationDate.AddDays(1),
                    RollType.Week => creationDate.AddDays(daysUntilNextMonday == 0 ? 7 : daysUntilNextMonday),
                    RollType.Month => new DateTime(creationDate.Year, creationDate.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1),
                    RollType.Year => new DateTime(creationDate.Year + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    RollType.Custom => creationDate.AddDays(rollDays),
                    _ => null,
                };
                if (expirationDate != null)
                {
                    expirationDate = expirationDate.Value.Date;
                }
            }
            else
            {
                expirationDate = rollType switch
                {
                    RollType.None => null,
                    RollType.Day => creationDate.AddHours(24),
                    RollType.Week => creationDate.AddDays(7),
                    RollType.Month => creationDate.AddDays(30),
                    RollType.Year => creationDate.AddDays(365),
                    RollType.Custom => creationDate.AddDays(rollDays),
                    _ => null,
                };
            }
            return expirationDate;
        }

        private string GenerateName(RollType rollType, string boardName, DateTime creationDate, int sprintNumber)
        {
            return rollType switch
            {
                RollType.None => boardName,
                RollType.Day => creationDate.ToString("dd MMMM", _culture),
                RollType.Week => $"Week {sprintNumber}",
                RollType.Month => creationDate.ToString("MMMM", _culture),
                RollType.Year => $"Year {creationDate.ToString("yyyy", _culture)}",
                RollType.Custom => $"Sprint {sprintNumber}",
                _ => "Nameless sprint",
            };
        }

        public int SelectImageForSprint(RollType rollType, DateTime creationDate, int lastImage)
        {
            if (rollType == RollType.Day)
            {
                return (int)creationDate.DayOfWeek;
            }
            else if (rollType == RollType.Month)
            {
                return creationDate.Month;
            }

            int maxVal;
            if (rollType == RollType.None)
            {
                maxVal = _backgroundImageService.BackgroundData!.Count.None;
            }
            else
            {
                maxVal = _backgroundImageService.BackgroundData!.Count.Other;
            }
            Random rnd = new Random();
            int newImage;
            do
            {
                newImage = rnd.Next(0, maxVal);
            }
            while (newImage == lastImage);
            return newImage;
        }
    }
}
