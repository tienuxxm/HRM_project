# Proposal: Phase A2A — Dashboard Simple Data Integration



> **Created**: 2026-07-21T22:30:00+07:00

> **Phase**: `phase-a2a-dashboard-simple-data-proposal` — AUDIT + PROPOSAL ONLY

> **Status**: 📋 PROPOSAL — Awaiting User/Codex review before implementation

> **Boundary**: Web.Backend -> Application -> Domain | Infrastructure -> Application/Domain

> **Scope**: AUDIT + DATA CONTRACT PROPOSAL for W1, W2, W3, W6, W8 ONLY. NO runtime code edits.



---



## 1. Architectural Guardrails & Rules Compliance



- **Boundary Preserved**:

  - `Web.Backend -> Application -> Domain`

  - `Infrastructure -> Application/Domain`

- **Scope Limit**:

  - Includes **W1 (My Leave Requests)**, **W2 (Leave Status Distribution)**, **W3 (Monthly Leave Trend)**, **W6 (My Leave Balance)**, **W8 (Upcoming Holidays & Work Calendar)**.

  - Excludes W4, W5 (Approval Queue/Aging), W7 (Low Balance Watchlist), W9 (Calendar Impact Alerts), W10 (Department Leave Load).

- **Security & Data Safety**:

  - 0 DB Seeding.

  - 0 Auth / Keycloak / Permission setup changes.

  - 0 Git stage / commit / push.

  - 0 Browser UAT.



---



## 2. GitNexus Impact Analysis



Before proposing modifications to C# symbols, GitNexus impact analysis was conducted:



| Symbol | Location | Impacted Count | Risk Level | Blast Radius / Notes |

|---|---|---|---|---|

| `DashboardController` | `Web.Backend/Controllers/DashboardController.cs` | 0 | **LOW** | 0 direct callers, 0 processes affected. Safe to inject `ISender` and query MediatR. |

| `DashboardViewModel` | `Web.Backend/Models/DashboardViewModel.cs` | 0 | **LOW** | 0 direct callers, 0 processes affected. Safe to add DTO properties for W1, W2, W3, W6, W8. |

| `GetLeaveRequestsQueryHandler` | `Application/LeaveRequests/Get/` | 0 | **LOW** | Standard Application query handler. Read-only. |

| `GetLeaveBalancesQueryHandler` | `Application/LeaveBalances/Get/` | 0 | **LOW** | Standard Application query handler. Read-only. |

| `GetWorkCalendarDaysQueryHandler` | `Application/WorkCalendars/GetWorkCalendarDays/` | 0 | **LOW** | Standard Application query handler. Read-only. |



---



## 3. Existing Codebase Audit



### 3.1 LeaveRequest Queries & Repositories

- **Entities**: `LeaveRequest`, `LeaveType`, `Employee` in `Domain`.

- **Existing Handlers**: `GetLeaveRequestsQueryHandler` filters by `EmployeeId`, `Status`, `LeaveTypeId`, with built-in permission evaluation (`VIEW_LEAVE_REQUEST`, `APPROVE_LEAVE_REQUEST`, `UPDATE_LEAVE_APPROVER_ASSIGNMENT`).

- **Audit Finding**:

  - **W1 (My Requests)** can utilize a dedicated MediatR query `GetMyLeaveRequestsQuery` or query `GetLeaveRequestsQuery` with page size 5.

  - **W2 (Status Distribution)** requires a lightweight aggregate query `GetLeaveStatusDistributionQuery` grouping by `Status` on visible leave requests.

  - **W3 (Monthly Leave Trend)** requires a lightweight time-series aggregate query `GetMonthlyLeaveTrendQuery` grouping by month (`StartDate`) for the past 6 months.



### 3.2 LeaveBalance Queries & Repositories

- **Entities**: `LeaveBalance`, `LeaveType`, `Employee` in `Domain`.

- **Existing Handlers**: `GetLeaveBalancesQueryHandler` calculates `PendingDays` dynamically from active pending requests and computes `AvailableDays = AllocatedDays - UsedDays - PendingDays`.

- **Audit Finding**:

  - **W6 (My Balance)** can utilize `GetMyLeaveBalanceQuery` fetching active balance for current user and current year, returning `AllocatedDays`, `UsedDays`, `PendingDays`, and calculated `AvailableDays`.



### 3.3 WorkCalendarDay Queries & Repositories

- **Entities**: `WorkCalendarDay` in `Domain`.

- **Existing Handlers**: `GetWorkCalendarDaysQueryHandler` queries active calendar days by year.

- **Audit Finding**:

  - **W8 (Upcoming Holidays)** requires a query `GetUpcomingHolidaysQuery` filtering `WorkCalendarDay` where `Date >= Today`, `DayType != WorkingDay` (Holiday or CompensatoryOff), top 5 ordered by date ascending.



---



## 4. Concrete Data Contract per Widget



### W1 — My Leave Requests (Đơn nghỉ phép của tôi)

- **Permission Gate**: `VIEW_LEAVE_REQUEST`

- **Scope**: Current logged-in user (`Employee.UserId == currentUser.Id`).

- **Data Fields**:

  - `Id` (Guid)

  - `LeaveTypeName` (string)

  - `StartDate` (DateOnly)

  - `EndDate` (DateOnly)

  - `Duration` (decimal)

  - `Status` (string: "Pending", "Approved", "Rejected", "Canceled")

  - `CreatedAt` (DateTime)

- **Empty State**: `"You have no leave requests recorded."`

- **Risk Level**: **LOW**



