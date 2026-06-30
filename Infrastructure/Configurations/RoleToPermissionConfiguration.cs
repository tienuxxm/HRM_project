using Domain.RoleToPermissions;
using Domain.Users;
using Domain.UserToRoles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class RoleToPermissionConfiguration : IEntityTypeConfiguration<RoleToPermission>
{
    public void Configure(EntityTypeBuilder<RoleToPermission> builder)
    {
        builder.ToTable("role_to_permission");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(id => id.Value, value => new RoleToPermissionId(value));
 
        builder.HasOne(rtp => rtp.Permission)
            .WithMany(u => u.Roles)
            .HasForeignKey(utr => utr.PermissionId);
        
        builder.HasOne(utr => utr.Role)
            .WithMany(r => r.Permissions)
            .HasForeignKey(utr => utr.RoleId);

    }
}