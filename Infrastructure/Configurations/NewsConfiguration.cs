using Domain.News;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal sealed class NewsConfiguration : IEntityTypeConfiguration<News>
{
    public void Configure(EntityTypeBuilder<News> builder)
    {
        builder.ToTable("news");

        builder.HasKey(news => news.Id);

        builder.Property(news => news.Id)
            .HasConversion(newsId => newsId.Value, value => new NewsId(value));

        builder.Property(news => news.Content)
            .HasConversion(content => content.Value, value => new Content(value));

        builder.Property(news => news.Description)
            .HasMaxLength(255)
            .HasConversion(des => des.Value, value => new Description(value));

        builder.Property(news => news.Title)
            .HasMaxLength(255)
            .HasConversion(title => title.Value, value => new Title(value));

        builder.Property(news => news.Thumbnail)
            .HasMaxLength(255)
            .HasConversion(thumbnail => thumbnail.Value, value => new ImageUrl(value));

        builder.Property(news => news.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_DATE");
    }
}