using Domain.Notifications;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notification");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new NotificationId(value));

        builder.Property(x => x.Title)
            .HasConversion(title => title.Value, value => new Title(value));

        builder.Property(x => x.NotificationType)
            .HasConversion(x => x.Value, value => new NotificationType(value));

        builder.Property(x => x.Content)
            .HasConversion(content => content.Value, value => new Content(value));

        builder.Property(x => x.ReferenceId)
            .HasConversion(id => id.Value, value => new ReferenceId(value));
    }
}