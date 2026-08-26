using Dtd.Application.Events;
using Dtd.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dtd.Infrastructure.Persistence;

/// <summary>
/// Commits the tracked changes and dispatches the domain events raised by the involved
/// aggregate roots (publishing them as <see cref="DomainEventNotification{TEvent}"/>).
/// </summary>
internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly DtdDbContext _dbContext;
    private readonly IMediator _mediator;

    public UnitOfWork(DtdDbContext dbContext, IMediator mediator)
    {
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var aggregates = _dbContext.ChangeTracker
            .Entries<AggregateRoot<Guid>>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        var result = await _dbContext.SaveChangesAsync(cancellationToken);

        await DispatchDomainEventsAsync(aggregates, cancellationToken);

        return result;
    }

    private async Task DispatchDomainEventsAsync(IReadOnlyCollection<AggregateRoot<Guid>> aggregates, CancellationToken cancellationToken)
    {
        foreach (var aggregate in aggregates)
        {
            var events = aggregate.DomainEvents.ToList();
            aggregate.ClearDomainEvents();

            foreach (var domainEvent in events)
            {
                var notification = Activator.CreateInstance(
                    typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType()),
                    domainEvent);

                if (notification is not null)
                {
                    await _mediator.Publish(notification, cancellationToken);
                }
            }
        }
    }
}