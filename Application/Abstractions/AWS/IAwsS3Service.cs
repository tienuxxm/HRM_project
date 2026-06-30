using Domain.Abstractions;

namespace Application.Abstractions.AWS;

public interface IAwsS3Service
{
    Task<Result> UploadFileAsync(MemoryStream stream, string key);

    string? GetUrlPresign(string key, int expiredMinute = 60);
    string? GetPreignUploadUrl(string key, string contentType);
}