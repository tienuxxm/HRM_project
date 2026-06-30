namespace Application.Bookings.BookingReport;

public class BookingReportResponse
{
    public BookingReport BookingSuccessReport { get; set; }
    public BookingReport BookingInProcessReport { get; set; }
    public BookingReport BookingCancelReport { get; set; }
    public int TotalCompleteOrder { get; set; }
    public int TotalInProgressOrder { get; set; }
    public int TotalCancelOrder { get; set; }
}

public class BookingReport
{
    public int TotalQuantity { get; set; }
    public int GrowthRate { get; set; }

    public string GrowthRateResponse => GrowthRate > 0
        ? "+" + GrowthRate.ToString() + "%"
        : (GrowthRate == 0 ? "0%" : "-" + GrowthRate.ToString() + "%");
}