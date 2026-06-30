using Application.Abstractions.Messaging;

namespace Application.Orders.ConfirmPayment;

public record ConfirmOrderIPNCommand(Dictionary<string, string> Query, string IP) : ICommand<IPNResponse>;