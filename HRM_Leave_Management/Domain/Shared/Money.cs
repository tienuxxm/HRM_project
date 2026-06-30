using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace Domain.Shared;

[Owned]
public record Money(decimal Amount, Currency Currency) : IComparable
{
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            Money TValue => TValue.Amount.CompareTo(Amount),
            _ => 1
        };
    }

    public static Money operator +(Money first, Money second)
    {
        if (first.Currency != second.Currency)
        {
            throw new InvalidOperationException("Currencies have to be equal");
        }

        return first with { Amount = first.Amount + second.Amount };
    }

    public static Money operator *(Money first, Money second)
    {
        if (first.Currency != second.Currency)
        {
            throw new InvalidOperationException("Currencies have to be equal");
        }

        return first with { Amount = first.Amount * second.Amount };
    }

    public static Money operator /(Money first, Money second)
    {
        if (first.Currency != second.Currency)
        {
            throw new InvalidOperationException("Currencies have to be equal");
        }

        if (second.Amount == 0)
        {
            throw new InvalidOperationException("Second money not be zerro");
        }

        return first with { Amount = first.Amount / second.Amount };
    }

    public static Money operator *(Money first, int second)
    {
        return first with { Amount = first.Amount * second };
    }

    public static Money Zero() => new(0, Currency.None);

    public static Money Zero(Currency currency) => new(0, currency);

    public bool IsZero() => this == Zero(Currency);
}

public class MoneyResponse
{
    public double Amount { get; set; }
    public Currency Currency { get; set; }
}