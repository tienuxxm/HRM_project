using Amazon.S3.Model.Internal.MarshallTransformations;
using Domain.Shared;
using Domain.Vouchers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> builder)
    {
        builder.ToTable("voucher");

        builder.HasKey(voucher => voucher.Id);

        builder.Property(v => v.Id)
            .HasConversion(voucher => voucher.Value, value => new VoucherId(value));

        builder.Property(v => v.ContentVoucher)
            .IsRequired(false)
            .HasConversion(content => content.Value, value => new ContentVoucher(value));

        builder.Property(v => v.TitleVoucher)
            .HasMaxLength(255)
            .HasConversion(title => title.Value, value => new TitleVoucher(value));

        builder.Property(v => v.ImageUrl)
            .HasConversion(value => value.Value, value => new ImageUrl(value));

        builder.Property(v => v.QrCodeImageUrl)
            .HasConversion(qrImage => qrImage.Value, value => new ImageUrl(value));

        builder.Property(v => v.QrCode)
            .HasConversion(qrCode => qrCode.Value, value => new QrCode(value));

        builder.Property(v => v.Place)
            .HasConversion(place => place.Value, value => new Place(value));

        builder.Property(v => v.Conditions)
            .HasConversion(conditions => conditions.Value, value => new Conditions(value));
    }
}