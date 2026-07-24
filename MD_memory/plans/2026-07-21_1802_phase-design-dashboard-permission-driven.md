# Phase: Dashboard Permission-Driven Design (Revised)

> **Created**: 2026-07-21T18:02:00+07:00
> **Status**: 📋 PROPOSAL — Awaiting User Approval
> **Supersedes**: `2026-07-21_1718_dashboard-permission-visibility-matrix.md` (sai scope/business rule)
> **Boundary**: Web.Backend → Application → Domain | Infrastructure → Application/Domain
> **Scope**: Stitch Canvas + Document ONLY — NO runtime code changes

---

## 0. Business Rules đã chốt (User 2026-07-21)

1. **Không có global bypass cho Admin/HR mặc định.** Dashboard phải scoped visibility.
2. **Không seed DB.** Permission quản trị qua UI. Nếu cần permission mới → đề xuất tên + lý do, chưa seed.
3. **Không tách dashboard theo group permission.** Không có "Admin Dashboard", "Approver Dashboard", "Employee Dashboard" riêng.
4. **Dashboard là shell chung.** Widget bật/tắt theo permission. Group permission thay đổi → dashboard tự điều chỉnh.

---

## 1. Phản biện & Clarification (Deliverable D)

### 1a. Permission names chính xác (evidence từ code)

| Permission thực tế | File | Line | Ghi chú |
|---|---|---|---|
| `VIEW_DASHBOARD` | DashboardController.cs | L29 | Page-level gate |
| `VIEW_LEAVE_REQUEST` | LeaveRequestController.cs | L44 | |
| `APPROVE_LEAVE_REQUEST` | LeaveRequestController.cs | L45 | ⚠️ Matrix cũ ghi sai là `APPROVE_LEAVE` |
| `CREATE_LEAVE_REQUEST` | LeaveRequestController.cs | L76 | ⚠️ Matrix cũ ghi sai là `CREATE_LEAVE` |
| `VIEW_EMPLOYEE` | EmployeeController.cs | L34 | |
| `VIEW_DEPARTMENT` | DepartmentController.cs | L30 | |
| `VIEW_POSITION` | PositionController.cs | L30 | |
| `VIEW_LEAVE_BALANCE` | LeaveBalanceController.cs | L42 | |
| `VIEW_LEAVE_APPROVER_ASSIGNMENT` | LeaveApproverAssignmentController.cs | L36 | |
| `VIEW_WORK_CALENDAR` | WorkCalendarController.cs | L36 | |
| `VIEW_LEAVE_TYPE` | LeaveTypeController.cs | L34 | |

### 1b. Scope tự nhiên vs scope cần thiết kế

| Data source | Scope hiện tại trong code | Cần scope thêm? |
|---|---|---|
| Employee list | **Global** — `VIEW_EMPLOYEE` trả tất cả | Nếu muốn scope theo department → needs backend query design later |
| Department list | **Global** — `VIEW_DEPARTMENT` trả tất cả | Không cần scope (department là tổ chức, không phải dữ liệu cá nhân) |
| Leave Requests | **Đã scoped** — `GetLeaveRequestsQueryHandler` filter theo: có `APPROVE_LEAVE_REQUEST` → xem requests mình có quyền duyệt; không → xem requests của bản thân | ✅ Đã có pattern |
| Leave Balance | **Scoped** — employee xem balance bản thân | ✅ |
| Work Calendar | **Global** — lịch nghỉ/làm việc chung cho org | Không cần scope (lịch làm việc là chung) |

### 1c. Câu hỏi nghiệp vụ chưa rõ

> **Q1**: Widget "KPI: Total Employees" — user có `VIEW_EMPLOYEE` → backend hiện trả global count. User có muốn widget này scope theo department không? Nếu có → cần backend mới. Nếu không → global count là chấp nhận được vì employee count là org-level metric, không chứa PII.

> **Q2**: Widget "My Submitted Requests" — permission nào gate widget này? Đề xuất `VIEW_LEAVE_REQUEST` (vì employee xem đơn bản thân đã dùng permission này). Hay cần permission riêng?

---

## 2. Permission-Driven Widget Matrix (Deliverable A)

### Nguyên tắc

