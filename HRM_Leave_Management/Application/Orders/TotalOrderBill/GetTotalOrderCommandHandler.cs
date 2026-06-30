using Application.Abstractions.Messaging;
using Application.Orders.Response;
using Domain.Abstractions;
using Domain.FreeServices;
using Domain.Orders;
using Domain.Products;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.TotalOrderBill;

public class GetTotalOrderCommandHandler : ICommandHandler<GetTotalOrderCommand, TotalOrderResponse>
{
    private readonly IFeeServiceRepository _feeServiceRepository;
    private readonly IProductRepository _productRepository;

    public GetTotalOrderCommandHandler(IFeeServiceRepository feeServiceRepository, IProductRepository productRepository)
    {
        _feeServiceRepository = feeServiceRepository;
        _productRepository = productRepository;
    }

    public async Task<Result<TotalOrderResponse>> Handle(GetTotalOrderCommand request,
        CancellationToken cancellationToken)
    {
        var productIds = request.LineItems.Select(x => x.ProductId);
        var products = await _productRepository.GetEntitiesAsQueryable()
            .AsNoTracking()
            .Where(x => productIds.Contains(x.Id))
            .Select(x => new { x.ProductName, x.Price, x.Id }).ToListAsync(cancellationToken);
        var prepareLineItems = request.LineItems.Select(x => new
        {
            x.Quantity,
            x.ProductId,
            products.First(p => p.Id.Equals(x.ProductId)).Price,
        }).ToList();

        var feeServices = (await _feeServiceRepository.GetAllActive(cancellationToken))
            ?.Where(x =>
                request.OrderType is OrderType.Booking
                    ? x.FeeType is not FeeType.DeliveryFee
                    : x.FeeType is FeeType.DeliveryFee or FeeType.ServiceFee).ToList();
        var feePercent = feeServices?.Where(x => x.IsPercent).Sum(x => x.FeePercent) ?? 0;
        var feeCharge = feeServices?.Where(x => !x.IsPercent && x.FeeAmount != null).Aggregate(
            new Money(0, Currency.Vnd),
            (s, d) => s + d.FeeAmount) ?? Money.Zero(Currency.Vnd);
        var currentTotalPrice = prepareLineItems.Aggregate(new Money(0, Currency.Vnd),
            (s, d) => s + (d.Price * d.Quantity));

        var totalFeePerson = currentTotalPrice * currentTotalPrice with { Amount = (decimal)feePercent } /
                             currentTotalPrice with { Amount = 100 };
        var totalCharge = totalFeePerson + feeCharge;
        var totalBill = currentTotalPrice + totalCharge;
        var orderFees = feeServices?.Select(fee =>
        {
            var feeValue = !fee.IsPercent
                ? fee.FeeAmount ?? Money.Zero(Currency.Vnd)
                : currentTotalPrice * currentTotalPrice with { Amount = (decimal)fee.FeePercent } /
                  currentTotalPrice with { Amount = 100 };
            var feeDisplay = feeValue.Amount + feeValue.Currency.Code;
            return new OrderFeeResponse()
            {
                FeeName = fee.IsPercent ? fee.FeeName.Value + " " + fee.GetInvoiceFeeString : fee.FeeName.Value,
                FeeValue = feeDisplay
            };
        }).ToList();
        return Result.Success(new TotalOrderResponse()
        {
            TotalBill = totalBill.Amount + " " + totalBill.Currency.Code,
            OrderFeeResponses = orderFees
        });
    }
}