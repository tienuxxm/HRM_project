# Phase A1: Dashboard Shell — Chi tiết Proposal

> **Created**: 2026-07-21T20:30:00+07:00
> **Phase**: `phase-a1-dashboard-shell`
> **Status**: 📋 PROPOSAL — Chờ User duyệt trước khi code
> **Boundary**: Web.Backend → Application → Domain | Infrastructure → Application/Domain

---

## 0. Entity Evidence — User Corrections Applied

### ✅ Q1 RESOLVED: LeaveApproverAssignment đã có

**File**: `HRM_Leave_Management/Domain/LeaveApproverAssignments/LeaveApproverAssignment.cs`
**GitNexus UID**: `Class:HRM_Leave_Management/Domain/LeaveApproverAssignments/LeaveApproverAssignment.cs:LeaveApproverAssignment`

```csharp
public class LeaveApproverAssignment : Entity<LeaveApproverAssignmentId>
{
    public EmployeeId ApproverEmployeeId { get; private set; }
    public Employee? Approver { get; private set; }
    public DepartmentId? TargetDepartmentId { get; private set; }
    public PositionId? TargetPositionId { get; private set; }
    public bool IsActive { get; private set; }
    public DateOnly? EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public DateTime CreatedDate { get; private set; }
}
```

**DbSet**: `ApplicationDbContext.LeaveApproverAssignments` (DbSet<LeaveApproverAssignment>)
**Kết luận**: W4 (Approval Queue) và W5 (Aging) khả thi — scoped query qua `ApproverEmployeeId` + `TargetDepartmentId`.

### ✅ Q5 RESOLVED: W9 không defer — Entities đã có

**CalendarImportBatch** — `HRM_Leave_Management/Domain/WorkCalendars/CalendarImportBatch.cs`
- Properties: `FileName`, `Status` (ImportBatchStatus: Draft/Applied/Failed), `CreatedBy`, `CreatedAt`, `ProcessedBy`, `ProcessedAt`, `Rows` (collection)
- DbSet: `ApplicationDbContext.CalendarImportBatches`

**LeaveRequestRecalculationAudit** — `HRM_Leave_Management/Domain/WorkCalendars/LeaveRequestRecalculationAudit.cs`
- Properties: `BatchId`, `LeaveRequestId`, `EmployeeId`, `OldStatus`, `NewStatus`, `OldDuration`, `NewDuration`, `RecalculatedAt`, `Status` (RecalculationAuditStatus), `ErrorMessage`
- DbSet: `ApplicationDbContext.LeaveRequestRecalculationAudits`

**Kết luận**: W9 (Calendar Change Impact) vào Phase D với empty state fallback nếu chưa có import gần nhất.

### ✅ Q2 RESOLVED: Low Balance threshold

- Mặc định: `AvailableDays <= 2.0m` (hardcoded constant, không thêm DB config)
- Áp dụng cho W7 (Low Balance Warning)

### ✅ Q4 RESOLVED: Period selector scope

- Period selector (Today/Month/Quarter/Custom) chỉ áp dụng cho **date/range widgets**: W1, W2, W3, W4, W5, W8, W9, W10
- **KHÔNG áp dụng** cho: W6 (My Balance snapshot), W7 (Low Balance snapshot)

---

## 1. Git Dirty Baseline

```
 M HRM_Leave_Management/Web.Backend/Views/WorkCalendar/Index.cshtml
 M HRM_Leave_Management/Web.Backend/Views/WorkCalendar/Preview.cshtml
 M HRM_Leave_Management/Web.Backend/Views/WorkCalendar/Summary.cshtml
?? MD_memory/plans/... (6 untracked plan files)
?? MD_memory/reports/... (2 untracked report files)
```

**3 modified WorkCalendar files** — từ phase trước, KHÔNG đụng vào.
**Không sửa**: WorkCalendar, Role, User, LeaveRequest, LeaveBalance, DB, Auth, Keycloak.

---

## 2. GitNexus Impact Analysis

| Symbol | Impact | Risk | Direct Callers | Processes | Kết luận |
|---|---|---|---|---|---|
| `DashboardController` | 0 upstream | LOW | 0 | 0 | ✅ An toàn rewrite |
| `DashboardViewModel` | 0 upstream | LOW | 0 | 0 | ✅ An toàn rewrite |

