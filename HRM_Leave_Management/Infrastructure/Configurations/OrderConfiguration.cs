using Domain.Deliveries;
using Domain.Invoices;
using Domain.Members;
using Domain.Orders;
using Domain.PaymentDetails;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Note = Domain.Orders.Note;

namespace Infrastructure.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(order => order.Id);

        builder.Property(order => order.Id)
            .HasConversion(order => order.Value, value => new OrderId(value));

        builder.Property(order => order.MemberId)
            .HasConversion(memberId => memberId.Value, value => new MemberId(value));

        builder.Property(order => order.Note)
            .HasMaxLength(2000)
            .HasConversion(note => note.Value, value => new Note(value));

        builder.Property(order => order.OrderCode)
            .HasConversion(code => code.Value, value => new Code(value));

        builder.HasOne(x => x.Member)
            .WithMany(x => x.Orders)
            .HasForeignKey(o => o.MemberId)
            .IsRequired();

        builder.HasMany(o => o.LineItems)
            .WithOne()
            .HasForeignKey(li => li.OrderId);

        builder.HasOne(x => x.Delivery)
            .WithOne()
            .HasForeignKey<Delivery>(x => x.OrderId);

        builder.HasOne(order => order.Invoice)
            .WithOne()
            .HasForeignKey<Invoice>(i => i.OrderId);

        builder.HasOne(order => order.Booking)
            .WithOne(booking => booking.Order)
            .HasForeignKey<Order>(order => order.BookingId);

        builder.HasMany(o => o.OrderFees)
            .WithOne()
            .HasForeignKey(x => x.OrderId);

        builder.OwnsOne(li => li.TotalBill, priceBuilder =>
        {
            priceBuilder.Property(money => money.Amount)
                .IsRequired();
            priceBuilder.Property(money => money.Currency)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
        });
    }
}