- Dashboard route vẫn gate bằng `VIEW_DASHBOARD` (page-level access).
- Mỗi widget kiểm tra thêm permission tương ứng.
- Nếu user không có permission → widget **ẩn hoàn toàn** (không render HTML), grid tự reflow.
- Nếu không có widget nào khả dụng → hiện **empty state** ("Chưa có dữ liệu phù hợp. Liên hệ quản trị viên nếu cần quyền truy cập.").
- **Không hardcode role name** — chỉ dùng permission string.
- **Không seed DB** — permission quản trị qua UI.

### Widget Catalog

| # | Widget | Permission | Data Scope | Empty/Hidden Behavior | Backend Status |
|---|---|---|---|---|---|
| W1 | **KPI: Employees** | `VIEW_EMPLOYEE` | Global count (backend flat) | Ẩn nếu không có permission | ⚠️ Needs query later |
| W2 | **KPI: Departments** | `VIEW_DEPARTMENT` | Global count (org-level) | Ẩn | ⚠️ Needs query later |
| W3 | **KPI: Pending Requests** | `APPROVE_LEAVE_REQUEST` | Scoped — count requests user có quyền duyệt | Ẩn | ⚠️ Needs scoped query later (JOIN approver assignment) |
| W4 | **KPI: My Leave Balance** | `VIEW_LEAVE_BALANCE` | Scoped — balance bản thân user | Ẩn | ⚠️ Needs query later |
| W5 | **My Submitted Requests** (list) | `VIEW_LEAVE_REQUEST` | Scoped — đơn user tự tạo, status mới nhất | Ẩn | ⚠️ Needs query later (filter by requester = current user) |
| W6 | **Pending Approval Queue** (list) | `APPROVE_LEAVE_REQUEST` | Scoped — requests trong phạm vi approver assignment | Ẩn | ⚠️ Needs scoped query later (pattern exists in `GetLeaveRequestsQueryHandler`) |
| W7 | **Leave Status Distribution** (chart) | `VIEW_LEAVE_REQUEST` | Scoped — aggregate status của requests user được phép xem | Ẩn | ⚠️ Needs scoped aggregate query later |
| W8 | **Monthly Leave Trend** (chart) | `VIEW_LEAVE_REQUEST` | Scoped — trend requests user được phép xem | Ẩn | ⚠️ Needs scoped time-series query later |
| W9 | **Upcoming Non-Working Days** (list) | `VIEW_WORK_CALENDAR` | Global (lịch nghỉ chung) | Ẩn | ⚠️ Needs query later |
| W10 | **Department Leave Load** (chart) | `VIEW_LEAVE_REQUEST` + `VIEW_DEPARTMENT` | Scoped — count requests grouped by department, chỉ departments + requests user được phép xem | Ẩn nếu thiếu BẤT KỲ permission nào trong 2 | ⚠️ Needs scoped query later |

### Quy tắc bật/tắt widget

```
for each widget W:
    if user.hasPermission(W.requiredPermission):
        render W with scoped data
    else:
        skip W entirely (no HTML output)

if no widget rendered:
    render emptyState("Chưa có dữ liệu phù hợp...")
```

### Ví dụ theo tổ hợp permission

| Tổ hợp permission | Widgets hiển thị |
|---|---|
| `VIEW_DASHBOARD` only | Empty state |
| `VIEW_DASHBOARD` + `VIEW_LEAVE_REQUEST` | W5, W7, W8 |
| `VIEW_DASHBOARD` + `VIEW_LEAVE_REQUEST` + `APPROVE_LEAVE_REQUEST` | W3, W5, W6, W7, W8 |
| `VIEW_DASHBOARD` + `VIEW_EMPLOYEE` + `VIEW_DEPARTMENT` + `VIEW_LEAVE_REQUEST` + `APPROVE_LEAVE_REQUEST` + `VIEW_LEAVE_BALANCE` + `VIEW_WORK_CALENDAR` | W1-W10 tất cả |
| `VIEW_DASHBOARD` + `VIEW_WORK_CALENDAR` | W9 |

### Permission mới đề xuất (CHƯA seed)

| Permission | Lý do | Trạng thái |
|---|---|---|
| *(Không có)* | Tất cả widget tái sử dụng permission đã tồn tại | ✅ Không cần permission mới |

---

## 3. Motion & Animation Proposal (Deliverable C)

