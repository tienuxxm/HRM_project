using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class DataProtectionKeyConfiguration : IEntityTypeConfiguration<DataProtectionKey>
{
    public void Configure(EntityTypeBuilder<DataProtectionKey> builder)
    {
        builder.HasKey(d => d.Id);
        builder.ToTable("data_protection_keys");
        builder.Property(x => x.Id)
            .HasColumnName("id");
        builder.Property(x => x.Xml)
            .HasColumnName("xlm");
        builder.Property(x => x.FriendlyName)
            .HasColumnName("friendly_name");
    }
}