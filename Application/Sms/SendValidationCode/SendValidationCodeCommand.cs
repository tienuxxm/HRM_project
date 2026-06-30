using Application.Abstractions.Messaging;
using Domain.PhoneValidationCheck;

namespace Application.Sms.SendValidationCode;

public record SendValidationCodeCommand(string PhoneNumber) : ICommand<string>;