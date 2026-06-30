using Application.Abstractions.Clock;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.InvoiceDetails;
using Domain.InvoiceFees;
using Domain.Invoices;
using Domain.Orders;
using Domain.PaymentDetails;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Application.Orders.SystemConfirmPayment;

public class SystemConfirmPaymentCommandHandler : ICommandHandler<SystemConfirmPaymentCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SystemConfirmPaymentCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider, IInvoiceRepository invoiceRepository)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _invoiceRepository = invoiceRepository;
    }

    public async Task<Result> Handle(SystemConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetEntitiesAsQueryable()
            .Include(x => x.OrderFees)
            .Include(x => x.LineItems)
            .FirstOrDefaultAsync(order => order.Id.Equals(new OrderId(request.OrderId)), cancellationToken);

        if (order is null)
            return Result.Failure(OrderErrors.NotFound);


        if (!order.HasPayment)
        {
            order.ConfirmPayment(_dateTimeProvider.UtcNow);
            using var transaction = _unitOfWork.BeginTransaction();
            try
            {
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
                    new Title("Pay Delivery"));

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
                if (invoiceFees != null)
                {
                    invoice.SetInvoiceFee(invoiceFees);
                }

                var paymentDetail = PaymentDetail.Create(invoice.Id, PaymentPlatform.VnPay,
                    new TransactionRefId(""),
                    new PaymentResponse(""),
                    _dateTimeProvider.UtcNow);

                invoice.SetInvoiceDetail(invoiceDetails);
                invoice.SetPaymentDetail(paymentDetail);
                order.SetInvoice(invoice);
                _orderRepository.Update(order);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                return Result.Failure(new Error("Order.Confirm.Fail", "Có lỗi xảy ra"));
            }

            return Result.Success();
        }

        return Result.Failure(OrderErrors.AlreadyPayment);
    }
}