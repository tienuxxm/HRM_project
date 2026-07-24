namespace Application.LeaveRequests.GetDepartmentLeaveLoad;

public class DepartmentLeaveLoadItem
{
    public string DepartmentName { get; set; } = string.Empty;
    public int ActiveLeaveCount { get; set; }
    public decimal TotalDays { get; set; }
    public double LoadPercentage { get; set; }
}
