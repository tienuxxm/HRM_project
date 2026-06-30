using Application.Abstractions.Messaging;
using Application.Abstractions.VnPay;
using Domain.Abstractions;
using Domain.Orders;
using Domain.SystemLog;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Payment;

public class OrderPaymentCommandHandler : ICommandHandler<OrderPaymentCommand, string>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ISystemLogRepository _systemLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVnPayService _vnPayService;

    public OrderPaymentCommandHandler(IVnPayService vnPayService, IOrderRepository orderRepository,
        ISystemLogRepository systemLogRepository, IUnitOfWork unitOfWork)
    {
        _vnPayService = vnPayService;
        _orderRepository = orderRepository;
        _systemLogRepository = systemLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(OrderPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetEntitiesAsQueryable()
                .FirstOrDefaultAsync(x => x.Id.Equals(request.OrderId), cancellationToken);
            if (order is null)
                return Result.Failure<string>(OrderErrors.NotFound);
            await Task.CompletedTask;

            _vnPayService.AddRequestData("vnp_Amount", ((int)order.TotalBill.Amount * 100).ToString());
            _vnPayService.AddRequestData("vnp_OrderInfo",
                "Thanh toan don hang: " + order.OrderCode.Value + "Tai Warningzone");
            _vnPayService.AddRequestData("vnp_TxnRef", order.Id.Value.ToString().Replace("-", "_"));
            _vnPayService.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            _vnPayService.AddRequestData("vnp_IpAddr", request.IpAdress);

            var requestPayment = _vnPayService.CreateRequestUrl();

            var log = SystemLog.Create($"Request IP: {request.IpAdress} - LOG: Request Payment URL - {requestPayment}");
            _systemLogRepository.Add(log);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(requestPayment);
        }
        catch (Exception e)
        {
            return Result.Failure<string>(new Error("Payment.Confirm.Fail", "Có lỗi xảy ra"));
        }
    }
}