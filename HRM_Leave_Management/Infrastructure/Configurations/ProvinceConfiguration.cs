using Domain.Provinces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal class ProvinceConfiguration : IEntityTypeConfiguration<Province>
{
    public void Configure(EntityTypeBuilder<Province> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ProvinceId(value));

        builder.HasMany(x => x.Districts)
            .WithOne(x => x.Province)
            .HasForeignKey(x => x.ProvinceId);
    }
}