**Không có file nào gọi trực tiếp vào DashboardController hay DashboardViewModel ngoài View binding.**

---

## 3. Phase A1 Scope — Chính xác

### Mục tiêu

Xóa 100% LUC legacy trong Dashboard runtime. Dựng Swiss monochrome shell với permission-driven widget visibility. Mọi widget hiển thị empty state + skeleton. **Chưa implement query backend phức tạp.**

### Sẽ SỬA (3 files)

| # | File | Action | Dòng hiện tại | Ghi chú |
|---|---|---|---|---|
| 1 | `HRM_Leave_Management/Web.Backend/Controllers/DashboardController.cs` | REWRITE | 67 lines | Xóa LUC imports + commands. Giữ permission gate. Thêm per-widget permission checks. |
| 2 | `HRM_Leave_Management/Web.Backend/Models/DashboardViewModel.cs` | REWRITE | 10 lines | Xóa Booking/Revenue. Thêm `ShowWidget*` flags + section headers. |
| 3 | `HRM_Leave_Management/Web.Backend/Views/Dashboard/Index.cshtml` | REWRITE | 241 lines (77KB) | Xóa 100% LUC. Tạo Swiss shell + 10 widget placeholders. |

### Sẽ TẠO (1 file)

| # | File | Action | Ghi chú |
|---|---|---|---|
| 4 | `HRM_Leave_Management/Web.Backend/wwwroot/css/dashboard.css` | CREATE | Scoped CSS cho dashboard shell, skeleton animations, widget layout |

### KHÔNG sửa

| Category | Files |
|---|---|
| Layout | `_Layout.cshtml`, sidebar, mobile bottom nav, global header/footer |
| Modules | WorkCalendar, Role, User, LeaveRequest, LeaveBalance, Employee, Department |
| Backend | Application layer queries/handlers, Infrastructure, Domain entities |
| DB | Migrations, seeding |
| Auth | Keycloak, JwtService, appsettings auth config |
| Git | Không stage, commit, push |

---

## 4. DashboardController.cs — Rewrite Contract

### Xóa (LUC legacy)

```diff
- using Application.Bookings.BookingReport;
- using Application.Orders.GetRevenue;
```

```diff
- var command = new GetBookingReportCommand();
- var result = await _sender.Send(command, cancellationToken);
- ... (booking logic)
- var revenueCommand = new GetRevenueCommand(...);
- ... (revenue logic)
```

```diff
- [HttpGet("get-revenue/{rangeType:int}")]
- public async Task<IActionResult> GetRevenue(...)
- { ... }
```

### Giữ nguyên

```csharp
// Permission gate VIEW_DASHBOARD — giữ 100%
var checkRoleExist = await _roleService.checkRoleExist(
    _userContext.IdentityId, "VIEW_DASHBOARD", cancellationToken);
if (!checkRoleExist.Value)
{
    var hasLeavePermission = await _roleService.checkRoleExist(
        _userContext.IdentityId, "VIEW_LEAVE_REQUEST", cancellationToken);
    if (hasLeavePermission.Value)
        return Redirect("/leave-request");
    return Redirect("/NoPermission");
}
```

### Thêm mới

```csharp
// Per-widget permission checks
var vm = new DashboardViewModel();

vm.ShowLeaveRequests = (await _roleService.checkRoleExist(
    _userContext.IdentityId, "VIEW_LEAVE_REQUEST", cancellationToken)).Value;

vm.ShowApprovalQueue = (await _roleService.checkRoleExist(
    _userContext.IdentityId, "APPROVE_LEAVE_REQUEST", cancellationToken)).Value;

vm.ShowLeaveBalance = (await _roleService.checkRoleExist(
    _userContext.IdentityId, "VIEW_LEAVE_BALANCE", cancellationToken)).Value;

vm.ShowLowBalanceAlert = vm.ShowLeaveBalance
    && (await _roleService.checkRoleExist(
        _userContext.IdentityId, "VIEW_EMPLOYEE", cancellationToken)).Value;

vm.ShowWorkCalendar = (await _roleService.checkRoleExist(
    _userContext.IdentityId, "VIEW_WORK_CALENDAR", cancellationToken)).Value;

vm.ShowDepartmentLoad = vm.ShowLeaveRequests
    && (await _roleService.checkRoleExist(
        _userContext.IdentityId, "VIEW_DEPARTMENT", cancellationToken)).Value;

return View(vm);
```

