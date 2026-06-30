using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Application.Abstractions.AWS;
using Domain.Abstractions;
using Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.RoleServices;

public class AwsS3Service : IAwsS3Service
{
    private readonly IAmazonS3 _awsS3Client;
    private readonly string? _defaultBucketName;

    public AwsS3Service(IAmazonS3 awsS3Client, IConfiguration configuration, IUserRepository userRepository)
    {
        _awsS3Client = awsS3Client;
        _defaultBucketName = configuration.GetSection("AWS").GetSection("Profile").Value;
    }

    public async Task<Result> UploadFileAsync(MemoryStream stream, string key)
    {
        try
        {
            var fileTransferUtility = new TransferUtility(_awsS3Client);
            await fileTransferUtility.UploadAsync(stream, _defaultBucketName, key);
            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(new Error("Upload.Fail", "Fail to upload file"));
        }
    }

    public Task<Result> UploadFileAsync(string bucketName)
    {
        throw new NotImplementedException();
    }

    public Task<Result> GetObjectFromS3Async(string bucketName, string keyName)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteObjectFromS3Async(string bucketName, string keyName)
    {
        throw new NotImplementedException();
    }

    public IFormFile GetFileFromS3(string s3Key)
    {
        var getObjectRequest = new GetObjectRequest
        {
            BucketName = _defaultBucketName,
            Key = s3Key
        };

        using (var getObjectResponse = _awsS3Client.GetObjectAsync(getObjectRequest).Result)
        {
            var formFile = new FormFile(getObjectResponse.ResponseStream, 0, getObjectResponse.Headers.ContentLength,
                "file", s3Key);
            return formFile;
        }
    }

    public string? GetUrlPresign(string key, int expiredMinute = 60)
    {
        return _awsS3Client.GetPreSignedURL(new GetPreSignedUrlRequest()
        {
            Expires = DateTime.Now.AddMinutes(expiredMinute),
            Key = key,
            BucketName = _defaultBucketName,
            Verb = HttpVerb.GET
        });
    }

    public string? GetPreignUploadUrl(string key, string contentType)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _defaultBucketName,
            Key = key,
            ContentType = contentType,
            Verb = HttpVerb.PUT, // Use PUT for uploading
            Expires = DateTime.UtcNow.AddMinutes(30)
        };
        return _awsS3Client.GetPreSignedURL(request);
    }
}