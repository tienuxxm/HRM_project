using Domain.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("role");
    
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.Id)
            .HasConversion(voucher => voucher.Value, value => new RoleId(value));
        
        builder.Property(r => r.ResourceName)
            .HasMaxLength(155)
            .IsRequired()
            .HasConversion(resourceName => resourceName.Value, value => new ResourceName(value));
        
        builder.Property(r => r.DisplayName)
            .IsRequired()
            .HasConversion(displayName => displayName.Value, value => new DisplayName(value));

        builder.HasMany(r => r.Users)
            .WithOne(utr => utr.Role)
            .HasForeignKey(utr => utr.RoleId);
        
        builder.HasMany(r => r.Permissions)
            .WithOne(rtp => rtp.Role)
            .HasForeignKey(utr => utr.RoleId);
    }
}