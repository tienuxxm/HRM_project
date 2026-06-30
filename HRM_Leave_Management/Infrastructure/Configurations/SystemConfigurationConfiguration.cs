using Domain.SystemConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class SystemConfigurationConfiguration : IEntityTypeConfiguration<SystemConfiguration>
{
    public void Configure(EntityTypeBuilder<SystemConfiguration> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, value => new SystemConfigurationId(value));

        builder.Property(x => x.ConfigName)
            .HasConversion(x => x.Value, value => new ConfigName(value));

        builder.Property(x => x.ConfigJsonValue)
            .HasConversion(x => x.Value, value => new ConfigJsonValue(value));
    }
}