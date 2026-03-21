using Domain.Users;
using Domain.Workouts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class WorkoutConfiguration : IEntityTypeConfiguration<Workout>
{
    public void Configure(EntityTypeBuilder<Workout> builder)
    {
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id)
            .HasConversion(id => id.Value, value => new WorkoutId(value));

        builder.Property(w => w.UserId)
            .HasConversion(id => id.Value, value => new UserId(value))
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(w => w.Notes)
            .HasMaxLength(1000);

        builder.HasMany(w => w.Exercises)
            .WithOne()
            .HasForeignKey(e => e.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
