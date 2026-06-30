using Application.Abstractions.Messaging;

namespace Application.InvoiceHistories.Cancel;

public record CancelInvoiceCommand(List<Guid> Transactions) : ICommand;