using Application.Abstractions.Messaging;
using Domain.Members;

namespace Application.Sms.ValidateCode;

public record ValidateSmsCodeCommand(PhoneNumber PhoneNumber, string Code) : ICommand;