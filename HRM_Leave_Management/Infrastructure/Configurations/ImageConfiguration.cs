using Domain.Images;
using Domain.Vouchers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class ImageConfiguration : IEntityTypeConfiguration<Image>
{
    public void Configure(EntityTypeBuilder<Image> builder)
    {
        builder.ToTable("image");
    
        builder.HasKey(image => image.Id);
        
        builder.Property(image => image.Id)
            .IsRequired()
            .HasConversion(partner => partner.Value, value => new ImageId(value));

        builder.Property(i => i.ImageLink)
            .IsRequired()
            .HasMaxLength(2000)
            .HasConversion(name => name.Value, value => new ImageLink(value));
        
        
        builder.Property(i => i.ImageName)
            .IsRequired()
            .HasMaxLength(255)
            .HasConversion(name => name.Value, value => new ImageName(value));
        
        
    }
}