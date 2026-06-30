using Application.Abstractions.Messaging;
using Domain.Partners;

namespace Application.Partners.GetOne;

public record GetOnePartnerCommand(PartnerId PartnerId) :  IQuery<PartnerResponse>;
