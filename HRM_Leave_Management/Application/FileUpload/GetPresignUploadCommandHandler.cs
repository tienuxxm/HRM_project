using Application.Abstractions.Authentication;
using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Domain.Abstractions;

namespace Application.FileUpload;

public class GetPresignUploadCommandHandler : ICommandHandler<GetPresignUploadCommand, PresignUploadResponse>
{
    private readonly IMemberContext _memberContext;
    private readonly IAwsS3Service _awsS3Service;

    public GetPresignUploadCommandHandler(IMemberContext memberContext, IAwsS3Service awsS3Service)
    {
        _memberContext = memberContext;
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<PresignUploadResponse>> Handle(GetPresignUploadCommand request,
        CancellationToken cancellationToken)
    {
        var key = _memberContext.IdentityId + "/" + Guid.NewGuid().ToString();
        var url = _awsS3Service.GetPreignUploadUrl(key, request.ContentType);
        if (string.IsNullOrEmpty(url))
        {
            return Result.Failure<PresignUploadResponse>(new Error("GetPresignUpload.Fail", "Fail to get Presign url"));
        }

        return await Task.FromResult(Result.Success(new PresignUploadResponse()
        {
            Url = url,
            Key = key
        }));
    }
}