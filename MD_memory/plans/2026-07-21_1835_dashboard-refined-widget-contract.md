# Dashboard Refined Design — Widget Data Contract & Animation Map

> **Created**: 2026-07-21T18:35:00+07:00
> **Status**: 📋 PROPOSAL — Screens refined, awaiting final user review
> **Supersedes**: `2026-07-21_1802_phase-design-dashboard-permission-driven.md` (layout sai, global KPI vô nghĩa)
> **Boundary**: Web.Backend → Application → Domain | Infrastructure → Application/Domain
> **Scope**: Stitch Canvas + Document ONLY — NO runtime code changes

---

## 0. Business Rules Recap

1. Dashboard = **1 shell chung**, widget bật/tắt theo permission.
2. Không hardcode role Admin/HR.
3. Không "Admin/HR thấy all" mặc định.
4. Không seed DB.
5. Monochrome Swiss: black/white/gray + red `#E62429` cho rejected/critical only.
6. Status colors: Approved=black chip, Pending=gray-border chip, Rejected=red chip, Canceled=#999 text.

---

## 1. Stitch Screens — Refined (Deliverable 1)

Project: `17479353588209716186` | Design System: `assets/f4fbeeb3791c4c52991dd52c4fb92635`

| Screen | ID | Device | Action |
|---|---|---|---|
| HRM Dashboard - Desktop | `8ef6915706a84ba6a42aff0e51995c3f` | DESKTOP | EDITED — rebuilt layout |
| HRM Dashboard - Mobile | `615239637dd743b4b62b3a83e2a80a12` | MOBILE | EDITED — rebuilt layout |

Changes from previous version:
- ❌ Removed global KPI cards (Nhân viên 128, Phòng ban 12, Số dư phép 14.5) from top
- ✅ Added Top Common Area (title, scope banner, period selector)
- ✅ Grouped widgets into 4 sections: Leave Ops, Workforce, Calendar, Balance
- ✅ Each widget has permission badge + scope note
- ✅ Monochrome status chips (no green/yellow)
- ✅ Workforce KPIs moved to Section B2 with scope notes ("Theo quyền truy cập")
- ✅ Leave Balance shows personal progress bar, not global stat

---

## 2. Updated Widget Data Contract Table (Deliverable 3)

### Section A: Top Common Area (luôn hiển thị)

| Element | Mô tả | Permission | Scope |
|---|---|---|---|
| Dashboard Title | "HRM Dashboard" | `VIEW_DASHBOARD` (page gate) | N/A |
| Scope Banner | "Hiển thị dữ liệu theo quyền truy cập" | Always visible | N/A |
| Period Selector | Hôm nay / Tháng này / Quý này / Tùy chỉnh | Always visible | Applies to all date-filtered widgets |
| Widget Counter | "Widget khả dụng: X" | Computed from permissions | N/A |

### Section B1: Leave Operations — QUẢN LÝ NGHỈ PHÉP

| # | Widget | Permission | Scope Rule | Fields/Data | Visual Type | Empty State | Loading | Animation |
|---|---|---|---|---|---|---|---|---|
| W1 | **Đơn nghỉ phép của tôi** | `VIEW_LEAVE_REQUEST` | Requests created by current user only | LeaveType, StartDate, EndDate, Status, CreatedAt | Table (5 rows) + "Xem tất cả" link | "Bạn chưa có đơn nghỉ phép nào" | 5-row skeleton table | Row reveal stagger 80ms |
| W2 | **Hàng đợi phê duyệt** | `APPROVE_LEAVE_REQUEST` | Requests in user's approver assignment scope | RequesterName, LeaveType, StartDate, EndDate, Status | Actionable list (4 items) + Approve/Reject buttons | "Không có đơn chờ duyệt" | 4-card skeleton | Card stagger 100ms |
| W3 | **Phân bổ trạng thái** | `VIEW_LEAVE_REQUEST` | Aggregate of requests user can see (own + assigned if approver) | StatusCount: {Approved, Pending, Rejected, Canceled} | Stacked horizontal bar (monochrome) | "Chưa có dữ liệu" | Rectangle skeleton | Draw-in left→right 600ms |
| W4 | **Xu hướng nghỉ phép** | `VIEW_LEAVE_REQUEST` | Monthly count of requests user can see, last 6 months | MonthLabel, RequestCount | Line chart (single black line + dots) | "Chưa có dữ liệu xu hướng" | Rectangle skeleton | Line draw-in 800ms |

