using Application.Abstractions.Messaging;

namespace Application.WorkCalendars.GetImportBatchSummary;

public sealed record GetCalendarImportBatchSummaryQuery(Guid BatchId) : IQuery<CalendarImportBatchSummaryResponse>;
