using Domain.Users;
using Domain.UserToRoles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class UserToRoleConfiguration : IEntityTypeConfiguration<UserToRole>
{
    public void Configure(EntityTypeBuilder<UserToRole> builder)
    {
        builder.ToTable("user_to_role");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(id => id.Value, value => new UserToRoleId(value));
 
        builder.HasOne(utr => utr.User)
            .WithMany(u => u.Roles)
            .HasForeignKey(utr => utr.UserId);
        
        builder.HasOne(utr => utr.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(utr => utr.RoleId);

    }
}