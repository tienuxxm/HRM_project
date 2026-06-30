namespace Web.Backend.Models;

public class UpdateBookingStatusRequest
{
    public int Status { get; set; }
    public string BookingId { get; set; }
}