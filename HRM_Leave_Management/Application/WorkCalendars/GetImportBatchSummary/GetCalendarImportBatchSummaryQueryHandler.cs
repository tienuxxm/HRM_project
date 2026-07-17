using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.WorkCalendars;
using Domain.LeaveRequests;
using Microsoft.EntityFrameworkCore;

namespace Application.WorkCalendars.GetImportBatchSummary;

internal sealed class GetCalendarImportBatchSummaryQueryHandler : IQueryHandler<GetCalendarImportBatchSummaryQuery, CalendarImportBatchSummaryResponse>
{
    private readonly ICalendarImportBatchRepository _batchRepository;
    private readonly ICalendarImportBatchRowRepository _rowRepository;
    private readonly ILeaveRequestRecalculationAuditRepository _auditRepository;
    private readonly ILeaveRequestRepository _leaveRequestRepository;

    public GetCalendarImportBatchSummaryQueryHandler(
        ICalendarImportBatchRepository batchRepository,
        ICalendarImportBatchRowRepository rowRepository,
        ILeaveRequestRecalculationAuditRepository auditRepository,
        ILeaveRequestRepository leaveRequestRepository)
    {
        _batchRepository = batchRepository;
        _rowRepository = rowRepository;
        _auditRepository = auditRepository;
        _leaveRequestRepository = leaveRequestRepository;
    }

    public async Task<Result<CalendarImportBatchSummaryResponse>> Handle(GetCalendarImportBatchSummaryQuery request, CancellationToken cancellationToken)
    {
        var batchId = new CalendarImportBatchId(request.BatchId);
        var batch = await _batchRepository.GetByIdAsync(batchId, cancellationToken);
        if (batch == null)
        {
            return Result.Failure<CalendarImportBatchSummaryResponse>(new Error("ImportBatch.NotFound", "Import batch not found."));
        }

        var rows = await _rowRepository.GetByBatchIdAsync(batchId, cancellationToken);
        var audits = await _auditRepository.GetByBatchIdAsync(batchId, cancellationToken);

        var affectedRequests = new List<RecalculationAuditResponse>();

        foreach (var audit in audits)
        {
            var lr = await _leaveRequestRepository.GetEntitiesAsQueryable()
                .Include(x => x.Employee)
                .Include(x => x.LeaveType)
                .FirstOrDefaultAsync(x => x.Id == audit.LeaveRequestId, cancellationToken);

            if (lr != null)
            {
                affectedRequests.Add(new RecalculationAuditResponse
                {
                    AuditId = audit.Id.Value,
                    LeaveRequestId = audit.LeaveRequestId.Value,
                    EmployeeName = lr.Employee?.FullName ?? "Unknown",
                    LeaveTypeName = lr.LeaveType?.Name ?? "Unknown",
                    StartDate = lr.StartDate,
                    EndDate = lr.EndDate,
                    OldStatus = audit.OldStatus.ToString(),
                    NewStatus = audit.NewStatus.ToString(),
                    OldDuration = audit.OldDuration,
                    NewDuration = audit.NewDuration
                });
            }
        }

        var response = new CalendarImportBatchSummaryResponse
        {
            BatchId = batch.Id.Value,
            FileName = batch.FileName,
            UploadedAt = batch.CreatedAt,
            ProcessedAt = batch.ProcessedAt,
            UpdatedDaysCount = rows.Count(r => r.Status == ImportRowStatus.Applied),
            AffectedRequests = affectedRequests
        };

        return response;
    }
}
