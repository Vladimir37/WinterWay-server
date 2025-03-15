using WinterWay.Models.Database.Auth;
using WinterWay.Models.Database.Calendar;
using WinterWay.Models.Database.Notification;
using WinterWay.Models.Database.Planner;
using WinterWay.Models.Database.Timer;

namespace WinterWay.Models.DTOs.Responses
{
    public class UserObjectDTO
    {
        public UserModel TargetUser { get; set; }
        public List<BoardModel> AllBoards { get; set; }
        public List<SprintModel> AllSprints { get; set; }
        public List<SprintResultModel> AllSprintResults { get; set; }
        public List<TaskModel> AllTasks { get; set; }
        public List<SubtaskModel> AllSubtasks { get; set; }
        public List<TextCounterModel> AllTextCounters { get; set; }
        public List<SumCounterModel> AllSumCounters { get; set; }
        public List<NumericCounterModel> AllNumericCounters { get; set; }
        public List<CalendarModel> AllCalendars { get; set; }
        public List<CalendarRecordModel> AllCalendarRecords { get; set; }
        public List<CalendarValueModel> AllCalendarValues { get; set; }
        public List<TimerModel> AllTimers { get; set; }
        public List<TimerSessionModel> AllTimerSessions { get; set; }
        public List<NotificationModel> AllNotifications { get; set; }
    }
}
