using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Application.Abstractions.VnPay;
using Domain.Abstractions;
using Domain.Invoices;
using Domain.MemberNotifications;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.ConfirmPayment;

public class ConfirmOrderPaymentCommandHandler : ICommandHandler<ConfirmOrderPaymentCommand, ConfirmPaymentResponse>
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IMemberNotificationRepository _memberNotificationRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVnPayService _vnPayService;

    public ConfirmOrderPaymentCommandHandler(IVnPayService vnPayService, IOrderRepository orderRepository,
        IInvoiceRepository invoiceRepository, IDateTimeProvider dateTimeProvider, IUnitOfWork unitOfWork,
        IMemberNotificationRepository memberNotificationRepository)
    {
        _vnPayService = vnPayService;
        _orderRepository = orderRepository;
        _invoiceRepository = invoiceRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
        _memberNotificationRepository = memberNotificationRepository;
    }

    public async Task<Result<ConfirmPaymentResponse>> Handle(ConfirmOrderPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var vnPayData = request.Query;
        foreach (var param in vnPayData)
            if (!string.IsNullOrEmpty(param.Key) && param.Key.StartsWith("vnp_"))
                _vnPayService.AddResponseData(param.Key, param.Value);

        var isValidId = Guid.TryParse(_vnPayService.GetResponseData("vnp_TxnRef").Replace("_", "-"), out var orderId);
        if (!isValidId)
            return Result.Failure<ConfirmPaymentResponse>(OrderErrors.NotFound);
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
        var order = await _orderRepository.GetEntitiesAsQueryable()
            .Include(x => x.OrderFees)
            .Include(x => x.LineItems)
            .FirstOrDefaultAsync(order => order.Id.Equals(new OrderId(orderId)), cancellationToken);

        if (order is null)
            return Result.Failure<ConfirmPaymentResponse>(OrderErrors.NotFound);


        if (checkSignature)
        {
            confirmPaymentResponse.TerminalId = terminalId;
            confirmPaymentResponse.OrderId = orderId.ToString();
            confirmPaymentResponse.TransactionId = vnPayTranId.ToString();
            confirmPaymentResponse.Amount = amount;
            confirmPaymentResponse.BankCode = bankCode;
            confirmPaymentResponse.CurrencyCode = "VND";
            confirmPaymentResponse.HasPayment = order.HasPayment;
            if (responseCode == "00" && transactionStatus == "00")
                // if (!order.HasPayment)
                // {
                //     using var transaction = _unitOfWork.BeginTransaction();
                //     try
                //     {
                //         confirmPaymentResponse.HasPayment = true;
                //         var latestInvoice =
                //             await _invoiceRepository.GetLatestByProperty(x => x.InvoiceCode, cancellationToken);
                //         var latestInvoiceCode = latestInvoice != null ? latestInvoice.InvoiceCode.Value : string.Empty;
                //         var invoiceCode = string.IsNullOrEmpty(latestInvoiceCode)
                //             ? "0".PadLeft(5, '0')
                //             : latestInvoiceCode.Remove(0, 2);
                //         var newInvoiceCode = "HD" + (int.Parse(invoiceCode) + 1).ToString().PadLeft(5, '0');
                //         var invoice = Invoice.Create(order.Id, new Code(newInvoiceCode), _dateTimeProvider.UtcNow,
                //             PaymentType.Banking,
                //             order.OrderType, order.LineItems.Sum(x => x.Quantity), order.TotalBill,
                //             new Title("Thanh toán giao hàng"));
                //
                //         var invoiceDetails =
                //             order.LineItems
                //                 .Select(l => InvoiceDetail.Create(invoice.Id, l.ProductName, l.Price, l.Quantity))
                //                 .ToList();
                //
                //         var invoiceFees = order.OrderFees?.Select(fee => InvoiceFee.Create(invoice.Id,
                //                 new InvoiceFeeAmount(
                //                     fee.OrderFeeValue.Value),
                //                 new InvoiceFeeName(fee.OrderFeeName.Value),
                //                 fee.OrderFeeCharge,
                //                 fee.IsPercent))
                //             .ToList();
                //         if (invoiceFees != null) invoice.SetInvoiceFee(invoiceFees);
                //
                //         var paymentDetail = PaymentDetail.Create(invoice.Id, PaymentPlatform.VnPay,
                //             new TransactionRefId(vnPayTranId.ToString()),
                //             new PaymentResponse(JsonConvert.SerializeObject(confirmPaymentResponse)),
                //             _dateTimeProvider.UtcNow);
                //
                //         invoice.SetInvoiceDetail(invoiceDetails);
                //         invoice.SetPaymentDetail(paymentDetail);
                //         order.ConfirmPayment(_dateTimeProvider.UtcNow);
                //         order.SetInvoice(invoice);
                //         _orderRepository.Update(order);
                //         await _unitOfWork.SaveChangesAsync(cancellationToken);
                //         transaction.Commit();
                //     }
                //     catch (Exception)
                //     {
                //         transaction.Rollback();
                //     }
                // }
                confirmPaymentResponse.Message =
                    "Giao dịch được thực hiện thành công. Cảm ơn quý khách đã sử dụng dịch vụ.";
            else
                confirmPaymentResponse.Error = "Có lỗi xảy ra trong quá trình xử lý.Mã lỗi: " + responseCode;
        }
        else
        {
            confirmPaymentResponse.Error = "Có lỗi xảy ra trong quá trình xử lý.Mã lỗi: " + responseCode;
        }

        return Result.Success(confirmPaymentResponse);
    }
}