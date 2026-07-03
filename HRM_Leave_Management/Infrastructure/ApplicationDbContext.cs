using System.Data;
using Application.Abstractions.Clock;
using Application.Exceptions;
using Domain.Abstractions;
using Domain.Positions;
using Domain.LeaveApproverAssignments;
using Infrastructure.Outbox;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;

namespace Infrastructure;

public sealed class ApplicationDbContext : DbContext, IUnitOfWork, IDataProtectionKeyContext
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
        NullValueHandling = NullValueHandling.Ignore
    };

    private readonly IDateTimeProvider _dateTimeProvider;

    public ApplicationDbContext(
        DbContextOptions options,
        IDateTimeProvider dateTimeProvider)
        : base(options)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            AddDomainEventsAsOutboxMessages();

            var result = await base.SaveChangesAsync(cancellationToken);

            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException("Concurrency exception occurred.", ex);
        }
    }

    public override int SaveChanges()
    {
        try
        {
            AddDomainEventsAsOutboxMessages();

            var result = base.SaveChanges();

            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException("Concurrency exception occurred.", ex);
        }
    }

    public IDbTransaction BeginTransaction()
    {
        return Database.BeginTransaction().GetDbTransaction();
    }

    private void AddDomainEventsAsOutboxMessages()
    {
        try
        {
            var entities = ChangeTracker
                .Entries<IEntity>().ToList();
            var outboxMessages = entities
                .Select(entry => entry.Entity)
                .SelectMany(entity =>
                {
                    var domainEvents = entity.GetDomainEvents();

                    entity.ClearDomainEvents();

                    return domainEvents;
                })
                .Select(domainEvent => new OutboxMessage(
                    Guid.NewGuid(),
                    _dateTimeProvider.UtcNow,
                    domainEvent.GetType().Name,
                    JsonConvert.SerializeObject(domainEvent, JsonSerializerSettings)))
                .ToList();

            AddRange(outboxMessages);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
    public DbSet<Position> Positions { get; set; } = null!;
    public DbSet<LeaveApproverAssignment> LeaveApproverAssignments { get; set; } = null!;
}