**Backend status**: W1 needs `Application` query filtering by `RequesterId = currentUser`. W2 needs scoped query joining `ApproverAssignment` — **needs backend audit later**. W3, W4 need aggregate queries on scoped results — **needs backend audit later**.

### Section B2: Workforce Context — BỐI CẢNH NHÂN SỰ

| # | Widget | Permission | Scope Rule | Fields/Data | Visual Type | Empty State | Loading | Animation |
|---|---|---|---|---|---|---|---|---|
| W5 | **Nhân sự trong phạm vi** | `VIEW_EMPLOYEE` | Count of employees user can view (currently global — **needs backend audit** if scope needed) | EmployeeCount | Compact KPI card (number + label) | Ẩn nếu không có permission | Number skeleton | Count-up 600ms |
| W6 | **Phòng ban** | `VIEW_DEPARTMENT` | Count of departments (org-level, no scope needed) | DepartmentCount | Compact KPI card | Ẩn | Number skeleton | Count-up 600ms |
| W7 | **Vị trí** | `VIEW_POSITION` | Count of positions (org-level, no scope needed) | PositionCount | Compact KPI card | Ẩn | Number skeleton | Count-up 600ms |
| W8 | **Nghỉ phép theo phòng ban** | `VIEW_LEAVE_REQUEST` + `VIEW_DEPARTMENT` | Leave request count grouped by department, only departments/requests in user scope | DepartmentName, LeaveCount | Horizontal bar chart (black bars) | "Chưa có dữ liệu" | Rectangle skeleton | Bars draw-in stagger 100ms |

**Backend status**: W5 employee count — current backend returns global. If user needs scoped count → **needs backend audit later**. W6, W7 — org-level counts, existing queries suffice. W8 needs cross-table aggregate — **needs backend audit later**.

### Section B3: Work Calendar — LỊCH LÀM VIỆC

| # | Widget | Permission | Scope Rule | Fields/Data | Visual Type | Empty State | Loading | Animation |
|---|---|---|---|---|---|---|---|---|
| W9 | **Ngày nghỉ sắp tới** | `VIEW_WORK_CALENDAR` | Global (lịch nghỉ chung cho org, không cần scope cá nhân) | Date, DayName, EventType (Ngày lễ/Nghỉ bù), DayType | Timeline list (5 rows) | "Chưa có ngày nghỉ sắp tới" | 5-row skeleton | Row reveal stagger 80ms |

**Backend status**: WorkCalendar queries exist. Need `Application` query to filter upcoming non-working days — **needs backend audit later** (likely simple date filter on existing `WorkCalendarDay` entity).

### Section B4: Leave Balance — SỐ DƯ PHÉP

| # | Widget | Permission | Scope Rule | Fields/Data | Visual Type | Empty State | Loading | Animation |
|---|---|---|---|---|---|---|---|---|
| W10 | **Số dư phép của tôi** | `VIEW_LEAVE_BALANCE` | Personal — current user's own balance only | AllocatedDays, UsedDays, AvailableDays (= Allocated - Used - Pending) | Large number + progress bar (black fill) | "Chưa có dữ liệu số dư" | Bar + number skeleton | Count-up 600ms + bar fill 400ms |
| W11 | **Cảnh báo số dư** | `VIEW_LEAVE_BALANCE` + `VIEW_EMPLOYEE` | Employees with low/exhausted balance in user's visible scope | EmployeeName, RemainingDays, DepartmentName | Compact list (3 items) | "Không có cảnh báo" | 3-row skeleton | Fade-in 300ms |

**Backend status**: W10 — LeaveBalance query for current user exists. W11 needs aggregate query filtering employees by scope + low balance threshold — **needs backend audit later**.

---

## 3. Updated Chart + Animation Mapping (Deliverable 4)

### Chart Types

