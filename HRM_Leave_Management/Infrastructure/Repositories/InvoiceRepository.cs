using Domain.Invoices;

namespace Infrastructure.Repositories;

internal sealed class InvoiceRepository : Repository<Invoice, InvoiceId>, IInvoiceRepository
{
    public InvoiceRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}