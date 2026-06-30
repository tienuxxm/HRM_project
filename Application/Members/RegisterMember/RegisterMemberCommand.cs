using Application.Abstractions.Messaging;

namespace Application.Members.RegisterMember;

public sealed record RegisterMemberCommand(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string PhoneNumber,
    string Address,
    int? DistrictId,
    DateTime? BirthDate) : ICommand<Guid>;