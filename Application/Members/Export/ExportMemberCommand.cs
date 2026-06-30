using Application.Abstractions.Messaging;

namespace Application.Members.Export;

public record ExportMemberCommand : ICommand<byte[]>;