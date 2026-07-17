using System.IO;
using Application.Abstractions.Messaging;

namespace Application.WorkCalendars.UploadImportBatch;

public sealed record UploadCalendarImportBatchCommand(Stream FileStream, string FileName) : ICommand<Guid>;
