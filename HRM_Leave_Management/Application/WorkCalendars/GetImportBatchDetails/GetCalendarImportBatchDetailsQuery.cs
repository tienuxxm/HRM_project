using Application.Abstractions.Messaging;

namespace Application.WorkCalendars.GetImportBatchDetails;

public sealed record GetCalendarImportBatchDetailsQuery(Guid BatchId) : IQuery<CalendarImportBatchDetailsResponse>;
