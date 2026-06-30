using Domain.Abstractions;

namespace Domain.Members.Events;

public record AssignedMembershipClassDomainEvent(MemberId MemberId) : IDomainEvent;