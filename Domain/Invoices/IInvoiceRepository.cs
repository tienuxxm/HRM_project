using System.Linq.Expressions;
using Domain.Abstractions;
using Domain.Orders;

namespace Domain.Invoices;

public interface IInvoiceRepository
{
    void Add(Invoice invoice);
    void RemoveRange(List<Invoice> invoices);
    void Remove(Invoice invoice);
    IQueryable<Invoice> GetEntitiesAsQueryable();

    Task<PagedList<Invoice>> GetAllPaged(PagedQuery<Invoice, InvoiceId> request,
        IQueryable<Invoice>? queryable = null);

    Task<Invoice?> GetByIdAsync(InvoiceId id, CancellationToken cancellationToken = default);

    Task<Invoice?> GetLatestByProperty(Expression<Func<Invoice, dynamic>> expression,
        CancellationToken cancellationToken = default);
}