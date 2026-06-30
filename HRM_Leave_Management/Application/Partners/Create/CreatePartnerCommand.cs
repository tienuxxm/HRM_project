using Application.Abstractions.Messaging;
using Domain.Partners;
using Domain.Shared;

namespace Application.Partners.Create;

public sealed record CreatePartnerCommand(
    PartnerName Name, PartnerAddress? Address, PhoneNumber? PhoneNumber, Email? Email) : ICommand<Guid>;