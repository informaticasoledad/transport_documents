using Dtd.Domain.Common;
using MediatR;

namespace Dtd.Application.Events;

/// <summary>
/// Wraps a domain event as a MediatR notification so that application-layer handlers
/// (e.g. to write an outbox entry or emit integration events) can react to it.
/// </summary>
public sealed record DomainEventNotification<TEvent>(TEvent DomainEvent) : INotification
    where TEvent : IDomainEvent;