using Domain.Partners;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.ToTable("partner");

        builder.HasKey(partner => partner.Id);

        builder.Property(partner => partner.Id)
            .HasConversion(partner => partner.Value, value => new PartnerId(value));

        builder.Property(p => p.PartnerName)
            .IsRequired()
            .HasMaxLength(155)
            .HasConversion(name => name.Value, value => new PartnerName(value));

        builder.Property(p => p.Address)
            .HasConversion(address => address.Value, value => new PartnerAddress(value));

        builder.Property(p => p.PhoneNumber)
            .HasConversion(phoneNumber => phoneNumber.Value, value => new PhoneNumber(value));

        builder.Property(p => p.Email)
            .HasConversion(email => email.Value, value => new Domain.Shared.Email(value));

        builder.Property(p => p.QrCode)
            .HasConversion(qr => qr.Value, value => new ImageUrl(value));


        builder.HasMany(partner => partner.Vouchers)
            .WithOne(voucher => voucher.Partner)
            .HasForeignKey(voucher => voucher.PartnerId);

        builder.Property(order => order.PartnerName)
            .HasMaxLength(255)
            .HasConversion(note => note.Value, value => new PartnerName(value));
    }
}