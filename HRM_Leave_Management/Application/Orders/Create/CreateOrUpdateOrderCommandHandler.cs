using Application.Abstractions.Authentication;
using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Application.Orders.Response;
using Domain.Abstractions;
using Domain.Deliveries;
using Domain.FreeServices;
using Domain.Invoices;
using Domain.Members;
using Domain.OrderFees;
using Domain.Orders;
using Domain.Products;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Note = Domain.Orders.Note;

namespace Application.Orders.Create;

internal sealed class
    CreateOrUpdateOrderCommandHandler : ICommandHandler<CreateOrUpdateOrderCommand, TotalOrderResponse>
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IDeliveryRepository _deliveryRepository;
    private readonly IFeeServiceRepository _feeServiceRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILineItemRepository _lineItemRepository;
    private readonly IMemberContext _memberContext;
    private readonly IMemberRepository _memberRepository;
    private readonly IOrderFeeRespository _orderFeeRespository;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrUpdateOrderCommandHandler(
        IMemberRepository memberRepository,
        IOrderRepository orderRepository,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork, IProductRepository productRepository, IInvoiceRepository invoiceRepository,
        IFeeServiceRepository feeServiceRepository, IMemberContext memberContext,
        ILineItemRepository lineItemRepository, IDeliveryRepository deliveryRepository,
        IOrderFeeRespository orderFeeRespository)
    {
        _memberRepository = memberRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _productRepository = productRepository;
        _invoiceRepository = invoiceRepository;
        _feeServiceRepository = feeServiceRepository;
        _memberContext = memberContext;
        _lineItemRepository = lineItemRepository;
        _deliveryRepository = deliveryRepository;
        _orderFeeRespository = orderFeeRespository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<TotalOrderResponse>> Handle(CreateOrUpdateOrderCommand request,
        CancellationToken cancellationToken)
    {
        if (request.LineItems is { Count: 0 })
            return Result.Failure<TotalOrderResponse>(OrderErrors.NotHaveLineItems);

        using (var transaction = _unitOfWork.BeginTransaction())
        {
            try
            {
                Member? member = null;
                if (request.MemberId.HasValue)
                    member = await _memberRepository.GetByIdAsync(new MemberId(request.MemberId.Value),
                        cancellationToken);
                else if (!string.IsNullOrEmpty(_memberContext.IdentityId))
                    member = await _memberRepository.GetByIdentityAsync(_memberContext.IdentityId, cancellationToken);

                if (member is null) return Result.Failure<TotalOrderResponse>(MemberErrors.NotFound);

                var latestOrder = await _orderRepository.GetLatestByProperty(x => x.OrderCode, cancellationToken);
                var latestOrderCode = latestOrder != null ? latestOrder.OrderCode.Value : string.Empty;
                var code = string.IsNullOrEmpty(latestOrderCode) ? "0".PadLeft(5, '0') : latestOrderCode.Remove(0, 2);
                var newCode = "DH" + (int.Parse(code) + 1).ToString().PadLeft(5, '0');

                var order = Order.Create(member.Id, new Code(newCode),
                    request.Note != null ? new Note(request.Note) : null,
                    _dateTimeProvider.UtcNow, request.OrderType, request.PaymentType);
                if (!request.MemberId.HasValue) order.SaveAtDraft();

                //Update process
                if (request.OrderId.HasValue)
                {
                    order =
                        await _orderRepository.GetEntitiesAsQueryable()
                            .Include(x => x.Delivery)
                            .Include(x => x.Invoice)
                            .Include(x => x.LineItems)
                            .Include(x => x.OrderFees)
                            .FirstOrDefaultAsync(x => x.Id.Equals(new OrderId(request.OrderId.Value)),
                                cancellationToken);
                    if (order is null)
                        return Result.Failure<TotalOrderResponse>(OrderErrors.NotFound);
                    _lineItemRepository.RemoveRange(order.LineItems);
                    _orderFeeRespository.RemoveRange(order.OrderFees);
                    if (order.Delivery is not null)
                        _deliveryRepository.Remove(order.Delivery);
                    if (order.Invoice is not null)
                        _invoiceRepository.Remove(order.Invoice);
                    order.Update(request.MemberId.HasValue ? new MemberId(request.MemberId.Value) : null,
                        string.IsNullOrEmpty(request.Note) ? null : new Note(request.Note),
                        request.PaymentType ?? null);
                }

                if (request.LineItems is not null)
                {
                    var productIds = request.LineItems.Select(x => x.ProductId);
                    var products = await _productRepository.GetEntitiesAsQueryable()
                        .AsNoTracking()
                        .Where(x => productIds.Contains(x.Id))
                        .Select(x => new { x.ProductName, x.Price, x.Id, x.ImageUrl }).ToListAsync(cancellationToken);
                    var prepareLineItems = request.LineItems.Select(x => new
                    {
                        x.Quantity,
                        x.ProductId,
                        products.First(p => p.Id.Equals(x.ProductId)).Price,
                        products.First(p => p.Id.Equals(x.ProductId)).ProductName,
                        products.First(p => p.Id.Equals(x.ProductId)).ImageUrl,
                        x.Note
                    });
                    var lineItems =
                        prepareLineItems
                            .Select(x => LineItem.Create(order.Id, x.ProductId, x.ProductName,
                                new Money(x.Price.Amount, x.Price.Currency), x.Quantity,
                                !string.IsNullOrEmpty(x.Note) ? new Note(x.Note) : null, x.ImageUrl))
                            .ToList();
                    order.AddLineItem(lineItems);
                }

                if (request.Delivery is not null)
                {
                    var requestDelivery = request.Delivery;
                    var delivery = Delivery.Create(order.Id, requestDelivery.ReceiverName, requestDelivery.PhoneNumber,
                        requestDelivery.ReceivingAddress, requestDelivery.Note, requestDelivery.HasIssueAnInvoice,
                        requestDelivery.CompanyTaxCode, requestDelivery.CompanyName, requestDelivery.CompanyEmail,
                        requestDelivery.CompanyAddress, requestDelivery.HasRequestCutlery);
                    order.SetDelivery(delivery);
                }

                var feeServices = (await _feeServiceRepository.GetAllActive(cancellationToken))
                    ?.Where(x =>
                        request.Delivery is null
                            ? x.FeeType is not FeeType.DeliveryFee
                            : x.FeeType is FeeType.DeliveryFee or FeeType.ServiceFee).ToList();
                var feePercent = feeServices?.Where(x => x.IsPercent).Sum(x => x.FeePercent) ?? 0;
                var feeCharge = feeServices?.Where(x => !x.IsPercent && x.FeeAmount != null).Aggregate(
                    new Money(0, order.LineItems.First().Price.Currency),
                    (s, d) => s + d.FeeAmount) ?? Money.Zero(Currency.Vnd);
                var currentTotalPrice = order.LineItems.Aggregate(new Money(0, order.LineItems.First().Price.Currency),
                    (s, d) => s + d.Price * d.Quantity);

                var totalFeePerson = currentTotalPrice * currentTotalPrice with { Amount = (decimal)feePercent } /
                                     currentTotalPrice with { Amount = 100 };
                var totalCharge = totalFeePerson + feeCharge;
                var totalBill = currentTotalPrice + totalCharge;


                var orderFees = feeServices?.Select(fee => OrderFee.Create(
                    order.Id,
                    new OrderFeeName(fee.FeeName.Value),
                    new OrderFeeValue(fee.GetInvoiceFeeString),
                    !fee.IsPercent
                        ? fee.FeeAmount ?? Money.Zero(Currency.Vnd)
                        : currentTotalPrice * currentTotalPrice with { Amount = (decimal)fee.FeePercent } /
                          currentTotalPrice with { Amount = 100 }, fee.IsPercent
                )).ToList();
                var fdmMember = await _memberRepository.GetEntitiesAsQueryable()
                    .Include(x => x.MembershipClass)
                    .Include(x => x.Orders)
                    .ThenInclude(x => x.Invoice)
                    .ThenInclude(x => x!.InvoiceFees)
                    .FirstOrDefaultAsync(x => x.Id == order.MemberId, cancellationToken);
                var discount = 0M;
                var currentDate = DateTime.Now;
                if (fdmMember?.BirthDate != null && fdmMember.BirthDate.Value.Month == currentDate.Month &&
                    fdmMember.BirthDate.Value.Year == currentDate.Year)
                {
                    var invoiceFeesOfOrderInYear = fdmMember.Orders.Where(x =>
                            x.PaymentDate.HasValue && x.PaymentDate.Value.Year == currentDate.Year &&
                            x.PaymentDate.Value.Month != currentDate.Month).Select(x => x.Invoice)
                        .SelectMany(x => x.InvoiceFees).ToList();
                    if (invoiceFeesOfOrderInYear.All(x => x.InvoiceFeeName.Value == "Giảm giá hạng thành viên"))
                        discount = -(decimal)(member?.MembershipClass?.PercentBirthDate ?? 0) / 100 *
                                   currentTotalPrice.Amount;
                }
                else
                {
                    discount = -(decimal)(member?.MembershipClass?.PercentDefault ?? 0) / 100 *
                               currentTotalPrice.Amount;
                }

                if (discount != 0)
                {
                    var discountFee = OrderFee.Create(
                        order.Id,
                        new OrderFeeName("Giảm giá hạng thành viên"),
                        new OrderFeeValue(discount + Currency.Vnd.Code),
                        new Money(discount, Currency.Vnd),
                        false
                    );
                    if (orderFees is not null)
                        orderFees.Add(discountFee);
                    else
                        orderFees = new List<OrderFee> { discountFee };
                }

                order.SetTotalBill(new Money(totalBill.Amount + discount, Currency.Vnd));
                if (orderFees is not null) order.SetOrderFee(orderFees);

                if (request.OrderId.HasValue)
                    _orderRepository.Update(order);
                else
                    _orderRepository.Add(order);

                if (request.Booking != null) order.SetBooking(request.Booking);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                transaction.Commit();
                var orderFeesResponses = feeServices?.Select(fee =>
                {
                    var feeValue = !fee.IsPercent
                        ? fee.FeeAmount ?? Money.Zero(Currency.Vnd)
                        : currentTotalPrice * currentTotalPrice with { Amount = (decimal)fee.FeePercent } /
                          currentTotalPrice with { Amount = 100 };
                    var feeDisplay = feeValue.Amount + feeValue.Currency.Code;
                    return new OrderFeeResponse
                    {
                        FeeName = fee.IsPercent ? fee.FeeName.Value + " " + fee.GetInvoiceFeeString : fee.FeeName.Value,
                        FeeValue = feeDisplay
                    };
                }).ToList();
                if (discount != 0)
                    if (orderFeesResponses != null)
                        orderFeesResponses.Add(new OrderFeeResponse
                        {
                            FeeName = "Giảm giá hạng thành viên",
                            FeeValue = "" + Math.Floor(discount) + Currency.Vnd.Code
                        });
                    else
                        orderFeesResponses = new List<OrderFeeResponse>
                        {
                            new()
                            {
                                FeeName = "Giảm giá hạng thành viên",
                                FeeValue = "" + Math.Floor(discount) + Currency.Vnd.Code
                            }
                        };
                {
                    var discountFee = OrderFee.Create(
                        order.Id,
                        new OrderFeeName(""),
                        new OrderFeeValue("-" + Math.Floor(discount) + Currency.Vnd.Code),
                        new Money(discount, Currency.Vnd),
                        false
                    );
                    if (orderFees is not null)
                        orderFees.Add(discountFee);
                    else
                        orderFees = new List<OrderFee> { discountFee };
                }
                return Result.Success(new TotalOrderResponse
                {
                    OrderId = order.Id.Value,
                    TotalBill = totalBill.Amount + discount + " " + totalBill.Currency.Code,
                    OrderFeeResponses = orderFeesResponses
                });
            }
            catch (Exception exception)
            {
                transaction.Rollback();
                return Result.Failure<TotalOrderResponse>(new Error("CreateOrder.Fail", "Fail to create an order"));
            }
        }
    }
}