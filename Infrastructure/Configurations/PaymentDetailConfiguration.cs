using Domain.Invoices;
using Domain.Orders;
using Domain.PaymentDetails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class PaymentDetailConfiguration : IEntityTypeConfiguration<PaymentDetail>
{
    public void Configure(EntityTypeBuilder<PaymentDetail> builder)
    {
        builder.ToTable("payment_detail");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new PaymentDetailId(value));

        builder.Property(x => x.InvoiceId)
            .HasConversion(id => id.Value, value => new InvoiceId(value));

        builder.Property(x => x.TransactionRefId)
            .HasConversion(x => x.Value, value => new TransactionRefId(value));

        builder.Property(x => x.PaymentResponse)
            .HasConversion(x => x.Value, value => new PaymentResponse(value));
    }
}