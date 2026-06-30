using Domain.MemberPointHistories;
using Domain.Members;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class MemberPointHistoryConfiguration : IEntityTypeConfiguration<MemberPointHistory>
{
    public void Configure(EntityTypeBuilder<MemberPointHistory> builder)
    {
        builder.ToTable("member_point_history");

        builder.HasKey(mPoint => mPoint.Id);

        builder.Property(mPoint => mPoint.Id)
            .HasConversion(id => id.Value, value => new MemberPointHistoryId(value));

        builder.Property(mPoint => mPoint.Title)
            .HasMaxLength(200)
            .IsRequired()
            .HasConversion(title => title.Value, value => new Title(value));

        builder.Property(mPoint => mPoint.MemberPoint)
            .IsRequired()
            .HasConversion(point => point.Value, value => new MemberPoint(value));

        builder.Property(mPoint => mPoint.MemberId)
            .HasConversion(memberPoint => memberPoint.Value, value => new MemberId(value));

        builder.HasOne<Member>()
            .WithMany(x => x.MemberPointHistories)
            .HasForeignKey(mPoint => mPoint.MemberId)
            .IsRequired();
    }
}