# Dashboard Permission & Data Visibility Matrix

> **Created**: 2026-07-21T17:18:00+07:00
> **Status**: 📋 PROPOSAL — Awaiting User Approval
> **Scope**: Design-only, chưa code. Document này bổ sung cho `2026-07-21_1655_phase-design-dashboard-stitch.md`.

---

## 1. Hiện trạng Permission Gating — DashboardController.cs

### Verified (L26-38)
```csharp
// L29: Page-level gate — duy nhất 1 permission
var checkRoleExist = await _roleService.checkRoleExist(
    _userContext.IdentityId, "VIEW_DASHBOARD", cancellationToken);

if (!checkRoleExist.Value)
{
    // L32: Fallback — redirect nếu có VIEW_LEAVE_REQUEST
    var hasLeavePermission = await _roleService.checkRoleExist(
        _userContext.IdentityId, "VIEW_LEAVE_REQUEST", cancellationToken);
    if (hasLeavePermission.Value)
        return Redirect("/leave-request");

    // L37: Không có permission nào → NoPermission page
    return Redirect("/NoPermission");
}
```

### Phát hiện quan trọng

| Đặc điểm | Giá trị hiện tại | Rủi ro |
|---|---|---|
| Page-level gate | `VIEW_DASHBOARD` (single permission) | ⚠️ Không phân biệt widget nào được thấy |
| Per-widget check | **Không có** | ⚠️ Ai có `VIEW_DASHBOARD` thấy TẤT CẢ |
| Data scoping | **Không có** | ⚠️ Mọi data là global, không filter theo scope |
| Role name hardcode | **Không có** ✅ | Đúng convention — dùng permission string |
| Fallback chain | `VIEW_DASHBOARD` → `VIEW_LEAVE_REQUEST` → `/NoPermission` | ✅ Hợp lý |

---

## 2. Permission Registry — Existing HRM Permissions

Danh sách permission đã seed/đang dùng trong HRM controllers:

| Permission | Controller | Mô tả |
|---|---|---|
| `VIEW_DASHBOARD` | DashboardController | Xem trang dashboard |
| `VIEW_EMPLOYEE` | EmployeeController | Xem danh sách nhân viên |
| `VIEW_DEPARTMENT` | DepartmentController | Xem danh sách phòng ban |
| `VIEW_POSITION` | PositionController | Xem danh sách chức vụ |
| `VIEW_LEAVE_REQUEST` | LeaveRequestController | Xem đơn nghỉ phép |
| `VIEW_LEAVE_TYPE` | LeaveTypeController | Xem loại phép |
| `VIEW_LEAVE_BALANCE` | LeaveBalanceController | Xem số dư phép |
| `VIEW_LEAVE_APPROVER_ASSIGNMENT` | LeaveApproverAssignmentController | Xem config approver |
| `VIEW_WORK_CALENDAR` | WorkCalendarController | Xem lịch làm việc |
| `APPROVE_LEAVE` | LeaveRequestController | Duyệt/từ chối đơn |
| `CREATE_LEAVE` | LeaveRequestController | Tạo đơn nghỉ phép |
| `VIEW_ROLE` | RoleController | Xem role config |
| `VIEW_USER` | UserController | Xem danh sách user |

---

## 3. Widget → Permission Matrix (Đề xuất)

### 3a. Dashboard cho Admin/HR (Global Data)

Người có `VIEW_DASHBOARD` thấy trang dashboard. Nhưng mỗi widget nên kiểm tra thêm permission tương ứng. Nếu user không có permission của widget → widget ẩn (không hiện section đó).

| # | Widget | Permission cần | Data Scope | Backend Query Status |
|---|---|---|---|---|
| W1 | **Total Employees** (KPI) | `VIEW_EMPLOYEE` | Global — count tất cả employee active | ⚠️ Needs backend query design later |
| W2 | **Departments** (KPI) | `VIEW_DEPARTMENT` | Global — count tất cả department active | ⚠️ Needs backend query design later |
| W3 | **Pending Requests** (KPI) | `VIEW_LEAVE_REQUEST` | Global — count LeaveRequest status=Pending | ⚠️ Needs backend query design later |
| W4 | **Upcoming Holidays** (KPI) | `VIEW_WORK_CALENDAR` | Global — count non-working days next 30d | ⚠️ Needs backend query design later |
| W5 | **Leave Status Distribution** (chart) | `VIEW_LEAVE_REQUEST` | Global — aggregate by status for current month | ⚠️ Needs backend query design later |
| W6 | **Pending Approval Queue** (list) | `APPROVE_LEAVE` | **Scoped** — chỉ requests mà user là approver | ⚠️ Needs backend query design later — requires approver-assignment join |
| W7 | **Monthly Leave Trend** (chart) | `VIEW_LEAVE_REQUEST` | Global — count requests per month, 6 months | ⚠️ Needs backend query design later |
| W8 | **Upcoming Non-Working Days** (list) | `VIEW_WORK_CALENDAR` | Global — next 3-5 entries từ WorkCalendar | ⚠️ Needs backend query design later |
| W9 | **Department Leave Load** (chart) | `VIEW_LEAVE_REQUEST` + `VIEW_DEPARTMENT` | Global — count requests grouped by department | ⚠️ Needs backend query design later |

### 3b. Dashboard cho Approver (Scoped Data)

Nếu user có `APPROVE_LEAVE` nhưng **KHÔNG có** `VIEW_DASHBOARD`:

| Behavior | Đề xuất |
|---|---|
| Hiện tại | Redirect `/leave-request` (đúng) |
| Đề xuất tương lai | Có thể cân nhắc "Approver Dashboard" — scope nhỏ hơn, chỉ hiện W6 (queue) + W3 (pending count scoped) |
| Backend feasibility | ⚠️ **Needs backend query design later** — hiện chưa có query nào filter leave requests theo approver-assignment scope |

