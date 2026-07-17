using Application.Abstractions.Messaging;
using Application.Abstractions.Authentication;
using Domain.Abstractions;
using Domain.WorkCalendars;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.WorkCalendars.UploadImportBatch;

internal sealed class UploadCalendarImportBatchCommandHandler : ICommandHandler<UploadCalendarImportBatchCommand, Guid>
{
    private readonly ICalendarImportService _calendarImportService;
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;

    public UploadCalendarImportBatchCommandHandler(
        ICalendarImportService calendarImportService,
        IUserContext userContext,
        IUserRepository userRepository)
    {
        _calendarImportService = calendarImportService;
        _userContext = userContext;
        _userRepository = userRepository;
    }

    public async Task<Result<Guid>> Handle(UploadCalendarImportBatchCommand request, CancellationToken cancellationToken)
    {
        if (request.FileStream == null || request.FileStream.Length == 0)
        {
            return Result.Failure<Guid>(new Error("UploadBatch.EmptyFile", "File is empty."));
        }

        var identityId = _userContext.IdentityId;
        var user = await _userRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(u => u.IdentityId == new IdentityId(identityId), cancellationToken);

        if (user == null)
        {
            return Result.Failure<Guid>(new Error("UploadBatch.UserNotFound", "User not found."));
        }

        var batchResult = await _calendarImportService.ParseAndSaveDraftAsync(
            request.FileName,
            request.FileStream,
            user.Id.Value,
            cancellationToken);

        if (batchResult.IsFailure)
        {
            return Result.Failure<Guid>(batchResult.Error);
        }

        return batchResult.Value.Id.Value;
    }
}
