using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Domain.Abstractions;

namespace Application.FileUpload;

public class FileUploadCommandHandler : ICommandHandler<FileUploadCommand, string>
{
    private readonly IAwsS3Service _awsS3Service;

    public FileUploadCommandHandler(IAwsS3Service awsS3Service)
    {
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<string>> Handle(FileUploadCommand request, CancellationToken cancellationToken)
    {
        var file = request.File;
        var result = await _awsS3Service.UploadFileAsync(file, request.fileName);
        if (result.IsFailure)
            return Result.Failure<string>(Error.NullValue);
        return _awsS3Service.GetUrlPresign(request.fileName);
    }
}