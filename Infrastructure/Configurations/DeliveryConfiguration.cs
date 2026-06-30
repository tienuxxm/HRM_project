using Domain.Deliveries;
using Domain.Orders;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Note = Domain.Deliveries.Note;

namespace Infrastructure.Configurations;

public class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<Delivery> builder)
    {
        builder.HasKey(delivery => delivery.Id);

        builder.Property(delivery => delivery.Id)
            .HasConversion(id => id.Value, value => new DeliveryId(value));

        builder.Property(delivery => delivery.Note)
            .HasConversion(note => note.Value, value => new Note(value));

        builder.Property(delivery => delivery.CompanyAddress)
            .HasConversion(companyAddress => companyAddress.Value, value => new CompanyAddress(value));

        builder.Property(delivery => delivery.CompanyEmail)
            .HasConversion(email => email.Value, value => new CompanyEmail(value));

        builder.Property(delivery => delivery.CompanyName)
            .HasConversion(name => name.Value, value => new CompanyName(value));

        builder.Property(delivery => delivery.PhoneNumber)
            .IsRequired()
            .HasConversion(phoneNumber => phoneNumber.Value, value => new PhoneNumber(value));

        builder.Property(delivery => delivery.ReceiverName)
            .IsRequired()
            .HasConversion(receiverName => receiverName.Value, value => new ReceiverName(value));

        builder.Property(delivery => delivery.ReceivingAddress)
            .IsRequired()
            .HasConversion(receivingAddress => receivingAddress.Value, value => new ReceivingAddress(value));

        builder.Property(delivery => delivery.CompanyTaxCode)
            .HasConversion(taxCode => taxCode.Value, value => new CompanyTaxCode(value));

        builder.Property(delivery => delivery.HasRequestCutlery)
            .HasConversion(hasRequestCutlery => hasRequestCutlery.Value, value => new HasRequestCutlery(value));

        builder.Property(delivery => delivery.HasIssueAnInvoice)
            .HasConversion(hasIssueAnInvoice => hasIssueAnInvoice.Value, value => new HasIssueAnInvoice(value));

        builder.Property(delivery => delivery.OrderId)
            .IsRequired()
            .HasConversion(orderId => orderId.Value, value => new OrderId(value));
    }
}