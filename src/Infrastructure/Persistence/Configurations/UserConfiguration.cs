using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(p => p.Value, value => new UserId(value));
        
        builder.Property(p => p.Email)
            .IsRequired()
            .HasMaxLength(255); 

        builder.Property(p => p.Name)
            .HasMaxLength(50); 
    }
}
