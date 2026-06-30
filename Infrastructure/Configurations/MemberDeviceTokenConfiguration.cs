using Domain.MemberDeviceTokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class MemberDeviceTokenConfiguration : IEntityTypeConfiguration<MemberDeviceToken>
{
    public void Configure(EntityTypeBuilder<MemberDeviceToken> builder)
    {
        builder.ToTable("member_device_token");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new MemberDeviceTokenId(value));
    }
}