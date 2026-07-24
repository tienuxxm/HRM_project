# Implementation Plan — Phase A2C: Dashboard Full Widgets & Swiss Alignment



> **Plan Location**: `MD_memory/plans/2026-07-22_1138_phase-a2c-dashboard-full-widgets-plan.md`

> **Phase**: Dashboard Phase A2C

> **Created**: 2026-07-22T11:38:00+07:00

> **Boundary**: Web.Backend → Application → Domain | Infrastructure → Application/Domain

> **Stitch Source of Truth**:

> - Desktop: `a86bf840299542ccab3243adab896721` (`HRM Dashboard - Swiss Operational Redesign`)

> - Mobile: `a7f62e7b9dd449ac9cdc8adaa3a1d61f` (`HRM Dashboard Mobile - Swiss Analytical Redesign`)



---



## 1. Objectives & Scope



1. **Purge Non-Approved Palette & Emojis**:

   - Eliminate all green, blue, yellow, amber, orange, teal, and pastel colors from `dashboard.css` and `Index.cshtml`.

   - Remove HTML entity emojis (`&#128203;`, `&#128202;`, `&#128200;`, `&#9989;`, `&#128276;`, `&#128178;`, `&#128197;`).

   - Retain standard monochrome palette: Black (`#111111`), White (`#FFFFFF`), Grays (`#E2E2E2`, `#F4F3F3`, `#999999`), and Swiss Red (`#E62429` for errors/rejections only).



2. **Refactor Existing Widgets (W1-W6)**:

   - **W1 (My Leave Requests)**: Keep data, convert status chips to monochrome/red.

   - **W2 (Leave Status Distribution)**: Replace 4 count cards with 1 horizontal stacked distribution bar (Black / Gray `#D1D1D1` / Red `#E62429` / Light Gray `#EEEEEE`) + JetBrains Mono 4-column legend matching Stitch.

   - **W3 (6-Month Trend)**: Replace vertical bars with an SVG trend line chart (`<polyline stroke="#111">` + `<circle fill="#111">` + JetBrains Mono month labels).

   - **W4 (Approval Queue)**: Keep A2B active employee logic (`IsActive == true`), align visual to Stitch monochrome.

   - **W5 (Approval Aging)**: Keep A2B active employee logic, reserve Swiss Red `#E62429` for overdue (3+ days) only.

   - **W6 (My Leave Balance)**: Replace green/orange grid with single horizontal progress bar (`bg-black` fill `14.5/20`) + metadata row below.

   - **W7 (Low Balance Watchlist)**: Align visual to Stitch, no color pollution.



3. **Implement Full Backend & UI for W8, W9, W10**:

   - **W8 (Upcoming Holidays)**: Integrated from `GetUpcomingHolidaysQueryHandler`. Display timeline list.

   - **W9 (Calendar Impact Alerts)**: Create `GetCalendarImpactAlertsQuery` & Handler in Application. Query upcoming public holidays in next 30 days overlapping with active leave requests.

   - **W10 (Department Leave Load)**: Create `GetDepartmentLeaveLoadQuery` & Handler in Application. Aggregate active leave requests by department for current month using `IDepartmentRepository` & `ILeaveRequestRepository`.



---



## 2. File Modification Matrix



### Application Layer

- [NEW] `Application/WorkCalendars/GetCalendarImpactAlerts/GetCalendarImpactAlertsQuery.cs`

- [NEW] `Application/WorkCalendars/GetCalendarImpactAlerts/GetCalendarImpactAlertsQueryHandler.cs`

- [NEW] `Application/WorkCalendars/GetCalendarImpactAlerts/CalendarImpactAlertItem.cs`

- [NEW] `Application/LeaveRequests/GetDepartmentLeaveLoad/GetDepartmentLeaveLoadQuery.cs`

- [NEW] `Application/LeaveRequests/GetDepartmentLeaveLoad/GetDepartmentLeaveLoadQueryHandler.cs`

- [NEW] `Application/LeaveRequests/GetDepartmentLeaveLoad/DepartmentLeaveLoadItem.cs`



### Web Layer

- [MODIFY] `Web.Backend/Models/DashboardViewModel.cs` (Add `CalendarImpactAlerts` and `DepartmentLeaveLoad` properties)

- [MODIFY] `Web.Backend/Controllers/DashboardController.cs` (Dispatch W9 & W10 MediatR queries)

- [MODIFY] `Web.Backend/wwwroot/css/dashboard.css` (Purge non-approved colors, style W8/W9/W10 and Swiss Monochrome components)

- [MODIFY] `Web.Backend/Views/Dashboard/Index.cshtml` (Render Swiss aligned W1-W10, purge emojis, implement line chart and stacked bars)



---



## 3. Verification Sequence



1. `rg` scan across `Views/Dashboard` and `wwwroot/css/dashboard.css` for color violations, emojis, and mojibake.

2. UTF-8 BOM verification via `python MD_memory/debug/2026-06-26_1430_scan-mojibake.py --require-bom`.

3. `git diff --check`

4. `git diff --name-status`

5. `dotnet build HRM_Leave_Management/Web.Backend/Web.Backend.csproj --no-restore`

6. GitNexus `detect_changes`
