using System.ComponentModel;

namespace Domain.Bookings;

public enum BookingStatus
{
    [Description("Vừa tạo")] Reserved = 1,
    [Description("Đã xác nhận")] Confirmed = 2,
    [Description("Reject")] Rejected = 3,
    [Description("Hủy")] Cancelled = 4,
    [Description("Completed")] Completed = 5
}