| Chart | Visual | Colors | Interaction |
|---|---|---|---|
| Status Distribution (W3) | Stacked horizontal bar | Black=Approved, #D1D1D1=Pending, #E62429=Rejected, #EEEEEE=Canceled | Hover: tooltip with count |
| Monthly Trend (W4) | Line chart + dots | Single black line, #111 dots | Hover: tooltip with month+count |
| Department Load (W8) | Horizontal bar | Black bars on white | Hover: bar highlight |
| Leave Balance Progress (W10) | Progress bar | Black fill on #F5F5F5 track | Static (no interaction) |

### Animation Map

| Area | Animation | Duration | Trigger |
|---|---|---|---|
| **Top Common Area** | Fade-in + slide-down | 200ms | Page load |
| **Section group headers** | Fade-in | 150ms, stagger 50ms per section | Page load |
| **KPI numbers (W5, W6, W7)** | Count-up from 0 | 600ms, ease-out | First viewport enter |
| **Personal balance (W10)** | Count-up + progress bar fill | 600ms number + 400ms bar | First viewport enter |
| **Table rows (W1, W9)** | Row reveal stagger | 80ms between rows | First viewport enter (IntersectionObserver) |
| **Approval cards (W2)** | Card stagger | 100ms between cards | First viewport enter |
| **Charts (W3, W4, W8)** | Draw-in from baseline | 600-800ms | First viewport enter |
| **Low balance list (W11)** | Fade-in | 300ms | First viewport enter |
| **Empty state** | Fade-in centered | 300ms | On render if no widgets |
| **Loading skeletons** | Pulse animation | Infinite until data | On mount |
| **Permission reflow** | CSS grid auto-fit snap | Instant (page reload, not live toggle) | Server-side render |
| **Buttons (W2 approve/reject)** | Tactile press feedback | 100ms scale(0.97) | On click |
| **Table row hover** | Background #F9F9F9 | 150ms | On hover |

---

## 4. Critical Self-Review (Deliverable 5)

### Còn widget global vô nghĩa không?

| Widget | Verdict | Lý do |
|---|---|---|
| W5 "Nhân sự trong phạm vi" | ⚠️ Tiềm ẩn global | Backend hiện trả global employee count. Label "trong phạm vi" + scope note "Theo quyền truy cập" đã clarify intent, nhưng runtime cần backend audit để thực sự scope. |
| W6 "Phòng ban" | ✅ OK | Department count là org-level metric, không chứa data cá nhân. Không cần scope. |
| W7 "Vị trí" | ✅ OK | Position count là org-level metric. |
| W10 "Số dư của tôi" | ✅ OK | Scoped to personal data. |
| Tất cả widget khác | ✅ OK | Permission-gated + scope notes. |

### Kết luận
- **1 widget tiềm ẩn global**: W5 (Employee count) — label + scope note đã compensate ở design level, nhưng backend cần audit.
- **0 widget global vô nghĩa còn sót** — đã bỏ "128 nhân viên", "12 phòng ban", "14.5 số dư" khỏi top KPI strip.
- **0 green/yellow/blue** — tất cả status dùng monochrome Swiss.
- **0 legacy LUC** — không có Booking/Orders/Revenue.
- **0 hardcoded role** — chỉ permission strings.

---

## 5. Backend Audit Summary

| Widget | Backend Status |
|---|---|
| W1 | Needs simple query: filter LeaveRequests by RequesterId = currentUser |
| W2 | **Needs scoped query** joining ApproverAssignment — complex |
| W3 | Needs aggregate on scoped results |
| W4 | Needs time-series aggregate on scoped results |
| W5 | Exists (flat/global) — needs audit if scope required |
| W6 | Exists (GetDepartments) |
| W7 | Exists (GetPositions) |
| W8 | **Needs cross-table aggregate** — complex |
| W9 | Needs date-filtered query on WorkCalendarDay |
| W10 | Exists (LeaveBalance for current user) |
| W11 | **Needs aggregate** filtering low balance + scope — complex |

---

## 6. Guardrails Checklist

- [x] Không sửa C#/Controller/Application/Domain/Infrastructure
- [x] Không sửa DB/seed
- [x] Không sửa Auth/Keycloak
- [x] Không stage/commit/push
- [x] Không hardcode role name
- [x] Không tách dashboard theo group permission
- [x] Tất cả widget trong 1 screen desktop + 1 screen mobile
- [x] Dùng Stitch MCP edit_screens (không tạo mới)
- [x] Encoding UTF-8 BOM
