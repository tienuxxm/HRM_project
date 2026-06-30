using Application.Abstractions.Authentication;
using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Invoices;
using Domain.Members;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Application.InvoiceHistories.Response;

public class
    GetAllInvoiceHistoryPagedCommandHandler : ICommandHandler<GetAllInvoiceHistoryPagedCommand,
    PagedList<InvoiceHistoryResponse>>
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IMemberContext _memberContext;
    private readonly IMemberRepository _memberRepository;
    private readonly IOrderRepository _orderRepository;

    public GetAllInvoiceHistoryPagedCommandHandler(IOrderRepository orderRepository,
        IInvoiceRepository invoiceRepository, IMemberContext memberContext, IMemberRepository memberRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _orderRepository = orderRepository;
        _invoiceRepository = invoiceRepository;
        _memberContext = memberContext;
        _memberRepository = memberRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<PagedList<InvoiceHistoryResponse>>> Handle(GetAllInvoiceHistoryPagedCommand request,
        CancellationToken cancellationToken)
    {
        Member? member;
        if (request.MemberId is null)
            member = await _memberRepository.GetByIdentityAsync(_memberContext.IdentityId, cancellationToken);
        else
            // member = await _memberRepository.GetByIdAsync(request.MemberId, cancellationToken);
            member = await _memberRepository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(x => x.Id == request.MemberId && x.IsActive, cancellationToken);
        if (member is null)
            return Result.Failure<PagedList<InvoiceHistoryResponse>>(MemberErrors.NotFound);
        var orderIds = await _orderRepository.GetEntitiesAsQueryable()
            .Where(x => x.MemberId.Equals(member.Id))
            .Select(x => x.Id).ToListAsync(cancellationToken);
        var invoiceQuery = _invoiceRepository.GetEntitiesAsQueryable()
            .Where(x => orderIds.Contains(x.OrderId) && !x.IsDeleted)
            .OrderByDescending(x => x.PaymentDate);
        var result = await _invoiceRepository.GetAllPaged(request, invoiceQuery);
        var invoiceHistories = result.Data.Select(x => new InvoiceHistoryResponse
        {
            Quantity = x.TotalQuantity,
            InvoiceCode = x.InvoiceCode.Value,
            OrderType = x.OrderType,
            PaymentDate = x.PaymentDate.HasValue
                ? _dateTimeProvider.ToVnTime(x.PaymentDate.Value)
                : _dateTimeProvider.UtcNow,
            PaymentType = x.PaymentType,
            TotalBill = x.TotalBill,
            Title = x.Title.Value
        }).ToList();
        return Result.Success(new PagedList<InvoiceHistoryResponse>(invoiceHistories, result.TotalCount,
            result.CurrentPage, result.PageSize));
    }
}