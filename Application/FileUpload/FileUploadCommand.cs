using Application.Abstractions.Messaging;

namespace Application.FileUpload;

public record FileUploadCommand(MemoryStream File, string fileName) : ICommand<string>;