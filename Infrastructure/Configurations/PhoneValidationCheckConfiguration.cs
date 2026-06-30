using Domain.PhoneValidationCheck;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class PhoneValidationCheckConfiguration : IEntityTypeConfiguration<PhoneValidationCheck>
{
    public void Configure(EntityTypeBuilder<PhoneValidationCheck> builder)
    {
        builder.ToTable("phone_validation_check");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new PhoneValidationCheckId(value));

        builder.Property(x => x.PhoneNumber)
            .HasConversion(phone => phone.Value, value => new PhoneNumber(value));

        builder.Property(x => x.Code)
            .HasConversion(code => code.Value, value => new Code(value));

        builder.Property(x => x.SendCodeCount)
            .HasConversion(count => count.Value, value => new SendCodeCount(value));
    }
}