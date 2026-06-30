using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Partners;

namespace Application.Partners.GetAllPaged;

public record GetAllPartnerPagedCommand() : PagedQuery<Partner, PartnerId>, ICommand<GetAllPartnerPagedResponse>;