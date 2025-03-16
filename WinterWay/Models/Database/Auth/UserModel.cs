using Microsoft.AspNetCore.Identity;
using WinterWay.Enums;
using WinterWay.Models.Database.Calendar;
using WinterWay.Models.Database.Diary;
using WinterWay.Models.Database.Notification;
using WinterWay.Models.Database.Planner;
using WinterWay.Models.Database.Timer;

namespace WinterWay.Models.Database.Auth
{
    public class UserModel : IdentityUser
    {
        public ThemeType Theme { get; set; } = ThemeType.Light;
        public bool AutoCompleteTasks { get; set; } = false;

        public int? BacklogSprintId { get; set; }
        public SprintModel? BacklogSprint { get; set; }
        public List<BoardModel> Boards { get; set; } = new List<BoardModel>();
        public List<CalendarModel> Calendars { get; set; } = new List<CalendarModel>();
        public List<TimerModel> Timers { get; set; } = new List<TimerModel>();
        public List<NotificationModel> Notifications { get; set; } = new List<NotificationModel>();
        public List<DiaryGroupModel> DiaryGroups { get; set; } = new List<DiaryGroupModel>();
        public List<DiaryRecordModel> DiaryRecords { get; set; } = new List<DiaryRecordModel>();
    }
}