**Lưu ý**: Phase A1 chỉ check permissions → populate flags. CHƯA gọi MediatR queries mới.

---

## 5. DashboardViewModel.cs — Rewrite Contract

```csharp
namespace Web.Backend.Models;

public class DashboardViewModel
{
    // Section B1: Quản lý nghỉ phép
    public bool ShowLeaveRequests { get; set; }       // W1, W2, W3

    // Section B2: Công việc duyệt đơn
    public bool ShowApprovalQueue { get; set; }       // W4, W5

    // Section B3: Số dư phép
    public bool ShowLeaveBalance { get; set; }        // W6
    public bool ShowLowBalanceAlert { get; set; }     // W7

    // Section B4: Lịch làm việc
    public bool ShowWorkCalendar { get; set; }        // W8, W9

    // Section B5: Bối cảnh nhân sự
    public bool ShowDepartmentLoad { get; set; }      // W10

    // Computed
    public bool HasAnyWidget =>
        ShowLeaveRequests || ShowApprovalQueue ||
        ShowLeaveBalance || ShowLowBalanceAlert ||
        ShowWorkCalendar || ShowDepartmentLoad;
}
```

---

## 6. Views/Dashboard/Index.cshtml — Rewrite Contract

### Layout Structure (Desktop: 2-column grid / Mobile: 1-column stack)

```
┌──────────────────────────────────────────────────┐
│  HRM Dashboard                    [Period ▾]     │  ← Page header + scope banner
├──────────────────────────────────────────────────┤
│                                                  │
│  ┌─ Section B1: QUẢN LÝ NGHỈ PHÉP ──────────┐  │
│  │  W1: Đơn nghỉ phép   │  W2: Phân bổ       │  │  ← 2-col desktop
│  │  [empty state]        │  [empty state]      │  │
│  ├───────────────────────┴────────────────────┤  │
│  │  W3: Xu hướng 6 tháng                      │  │  ← full-width
│  │  [empty state]                              │  │
│  └────────────────────────────────────────────┘  │
│                                                  │
│  ┌─ Section B2: CÔNG VIỆC DUYỆT ĐƠN ────────┐  │
│  │  W4: Hàng đợi phê duyệt                    │  │  ← full-width
│  │  [empty state]                              │  │
│  ├────────────────────────────────────────────┤  │
│  │  W5: Tồn đọng duyệt                        │  │
│  │  [empty state]                              │  │
│  └────────────────────────────────────────────┘  │
│                                                  │
│  ┌─ Section B3: SỐ DƯ PHÉP ──────────────────┐  │
│  │  W6: Số dư phép       │  W7: Cảnh báo      │  │  ← 2-col desktop
│  │  [empty state]        │  [empty state]      │  │
│  └────────────────────────────────────────────┘  │
│                                                  │
│  ┌─ Section B4: LỊCH LÀM VIỆC ───────────────┐  │
│  │  W8: Ngày nghỉ sắp tới │  W9: Thay đổi    │  │  ← 2-col desktop
│  │  [empty state]          │  [empty state]    │  │
│  └────────────────────────────────────────────┘  │
│                                                  │
│  ┌─ Section B5: BỐI CẢNH NHÂN SỰ ────────────┐  │
│  │  W10: Nghỉ phép theo phòng ban              │  │  ← full-width
│  │  [empty state]                              │  │
│  └────────────────────────────────────────────┘  │
│                                                  │
│  ┌─ NO WIDGETS STATE ────────────────────────┐   │  ← Chỉ hiện khi !HasAnyWidget
│  │  "Bạn chưa được cấp quyền..."             │   │
│  └────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────┘
```

### Razor Conditional Pattern

