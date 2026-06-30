using Domain.LeaveTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal sealed class LeaveTypeConfiguration : IEntityTypeConfiguration<LeaveType>
{
    public void Configure(EntityTypeBuilder<LeaveType> builder)
    {
        builder.ToTable("leave_type");

        builder.HasKey(lt => lt.Id);

        builder.Property(lt => lt.Id)
            .HasConversion(id => id.Value, value => new LeaveTypeId(value));

        builder.Property(lt => lt.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(lt => lt.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(lt => lt.Code).IsUnique();

        builder.Property(lt => lt.Description)
            .HasMaxLength(500);

        builder.Property(lt => lt.DefaultDays)
            .IsRequired();
    }
}
