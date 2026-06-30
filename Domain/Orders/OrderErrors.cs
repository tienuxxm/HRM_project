using Domain.Abstractions;

namespace Domain.Orders;

public static class OrderErrors
{
    public static Error NotFound = new(
        "Order.Found",
        "Order Not Found");

    public static Error AlreadyCanceled = new(
        "Order.AlreadyCanceled",
        "Order Has Been Canceled");

    public static Error AlreadyPayment = new(
        "Order.AlreadyPayment",
        "Order Has Been Paid");

    public static Error NotPayment = new(
        "Order.NotPayment",
        "Order Has Not Been Paid");

    public static Error NotHaveLineItems = new(
        "Order.NotHaveLineItems",
        "No Dishes Were Ordered");

    public static Error AlreadyDone = new("Order.AlreadyDone",
        "Order Has Been Completed");
}