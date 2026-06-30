using Domain.Abstractions;

namespace Domain.Images;

public class ImageErrors
{
    public static Error NotFound = new(
        "Image.Found",
        "The member with the specified identifier was not found");

    public static Error InvalidCredentials = new(
        "Image.InvalidCredentials",
        "The provided credentials were invalid");
}