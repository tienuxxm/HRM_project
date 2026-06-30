using Domain.Districts;
using Domain.Members;
using Domain.MembershipClasses;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoneNumber = Domain.Members.PhoneNumber;

namespace Infrastructure.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("members");

        builder.HasKey(member => member.Id);

        builder.Property(member => member.Id)
            .HasConversion(memberId => memberId.Value, value => new MemberId(value));

        builder.Property(member => member.PhoneNumber)
            .HasMaxLength(200)
            .HasConversion(phoneNumber => phoneNumber.Value, value => new PhoneNumber(value));

        builder.Property(member => member.FirstName)
            .HasMaxLength(200)
            .HasConversion(firstName => firstName.Value, value => new FirstName(value));

        builder.Property(member => member.LastName)
            .HasMaxLength(200)
            .HasConversion(firstName => firstName.Value, value => new LastName(value));

        builder.Property(member => member.Address)
            .HasMaxLength(400)
            .HasConversion(address => address.Value, value => new Address(value));

        builder.Property(member => member.Email)
            .HasMaxLength(400)
            .HasConversion(email => email.Value, value => new Domain.Members.Email(value));

        builder.Property(member => member.MembershipClassId)
            .HasConversion(membershipClass => membershipClass.Value, value => new MembershipClassId(value));

        builder.Property(member => member.Avatar)
            .HasConversion(avatar => avatar.Value, value => new ImageUrl(value));

        builder.Property(x => x.MemberCode)
            .HasConversion(code => code.Value, value => new Code(value));

        builder.Property(x => x.DistrictId)
            .HasConversion(x => x.Value, value => new DistrictId(value));

        builder.HasOne(x => x.District)
            .WithMany()
            .HasForeignKey(x => x.DistrictId);

        builder.HasOne(member => member.MembershipClass)
            .WithMany()
            .HasForeignKey(member => member.MembershipClassId);

        builder.HasMany(member => member.MemberVouchers)
            .WithOne()
            .HasForeignKey(memberVoucher => memberVoucher.MemberId);

        builder.HasIndex(member => member.IdentityId).IsUnique();
    }
}