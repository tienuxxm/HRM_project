using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Application.Abstractions.VnPay;
using Domain.Abstractions;
using Domain.InvoiceDetails;
using Domain.InvoiceFees;
using Domain.Invoices;
using Domain.MemberNotifications;
using Domain.Members;
using Domain.Notifications;
using Domain.Orders;
using Domain.PaymentDetails;
using Domain.Shared;
using Domain.SystemLog;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Content = Domain.MemberNotifications.Content;
using ReferenceId = Domain.MemberNotifications.ReferenceId;

namespace Application.Orders.ConfirmPayment;

public class ConfirmOrderIPNCommandHandler : ICommandHandler<ConfirmOrderIPNCommand, IPNResponse>
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IMemberNotificationRepository _memberNotificationRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ISystemLogRepository _systemLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVnPayService _vnPayService;

    public ConfirmOrderIPNCommandHandler(IVnPayService vnPayService, IOrderRepository orderRepository,
        IInvoiceRepository invoiceRepository, IDateTimeProvider dateTimeProvider, IUnitOfWork unitOfWork,
        IMemberNotificationRepository memberNotificationRepository, ISystemLogRepository systemLogRepository,
        IMemberRepository memberRepository)
    {
        _vnPayService = vnPayService;
        _orderRepository = orderRepository;
        _invoiceRepository = invoiceRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
        _memberNotificationRepository = memberNotificationRepository;
        _systemLogRepository = systemLogRepository;
        _memberRepository = memberRepository;
    }

    public async Task<Result<IPNResponse>> Handle(ConfirmOrderIPNCommand request,
        CancellationToken cancellationToken)
    {
        var vnPayData = request.Query;
        var logString = JsonConvert.SerializeObject(vnPayData);
        var log = SystemLog.Create($"Request IP: {request.IP} - LOG: IPN - {logString}");
        _systemLogRepository.Add(log);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var param in vnPayData)
            if (!string.IsNullOrEmpty(param.Key) && param.Key.StartsWith("vnp_"))
                _vnPayService.AddResponseData(param.Key, param.Value);

        var isValidId = Guid.TryParse(_vnPayService.GetResponseData("vnp_TxnRef").Replace("_", "-"), out var orderId);
        if (!isValidId) return Result.Success(new IPNResponse { RspCode = "01", Message = "Order not Found" });
        var vnPayTranId = Convert.ToInt64(_vnPayService.GetResponseData("vnp_TransactionNo"));
        var responseCode = _vnPayService.GetResponseData("vnp_ResponseCode");
        var transactionStatus = _vnPayService.GetResponseData("vnp_TransactionStatus");
        var secureHash = _vnPayService.GetResponseData("vnp_SecureHash");
        var terminalId = _vnPayService.GetResponseData("vnp_TmnCode");
        var amount = Convert.ToInt64(_vnPayService.GetResponseData("vnp_Amount")) / 100;
        var bankCode = _vnPayService.GetResponseData("vnp_BankCode");

        var checkSignature = _vnPayService.ValidateSignature(secureHash);
        var confirmPaymentResponse = new ConfirmPaymentResponse
        {
            TransactionDate = DateTime.UtcNow
        };


        if (checkSignature)
        {
            var order = await _orderRepository.GetEntitiesAsQueryable()
                .Include(x => x.OrderFees)
                .Include(x => x.LineItems)
                .FirstOrDefaultAsync(order => order.Id.Equals(new OrderId(orderId)), cancellationToken);

            if (order is null)
                return Result.Success(new IPNResponse { RspCode = "01", Message = "Order not Found" });
            confirmPaymentResponse.TerminalId = terminalId;
            confirmPaymentResponse.OrderId = orderId.ToString();
            confirmPaymentResponse.TransactionId = vnPayTranId.ToString();
            confirmPaymentResponse.Amount = amount;
            confirmPaymentResponse.BankCode = bankCode;
            confirmPaymentResponse.CurrencyCode = "VND";
            confirmPaymentResponse.HasPayment = order.HasPayment;
            if ((long)order.TotalBill.Amount != amount)
                return Result.Success(new IPNResponse { RspCode = "04", Message = "invalid amount" });
            if (order.HasPayment)
                return Result.Success(new IPNResponse { RspCode = "02", Message = "Order already confirmed" });
            {
                using var transaction = _unitOfWork.BeginTransaction();
                try
                {
                    if (responseCode == "99")
                        return Result.Success(new IPNResponse { RspCode = "00", Message = "Confirm Success" });
                    confirmPaymentResponse.HasPayment = true;
                    var latestInvoice =
                        await _invoiceRepository.GetLatestByProperty(x => x.InvoiceCode, cancellationToken);
                    var latestInvoiceCode = latestInvoice != null ? latestInvoice.InvoiceCode.Value : string.Empty;
                    var invoiceCode = string.IsNullOrEmpty(latestInvoiceCode)
                        ? "0".PadLeft(5, '0')
                        : latestInvoiceCode.Remove(0, 2);
                    var newInvoiceCode = "HD" + (int.Parse(invoiceCode) + 1).ToString().PadLeft(5, '0');


                    var invoice = Invoice.Create(order.Id, new Code(newInvoiceCode), _dateTimeProvider.UtcNow,
                        PaymentType.Banking,
                        order.OrderType, order.LineItems.Sum(x => x.Quantity), order.TotalBill,
                        new Title("Payment Delivery"));

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

                    var paymentDetail = PaymentDetail.Create(invoice.Id, PaymentPlatform.VnPay,
                        new TransactionRefId(vnPayTranId.ToString()),
                        new PaymentResponse(JsonConvert.SerializeObject(confirmPaymentResponse)),
                        _dateTimeProvider.UtcNow);

                    invoice.SetInvoiceDetail(invoiceDetails);
                    invoice.SetPaymentDetail(paymentDetail);
                    order.ConfirmPayment(_dateTimeProvider.UtcNow);
                    order.SetInvoice(invoice);
                    var memberNotification = MemberNotification.Create(order.MemberId,
                        new Title(
                            $"Quý khách đã đặt hàng thành công đơn hàng {order.OrderCode.Value}. Cảm ơn quý khách đã sử dụng dịch vụ tại Warning Zone"),
                        new Content(
                            $"Quý khách đã đặt hàng thành công đơn hàng {order.OrderCode.Value}. Cảm ơn quý khách đã sử dụng dịch vụ tại Warning Zone"),
                        new MemberNotificationType(NotificationTypes.Order), new ReferenceId(order.Id.Value),
                        _dateTimeProvider.UtcNow);
                    _memberNotificationRepository.Add(memberNotification);
                    _orderRepository.Update(order);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    transaction.Commit();
                    return Result.Success(new IPNResponse { RspCode = "00", Message = "Confirm Success" });
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return Result.Success(new IPNResponse { RspCode = "99", Message = "Error" });
                }
            }
        }


        return Result.Success(new IPNResponse { RspCode = "97", Message = "Invalid signature" });
    }
}