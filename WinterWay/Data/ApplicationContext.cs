using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WinterWay.Models.Database;

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
        public DbSet<NumericCounter> NumericCounters { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            //
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
