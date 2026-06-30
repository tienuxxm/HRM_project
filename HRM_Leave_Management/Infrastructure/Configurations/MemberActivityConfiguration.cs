using Domain.MemberActivities;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class MemberActivityConfiguration : IEntityTypeConfiguration<MemberActivity>
{
    public void Configure(EntityTypeBuilder<MemberActivity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new MemberActivityId(value));

        builder.Property(x => x.Message)
            .HasConversion(message => message.Value, value => new LogMessage(value));
    }
}