using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Domain.Abstractions;

namespace Application.Members.Import;

public class GetExampleImportFileCommandHandler : ICommandHandler<GetExampleImportFileCommand, string>
{
    private readonly IAwsS3Service _awsS3Service;

    public GetExampleImportFileCommandHandler(IAwsS3Service awsS3Service)
    {
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<string>> Handle(GetExampleImportFileCommand request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(
            Result.Success(_awsS3Service.GetUrlPresign("MemberImportExampleV2.xlsx", 60) ?? ""));
    }
}