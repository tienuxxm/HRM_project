using Application.Abstractions.Messaging;
using Application.InvoiceHistories.Response;
using Domain.Abstractions;
using Domain.Invoices;
using Domain.Members;

namespace Application.InvoiceHistories;

public record GetAllInvoiceHistoryPagedCommand(MemberId? MemberId) : PagedQuery<Invoice, InvoiceId>,
    ICommand<PagedList<InvoiceHistoryResponse>>;