using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Members;
using Domain.Orders;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.GetOrder;

public class GetOrderCommandHandler : ICommandHandler<GetOrderCommand, OrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IAwsS3Service _awsS3Service;

    public GetOrderCommandHandler(IOrderRepository orderRepository, IMemberRepository memberRepository,
        IAwsS3Service awsS3Service)
    {
        _orderRepository = orderRepository;
        _memberRepository = memberRepository;
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<OrderResponse>> Handle(GetOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository
            .GetEntitiesAsQueryable()
            .Include(o => o.Delivery)
            .Include(o => o.OrderFees)
            .Include(o => o.LineItems)
            .Include(o => o.Invoice)
            .FirstOrDefaultAsync(o => o.Id.Equals(new OrderId(request.OrderId)), cancellationToken);
        if (order is null)
            return Result.Failure<OrderResponse>(OrderErrors.NotFound);

        var orderResponse = new OrderResponse
        {
            Id = order.Id.Value,
            Note = order.Note?.Value,
            MemberId = order.MemberId.Value,
            Status = order.Status,
            CreatedDate = order.CreatedDate,
            PaymentType = order.PaymentType,
            OrderCode = order.OrderCode.Value,
            HasPayment = order.HasPayment,
            TotalPrice = order.TotalBill,
            LineItems = new List<LineItemResponse>(),
            OrderFees = order.OrderFees.Select(fee => new FeeResponse()
            {
                ChargeFee = fee.OrderFeeCharge,
                FeeName = fee.OrderFeeName.Value,
                FeeValue = fee.OrderFeeValue.Value,
                IsPercent = fee.IsPercent
            }).ToList()
        };
        var memberAvatar = (await _memberRepository.GetEntitiesAsQueryable()
            .FirstOrDefaultAsync(x => x.Id.Equals(orderResponse.MemberId), cancellationToken))?.Avatar?.Value;
        var memberAvatarUrl = string.IsNullOrEmpty(memberAvatar) ? "" : _awsS3Service.GetUrlPresign(memberAvatar);
        orderResponse.MemberAvatar = memberAvatarUrl;

        var delivery = order.Delivery;
        if (delivery is not null)
        {
            var deliveryResponse = new DeliveryResponse()
            {
                Note = delivery.Note.Value,
                CompanyAddress = delivery.CompanyAddress.Value,
                CompanyEmail = delivery.CompanyEmail.Value,
                PhoneNumber = delivery.PhoneNumber.Value,
                CompanyName = delivery.CompanyName.Value,
                ReceiverName = delivery.ReceiverName.Value,
                ReceivingAddress = delivery.ReceivingAddress.Value,
                CompanyTaxCode = delivery.CompanyTaxCode.Value,
                HasRequestCutlery = delivery.HasRequestCutlery.Value,
                HasIssueAnInvoice = delivery.HasIssueAnInvoice.Value,
            };

            orderResponse.Delivery = deliveryResponse;
        }

        foreach (var item in order.LineItems)
        {
            var line = new LineItemResponse
            {
                Id = item.Id.Value,
                OrderId = item.OrderId.Value,
                ProductId = item.ProductId.Value,
                Price = item.Price,
                ProductName = item.ProductName.Value,
                Quantity = item.Quantity,
                Note = item.Note?.Value,
                ProductImage = item.ProductImageUrl is null
                    ? ""
                    : _awsS3Service.GetUrlPresign(item.ProductImageUrl.Value)
            };
            orderResponse.LineItems.Add(line);
        }

        var invoice = order.Invoice;
        if (invoice is not null)
        {
            orderResponse.InvoiceResponse = new InvoiceResponse()
            {
                InvoiceCode = invoice.InvoiceCode.Value,
                PaymentDate = invoice.PaymentDate,
                PaymentType = invoice.PaymentType,
                TotalBill = invoice.TotalBill,
                TotalQuantity = invoice.TotalQuantity
            };
        }

        return orderResponse;
    }
}