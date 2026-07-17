using Application.Abstractions.Messaging;

namespace Application.WorkCalendars.ConfirmImportBatch;

public sealed record ConfirmCalendarImportBatchCommand(Guid BatchId) : ICommand;
