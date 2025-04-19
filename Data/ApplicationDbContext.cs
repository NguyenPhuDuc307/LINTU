using LMS.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LMS.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public ApplicationDbContext()
    {
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        IEnumerable<EntityEntry> modified = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);
        foreach (EntityEntry item in modified)
        {
            if (item.Entity is IDateTracking changedOrAddedItem)
            {
                if (item.State == EntityState.Added)
                {
                    changedOrAddedItem.CreateDate = DateTime.Now;
                }
                else
                {
                    changedOrAddedItem.LastModifiedDate = DateTime.Now;
                }
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityRole>().ToTable("Roles").Property(x => x.Id).HasMaxLength(50).IsUnicode(false);
        builder.Entity<User>().ToTable("Users").Property(x => x.Id).HasMaxLength(50).IsUnicode(false);
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        // Cấu hình `ClassRoomId` là `Guid`
        builder.Entity<ClassRoom>()
            .Property(cr => cr.Id)
            .HasDefaultValueSql("NEWID()");
        builder.Entity<ClassDetail>()
            .HasKey(cd => new { cd.ClassRoomId, cd.UserId });
        // Cấu hình mối quan hệ giữa `ClassDetail` và `ClassRoom`
        builder.Entity<ClassDetail>()
            .Property(cd => cd.ClassRoomId)
            .HasColumnType("uniqueidentifier");

        // Cấu hình mối quan hệ giữa `Post` và `ClassRoom`
        builder.Entity<Post>()
            .Property(p => p.ClassRoomId)
            .HasColumnType("uniqueidentifier");

        // Cấu hình mối quan hệ giữa `Comment` và `ClassRoom`
        builder.Entity<Comment>()
            .Property(c => c.ClassRoomId)
            .HasColumnType("uniqueidentifier");

        // Cấu hình mối quan hệ giữa `Comment` và `Post`
        builder.Entity<Comment>()
            .HasOne(c => c.Post)
            .WithMany()
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.NoAction);

        // Cấu hình mối quan hệ giữa `Comment` và `User`
        builder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cấu hình mối quan hệ cho CompletedLecture để tránh multiple cascade paths
        builder.Entity<CompletedLecture>()
            .HasOne(cl => cl.ClassRoom)
            .WithMany()
            .HasForeignKey(cl => cl.ClassRoomId)
            .OnDelete(DeleteBehavior.Restrict); // Change from Cascade to Restrict

        builder.Entity<CompletedLecture>()
            .HasOne(cl => cl.Lecture)
            .WithMany()
            .HasForeignKey(cl => cl.LectureId)
            .OnDelete(DeleteBehavior.Cascade); // Keep this as Cascade

        builder.Entity<CompletedLecture>()
            .HasOne(cl => cl.User)
            .WithMany()
            .HasForeignKey(cl => cl.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Set this to Restrict as well

        // Apply the same fix for LectureNote to prevent similar issues
        builder.Entity<LectureNote>()
            .HasOne(ln => ln.ClassRoom)
            .WithMany()
            .HasForeignKey(ln => ln.ClassRoomId)
            .OnDelete(DeleteBehavior.Restrict);
    }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<ClassRoom> ClassRooms { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<ClassDetail> ClassDetails { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<ChatRoom> ChatRooms { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Submission> Submissions { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Lecture> Lectures { get; set; }
    public DbSet<CompletedLecture> CompletedLectures { get; set; }
    public DbSet<LectureNote> LectureNotes { get; set; }
}
