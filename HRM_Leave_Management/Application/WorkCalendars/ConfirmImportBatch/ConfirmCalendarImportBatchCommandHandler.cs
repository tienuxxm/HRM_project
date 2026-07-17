using Application.Abstractions.Messaging;
using Application.Abstractions.Authentication;
using Domain.Abstractions;
using Domain.WorkCalendars;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.WorkCalendars.ConfirmImportBatch;

internal sealed class ConfirmCalendarImportBatchCommandHandler : ICommandHandler<ConfirmCalendarImportBatchCommand>
{
    private readonly ICalendarImportService _calendarImportService;
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;

    public ConfirmCalendarImportBatchCommandHandler(
        ICalendarImportService calendarImportService,
        IUserContext userContext,
        IUserRepository userRepository)
    {
        _calendarImportService = calendarImportService;
        _userContext = userContext;
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(ConfirmCalendarImportBatchCommand request, CancellationToken cancellationToken)
    {
        var identityId = _userContext.IdentityId;
        var user = await _userRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(u => u.IdentityId == new IdentityId(identityId), cancellationToken);

        if (user == null)
        {
            return Result.Failure(new Error("ConfirmBatch.UserNotFound", "User not found."));
        }

        var batchId = new CalendarImportBatchId(request.BatchId);
        var result = await _calendarImportService.ApplyBatchAsync(batchId, user.Id.Value, cancellationToken);
        return result;
    }
}
