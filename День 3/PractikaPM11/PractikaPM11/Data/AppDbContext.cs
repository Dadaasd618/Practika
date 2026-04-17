using Microsoft.EntityFrameworkCore;

namespace PractikaPM11.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Visitor> Visitors { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<VisitPurpose> VisitPurposes { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<IndividualRequest> IndividualRequests { get; set; }
        public DbSet<GroupRequest> GroupRequests { get; set; }
        public DbSet<GroupMemberEntity> GroupMembers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=Practice_KhranitelPRO;Username=postgres;Password=postgres");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<GroupMemberEntity>().ToTable("group_members");
        }
    }

    public class GroupRequest
    {
        public int GroupId { get; set; }
        public int RequestId { get; set; }
        public string? TemplateFilePath { get; set; }
        public string? PhotosArchivePath { get; set; }
    }

    public class GroupMemberEntity
    {
        public int MemberId { get; set; }
        public int GroupId { get; set; }
        public int VisitorId { get; set; }
        public int RowNumber { get; set; }
    }
}