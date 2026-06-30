using Domain.Abstractions;
using Domain.Bookings;
using Domain.Deliveries;
using Domain.Invoices;
using Domain.Members;
using Domain.OrderFees;
using Domain.Orders.Events;
using Domain.Products;
using Domain.Restaurants;
using Domain.Shared;

namespace Domain.Orders;

public sealed class Order : Entity<OrderId>
{
    private readonly List<LineItem> _lineItems = new();

    private Order()
    {
    }

    private Order(OrderId id, Code orderCode, MemberId memberId, OrderStatus status, Note? note, DateTime createdDate,
        OrderType orderType, PaymentType? paymentType) : base(id)
    {
        MemberId = memberId;
        Note = note;
        CreatedDate = createdDate;
        Status = status;
        OrderType = orderType;
        OrderCode = orderCode;
        PaymentType = paymentType;
        HasPayment = false;
    }

    public Guid? OrderRef { get; private set; }
    public Code OrderCode { get; private set; }
    public RestaurantId? RestaurantId { get; private set; }
    public Restaurant? Restaurant { get; private set; } = null;
    public Note? Note { get; private set; }
    public OrderStatus Status { get; private set; }
    public MemberId MemberId { get; private set; }
    public Member Member { get; }
    public DateTime CreatedDate { get; private set; }
    public DateTime? CompletedDate { get; }
    public Delivery? Delivery { get; private set; }
    public OrderType OrderType { get; private set; }
    public PaymentType? PaymentType { get; private set; }
    public DateTime? PaymentDate { get; private set; }
    public Invoice? Invoice { get; private set; }
    public List<LineItem> LineItems { get; set; }
    public BookingId? BookingId { get; private set; } = null;
    public Booking? Booking { get; private set; }
    public Money TotalBill { get; private set; }
    public bool HasPayment { get; private set; }
    public List<OrderFee> OrderFees { get; private set; }

    public void SetBooking(Booking booking)
    {
        Booking = booking;
    }

    public void ConfirmPayment(DateTime paymentDate)
    {
        HasPayment = true;
        PaymentDate = paymentDate;

        if (Status is OrderStatus.Created or OrderStatus.Draft) Status = OrderStatus.Process;

        RaiseDomainEvent(new ConfirmOrderPaymentEvent(Id));
    }

    public Result CancelOrder()
    {
        Status = OrderStatus.Cancel;
        RaiseDomainEvent(new CancelOrderEvent(Id));
        return Result.Success();
    }

    public Result ProcessOrder()
    {
        switch (Status)
        {
            case OrderStatus.Cancel:
                return Result.Failure(OrderErrors.AlreadyCanceled);
            case OrderStatus.Done:
                return Result.Failure(OrderErrors.AlreadyDone);
            default:
                Status = OrderStatus.Process;
                RaiseDomainEvent(new ProcessOrderEvent(Id));
                return Result.Success();
        }
    }


    public Result ShippingOrder()
    {
        switch (Status)
        {
            case OrderStatus.Cancel:
                return Result.Failure(OrderErrors.AlreadyCanceled);
            case OrderStatus.Done:
                return Result.Failure(OrderErrors.AlreadyDone);
            default:
                Status = OrderStatus.Shipping;
                RaiseDomainEvent(new ShippingOrderEvent(Id));
                return Result.Success();
        }
    }


    public Result MarkDone()
    {
        if (!HasPayment)
            return Result.Failure(new Error("Order.NotConfirmPayment",
                "Please make payment before completing the order"));
        Status = OrderStatus.Done;
        RaiseDomainEvent(new MarkDoneOrderEvent(Id));
        return Result.Success();
    }

    public static Order Create(MemberId memberId, Code orderCode, Note? note, DateTime createdDate, OrderType orderType,
        PaymentType? paymentType)
    {
        var order = new Order(OrderId.New(), orderCode, memberId, OrderStatus.Created, note, createdDate, orderType,
            paymentType);
        order.RaiseDomainEvent(new CreateOrderEvent(order.Id));
        return order;
    }

    public void SaveAtDraft()
    {
        Status = OrderStatus.Draft;
    }

    public void SetOrderRef(Guid? orderRef)
    {
        OrderRef = orderRef;
    }

    public void Update(MemberId? memberId, Note? note, PaymentType? paymentType)
    {
        MemberId = memberId ?? MemberId;
        Note = note ?? Note;
        PaymentType = paymentType ?? PaymentType;
    }

    public void AddLineItem(List<LineItem> items)
    {
        LineItems = items;
    }

    public void SetDelivery(Delivery delivery)
    {
        Delivery = delivery;
    }

    public void SetInvoice(Invoice invoice)
    {
        Invoice = invoice;
    }

    public void AddLineItem(ProductId productId, ProductName productName, Money price, int quantity,
        ImageUrl? productImage)
    {
        _lineItems.Add(LineItem.Create(Id, productId, productName, price, quantity, null, productImage));
    }

    public void RemoveLineItem(LineItemId lineItemId)
    {
        if (HasOneLineItem()) return;

        var lineItem = _lineItems.FirstOrDefault(li => li.Id == lineItemId);
        if (lineItem is null) return;

        _lineItems.Remove(lineItem);
    }

    public void SetTotalBill(Money totalBill)
    {
        TotalBill = totalBill;
    }

    public void SetRestaurant(RestaurantId restaurantId)
    {
        RestaurantId = restaurantId;
    }

    public void SetOrderFee(List<OrderFee> orderFees)
    {
        OrderFees = orderFees;
    }

    private bool HasOneLineItem()
    {
        return _lineItems.Count == 1;
    }
}