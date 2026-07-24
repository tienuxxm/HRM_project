using Application.LeaveBalances.GetMyLeaveBalance;
using Application.LeaveRequests.GetApprovalAging;
using Application.LeaveRequests.GetDepartmentLeaveLoad;
using Application.LeaveRequests.GetLeaveStatusDistribution;
using Application.LeaveRequests.GetMonthlyLeaveTrend;
using Application.LeaveRequests.GetMyLeaveRequests;
using Application.LeaveRequests.GetPendingApprovals;
using Application.WorkCalendars.GetCalendarImpactAlerts;
using Application.WorkCalendars.GetUpcomingHolidays;

namespace Web.Backend.Models;

/// <summary>
/// Permission-aware ViewModel for the Swiss Operational Dashboard (Phase A2A/A2C/Phase 7).
/// All widget visibility is strictly controlled by explicit permission flags.
/// Data properties are populated by MediatR queries dispatched from DashboardController.
/// </summary>
public class DashboardViewModel
{
    // --- Permission Flags ---
    public bool CanViewDashboard { get; set; }
    public bool CanViewLeaveRequest { get; set; }
    public bool CanApproveLeaveRequest { get; set; }
    /// <summary>Global Admin/HR approval queue oversight permission (UPDATE_LEAVE_APPROVER_ASSIGNMENT).</summary>
    public bool CanViewApprovalOversight { get; set; }
    public bool CanViewLeaveBalance { get; set; }
    public bool CanViewEmployee { get; set; }
    public bool CanViewDepartment { get; set; }
    public bool CanViewWorkCalendar { get; set; }

    // --- Widget Visibility Mapping ---
    public bool ShowW1MyLeaveRequests => CanViewLeaveRequest;
    public bool ShowW2StatusDistribution => CanViewLeaveRequest;
    public bool ShowW3MonthlyTrend => CanViewLeaveRequest;
    public bool ShowW4ApprovalQueue => CanApproveLeaveRequest || CanViewApprovalOversight;
    public bool ShowW5ApprovalAging => CanApproveLeaveRequest || CanViewApprovalOversight;
    public bool ShowW6MyLeaveBalance => CanViewLeaveBalance;
    public bool ShowW7LowBalanceWatchlist => CanViewLeaveBalance && CanViewEmployee;
    public bool ShowW8UpcomingHolidays => CanViewWorkCalendar;
    public bool ShowW9CalendarImpactAlerts => CanViewWorkCalendar;
    public bool ShowW10DepartmentLeaveLoad => CanViewLeaveRequest && CanViewDepartment;

    // --- Filter Controls ---
    public string PeriodFilter { get; set; } = "CurrentMonth";

    // --- W1: My Leave Requests Data ---
    public List<MyLeaveRequestItem> MyLeaveRequests { get; set; } = new();

    // --- W2: Leave Status Distribution Data ---
    public LeaveStatusDistributionResult? StatusDistribution { get; set; }

    // --- W3: Monthly Leave Trend Data ---
    public List<MonthlyLeaveTrendItem> MonthlyTrend { get; set; } = new();

    // --- W6: My Leave Balance Data ---
    public MyLeaveBalanceResult? MyLeaveBalance { get; set; }

    // --- W4: Approval Queue Data ---
    public List<PendingApprovalItem> PendingApprovals { get; set; } = new();

    // --- W5: Approval Aging Data ---
    public ApprovalAgingResult? ApprovalAging { get; set; }

    // --- W8: Upcoming Holidays Data ---
    public List<UpcomingHolidayItem> UpcomingHolidays { get; set; } = new();

    // --- W9: Calendar Impact Alerts Data (Phase A2C) ---
    public List<CalendarImpactAlertItem> CalendarImpactAlerts { get; set; } = new();

    // --- W10: Department Leave Load Data (Phase A2C) ---
    public List<DepartmentLeaveLoadItem> DepartmentLeaveLoad { get; set; } = new();
}
