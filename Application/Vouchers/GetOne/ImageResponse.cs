
namespace Application.Vouchers.GetOne;

public sealed class ImageResponse{
 
    public Guid Id { get; init; }
    
    public string ImageName { get; init; }
    
    public string ImageLink { get; init; }

    public DateTime CreatedDate { get; init; }
    
}