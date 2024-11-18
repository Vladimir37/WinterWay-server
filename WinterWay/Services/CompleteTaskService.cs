using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using WinterWay.Data;
using WinterWay.Enums;
using WinterWay.Models.Database;

namespace WinterWay.Services
{
    public class CompleteTaskService
    {
        private readonly ApplicationContext _db;
        private readonly NotificationService _notificationService;

        public CompleteTaskService(ApplicationContext db, NotificationService notificationService)
        {
            _db = db;
            _notificationService = notificationService;
        }

        public TaskModel ChangeStatus(TaskModel targetTask, bool status)
        {
            if (targetTask.IsDone == status)
            {
                return targetTask;
            }

            var tasksInSprintInStatusCount = _db.Tasks
                .Include(t => t.Board)
                .Where(t => t.IsDone == status)
                .Where(t => t.SprintId == targetTask.SprintId)
                .Where(t => t.Board.UserId == targetTask.Board.UserId)
                .Count();

            if (status)
            {
                targetTask.IsDone = true;
                targetTask.SortOrder = tasksInSprintInStatusCount;
                targetTask.ClosingDate = DateTime.UtcNow;
            } 
            else
            {
                targetTask.IsDone = false;
                targetTask.SortOrder = tasksInSprintInStatusCount;
                targetTask.ClosingDate = null;
            }

            _db.SaveChanges();
            return targetTask;
        }

        public async Task<TaskModel> CheckAutocompleteStatus(TaskModel targetTask, string userId)
        {
            bool needsToBeClosed = false;

            if (!targetTask.IsDone && targetTask.AutoComplete)
            {
                needsToBeClosed = targetTask.Type switch
                {
                    TaskType.TodoList => targetTask.Subtasks.All(s => s.IsDone),
                    TaskType.TextCounter => (targetTask.TextCounters.Count() >= targetTask.MaxCounter) && targetTask.MaxCounter > 0,
                    TaskType.NumericCounter => (targetTask.NumericCounter!.Value >= targetTask.MaxCounter) && targetTask.MaxCounter > 0,
                    _ => false,
                };
            }

            if (needsToBeClosed && !targetTask.IsDone)
            {
                ChangeStatus(targetTask, true);
                await _notificationService.CreateNotification(NotificationType.TaskCounterReachedMaxValue, targetTask.Id, userId);
            }

            return targetTask;
        }

        public List<TaskModel> SortAllTasks(List<TaskModel> tasks)
        {
            tasks = tasks
                .OrderBy(t => t.SortOrder)
                .ToList();

            var num = 0;
            foreach (var task in tasks)
            {
                task.SortOrder = num;
                num++;
            }

            _db.SaveChanges();

            return tasks;
        }
    }
}
