using Application.Abstractions.Messaging;
using Application.Partners.GetOne;

namespace Application.Partners.GetAll;

public record GetAllPartnerCommand() : ICommand<List<PartnerResponse>>;