### 3c. Employee thường (No Dashboard)

| Behavior | Đề xuất |
|---|---|
| Hiện tại | Redirect `/leave-request` nếu có `VIEW_LEAVE_REQUEST`, hoặc `/NoPermission` |
| Đề xuất | ✅ **Giữ nguyên** — Employee không thấy global dashboard |
| "My Leave Dashboard" | Có thể xây riêng sau (hiện số phép còn, đơn đang chờ của bản thân), nhưng **ngoài scope hiện tại** |

---

## 4. Access Matrix Summary

| Vai trò logic | `VIEW_DASHBOARD` | Widgets thấy | Data scope |
|---|---|---|---|
| **Admin/HR** | ✅ Có | W1-W9 tất cả (tuỳ permission phụ) | Global |
| **Manager/Approver** (có `VIEW_DASHBOARD`) | ✅ Có | W1-W9 nhưng **W6 phải scoped** theo approver assignment | ⚠️ W6 scoped, còn lại global |
| **Manager/Approver** (KHÔNG có `VIEW_DASHBOARD`) | ❌ Không | Redirect → `/leave-request` | N/A |
| **Employee thường** | ❌ Không | Redirect → `/leave-request` hoặc `/NoPermission` | N/A |

---

## 5. Risk Analysis

### Risk 1: `VIEW_DASHBOARD` là flat permission — không phân biệt widget
- **Mức độ**: MEDIUM
- **Mô tả**: Ai có `VIEW_DASHBOARD` thấy toàn bộ. Nếu sau này assign `VIEW_DASHBOARD` cho Manager nhưng không muốn họ thấy employee count hay department load → không filter được.
- **Đề xuất**: Khi implement, controller kiểm tra từng permission phụ trước khi populate data cho từng widget. View dùng `if (Model.HasWidget_X)` để ẩn/hiện section.
- **KHÔNG cần thêm permission mới** — tái sử dụng permission đã có (`VIEW_EMPLOYEE`, `VIEW_DEPARTMENT`, `VIEW_LEAVE_REQUEST`, `APPROVE_LEAVE`, `VIEW_WORK_CALENDAR`).

### Risk 2: W6 (Pending Queue) cần data scoping theo approver
- **Mức độ**: HIGH
- **Mô tả**: Nếu Manager A chỉ approve Department X, họ không nên thấy pending requests của Department Y trong queue.
- **Đề xuất**: Backend query phải JOIN `leave_approver_assignment` để filter. **Needs backend query design later** — chưa có query này.
- **Ngoại lệ**: Admin/HR có thể thấy tất cả pending requests (tùy business rule — user cần chốt).

### Risk 3: Không có backend aggregation queries
- **Mức độ**: MEDIUM
- **Mô tả**: Tất cả 9 widgets đều cần Application-layer queries mới (count, aggregate, group by). Hiện tại chưa có query nào cho dashboard data.
- **Đề xuất**: Tạo mới trong phase implement:
  - `GetDashboardKpiQuery` → W1, W2, W3, W4
  - `GetLeaveStatusDistributionQuery` → W5
  - `GetPendingApprovalQueueQuery` → W6 (scoped)
  - `GetMonthlyLeaveTrendQuery` → W7
  - `GetUpcomingNonWorkingDaysQuery` → W8
  - `GetDepartmentLeaveLoadQuery` → W9

### Risk 4: GetRevenue endpoint không có permission check
- **Mức độ**: LOW (sẽ bị xóa)
- **Mô tả**: `GET /get-revenue/{rangeType}` (L58-66) không check permission. Đây là legacy LUC endpoint.
- **Đề xuất**: Sẽ bị remove hoàn toàn khi refactor dashboard.

---

## 6. Quyết định cần User chốt

| # | Câu hỏi | Options | Ghi chú |
|---|---|---|---|
| Q1 | W6 Pending Queue — Admin/HR thấy ALL pending hay chỉ thấy pending của bản thân (nếu họ cũng là approver)? | A: ALL / B: Scoped / C: Cả hai (toggle) | Ảnh hưởng query design |
| Q2 | Có cần "Approver Dashboard" riêng cho người có `APPROVE_LEAVE` nhưng không có `VIEW_DASHBOARD`? | A: Có / B: Không, giữ redirect `/leave-request` | Scope lớn nếu chọn A |
| Q3 | Có cần "My Leave Dashboard" cho employee thường? | A: Có (phase sau) / B: Không | Ngoài scope hiện tại |
| Q4 | Có cần seed `VIEW_DASHBOARD` cho role nào ngoài admin mặc định? | Liệt kê role | Ảnh hưởng migration seed |

---

## 7. Implementation Checklist (khi được approve)

- [ ] Controller: Kiểm tra permission phụ cho mỗi widget group
- [ ] ViewModel: Thêm `bool ShowWidgetX` flags dựa trên permission check
- [ ] View: Dùng `@if (Model.ShowWidgetX)` ẩn/hiện section
- [ ] Backend: Tạo 6 Application queries mới (liệt kê ở Risk 3)
- [ ] W6: Design scoped query cho pending approval queue
- [ ] Seed: Đảm bảo `VIEW_DASHBOARD` + các permission phụ đã seed cho admin role
- [ ] Remove: Xóa legacy `GetRevenue` endpoint + `GetBookingReport` references
- [ ] Test: Verify ẩn/hiện widget khi toggle permission

---

## Tham chiếu

- Controller: [`DashboardController.cs`](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Web.Backend/Controllers/DashboardController.cs)
- Design Plan: [`2026-07-21_1655_phase-design-dashboard-stitch.md`](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/plans/2026-07-21_1655_phase-design-dashboard-stitch.md)
