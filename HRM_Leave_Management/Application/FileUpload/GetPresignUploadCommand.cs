using Application.Abstractions.Messaging;

namespace Application.FileUpload;

public record GetPresignUploadCommand(string ContentType) : ICommand<PresignUploadResponse>;