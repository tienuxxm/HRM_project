using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Categories;
using Domain.Shared;
using Domain.Members;

namespace Infrastructure.Configurations;


    
internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Id)
            .HasConversion(category => category.Value, value => new CategoryId(value));


        builder.Property(category => category.CategoryName)
            .HasMaxLength(200)
            .HasConversion(categoryName => categoryName.Value, value => new CategoryName(value));


        builder.Property(order => order.Description)
         .HasMaxLength(2000)
         .HasConversion(note => note.Value, value => new Description(value));

    }
}

