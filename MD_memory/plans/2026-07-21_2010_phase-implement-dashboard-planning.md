# Phase: implement-dashboard — Planning & Audit Report

> **Created**: 2026-07-21T20:10:00+07:00
> **Phase**: `phase-implement-dashboard-planning` — AUDIT + PROPOSAL ONLY
> **Status**: 📋 PROPOSAL — Chờ User/Codex duyệt trước khi code
> **Boundary**: Web.Backend → Application → Domain | Infrastructure → Application/Domain
> **Scope**: Phân tích + proposal triển khai. KHÔNG sửa runtime code lượt này.

---

## 1. Source-of-Truth Screen Verification

### ✅ Screens đã chốt (verified qua Stitch MCP `get_screen`)

| Screen | Title | ID | Device | Size | Status |
|---|---|---|---|---|---|
| Desktop | HRM Dashboard - Swiss Operational Redesign | `a86bf840299542ccab3243adab896721` | DESKTOP | 2560×5408 | ✅ Verified |
| Mobile | HRM Dashboard Mobile - Swiss Analytical Redesign | `a7f62e7b9dd449ac9cdc8adaa3a1d61f` | MOBILE | 780×5576 | ✅ Verified |

Project: `17479353588209716186`

### ❌ Screens cũ — KHÔNG dùng

| Screen ID | Lý do loại |
|---|---|
| `8ef6915706a84ba6a42aff0e51995c3f` | Screen Desktop trung gian, còn stat cards W5/W6/W7 |
| `615239637dd743b4b62b3a83e2a80a12` | Screen Mobile trung gian, tương tự |

---

## 2. Current Dashboard Runtime Audit

### 2.1. DashboardController.cs

**File**: `HRM_Leave_Management/Web.Backend/Controllers/DashboardController.cs` (67 lines)

**Dependencies**:
- `Application.Bookings.BookingReport.GetBookingReportCommand` ← LUC legacy
- `Application.Orders.GetRevenue.GetRevenueCommand` ← LUC legacy
- `Application.Orders.GetRevenue.RevenueDataRangeType` ← LUC legacy
- `Web.Backend.Models.DashboardViewModel` ← cần thay thế hoàn toàn

