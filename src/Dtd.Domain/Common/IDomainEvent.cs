namespace Dtd.Domain.Common;

/// <summary>
/// Marker interface for domain events raised by an aggregate root.
/// The Domain layer stays free of infrastructure concerns: the Application layer
/// is responsible for dispatching these (e.g. through MediatR notifications).
/// </summary>
public interface IDomainEvent
{
}