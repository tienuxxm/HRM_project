using Domain.MemberNotifications;
using Domain.Members;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class MemberNotificationConfiguration : IEntityTypeConfiguration<MemberNotification>
{
    public void Configure(EntityTypeBuilder<MemberNotification> builder)
    {
        builder.ToTable("member_notification");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new MemberNotificationId(value));

        builder.Property(x => x.Content)
            .HasConversion(content => content.Value, value => new Content(value));

        builder.Property(x => x.Title)
            .HasConversion(title => title.Value, value => new Title(value));

        builder.Property(x => x.MemberId)
            .HasConversion(id => id.Value, value => new MemberId(value));

        builder.Property(x => x.NotificationType)
            .HasConversion(type => type.Value, value => new MemberNotificationType(value));

        builder.Property(x => x.ReferenceId)
            .HasConversion(id => id.Value, value => new ReferenceId(value));
    }
}