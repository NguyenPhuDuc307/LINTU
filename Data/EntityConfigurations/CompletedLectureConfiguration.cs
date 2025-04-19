using LMS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Data.EntityConfigurations
{
    public class CompletedLectureConfiguration : IEntityTypeConfiguration<CompletedLecture>
    {
        public void Configure(EntityTypeBuilder<CompletedLecture> builder)
        {
            // Configure the primary key
            builder.HasKey(cl => cl.Id);

            // Configure the relationship with ClassRoom
            builder.HasOne(cl => cl.ClassRoom)
                .WithMany()
                .HasForeignKey(cl => cl.ClassRoomId)
                .OnDelete(DeleteBehavior.Restrict); // Change from Cascade to Restrict

            // Configure the relationship with Lecture
            builder.HasOne(cl => cl.Lecture)
                .WithMany()
                .HasForeignKey(cl => cl.LectureId)
                .OnDelete(DeleteBehavior.Cascade); // Keep this as Cascade

            // Configure the relationship with User
            builder.HasOne(cl => cl.User)
                .WithMany()
                .HasForeignKey(cl => cl.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Set this to Restrict as well
        }
    }
}
