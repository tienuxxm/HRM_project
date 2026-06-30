
namespace Domain.Images;

public interface IImageRepository
{
    Task<Image?> GetByIdAsync(ImageId id, CancellationToken cancellationToken = default);
    Task<List<Image>?> GetByIdsAsync(List<ImageId> id, CancellationToken cancellationToken = default);
    
    void Add(Image image);

    void Update(Image image);
    
    void Remove(Image image);
    
    void RemoveRange(List<Image> image);
    
}