```cshtml
@if (Model.ShowLeaveRequests)
{
    <section class="dash-section" id="section-leave-requests">
        <h2 class="dash-section__title">QUẢN LÝ NGHỈ PHÉP</h2>
        <div class="dash-grid dash-grid--2col">
            <!-- W1: Đơn nghỉ phép của tôi -->
            <div class="dash-widget" id="widget-my-requests">
                <h3 class="dash-widget__title">Đơn nghỉ phép của tôi</h3>
                <div class="dash-widget__empty">
                    <span>Bạn chưa có đơn nghỉ phép</span>
                </div>
            </div>
            <!-- W2: Phân bổ trạng thái -->
            <div class="dash-widget" id="widget-status-distribution">
                <h3 class="dash-widget__title">Phân bổ trạng thái</h3>
                <div class="dash-widget__empty">
                    <span>Chưa có dữ liệu</span>
                </div>
            </div>
        </div>
        <!-- W3: Xu hướng 6 tháng -->
        <div class="dash-widget dash-widget--full" id="widget-trend">
            <h3 class="dash-widget__title">Xu hướng 6 tháng</h3>
            <div class="dash-widget__empty">
                <span>Chưa có xu hướng</span>
            </div>
        </div>
    </section>
}
```

### CSS Design Tokens (dashboard.css)

```css
/* === DASHBOARD SWISS MONOCHROME === */
:root {
    --dash-black: #111111;
    --dash-white: #FFFFFF;
    --dash-gray-100: #F5F5F5;
    --dash-gray-200: #E5E5E5;
    --dash-gray-300: #D1D1D1;
    --dash-gray-500: #8A8A8A;
    --dash-gray-700: #555555;
    --dash-red: #E62429;           /* Critical/Rejected only */
    --dash-radius: 2px;            /* Swiss: minimal rounding */
    --dash-font: 'Inter', -apple-system, sans-serif;
}

/* Skeleton animation */
@keyframes dash-skeleton-pulse {
    0%, 100% { opacity: 0.4; }
    50% { opacity: 0.8; }
}
.dash-skeleton {
    background: var(--dash-gray-200);
    animation: dash-skeleton-pulse 1.5s ease-in-out infinite;
    border-radius: var(--dash-radius);
}

/* Grid layout */
.dash-grid--2col {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 24px;
}
@media (max-width: 768px) {
    .dash-grid--2col {
        grid-template-columns: 1fr;
    }
}

/* Empty state */
.dash-widget__empty {
    display: flex;
    align-items: center;
    justify-content: center;
    min-height: 160px;
    color: var(--dash-gray-500);
    font-size: 14px;
    border: 1px dashed var(--dash-gray-300);
    border-radius: var(--dash-radius);
}
```

---

## 7. Final Widget Matrix (Updated)

| # | Widget | Permission Gate | Scope Rule | Data Fields (Phase B+) | Insight cho User | User Action | Visual Type | Period Filter | Empty State Text | Loading | Animation |
|---|---|---|---|---|---|---|---|---|---|---|---|
| W1 | Đơn nghỉ phép của tôi | `VIEW_LEAVE_REQUEST` | RequesterId = currentUser | LeaveType, DateRange, Duration, Status | Đơn nào pending/rejected | Xem chi tiết, tạo mới | Dense table 5 rows | ✅ | "Bạn chưa có đơn nghỉ phép" | 5-row skeleton | Row stagger 80ms |
| W2 | Phân bổ trạng thái | `VIEW_LEAVE_REQUEST` | Aggregate in user scope | StatusCounts | Luồng nghỉ phép | Filter by status | Stacked bar | ✅ | "Chưa có dữ liệu" | Rect skeleton | Draw-in 600ms |
| W3 | Xu hướng 6 tháng | `VIEW_LEAVE_REQUEST` | Monthly count, user scope | MonthLabel, Count | Trend tăng/giảm | Lên kế hoạch | Line chart, black | ✅ | "Chưa có xu hướng" | Rect skeleton | Line draw 800ms |
| W4 | Hàng đợi phê duyệt | `APPROVE_LEAVE_REQUEST` | ApproverEmployeeId scope | Requester, Dept, DateRange, Duration | Đơn cần xử lý | Approve/Reject | Actionable queue 4 items | ✅ | "Không có đơn chờ duyệt" | 4-card skeleton | Card stagger 100ms |
| W5 | Tồn đọng duyệt | `APPROVE_LEAVE_REQUEST` | Age distribution pending | AgeBuckets {Today, 1-2d, 3+d} | Backlog SLA | Ưu tiên đơn cũ | 3 aging bars | ✅ | "Không tồn đọng" | 3-bar skeleton | Bars draw 100ms |
| W6 | Số dư phép của tôi | `VIEW_LEAVE_BALANCE` | Personal (currentUser) | Allocated, Used, Pending, Available | Còn bao nhiêu ngày | Tạo đơn | Progress bar + breakdown | ❌ | "Chưa có dữ liệu số dư" | Bar + text skeleton | Fill 400ms |
| W7 | Cảnh báo số dư | `VIEW_LEAVE_BALANCE` + `VIEW_EMPLOYEE` | Available ≤ 2.0 days | EmployeeName, Dept, Available | NV sắp hết phép | Thông báo | Compact list, red exhausted | ❌ | "Không có cảnh báo" | 3-row skeleton | Fade-in 300ms |
| W8 | Ngày nghỉ sắp tới | `VIEW_WORK_CALENDAR` | Date ≥ today, DayType ≠ Working | Date, DayName, DayType, Description | Ngày nghỉ sắp tới | Tránh trùng ngày | Timeline list 5 entries | ✅ | "Chưa có ngày nghỉ sắp tới" | 5-row skeleton | Row stagger 80ms |
| W9 | Thay đổi lịch gần đây | `VIEW_WORK_CALENDAR` | Latest CalendarImportBatch(Applied) | BatchFile, ProcessedAt, AffectedRequests | Thay đổi lịch | Kiểm tra đơn ảnh hưởng | Impact summary | ✅ | "Không có thay đổi gần đây" | Text skeleton | Fade-in 300ms |
| W10 | Nghỉ phép theo phòng ban | `VIEW_LEAVE_REQUEST` + `VIEW_DEPARTMENT` | Cross-table aggregate | DeptName, ActiveCount, Duration | Phòng ban nhiều nghỉ | Điều phối | Horizontal bar chart | ✅ | "Chưa có dữ liệu" | Rect skeleton | Bars stagger 100ms |

