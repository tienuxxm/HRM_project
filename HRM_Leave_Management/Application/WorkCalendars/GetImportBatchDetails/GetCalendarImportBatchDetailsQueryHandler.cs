using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.WorkCalendars;

namespace Application.WorkCalendars.GetImportBatchDetails;

internal sealed class GetCalendarImportBatchDetailsQueryHandler : IQueryHandler<GetCalendarImportBatchDetailsQuery, CalendarImportBatchDetailsResponse>
{
    private readonly ICalendarImportBatchRepository _batchRepository;
    private readonly ICalendarImportBatchRowRepository _rowRepository;

    public GetCalendarImportBatchDetailsQueryHandler(
        ICalendarImportBatchRepository batchRepository,
        ICalendarImportBatchRowRepository rowRepository)
    {
        _batchRepository = batchRepository;
        _rowRepository = rowRepository;
    }

    public async Task<Result<CalendarImportBatchDetailsResponse>> Handle(GetCalendarImportBatchDetailsQuery request, CancellationToken cancellationToken)
    {
        var batchId = new CalendarImportBatchId(request.BatchId);
        var batch = await _batchRepository.GetByIdAsync(batchId, cancellationToken);
        if (batch == null)
        {
            return Result.Failure<CalendarImportBatchDetailsResponse>(new Error("ImportBatch.NotFound", "Import batch not found."));
        }

        var rows = await _rowRepository.GetByBatchIdAsync(batchId, cancellationToken);

        var response = new CalendarImportBatchDetailsResponse
        {
            BatchId = batch.Id.Value,
            Status = batch.Status.ToString(),
            FileName = batch.FileName,
            UploadedAt = batch.CreatedAt,
            Rows = rows.Select(r => new BatchRowResponse
            {
                Id = r.Id.Value,
                RowIndex = r.RowIndex,
                Date = r.Date,
                DayType = r.DayType.ToString(),
                WorkShift = r.WorkShift.ToString(),
                Description = r.Description,
                IsValid = r.Status == ImportRowStatus.Valid || r.Status == ImportRowStatus.Applied,
                ErrorMessage = r.ErrorMessage
            })
            .OrderBy(r => r.RowIndex)
            .ToList()
        };

        return response;
    }
}
