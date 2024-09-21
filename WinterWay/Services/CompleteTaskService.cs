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

        public CompleteTaskService(ApplicationContext db)
        {
            _db = db;
        }

        public TaskModel ChangeStatus(TaskModel TargetTask, bool status)
        {
            if (status)
            {
                TargetTask.IsDone = true;
                TargetTask.ClosingDate = DateTime.UtcNow;
            } 
            else
            {
                TargetTask.IsDone = false;
                TargetTask.ClosingDate = null;
            }

            _db.SaveChanges();
            return TargetTask;
        }

        public TaskModel CheckAutocompleteStatus(TaskModel TargetTask)
        {
            bool needsToBeClosed = false;

            if (!TargetTask.IsDone && TargetTask.AutoComplete)
            {
                needsToBeClosed = TargetTask.Type switch
                {
                    TaskType.TodoList => TargetTask.Subtasks.All(s => s.IsDone),
                    TaskType.TextCounter => TargetTask.TextCounters.Count() >= TargetTask.MaxCounter,
                    TaskType.NumericCounter => TargetTask.NumericCounter!.Value >= TargetTask.MaxCounter,
                    _ => false,
                };
            }

            if (needsToBeClosed)
            {
                ChangeStatus(TargetTask, true);
            }

            return TargetTask;
        }
    }
}
