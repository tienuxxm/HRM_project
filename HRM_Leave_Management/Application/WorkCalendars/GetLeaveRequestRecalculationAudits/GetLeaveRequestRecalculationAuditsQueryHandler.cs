using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.WorkCalendars;
using Domain.LeaveRequests;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.WorkCalendars.GetLeaveRequestRecalculationAudits;

internal sealed class GetLeaveRequestRecalculationAuditsQueryHandler : IQueryHandler<GetLeaveRequestRecalculationAuditsQuery, List<LeaveRequestRecalculationAuditResponse>>
{
    private readonly ILeaveRequestRecalculationAuditRepository _auditRepository;
    private readonly IUserRepository _userRepository;

    public GetLeaveRequestRecalculationAuditsQueryHandler(
        ILeaveRequestRecalculationAuditRepository auditRepository,
        IUserRepository userRepository)
    {
        _auditRepository = auditRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<List<LeaveRequestRecalculationAuditResponse>>> Handle(GetLeaveRequestRecalculationAuditsQuery request, CancellationToken cancellationToken)
    {
        var leaveRequestId = new LeaveRequestId(request.LeaveRequestId);

        var audits = await _auditRepository.GetByLeaveRequestIdAsync(leaveRequestId, cancellationToken);

        var responseList = new List<LeaveRequestRecalculationAuditResponse>();

        foreach (var a in audits)
        {
            string? oldProcessedByName = null;
            if (a.OldProcessedBy.HasValue)
            {
                var user = await _userRepository.GetByIdAsync(new UserId(a.OldProcessedBy.Value), cancellationToken);
                oldProcessedByName = user?.Name?.Value;
            }

            responseList.Add(new LeaveRequestRecalculationAuditResponse
            {
                Id = a.Id.Value,
                BatchId = a.BatchId?.Value,
                OldStatus = a.OldStatus.ToString(),
                NewStatus = a.NewStatus.ToString(),
                OldDuration = a.OldDuration,
                NewDuration = a.NewDuration,
                OldProcessedBy = oldProcessedByName,
                OldProcessedAt = a.OldProcessedAt,
                RecalculatedAt = a.RecalculatedAt
            });
        }

        return responseList;
    }
}
