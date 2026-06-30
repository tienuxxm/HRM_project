using Domain.Abstractions;
using Domain.Images;
using Domain.Vouchers;

namespace Domain.Images;

public sealed class Image : Entity<ImageId>
{
    private Image(ImageId id,  ImageName imageName, ImageLink imageLink,  DateTime createdDate )
    {
        Id = id;
        ImageName = imageName;
        ImageLink = imageLink;
        CreatedDate = createdDate;
    }

    
    
    public ImageName ImageName { get; private set; }
    
    public ImageLink ImageLink { get; private set; }

    public DateTime CreatedDate { get; private set; }

    public static Image Create( ImageName imageName, ImageLink imageLink,DateTime createdDate)
    {
        return new Image(ImageId.New(),imageName, imageLink, createdDate); 
    }
    
    
    public void Update(ImageName imageName, ImageLink imageLink)
    {
        ImageName = imageName;
        ImageLink = imageLink;
    }
    
}