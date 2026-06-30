using Application.Abstractions.Messaging;

namespace Application.Orders.SystemConfirmPayment;

public record SystemConfirmPaymentCommand(Guid OrderId) : ICommand;