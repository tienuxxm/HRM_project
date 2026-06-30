using Domain.Districts;
using Domain.Provinces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class DistrictConfiguration : IEntityTypeConfiguration<District>
{
    public void Configure(EntityTypeBuilder<District> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new DistrictId(value));

        builder.Property(x => x.ProvinceId)
            .HasConversion(id => id.Value, value => new ProvinceId(value));

        builder.HasMany(x => x.Wards)
            .WithOne(x => x.District)
            .HasForeignKey(x => x.DistrictId);
    }
}