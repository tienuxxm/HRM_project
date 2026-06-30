using Application.Abstractions.Messaging;
using Domain.Partners;
using Domain.Shared;
using Domain.Vouchers;

namespace Application.Partners.Update;

public record UpdatePartnerCommand(
    PartnerId PartnerId,
    string? Name,
    string? email,
    string? phoneNumber,
    string? address,
    List<Guid>? VoucherIds) : ICommand<Partner>;

