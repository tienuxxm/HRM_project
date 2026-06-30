using Application.Abstractions.Messaging;

namespace Application.Orders.ConfirmPayment;

public record ConfirmOrderPaymentCommand(Dictionary<string, string> Query)
    : ICommand<ConfirmPaymentResponse>;