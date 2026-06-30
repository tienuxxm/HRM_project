using Application.Bookings.BookingReport;
using Application.Orders.GetRevenue;

namespace Web.Backend.Models;

public class DashboardViewModel
{
    public BookingReportResponse BookingReportResponse { get; set; }
    public int RevenueRange { get; set; }
    public List<RevenueResponse> RevenueResponse { get; set; }
}