using Application.Abstractions.Messaging;
using Domain.Partners;

namespace Application.Partners.Delete;

public record DeletePartnerCommand(PartnerId PartnerId) : ICommand;
