using Application.Abstractions.Messaging;
using Application.Members.Responses;

namespace Application.Members.Search;

public record MemberSearchCommand(string SearchValue) : ICommand<List<MemberResponse>>
{
}