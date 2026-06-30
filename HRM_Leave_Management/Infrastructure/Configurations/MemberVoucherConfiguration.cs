using Domain.Members;
using Domain.MemberVouchers;
using Domain.Vouchers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class MemberVoucherConfiguration : IEntityTypeConfiguration<MemberVoucher>
{
    public void Configure(EntityTypeBuilder<MemberVoucher> builder)
    {
        builder.ToTable("member_voucher");

        builder.HasKey(mv => mv.Id);

        builder.Property(mv => mv.Id)
            .HasConversion(id => id.Value, value => new MemberVoucherId(value));

        builder.Property(mv => mv.MemberId)
            .HasConversion(id => id.Value, value => new MemberId(value));

        builder.Property(mv => mv.VoucherId)
            .HasConversion(id => id.Value, value => new VoucherId(value));

        builder.HasOne(mv => mv.Voucher)
            .WithMany()
            .HasForeignKey(mv => mv.VoucherId);

        builder.HasOne(mv => mv.Member)
            .WithMany()
            .HasForeignKey(mv => mv.MemberId);
    }
}