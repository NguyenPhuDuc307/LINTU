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

}
