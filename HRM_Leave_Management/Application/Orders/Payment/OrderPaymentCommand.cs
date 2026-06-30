using Application.Abstractions.Messaging;
using Domain.Orders;

namespace Application.Orders.Payment;

public record OrderPaymentCommand(OrderId OrderId, string IpAdress) : ICommand<string>;