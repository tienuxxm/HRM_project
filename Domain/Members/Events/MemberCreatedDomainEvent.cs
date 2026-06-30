using Domain.Abstractions;

namespace Domain.Members.Events;

public sealed record MemberCreatedDomainEvent(MemberId MemberId) : IDomainEvent;