---

## 8. Revised Phase Split

| Phase | Scope | Files sửa/tạo | Risk |
|---|---|---|---|
| **A1** (hiện tại) | Remove LUC + Swiss shell + empty states | 3 rewrite + 1 create = 4 files | LOW |
| **A2** | Simple backend queries: W1, W6, W8 (entities có sẵn) | 4-6 new Application files + update Controller/ViewModel/View | LOW |
| **C** | Scoped queries: W2, W4, W5, W7, W10 (aggregate + JOIN) | 8-10 new Application files + update View | MEDIUM-HIGH |
| **D** | Charts + W3, W9 + CSS animations | Chart.js config + new queries + dashboard.css polish | MEDIUM |
| **E** | Browser UAT (chỉ khi User yêu cầu) | 0 code files | LOW |

---

## 9. Verification Plan cho Phase A1

| Step | Command | Mục đích |
|---|---|---|
| 1 | `git status --short` | Baseline: chỉ 3 WorkCalendar + plan/report files |
| 2 | `dotnet build` | Verify controller compiles sau khi xóa LUC imports |
| 3 | `git diff --stat` | Verify đúng 4 files thay đổi |
| 4 | `git diff --name-status` | Verify file list chính xác |
| 5 | Mojibake scan cho report cuối phase | UTF-8 BOM integrity |
| 6 | `detect_changes` (GitNexus) | Verify scope thay đổi trước khi commit |

**KHÔNG**: stage, commit, push. Chờ User duyệt kết quả A1 build.

---

## 10. Guardrails Checklist

- [x] Không code lượt này — chỉ proposal
- [x] Không sửa WorkCalendar, Role, User, LeaveRequest, LeaveBalance
- [x] Không sửa `_Layout.cshtml`, sidebar, mobile bottom nav
- [x] Không sửa DB/Auth/Keycloak
- [x] Không stage/commit/push
- [x] Không stat/KPI number-only cards
- [x] Không green/blue/yellow
- [x] Không hardcode role
- [x] Widget visibility theo permission flags
- [x] Dashboard = 1 shell, không tách theo group
- [x] Verified LeaveApproverAssignment entity (Q1 resolved)
- [x] W9 vào Phase D với fallback empty state (Q5 resolved)
- [x] Low balance threshold = 2.0 days (Q2 resolved)
- [x] Period selector chỉ cho date/range widgets (Q4 resolved)
- [x] DashboardController impact: LOW (0 upstream)
- [x] DashboardViewModel impact: LOW (0 upstream)
- [x] Restated boundary: Web.Backend → Application → Domain | Infrastructure → Application/Domain