**Permission logic hiện tại** (line 28-38):
```csharp
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
**Đánh giá**: Gate `VIEW_DASHBOARD` OK giữ lại. Fallback logic redirect cũng hợp lý.

**Route `get-revenue/{rangeType:int}`** (line 58-66): LUC legacy, cần xóa.

**GitNexus Impact**: `DashboardController` — **LOW risk** (0 upstream dependents, 0 processes affected). An toàn để refactor.

### 2.2. DashboardViewModel.cs

**File**: `HRM_Leave_Management/Web.Backend/Models/DashboardViewModel.cs` (10 lines)

```csharp
public class DashboardViewModel
{
    public BookingReportResponse BookingReportResponse { get; set; }
    public int RevenueRange { get; set; }
    public List<RevenueResponse> RevenueResponse { get; set; }
}
```

**100% LUC legacy** — cần thay thế hoàn toàn bằng HRM dashboard ViewModel.

### 2.3. Views/Dashboard/Index.cshtml

**File**: `HRM_Leave_Management/Web.Backend/Views/Dashboard/Index.cshtml` (241 lines, 77KB)

**Nội dung 100% LUC**:
- Line 1-3: `@using Application.Orders.GetRevenue` + `@using Newtonsoft.Json`
- Line 13: `<span>BOOKING</span>` — booking header
- Line 15-108: 3 booking stat cards (Completed/Processing/Canceled) với inline SVG base64 icons (~30KB mỗi icon)
- Line 110-122: Doughnut chart (Orders by status) — Chart.js
- Line 123-165: Bar chart (SALES REVENUE by week/month) — Chart.js
- Line 170-241: `@section Scripts` — Chart.js doughnut + bar chart initialization

**Kết luận**: Toàn bộ file cần viết lại. Không có phần nào tái sử dụng cho HRM dashboard.

### 2.4. Legacy LUC còn sót

| Item | Location | Loại |
|---|---|---|
| `GetBookingReportCommand` | Controller line 41 | Command |
| `GetRevenueCommand` | Controller line 46 | Command |
| `BookingReportResponse` | ViewModel | Property |
| `RevenueResponse` | ViewModel | Property |
| `RevenueDataRangeType` | Controller + View | Enum |
| "BOOKING" header | View line 13 | UI |
| Booking stat cards (3) | View lines 15-108 | UI |
| Orders doughnut chart | View lines 110-122, 172-208 | UI + JS |
| SALES REVENUE bar chart | View lines 123-165, 210-237 | UI + JS |
| `get-revenue/{rangeType}` | Controller line 58 | Route |

---

## 3. Widget Implementation Matrix

Dựa trên 2 source-of-truth screens đã chốt. **0 stat cards**.

### A. Top Common Area (luôn hiển thị khi có VIEW_DASHBOARD)

| Element | Permission | Mô tả | Backend | Files dự kiến | Risk |
|---|---|---|---|---|---|
| Title "HRM Dashboard" | `VIEW_DASHBOARD` | Page gate, giữ logic redirect hiện tại | Có sẵn | Controller (giữ) | LOW |
| Scope banner | Always | "Dữ liệu hiển thị theo quyền truy cập" | Static text | View only | LOW |
| Period selector | Always | Hôm nay/Tháng này/Quý này/Tùy chỉnh | New query param | Controller + View | LOW |
| Empty state | When 0 widgets | "Không có widget khả dụng" | Computed | View only | LOW |

### B1. QUẢN LÝ NGHỈ PHÉP

| # | Widget | Permission | Scope | Data Fields | Insight | Action | Visual | Empty | Loading | Animation | Backend | Files dự kiến | Risk |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| W1 | Đơn nghỉ phép của tôi | `VIEW_LEAVE_REQUEST` | `RequesterId = currentUser` | LeaveType, StartDate, EndDate, Duration, Status, LastUpdated | Đơn nào pending/rejected cần theo dõi | Xem chi tiết, tạo đơn mới | Dense table 5 rows + "Xem tất cả →" | "Bạn chưa có đơn nghỉ phép" | 5-row skeleton | Row stagger 80ms | Cần query mới (filter by RequesterId) | Application: new Query + Handler; Controller; View | MEDIUM |
| W2 | Phân bổ trạng thái | `VIEW_LEAVE_REQUEST` | Aggregate requests user can see | StatusCounts {Approved, Pending, Rejected, Canceled} | Tình trạng luồng nghỉ phép | Click status để filter | Stacked bar (black/#D1D1D1/#E62429/#EEE) + legend | "Chưa có dữ liệu" | Rect skeleton | Draw-in 600ms | **Needs backend audit** | Application: new Query; View partial | MEDIUM |
| W3 | Xu hướng 6 tháng | `VIEW_LEAVE_REQUEST` | Monthly count in user scope | MonthLabel, RequestCount | Xu hướng tăng/giảm | Lên kế hoạch | Line chart, single black line | "Chưa có xu hướng" | Rect skeleton | Line draw 800ms | **Needs backend audit** | Application: new Query; View partial + JS | MEDIUM |

### B2. CÔNG VIỆC DUYỆT ĐƠN

| # | Widget | Permission | Scope | Data Fields | Insight | Action | Visual | Empty | Loading | Animation | Backend | Files dự kiến | Risk |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| W4 | Hàng đợi phê duyệt | `APPROVE_LEAVE_REQUEST` | Approver assignment scope | RequesterName, Dept, LeaveType, DateRange, Duration, CreatedAt, Reason | Đơn cần xử lý ngay, ai gửi, bao lâu | Chi tiết → Approve/Reject | Actionable queue 4 items, 3 buttons | "Không có đơn chờ duyệt" | 4-card skeleton | Card stagger 100ms | **Needs backend audit** — scoped JOIN | Application: new Query; Controller; View partial | HIGH |
| W5 | Tồn đọng duyệt | `APPROVE_LEAVE_REQUEST` | Age distribution pending | AgeBuckets {Today, 1-2d, 3+d} | Backlog tồn đọng, quá SLA? | Ưu tiên đơn cũ | 3 aging bars (black/gray/red-border) | "Không tồn đọng" | 3-bar skeleton | Bars draw 100ms | Computed from W4 + CreatedAt | View partial + JS | MEDIUM |

### B3. SỐ DƯ PHÉP

| # | Widget | Permission | Scope | Data Fields | Insight | Action | Visual | Empty | Loading | Animation | Backend | Files dự kiến | Risk |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| W6 | Số dư phép của tôi | `VIEW_LEAVE_BALANCE` | Personal (currentUser) | AllocatedDays, UsedDays, PendingDays, AvailableDays | Còn bao nhiêu ngày nghỉ | Quyết định tạo đơn | Progress bar + breakdown (NOT stat card) | "Chưa có dữ liệu" | Bar + text skeleton | Fill 400ms + count-up 600ms | Có sẵn (LeaveBalance) | Controller; View partial | LOW |
| W7 | Cảnh báo số dư | `VIEW_LEAVE_BALANCE` + `VIEW_EMPLOYEE` | Low balance in user scope | EmployeeName, Dept, AvailableDays, PendingDays | Nhân sự sắp hết phép | Thông báo, lên kế hoạch | Compact list, red cho exhausted | "Không có cảnh báo" | 3-row skeleton | Fade-in 300ms | **Needs backend audit** | Application: new Query; View partial | MEDIUM |

### B4. LỊCH LÀM VIỆC

| # | Widget | Permission | Scope | Data Fields | Insight | Action | Visual | Empty | Loading | Animation | Backend | Files dự kiến | Risk |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| W8 | Ngày nghỉ sắp tới | `VIEW_WORK_CALENDAR` | Global (org-level) | Date, DayName, DayType, Description | Ngày nghỉ ảnh hưởng kế hoạch | Tránh trùng ngày nghỉ | Timeline list 5 entries | "Chưa có ngày nghỉ sắp tới" | 5-row skeleton | Row stagger 80ms | **Needs simple date filter** on WorkCalendarDay | Application: new Query; View partial | LOW |
| W9 | Thay đổi lịch gần đây | `VIEW_WORK_CALENDAR` | Recent changes | LastImportAt, ChangedDays, AffectedRequests | Thay đổi lịch ảnh hưởng đơn | Kiểm tra đơn bị ảnh hưởng | Impact summary + change list | "Không có thay đổi" | Text skeleton | Fade-in 300ms | **Needs backend audit** — change tracking | Application: new Query; View partial | HIGH |

### B5. BỐI CẢNH NHÂN SỰ

| # | Widget | Permission | Scope | Data Fields | Insight | Action | Visual | Empty | Loading | Animation | Backend | Files dự kiến | Risk |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| W10 | Nghỉ phép theo phòng ban | `VIEW_LEAVE_REQUEST` + `VIEW_DEPARTMENT` | Departments + requests in user scope | DeptName, ActiveLeaveCount, TotalDuration | Phòng ban nào nhiều người nghỉ | Điều phối nhân lực | Horizontal bar chart, black bars | "Chưa có dữ liệu" | Rect skeleton | Bars stagger 100ms | **Needs backend audit** — cross-table aggregate | Application: new Query; View partial + JS | MEDIUM |

---

## 4. Micro-Phase Implementation Plan

### Phase A: Remove Legacy + Build Razor Shell

**Mục tiêu**: Xóa LUC dashboard, tạo shell mới với empty/loading placeholders.

**Files sửa**:
| File | Action | Chi tiết |
|---|---|---|
| `DashboardController.cs` | REWRITE | Giữ `VIEW_DASHBOARD` gate + redirect logic. Xóa `GetBookingReportCommand`, `GetRevenueCommand`, `get-revenue` route. Thêm permission checks per widget → populate ViewModel flags. |
| `DashboardViewModel.cs` | REWRITE | Xóa `BookingReportResponse`, `RevenueRange`, `RevenueResponse`. Thêm `bool ShowWidget*` flags + empty data containers per widget. |
| `Views/Dashboard/Index.cshtml` | REWRITE | Xóa 100% LUC content. Tạo Swiss monochrome shell: Top Area + 5 sections (B1-B5) với `@if(Model.ShowWidgetX)` conditionals + empty state + loading skeletons. |
| `Views/Dashboard/_Partials/*.cshtml` | CREATE | Mỗi widget = 1 partial view. Phase A chỉ tạo skeleton + empty state. |

**Files KHÔNG sửa**: `_Layout.cshtml`, sidebar, mobile bottom nav, global header/footer, DB, Auth.

**Verify**: `dotnet build`, login → dashboard hiển thị empty shell.

**Risk**: LOW — controller impact = 0 upstream dependents.

### Phase B: Simple Backend Queries (dữ liệu hiện có)

**Mục tiêu**: Implement widgets có dữ liệu/entity sẵn.

| Widget | Query cần tạo | Entity có sẵn? |
|---|---|---|
| W1 (My Requests) | `GetMyLeaveRequestsQuery` — filter by RequesterId, limit 5, sort by CreatedAt desc | LeaveRequest ✅ |
| W6 (My Balance) | `GetMyLeaveBalanceQuery` — filter by UserId | LeaveBalance ✅ |
| W8 (Upcoming Days) | `GetUpcomingNonWorkingDaysQuery` — filter WorkCalendarDay by Date >= today, DayType != Working, limit 5 | WorkCalendarDay ✅ |

**Files sửa**:
| File | Action |
|---|---|
| `Application/LeaveRequests/GetMyLeaveRequests/` | CREATE — Query + Handler |
| `Application/LeaveBalances/GetMyLeaveBalance/` | CREATE — Query + Handler (hoặc dùng existing query) |
| `Application/WorkCalendars/GetUpcomingDays/` | CREATE — Query + Handler |
| `DashboardController.cs` | UPDATE — gọi MediatR cho W1, W6, W8 |
| `DashboardViewModel.cs` | UPDATE — thêm data properties |
| `Views/Dashboard/_Partials/` | UPDATE — render actual data |

**Verify**: `dotnet build` + manual verify 3 widgets hiển thị data.

**Risk**: LOW — query mới, không sửa entity/repository.

### Phase C: Scoped Approval + Leave Widgets

**Mục tiêu**: Implement W2 (Status Distribution), W4 (Approval Queue), W5 (Aging), W7 (Low Balance).

| Widget | Khó khăn |
|---|---|
| W2 | Aggregate query — cần GROUP BY Status trên scoped results |
| W4 | Scoped query JOIN ApproverAssignment — cần audit hiện trạng approval flow |
| W5 | Computed từ W4 data + DATEDIFF trên CreatedAt |
| W7 | Aggregate: filter employees by scope + AllocatedDays - UsedDays <= threshold |

**⚠️ Cần backend audit TRƯỚC Phase C**:
1. Kiểm tra `ApproverAssignment` entity/table có tồn tại chưa.
2. Kiểm tra query pattern cho scoped leave requests.
3. Xác nhận threshold cho "low balance" (≤ 2 ngày? configurable?).

**Risk**: HIGH — phụ thuộc vào approval flow architecture chưa audit.

### Phase D: Charts + Animation + Remaining Widgets

**Mục tiêu**: W3 (Trend), W9 (Calendar Impact), W10 (Dept Load) + CSS animations.

| Item | Chi tiết |
|---|---|
| W3 (Trend) | Time-series aggregate — GROUP BY month, last 6 months |
| W9 (Calendar Impact) | Cần change tracking mechanism — **có thể defer** nếu chưa có audit trail |
| W10 (Dept Load) | Cross-table aggregate LeaveRequest × Department |
| Animations | CSS skeleton pulse, `IntersectionObserver` for reveal, chart draw-in via JS |

**Chart library**: Dùng Chart.js (đã có trong project qua LUC). Chuyển từ doughnut/bar sang line + horizontal bar.

**Risk**: MEDIUM — W9 có thể cần defer nếu chưa có change audit trail.

### Phase E: Browser UAT

**Chỉ chạy khi User yêu cầu rõ ràng.**

Manual UAT report template:
- URL: `/` (dashboard route)
- Account: `admin` / `Admin@123456`
- Keycloak: `http://localhost:8080`, realm `hrm`, `UseMockAuth: false`
- Prerequisites: permissions seeded (`VIEW_DASHBOARD`, `VIEW_LEAVE_REQUEST`, etc.)
- Test cases per widget

---

## 5. Permission / Render Rules

| Rule | Implementation |
|---|---|
| Page gate | `VIEW_DASHBOARD` — giữ logic hiện tại trong Controller |
| Widget visibility | `DashboardViewModel.ShowWidgetX` boolean flags, set trong Controller dựa trên `_roleService.checkRoleExist()` |
| No hardcoded role | Chỉ dùng permission strings, KHÔNG `if role == "ADMIN"` |
| No global bypass | Admin/HR không tự động thấy all widgets — phải có permission |
| No DB seed | Permissions quản lý qua UI (Permission Management) |
| Empty state | Khi user không có permission cho bất kỳ widget nào → hiển thị empty state centered |

---

## 6. UI Implementation Constraints

| Constraint | Detail |
|---|---|
| Monochrome Swiss | Black #111, white #FFF, grays, red #E62429 rejected/critical only |
| No stat cards | 0 number-only KPI cards anywhere |
| No green/blue/yellow | Swiss monochrome palette only |
| No pie/doughnut chart | Stacked bar + line + horizontal bar only |
| No LUC legacy | 0 Booking/Orders/Revenue |
| 1 dashboard | Widget toggle by permission, KHÔNG tách theo role |
| Responsive | Desktop = 2-column grid, Mobile = single column stack |
| _Layout.cshtml | KHÔNG sửa |
| Sidebar/bottom nav | KHÔNG sửa |

---

## 7. Animation Proposal (Razor MVC compatible)

Tất cả animation dùng CSS + vanilla JS, không cần framework.

| Animation | Implementation | Duration | Trigger |
|---|---|---|---|
| **Skeleton loading** | CSS `@keyframes pulse` trên `.skeleton` class | Infinite until data | On mount (server renders skeleton, JS replaces) |
| **Row reveal** | CSS `opacity: 0 → 1`, `translateY(8px → 0)` per row, stagger via `animation-delay` | 80ms stagger | IntersectionObserver |
| **Chart draw-in** | Chart.js built-in animation config | 600-800ms | Chart render |
| **Progress bar fill** | CSS `width: 0 → X%` transition | 400ms ease-out | IntersectionObserver |
| **Count-up** | Vanilla JS `requestAnimationFrame` counter | 600ms | IntersectionObserver |
| **Button feedback** | CSS `transform: scale(0.97)` on `:active` | 100ms | On click |
| **Section fade** | CSS `opacity: 0 → 1`, stagger per section | 150ms, 50ms stagger | Page load |
| **Row hover** | CSS `background-color: #F9F9F9` | 150ms | On hover |

**Không dùng**: bouncy, elastic, glow, hoặc gây layout shift.

---

## 8. Câu hỏi nghiệp vụ còn mở

| # | Câu hỏi | Ảnh hưởng | Cần trả lời trước phase |
|---|---|---|---|
| Q1 | `ApproverAssignment` entity/table đã có chưa? Flow approve hiện tại dùng gì? | W4, W5 implementation | Phase C |
| Q2 | Threshold "low balance" là bao nhiêu? Có configurable không? | W7 filter logic | Phase C |
| Q3 | Calendar change tracking có audit trail không? | W9 feasibility | Phase D |
| Q4 | Period selector (Hôm nay/Tháng/Quý) áp dụng cho widgets nào? Tất cả hay chỉ date-filtered? | Filter scope | Phase A |
| Q5 | W9 (Calendar Change Impact) có nên defer nếu chưa có change log? | Scope Phase D | Phase D |

---

## 9. File List Summary

### Phase A — Files sẽ sửa

| File | Action | Loại thay đổi |
|---|---|---|
| `HRM_Leave_Management/Web.Backend/Controllers/DashboardController.cs` | REWRITE | Xóa LUC, thêm permission checks, tạo HRM ViewModel |
| `HRM_Leave_Management/Web.Backend/Models/DashboardViewModel.cs` | REWRITE | Xóa LUC properties, thêm HRM widget flags + data |
| `HRM_Leave_Management/Web.Backend/Views/Dashboard/Index.cshtml` | REWRITE | Xóa 100% LUC, tạo Swiss monochrome shell |
| `HRM_Leave_Management/Web.Backend/Views/Dashboard/_Partials/` | CREATE | Widget partial views (10 files) |

### Phase B — Files sẽ tạo

| File | Action |
|---|---|
| `HRM_Leave_Management/Application/LeaveRequests/GetMyLeaveRequests/GetMyLeaveRequestsQuery.cs` | CREATE |
| `HRM_Leave_Management/Application/LeaveRequests/GetMyLeaveRequests/GetMyLeaveRequestsQueryHandler.cs` | CREATE |
| `HRM_Leave_Management/Application/WorkCalendars/GetUpcomingDays/GetUpcomingNonWorkingDaysQuery.cs` | CREATE |
| `HRM_Leave_Management/Application/WorkCalendars/GetUpcomingDays/GetUpcomingNonWorkingDaysQueryHandler.cs` | CREATE |

**Tổng files dự kiến**: ~16 files mới + 3 files rewrite qua Phase A-D.

---

## 10. Guardrails Checklist

- [x] Không sửa runtime code lượt này (audit + proposal only)
- [x] Không sửa `_Layout.cshtml`, sidebar, global header/footer
- [x] Không sửa DB/migration/Auth/Keycloak
- [x] Không seed DB
- [x] Không stage/commit/push
- [x] Không dùng `git checkout`/`restore`/`reset`/`clean`
- [x] Dùng Stitch MCP `get_screen` verify đúng 2 screen ID
- [x] Không dùng screen cũ `8ef6915...` / `615239...`
- [x] Dùng GitNexus impact cho DashboardController → LOW risk
- [x] Đọc `luc-hrm-refactor-guard/SKILL.md` trước khi bắt đầu
- [x] File report UTF-8 BOM
- [x] Restated boundary: Web.Backend → Application → Domain | Infrastructure → Application/Domain
