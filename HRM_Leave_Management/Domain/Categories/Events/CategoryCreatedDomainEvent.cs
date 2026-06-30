using Domain.Abstractions;

namespace Domain.Categories.Events;

public sealed record CategoryCreatedDomainEvent(CategoryId CategoryId) : IDomainEvent;