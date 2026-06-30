using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Application.Orders.UpdateOrderStatus;
using Domain.Abstractions;
using Domain.InvoiceDetails;
using Domain.InvoiceFees;
using Domain.Invoices;
using Domain.Members;
using Domain.MembershipClasses;
using Domain.MemberVouchers;
using Domain.OrderFees;
using Domain.Orders;
using Domain.PaymentDetails;
using Domain.Products;
using Domain.Restaurants;
using Domain.Shared;
using Domain.Vouchers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Address = Domain.Members.Address;
using Email = Domain.Members.Email;
using PhoneNumber = Domain.Members.PhoneNumber;

namespace Application.InvoiceHistories.Create;

internal sealed class CreateInvoiceCommandHandler : ICommandHandler<CreateInvoiceCommand, InvoiceResponse>
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IMembershipClassRepository _membershipClassRepository;
    private readonly IMemberVoucherRepository _memberVoucherRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly ISender _sender;
    private readonly IUnitOfWork _unitOfWork;

    private Member _member;

    public CreateInvoiceCommandHandler(
        IMemberRepository memberRepository,
        IOrderRepository orderRepository,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork,
        IMembershipClassRepository membershipClassRepository, ISender sender, IInvoiceRepository invoiceRepository,
        IRestaurantRepository restaurantRepository, IMemberVoucherRepository memberVoucherRepository)
    {
        _memberRepository = memberRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _membershipClassRepository = membershipClassRepository;
        _sender = sender;
        _invoiceRepository = invoiceRepository;
        _restaurantRepository = restaurantRepository;
        _memberVoucherRepository = memberVoucherRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<InvoiceResponse>> Handle(CreateInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        if (request.ItemInfos is { Count: 0 })
            return Result.Failure<InvoiceResponse>(OrderErrors.NotHaveLineItems);

        using (var transaction = _unitOfWork.BeginTransaction())
        {
            try
            {
                var memberIsExist = await _memberRepository.GetEntitiesAsQueryable()
                    .FirstOrDefaultAsync(
                        x => x.PhoneNumber.Equals(new PhoneNumber(request.CustomerInfo.CustomerPhoneNumber)) &&
                             x.IsActive,
                        cancellationToken);
                if (memberIsExist is null)
                {
                    var customerInfo = request.CustomerInfo;
                    var latestMember =
                        await _memberRepository.GetLatestByProperty(x => x.MemberCode, cancellationToken);
                    var latestBookingCode = latestMember != null ? latestMember.MemberCode.Value : string.Empty;
                    var memberCode = string.IsNullOrEmpty(latestBookingCode)
                        ? "0".PadLeft(5, '0')
                        : latestBookingCode.Remove(0, 2);
                    var newMemberCode = "KH" + (int.Parse(memberCode) + 1).ToString().PadLeft(5, '0');
                    var member = Member.Create(
                        new Code(newMemberCode),
                        new FirstName(customerInfo.CustomerName),
                        new LastName(customerInfo.CustomerName),
                        new Email(""),
                        new PhoneNumber(customerInfo.CustomerPhoneNumber),
                        new Address(customerInfo.CustomerAddressLine ?? ""),
                        _dateTimeProvider.UtcNow,
                        null,
                        null
                    );

                    var lowestMembershipClass =
                        await _membershipClassRepository.GetLowestMembershipClass(cancellationToken);
                    member.AssignMembershipClass(lowestMembershipClass);

                    _memberRepository.Add(member);
                    _member = member;
                }
                else
                {
                    _member = memberIsExist;
                }

                var latestOrder = await _orderRepository.GetLatestByProperty(x => x.OrderCode, cancellationToken);
                var latestOrderCode = latestOrder != null ? latestOrder.OrderCode.Value : string.Empty;
                var code = string.IsNullOrEmpty(latestOrderCode) ? "0".PadLeft(5, '0') : latestOrderCode.Remove(0, 2);
                var newCode = "DH" + (int.Parse(code) + 1).ToString().PadLeft(5, '0');

                var order = Order.Create(_member.Id,
                    new Code(newCode),
                    null,
                    _dateTimeProvider.UtcNow, OrderType.Booking
                    , PaymentType.Banking);

                var lineItems = request.ItemInfos.Select(x => LineItem.Create(
                        order.Id, null, new ProductName(x.ItemName),
                        new Money(x.UnitPrice,
                            Currency.FromCode(request.GeneralInvoiceInfo.CurrencyCode ?? Currency.Vnd.Code)),
                        x.Quantity,
                        null,
                        null
                    )
                ).ToList();

                order.AddLineItem(lineItems);
                order.SetOrderRef(request.GeneralInvoiceInfo.TransactionUuid);

                var totalCharge = new Money(request.SummarizeInfo.TotalTaxAmount,
                    Currency.FromCode(request.GeneralInvoiceInfo.CurrencyCode ?? Currency.Vnd.Code));
                var totalBill = new Money(request.SummarizeInfo.TotalAmountWithTax,
                    Currency.FromCode(request.GeneralInvoiceInfo.CurrencyCode ?? Currency.Vnd.Code));


                var orderFees = new List<OrderFee>
                {
                    OrderFee.Create(order.Id, new OrderFeeName("VAT"),
                        new OrderFeeValue(request.SummarizeInfo.TaxPercentage.ToString()), totalCharge, true)
                };

                order.SetTotalBill(totalBill);
                order.SetOrderFee(orderFees);
                var restaurant = await _restaurantRepository.GetEntitiesAsQueryable()
                    .FirstOrDefaultAsync(x =>
                            x.RestaurantName.Equals(new RestaurantName(request.BranchInfo.BranchLegalName)),
                        cancellationToken);
                if (restaurant is not null) order.SetRestaurant(restaurant.Id);

                if (request.GeneralInvoiceInfo.PaymentStatus.HasValue && request.GeneralInvoiceInfo.PaymentStatus.Value)
                {
                    var tryParseIssued = long.TryParse(request.GeneralInvoiceInfo.InvoiceIssuedDate,
                        out var issuedTimeStamp);
                    var issuedDate = tryParseIssued
                        ? _dateTimeProvider.TimeStampToUtc(issuedTimeStamp)
                        : _dateTimeProvider.UtcNow;

                    var latestInvoice =
                        await _invoiceRepository.GetLatestByProperty(x => x.InvoiceCode, cancellationToken);
                    var latestInvoiceCode = latestInvoice != null ? latestInvoice.InvoiceCode.Value : string.Empty;
                    var invoiceCode = string.IsNullOrEmpty(latestInvoiceCode)
                        ? "0".PadLeft(5, '0')
                        : latestInvoiceCode.Remove(0, 2);
                    var newInvoiceCode = "HD" + (int.Parse(invoiceCode) + 1).ToString().PadLeft(5, '0');
                    var invoice = Invoice.Create(order.Id, new Code(newInvoiceCode), issuedDate,
                        PaymentType.Banking,
                        order.OrderType, order.LineItems.Sum(x => x.Quantity), order.TotalBill,
                        new Title($"Thanh toán hóa đơn tại {request.BranchInfo.BranchLegalName}"));

                    var invoiceDetails =
                        order.LineItems
                            .Select(l => InvoiceDetail.Create(invoice.Id, l.ProductName, l.Price, l.Quantity))
                            .ToList();

                    var invoiceFees = order.OrderFees?.Select(fee => InvoiceFee.Create(invoice.Id,
                            new InvoiceFeeAmount(
                                fee.OrderFeeValue.Value),
                            new InvoiceFeeName(fee.OrderFeeName.Value),
                            fee.OrderFeeCharge,
                            fee.IsPercent))
                        .ToList();
                    if (invoiceFees != null) invoice.SetInvoiceFee(invoiceFees);

                    var paymentDetail = PaymentDetail.Create(invoice.Id, PaymentPlatform.CashOrBanking,
                        new TransactionRefId(request.GeneralInvoiceInfo.TransactionUuid.ToString()),
                        new PaymentResponse(request.GeneralInvoiceInfo.ToString()),
                        _dateTimeProvider.UtcNow);

                    invoice.SetInvoiceDetail(invoiceDetails);
                    invoice.SetPaymentDetail(paymentDetail);
                    _invoiceRepository.Add(invoice);
                    order.ConfirmPayment(_dateTimeProvider.UtcNow);
                }

                _orderRepository.Add(order);
                if (request?.Vouchers != null)
                {
                    var voucherIds = request.Vouchers.Select(x => new VoucherId(x.VoucherId)).ToList();
                    var memberVoucher = await _memberVoucherRepository.GetEntitiesAsQueryable()
                        .Include(x => x.Voucher)
                        .Where(x => voucherIds.Any(k => k == x.VoucherId) && x.MemberId == _member.Id)
                        .ToListAsync(cancellationToken);
                    memberVoucher.ForEach(x => x.UseVoucher());
                    _memberVoucherRepository.UpdateRange(memberVoucher);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                var updateStatusOrderCommand = new UpdateOrderStatusCommand(order.Id.Value, OrderStatus.Done);
                var updateOrderResult = await _sender.Send(updateStatusOrderCommand, cancellationToken);
                if (updateOrderResult.IsFailure)
                    return Result.Failure<InvoiceResponse>(new Error("UpdateOrder.Fail", "Fail to update order"));

                transaction.Commit();
            }
            catch (Exception exception)
            {
                transaction.Rollback();
                return Result.Failure<InvoiceResponse>(new Error("CreateOrder.Fail", "Fail to create an order"));
            }
        }

        return Result.Success(new InvoiceResponse { Message = "Update Successful" });
    }
}