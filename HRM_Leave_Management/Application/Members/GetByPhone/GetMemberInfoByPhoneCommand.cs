using Application.Abstractions.Messaging;
using Application.Members.Responses;

namespace Application.Members.GetByPhone;

public record GetMemberInfoByPhoneCommand(string PhoneNumber) : ICommand<MemberResponse>;