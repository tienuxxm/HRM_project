using Domain.Images;
using Domain.Partners;

namespace Infrastructure.Repositories;

internal sealed class ImageRepository : Repository<Image, ImageId>, IImageRepository
{
    public ImageRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}