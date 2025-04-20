using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WinterWay.Models.Database.Auth;
using WinterWay.Models.Database.Calendar;
using WinterWay.Models.Database.Diary;
using WinterWay.Models.Database.Notification;
using WinterWay.Models.Database.Planner;
using WinterWay.Models.Database.Timer;

namespace WinterWay.Data
{
    public class ApplicationContext : IdentityDbContext<UserModel>
    {
        public DbSet<BoardModel> Boards { get; set; }
        public DbSet<SprintModel> Sprints { get; set; }
        public DbSet<SprintResultModel> SprintResults { get; set; }
        public DbSet<TaskModel> Tasks { get; set; }
        public DbSet<SubtaskModel> Subtasks { get; set; }
        public DbSet<TextCounterModel> TextCounters { get; set; }
        public DbSet<SumCounterModel> SumCounters { get; set; }
        public DbSet<NumericCounterModel> NumericCounters { get; set; }
        public DbSet<CalendarModel> Calendars { get; set; }
        public DbSet<CalendarRecordModel> CalendarRecords { get; set; }
        public DbSet<CalendarRecordBooleanModel> CalendarRecordBooleans { get; set; }
        public DbSet<CalendarRecordNumericModel> CalendarRecordNumerics { get; set; }
        public DbSet<CalendarRecordTimeModel> CalendarRecordTimes { get; set; }
        public DbSet<CalendarRecordFixedModel> CalendarRecordFixeds { get; set; }
        public DbSet<CalendarValueModel> CalendarValues { get; set; }
        public DbSet<TimerModel> Timers { get; set; }
        public DbSet<TimerSessionModel> TimerSessions { get; set; }
        public DbSet<NotificationModel> Notifications { get; set; }
        public DbSet<DiaryGroupModel> DiaryGroups { get; set; }
        public DbSet<DiaryActivityModel> DiaryActivities { get; set; }
        public DbSet<DiaryRecordModel> DiaryRecords { get; set; }
        public DbSet<DiaryRecordGroupModel> DiaryRecordGroups { get; set; }
        public DbSet<DiaryRecordActivityModel> DiaryRecordActivities { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {}
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<BoardModel>()
                .HasMany(b => b.AllSprints)
                .WithOne(s => s.Board)
                .HasForeignKey(s => s.BoardId);

            builder.Entity<BoardModel>()
                .HasOne(b => b.ActualSprint)
                .WithMany()
                .HasForeignKey(s => s.ActualSprintId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<CalendarModel>()
                .HasOne(c => c.DefaultRecord)
                .WithMany() 
                .HasForeignKey(c => c.DefaultRecordId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<CalendarRecordModel>()
                .HasOne(r => r.Calendar)
                .WithMany(c => c.CalendarRecords)
                .HasForeignKey(r => r.CalendarId);
        }
    }
}