### KPI Cards
- **Count-up**: Số từ 0 → giá trị thực, duration 800ms, easing `ease-out`
- **Stagger**: Cards enter lần lượt, delay 100ms giữa mỗi card
- Không bounce/overshoot

### Charts (W7, W8, W10)
- **Draw-in**: Lines/bars animate từ baseline, duration 600ms
- **No rotation** (không spin pie chart)
- Charts chỉ animate khi lần đầu scroll vào viewport (IntersectionObserver)

### Widget Grid Reflow
- **Layout transition**: Khi widget ẩn/hiện do permission thay đổi, grid dùng CSS `gap` + auto-fit columns
- Không animation reflow (instant snap) — vì permission thay đổi = page reload, không live toggle
- Nếu sau này thêm live toggle → dùng `transition: all 300ms ease` trên grid items

### Skeleton Loading
- Mỗi widget slot hiện skeleton placeholder (pulsing gray bars) trước khi data load xong
- Skeleton shape match widget layout: KPI = 1 large number + 1 label; chart = rectangle; list = 3 rows

### Empty State
- Fade-in 300ms
- Icon + text centered trong dashboard content area
- Không animation phức tạp

---

## 4. Dashboard Grid Layout

### Desktop (≥1024px)
```
┌─────────────────────────────────────────────┐
│  HEADER: "Tổng quan" + breadcrumb           │
├────────┬────────┬────────┬────────┬─────────┤
│  W1    │  W2    │  W3    │  W4    │  (auto) │  ← KPI strip: auto-fit, min 200px
├────────┴────────┴────────┴────────┴─────────┤
│  W5: My Submitted Requests    │  W6: Queue  │  ← 60/40 split
├───────────────────────────────┼─────────────┤
│  W7: Status Distribution      │  W9: Calendar│ ← 50/50 split
├───────────────────────────────┼─────────────┤
│  W8: Monthly Trend            │  W10: Dept  │  ← 50/50 split
├───────────────────────────────┴─────────────┤
│  [EMPTY STATE nếu không có widget nào]      │
└─────────────────────────────────────────────┘
```

**Khi widget ẩn**: Grid dùng CSS `auto-fit` columns. Nếu cột phải trống → cột trái giãn full-width. Nếu KPI strip thiếu card → cards tự giãn đều. Không để gap trống xấu.

### Mobile (<768px)
- Single column stack
- KPI: 2×2 grid
- Lists/charts: full-width stack
- Bottom safe space 80px

---

## 5. Stitch Design Screens (Deliverable B) — ✅ GENERATED

Project: `17479353588209716186` | Design System: `assets/f4fbeeb3791c4c52991dd52c4fb92635`

| Screen | ID | Dimensions | Device |
|---|---|---|---|
| HRM Analytics Dashboard - Swiss International Desktop | `8ef6915706a84ba6a42aff0e51995c3f` | 2560 × 3464 px | DESKTOP |
| HRM Analytics Dashboard - Swiss Mobile Permission-Driven | `615239637dd743b4b62b3a83e2a80a12` | 780 × 4146 px | MOBILE |

Cả 2 screens đều:
- Dùng permission badges (8px monospace) tại mỗi widget
- Grid auto-fit cho reflow khi widget bị ẩn
- Empty state reference ở bottom
- Vietnamese text toàn bộ
- Swiss International design tokens (0px radius, hairline borders, Geist font)

---

## 6. Guardrails Checklist

- [x] Không sửa C#/Controller/Application/Domain/Infrastructure
- [x] Không sửa DB/seed
- [x] Không sửa `_Layout.cshtml`
- [x] Không sửa Auth/Keycloak
- [x] Không stage/commit/push
- [x] Không hardcode role name
- [x] Permission names dùng đúng từ code
- [x] Stitch MCP + skills, không browser thủ công
- [x] Report UTF-8 BOM, sẽ chạy mojibake scan

---

## Tham chiếu

- DashboardController: [L26-55](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Web.Backend/Controllers/DashboardController.cs)
- LeaveRequestController: [L44-96](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Web.Backend/Controllers/LeaveRequestController.cs)
- GetLeaveRequestsQueryHandler (scoped pattern): [file](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Application/LeaveRequests/Get/GetLeaveRequestsQueryHandler.cs)
- Refactor Guard: [SKILL.md](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/.agents/skills/luc-hrm-refactor-guard/SKILL.md)
