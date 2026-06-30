using Application.Abstractions.Authentication;
using Application.Abstractions.AWS;
using Application.Abstractions.Messaging;
using Application.Orders.GetOrder;
using Domain.Abstractions;
using Domain.Members;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.MyOrder;

public class GetMyOrderCommandHandler : ICommandHandler<GetMyOrderCommand, PagedList<OrderResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMemberContext _memberContext;
    private readonly IMemberRepository _memberRepository;
    private readonly IAwsS3Service _awsS3Service;

    public GetMyOrderCommandHandler(IOrderRepository orderRepository, IMemberContext memberContext,
        IMemberRepository memberRepository, IAwsS3Service awsS3Service)
    {
        _orderRepository = orderRepository;
        _memberContext = memberContext;
        _memberRepository = memberRepository;
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<PagedList<OrderResponse>>> Handle(GetMyOrderCommand request,
        CancellationToken cancellationToken)
    {
        var member =
            await _memberRepository.GetByIdentityAsync(_memberContext.IdentityId, cancellationToken);
        if (member is null)
            return Result.Failure<PagedList<OrderResponse>>(MemberErrors.NotFound);

        var query = _orderRepository.GetEntitiesAsQueryable()
            .Where(x => x.OrderType != OrderType.Booking)
            .Include(x => x.LineItems)
            .Include(x => x.Delivery)
            .Include(x => x.OrderFees)
            .OrderByDescending(x => x.CreatedDate)
            .Where(x => x.MemberId.Equals(member.Id));
        var result = await _orderRepository.GetAllPaged(request, query);
        var orders = result.Data.Select(order => new OrderResponse()
        {
            Id = order.Id.Value,
            Note = order.Note?.Value,
            Status = order.Status,
            CompletedDate = order.CompletedDate,
            CreatedDate = order.CreatedDate,
            MemberId = order.MemberId.Value,
            OrderCode = order.OrderCode.Value,
            HasPayment = order.HasPayment,
            PaymentType = order.PaymentType,
            TotalPrice = order.TotalBill,
            TotalQuantity = order.LineItems.Count > 0 ? order.LineItems.Sum(x => x.Quantity) : 0,
            Delivery = order.Delivery != null
                ? new DeliveryResponse()
                {
                    Note = order.Delivery.Note.Value,
                    CompanyAddress = order.Delivery.CompanyAddress.Value,
                    CompanyEmail = order.Delivery.CompanyEmail.Value,
                    CompanyName = order.Delivery.CompanyName.Value,
                    CompanyTaxCode = order.Delivery.CompanyTaxCode.Value,
                    PhoneNumber = order.Delivery.PhoneNumber.Value,
                    ReceiverName = order.Delivery.ReceiverName.Value,
                    ReceivingAddress = order.Delivery.ReceivingAddress.Value,
                    HasRequestCutlery = order.Delivery.HasRequestCutlery.Value,
                    HasIssueAnInvoice = order.Delivery.HasIssueAnInvoice.Value,
                }
                : null,
            OrderFees = order.OrderFees.Select(fee => new FeeResponse()
            {
                ChargeFee = fee.OrderFeeCharge,
                FeeName = fee.OrderFeeName.Value,
                FeeValue = fee.OrderFeeValue.Value,
                IsPercent = fee.IsPercent
            }).ToList(),
            LineItems = order.LineItems.Select(l => new LineItemResponse()
            {
                Id = l.Id.Value,
                Price = l.Price,
                Quantity = l.Quantity,
                ProductId = l.ProductId.Value,
                OrderId = l.OrderId.Value,
                ProductName = l.ProductName.Value,
                Note = l.Note?.Value,
                ProductImage = l.ProductImageUrl != null ? _awsS3Service.GetUrlPresign(l.ProductImageUrl.Value) : null
            }).ToList()
        }).ToList();
        return Result.Success(new PagedList<OrderResponse>(orders, result.TotalCount,
            result.CurrentPage, result.PageSize));
    }
}