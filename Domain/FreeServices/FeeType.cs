using System.ComponentModel;

namespace Domain.FreeServices;

public enum FeeType
{
    [Description("Phí dịch vụ")] ServiceFee = 0,
    [Description("Phí vận chuyển")] DeliveryFee = 1,
}