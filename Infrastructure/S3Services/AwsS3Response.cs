using System.Net;

namespace Infrastructure.S3Services;

public class AwsS3Response
{
    public HttpStatusCode Status { get; set; }

    public string Message { get; set; }
}