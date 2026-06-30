using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("user");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(id => id.Value, value => new UserId(value));


        builder.Property(u => u.Name)
            .IsRequired()
            .HasConversion(name => name.Value, value => new Name(value));

        builder.Property(u => u.IdentityId)
            .HasConversion(id => id.Value, value => new IdentityId(value));

        builder.Property(u => u.Username)
            .IsUnicode()
            .IsRequired()
            .HasConversion(username => username.Value, value => new Username(value));

        builder.Property(u => u.PhoneNumber)
            .IsUnicode()
            .HasConversion(phone => phone.Value, value => new PhoneNumber(value));

        builder.Property(u => u.Email)
            .IsUnicode()
            .HasConversion(email => email.Value, value => new Domain.Users.Email(value));


        builder.HasMany(u => u.Roles)
            .WithOne(utr => utr.User)
            .HasForeignKey(utr => utr.UserId);
    }
}