using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Members;

namespace Application.Members.IsPhoneNumberValid;

public record IsPhoneNumberValidCommand(PhoneNumber PhoneNumber) : ICommand<BooleanResponse>;