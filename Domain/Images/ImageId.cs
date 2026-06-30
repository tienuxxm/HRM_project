namespace Domain.Images;

public record ImageId(Guid Value)
{
    public static ImageId New() => new(Guid.NewGuid());
}