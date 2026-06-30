using Application.Abstractions.Messaging;
using Domain.Members;

namespace Application.Members.Create;

public record CreateMemberCommand(FirstName FirstName, LastName LastName, Email Email, PhoneNumber PhoneNumber,
    Address Address, DateTime? BirthDate) : ICommand;