### W2 — Leave Status Distribution (Phân bổ trạng thái)

- **Permission Gate**: `VIEW_LEAVE_REQUEST`

- **Scope**: Aggregated counts of requests visible to current user (own requests if employee, or scoped/all if manager/HR).

- **Data Fields**:

  - `ApprovedCount` (int)

  - `PendingCount` (int)

  - `RejectedCount` (int)

  - `CanceledCount` (int)

  - `TotalCount` (int)

- **Empty State**: `"No leave request status data available."`

- **Risk Level**: **LOW**



### W3 — Monthly Leave Trend (Xu hướng nghỉ phép 6 tháng)

- **Permission Gate**: `VIEW_LEAVE_REQUEST`

- **Scope**: Monthly request counts in user's visible scope for the past 6 months.

- **Data Fields**:

  - List of `MonthlyTrendItem`:

    - `MonthLabel` (string, e.g. "Feb", "Mar")

    - `Year` (int)

    - `RequestCount` (int)

- **Empty State**: `"No leave trend data available for the past 6 months."`

- **Risk Level**: **LOW**



### W6 — My Leave Balance (Số dư phép của tôi)

- **Permission Gate**: `VIEW_LEAVE_BALANCE`

- **Scope**: Current logged-in employee (`UserId == currentUser.Id`), active balance for current year.

- **Data Fields**:

  - `AllocatedDays` (decimal)

  - `UsedDays` (decimal)

  - `PendingDays` (decimal)

  - `AvailableDays` (decimal = `AllocatedDays - UsedDays - PendingDays`)

  - `LeaveTypeName` (string, e.g. "Annual Leave")

  - `Year` (int)

- **Empty State**: `"No leave balance data allocated for current year."`

- **Risk Level**: **LOW**



### W8 — Upcoming Holidays & Work Calendar (Ngày nghỉ sắp tới)

- **Permission Gate**: `VIEW_WORK_CALENDAR`

- **Scope**: Global organization-wide active work calendar.

- **Data Fields**:

  - List of `UpcomingHolidayItem`:

    - `Date` (DateOnly)

    - `DayName` (string, e.g. "Monday")

    - `DayType` (string: "Holiday", "CompensatoryOff")

    - `Description` (string)

- **Empty State**: `"No upcoming holidays or non-working days scheduled."`

- **Risk Level**: **LOW**



---



## 5. Summary of Files to Modify & Create



### 5.1 Web.Backend Layer (Modify)

1. `HRM_Leave_Management/Web.Backend/Models/DashboardViewModel.cs`

   - Add DTO classes & properties for W1, W2, W3, W6, W8.

2. `HRM_Leave_Management/Web.Backend/Controllers/DashboardController.cs`

   - Inject `ISender _mediator`.

   - Dispatch queries for W1, W2, W3, W6, W8 based on permission flags.

3. `HRM_Leave_Management/Web.Backend/Views/Dashboard/Index.cshtml`

   - Replace skeleton static text in W1, W2, W3, W6, W8 with Razor model bindings and empty state checks.



### 5.2 Application Layer (Create New Additive Handlers)

1. `HRM_Leave_Management/Application/LeaveRequests/GetMyLeaveRequests/`

   - `GetMyLeaveRequestsQuery.cs` & `GetMyLeaveRequestsQueryHandler.cs`

2. `HRM_Leave_Management/Application/LeaveRequests/GetLeaveStatusDistribution/`

   - `GetLeaveStatusDistributionQuery.cs` & `GetLeaveStatusDistributionQueryHandler.cs`

3. `HRM_Leave_Management/Application/LeaveRequests/GetMonthlyLeaveTrend/`

   - `GetMonthlyLeaveTrendQuery.cs` & `GetMonthlyLeaveTrendQueryHandler.cs`

4. `HRM_Leave_Management/Application/LeaveBalances/GetMyLeaveBalance/`

   - `GetMyLeaveBalanceQuery.cs` & `GetMyLeaveBalanceQueryHandler.cs`

5. `HRM_Leave_Management/Application/WorkCalendars/GetUpcomingHolidays/`

   - `GetUpcomingHolidaysQuery.cs` & `GetUpcomingHolidaysQueryHandler.cs`



---



## 6. Open Clarification Questions for User / Codex



Before proceeding to implementation, please confirm the following business choices:



1. **W2 & W3 Aggregate Scope**: For users with `UPDATE_LEAVE_APPROVER_ASSIGNMENT` or `APPROVE_LEAVE_REQUEST`, should status distribution (W2) and 6-month trend (W3) aggregate across **all requests within their approval/admin scope**, or strictly personal requests unless an explicit scope toggle is selected?

2. **W6 Multiple Leave Types**: If an employee has multiple active leave balance records for the current year (e.g. Annual Leave, Sick Leave, Unpaid Leave), should W6 display the **primary Annual Leave** balance or sum allocated/used days across all leave types?

3. **W8 Date Range Boundary**: For upcoming holidays, should W8 filter strictly by `Date >= Today` up to 5 events regardless of year boundary (e.g., rolling into next year if in December)?



---



## 7. Verification Checklist & Next Steps



- [x] Read guard skills (`luc-hrm-refactor-guard/SKILL.md`, `root-architecture.md`, `project.md`).

- [x] Ran GitNexus impact analysis for `DashboardController` and `DashboardViewModel` (both **LOW** risk).

- [x] Documented data contract for W1, W2, W3, W6, W8.

- [x] Saved proposal document.

- [ ] Convert document to UTF-8 BOM & run mojibake scan.

- [ ] Await User/Codex approval before writing any C# code.
