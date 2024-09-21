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
        public DbSet<NumericCounterModel> NumericCounters { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            //
        }
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
        }
    }
}
