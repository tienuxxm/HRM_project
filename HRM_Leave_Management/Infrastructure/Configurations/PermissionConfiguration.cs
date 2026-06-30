using Domain.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permission");
    
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.Id)
            .HasConversion(voucher => voucher.Value, value => new PermissionId(value));
        
        builder.Property(r => r.ResourceName)
            .HasMaxLength(155)
            .IsRequired()
            .HasConversion(resourceName => resourceName.Value, value => new ResourceName(value));
        
        builder.Property(r => r.DisplayName)
            .IsRequired()
            .HasConversion(displayName => displayName.Value, value => new DisplayName(value));

        builder.HasMany(r => r.Roles)
            .WithOne(rtp => rtp.Permission)
            .HasForeignKey(rtp => rtp.PermissionId);
    }
}