using System.ComponentModel;

namespace Domain.Invoices;

public enum PaymentType
{
    [Description("Cash")] Cash,
    [Description("Ngân hàng")] Banking
}