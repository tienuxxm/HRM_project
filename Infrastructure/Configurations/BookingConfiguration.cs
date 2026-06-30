using Domain.Bookings;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Restaurants;
using Domain.Members;
using Domain.Orders;
using Domain.Shared;
using Note = Domain.Bookings.Note;
using PhoneNumber = Domain.Shared.PhoneNumber;

namespace Infrastructure.Configurations
{
    internal sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("bookings");

            builder.HasKey(booking => booking.Id);

            builder.Property(booking => booking.Id)
                .HasConversion(bookingId => bookingId.Value, value => new BookingId(value));

            builder.Property(booking => booking.FullName)
                .HasConversion(fullName => fullName.Value, value => new FullName(value));

            builder.Property(booking => booking.PhoneNumber)
                .HasConversion(phone => phone.Value, value => new PhoneNumber(value));

            builder.HasOne(booking => booking.Restaurant)
                .WithMany()
                .HasForeignKey(booking => booking.RestaurantId);

            builder.HasOne(x => x.Member)
                .WithMany()
                .HasForeignKey(booking => booking.MemberId);


            builder.Property(booking => booking.BookingCode)
                .HasConversion(code => code.Value, value => new Code(value));

            builder.Property(booking => booking.Note)
                .HasConversion(note => note.Value, value => new Note(value));
        }
    }
}