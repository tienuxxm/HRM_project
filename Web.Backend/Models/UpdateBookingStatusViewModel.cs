using Application.Bookings.GetBooking;
using Domain.Bookings;

namespace Web.Backend.Models;

public class UpdateBookingStatusViewModel
{
    public BookingResponse BookingResponse { get; set; }

    public Dictionary<int, string> ActiveBookingStatus
    {
        get
        {
            if (BookingResponse.Status == BookingStatus.Reserved)
            {
                return new Dictionary<int, string>()
                {
                    { (int)BookingStatus.Confirmed, "Confirm" },
                    { (int)BookingStatus.Rejected, "Reject" },
                };
            }

            if (BookingResponse.Status == BookingStatus.Confirmed)
            {
                return new Dictionary<int, string>()
                {
                    { (int)BookingStatus.Completed, "Completed" },
                    { (int)BookingStatus.Cancelled, "Canceled" },
                };
            }

            return new Dictionary<int, string>();
        }
    }
}