# Dashboard Final Design — Widget Contract, Chart & Animation Map

> **Created**: 2026-07-21T18:55:00+07:00
> **Status**: 📋 FINAL PROPOSAL — Awaiting user review on Stitch Canvas
> **Supersedes**: `2026-07-21_1835_dashboard-refined-widget-contract.md` (còn stat cards W5/W6/W7)
> **Boundary**: Web.Backend → Application → Domain | Infrastructure → Application/Domain
> **Scope**: Stitch Canvas + Document ONLY — NO runtime code changes

---

## 0. Business Rules

1. Dashboard = **1 shell chung**, widget bật/tắt theo permission.
2. Không hardcode role Admin/HR.
3. Không "Admin/HR thấy all" mặc định.
4. Không seed DB.
5. Monochrome Swiss: black/white/gray + red `#E62429` cho rejected/critical only.
6. **Không stat/KPI card dạng số đếm rời rạc** (EmployeeCount, DeptCount, PositionCount).
7. Mỗi widget phải có **insight** (user hiểu gì) và **action** (user làm gì tiếp).

---

## 1. Stitch Screens (Deliverable 1 & 2)

Project: `17479353588209716186` | Design System: `assets/f4fbeeb3791c4c52991dd52c4fb92635`

| Screen | ID | Device | Status |
|---|---|---|---|
| HRM Dashboard Desktop | `8ef6915706a84ba6a42aff0e51995c3f` | DESKTOP | ✅ EDITED — stat cards removed |
| HRM Dashboard Mobile | `615239637dd743b4b62b3a83e2a80a12` | MOBILE | ✅ EDITED — stat cards removed |

---

## 2. Widget Data Contract (Deliverable 3)

### Section A: Top Common Area (luôn hiển thị nếu có VIEW_DASHBOARD)

| Element | Permission | Mô tả |
|---|---|---|
| Title "HRM Dashboard" | `VIEW_DASHBOARD` | Page-level gate |
| Scope explanation | Always | "Dữ liệu hiển thị theo quyền truy cập của bạn" |
| Period selector | Always | Hôm nay / Tháng này / Quý này / Tùy chỉnh |
| Empty state | When 0 widgets visible | "Không có widget khả dụng. Liên hệ quản trị viên." |

**Không có stat/KPI card ở đây.**

---

### Section B1: QUẢN LÝ NGHỈ PHÉP

#### W1 — Đơn nghỉ phép của tôi

| Attribute | Value |
|---|---|
| **Permission** | `VIEW_LEAVE_REQUEST` |
| **Scope** | Requests created by current user only (`RequesterId = currentUser`) |
| **Data fields** | LeaveType, StartDate, EndDate, Duration (days), Status, LastUpdated |
| **User insight** | Biết đơn nào đang pending/rejected/approved và cần theo dõi gì |
| **User action** | Xem chi tiết đơn, theo dõi trạng thái, tạo đơn mới |
| **Visual** | Dense table/list, 5 rows, status chips (black/gray-border/red), "Xem tất cả →" link |
| **Empty state** | "Bạn chưa có đơn nghỉ phép nào" |
| **Loading** | 5-row skeleton table |
| **Animation** | Row reveal stagger 80ms |
| **Backend** | Needs Application query: filter by RequesterId. Pattern exists in `GetLeaveRequestsQueryHandler` |

#### W2 — Phân bổ trạng thái

| Attribute | Value |
|---|---|
| **Permission** | `VIEW_LEAVE_REQUEST` |
| **Scope** | Aggregate status counts of requests user can see (own + assigned if approver) |
| **Data fields** | StatusCounts: {Approved, Pending, Rejected, Canceled} |
| **User insight** | Tình trạng tổng thể luồng nghỉ phép — có bao nhiêu đang chờ/bị từ chối |
| **User action** | Nhận biết bottleneck, click status để filter danh sách |
| **Visual** | Stacked horizontal bar (black=Approved, #D1D1D1=Pending, #E62429=Rejected, #EEE=Canceled) + legend with counts |
| **Empty state** | "Chưa có dữ liệu trạng thái" |
| **Loading** | Rectangle skeleton |
| **Animation** | Draw-in left→right 600ms |
| **Backend** | **Needs backend audit later** — aggregate query on scoped results |

#### W3 — Xu hướng nghỉ phép 6 tháng

| Attribute | Value |
|---|---|
| **Permission** | `VIEW_LEAVE_REQUEST` |
| **Scope** | Monthly request count in user's visible scope, last 6 months |
| **Data fields** | MonthLabel, RequestCount |
| **User insight** | Xu hướng nghỉ phép tăng/giảm theo thời gian — dự đoán tải duyệt |
| **User action** | Nhận biết mùa cao điểm, lên kế hoạch nhân sự |
| **Visual** | Line chart, single black line + dots, X: T2→T7, Y: count |
| **Empty state** | "Chưa có dữ liệu xu hướng" |
| **Loading** | Rectangle skeleton |
| **Animation** | Line draw-in 800ms (IntersectionObserver) |
| **Backend** | **Needs backend audit later** — time-series aggregate |

---

### Section B2: CÔNG VIỆC DUYỆT ĐƠN

#### W4 — Hàng đợi phê duyệt

| Attribute | Value |
|---|---|
| **Permission** | `APPROVE_LEAVE_REQUEST` |
| **Scope** | Pending requests in user's approver assignment scope |
| **Data fields** | RequesterName, Department, LeaveType, StartDate, EndDate, Duration, CreatedAt (SLA age), ReasonSnippet |
| **User insight** | Đơn nào cần xử lý ngay, ai gửi, bao lâu rồi |
| **User action** | Xem chi tiết → Phê duyệt / Từ chối |
| **Visual** | Actionable queue, 4 cards with name+dept, type+dates+duration, age, 3 buttons (Chi tiết / Phê duyệt / Từ chối) |
| **Empty state** | "Không có đơn chờ duyệt trong phạm vi của bạn" |
| **Loading** | 4-card skeleton |
| **Animation** | Card stagger 100ms |
| **Backend** | **Needs backend audit later** — scoped query JOIN ApproverAssignment, pattern exists in `GetLeaveRequestsQueryHandler` |

#### W5 — Tồn đọng duyệt (Approval Aging)

| Attribute | Value |
|---|---|
| **Permission** | `APPROVE_LEAVE_REQUEST` |
| **Scope** | Age distribution of pending requests in approver's scope |
| **Data fields** | AgeBuckets: {Today count, 1-2 days count, 3+ days count} |
| **User insight** | Backlog duyệt có bị tồn đọng không — có đơn nào quá hạn SLA |
| **User action** | Ưu tiên xử lý đơn cũ, giảm tồn đọng |
| **Visual** | 3 horizontal aging bars: "Hôm nay" (black), "1-2 ngày" (dark gray), "3+ ngày" (red left-border ⚠). Total count below. |
| **Empty state** | "Không có đơn tồn đọng" |
| **Loading** | 3-bar skeleton |
| **Animation** | Bars draw-in stagger 100ms |
| **Backend** | **Needs backend audit later** — computed from W4 data + CreatedAt age calculation |

---

### Section B3: SỐ DƯ PHÉP

#### W6 — Số dư phép của tôi

| Attribute | Value |
|---|---|
| **Permission** | `VIEW_LEAVE_BALANCE` |
| **Scope** | Personal — current user's own balance only |
| **Data fields** | AllocatedDays, UsedDays, PendingDays, AvailableDays (= Allocated - Used - Pending) |
| **User insight** | Còn bao nhiêu ngày có thể xin nghỉ, bao nhiêu đang chờ duyệt |
| **User action** | Quyết định có tạo đơn nghỉ mới không |
| **Visual** | Progress bar (black fill on #F5F5F5 track, 14.5/20) + breakdown line: "Tổng: 20 | Đã dùng: 4.5 | Đang chờ: 1 | Còn lại: 14.5". **NOT a stat card.** |
| **Empty state** | "Chưa có dữ liệu số dư phép" |
| **Loading** | Bar + text skeleton |
| **Animation** | Progress bar fill 400ms + count-up 600ms |
| **Backend** | LeaveBalance query for current user — exists. AvailableDays = AllocatedDays - UsedDays - PendingDays (Phase 2C.3 formula) |

#### W7 — Cảnh báo số dư (Low Balance Watchlist)

| Attribute | Value |
|---|---|
| **Permission** | `VIEW_LEAVE_BALANCE` + `VIEW_EMPLOYEE` |
| **Scope** | Employees with low/exhausted balance in user's visible scope |
| **Data fields** | EmployeeName, Department, AvailableDays, PendingDays |
| **User insight** | Nhân sự nào sắp hết phép hoặc có rủi ro nghỉ không phép |
| **User action** | Lên kế hoạch thay thế, thông báo nhân sự |
| **Visual** | Compact list, 3 items, each with name/dept/remaining. Exhausted balance → #E62429 text. "Xem →" link per row. |
| **Empty state** | "Không có cảnh báo số dư" |
| **Loading** | 3-row skeleton |
| **Animation** | Fade-in 300ms |
| **Backend** | **Needs backend audit later** — aggregate query filtering employees by scope + low balance threshold |

---

### Section B4: LỊCH LÀM VIỆC

#### W8 — Ngày nghỉ sắp tới

| Attribute | Value |
|---|---|
| **Permission** | `VIEW_WORK_CALENDAR` |
| **Scope** | Global (lịch nghỉ chung cho tổ chức, không cần scope cá nhân) |
| **Data fields** | Date, DayName, DayType (Ngày lễ / Nghỉ bù), Description |
| **User insight** | Các ngày nghỉ sắp tới ảnh hưởng kế hoạch nghỉ phép |
| **User action** | Tránh tạo đơn trùng ngày nghỉ, lên kế hoạch trước |
| **Visual** | Timeline list, 5 entries, date bold left + event chip right, thin left border connecting |
| **Empty state** | "Chưa có ngày nghỉ sắp tới trong lịch" |
| **Loading** | 5-row skeleton |
| **Animation** | Row reveal stagger 80ms |
| **Backend** | **Needs backend audit later** — date-filtered query on WorkCalendarDay entity (likely simple) |

#### W9 — Thay đổi lịch gần đây (Calendar Change Impact)

| Attribute | Value |
|---|---|
| **Permission** | `VIEW_WORK_CALENDAR` |
| **Scope** | Recent calendar changes and affected leave requests |
| **Data fields** | LastImportAt, ChangedDays (count), AffectedLeaveRequests (count), ChangedDayDetails: {Date, OldType, NewType} |
| **User insight** | Thay đổi lịch gần đây có ảnh hưởng đơn nghỉ đang xử lý không |
| **User action** | Kiểm tra đơn bị ảnh hưởng, điều chỉnh nếu cần |
| **Visual** | Impact summary: "Nhập cuối: 18/07" + "3 thay đổi, 2 đơn ảnh hưởng" + compact change list |
| **Empty state** | "Không có thay đổi lịch gần đây" |
| **Loading** | Text + list skeleton |
| **Animation** | Fade-in 300ms |
| **Backend** | **Needs backend audit later** — needs tracking of calendar changes + cross-reference with active leave requests |

---

### Section B5: BỐI CẢNH NHÂN SỰ

#### W10 — Nghỉ phép theo phòng ban (Department Leave Load)

| Attribute | Value |
|---|---|
| **Permission** | `VIEW_LEAVE_REQUEST` + `VIEW_DEPARTMENT` |
| **Scope** | Leave request count grouped by department, only departments + requests in user's scope |
| **Data fields** | DepartmentName, ActiveLeaveCount, TotalDuration |
| **User insight** | Phòng ban nào đang có nhiều người nghỉ — rủi ro thiếu nhân lực |
| **User action** | Ưu tiên duyệt/từ chối dựa trên tải phòng ban, điều phối |
| **Visual** | Horizontal bar chart, 5 departments, black bars on white, dept name left, count right |
| **Empty state** | "Chưa có dữ liệu nghỉ phép theo phòng ban" |
| **Loading** | Rectangle skeleton |
| **Animation** | Bars draw-in stagger 100ms |
| **Backend** | **Needs backend audit later** — cross-table aggregate LeaveRequest × Department |

---

## 3. Chart + Animation Map (Deliverable 4)

### Chart Types

| Widget | Chart | Colors | Hover |
|---|---|---|---|
| W2 Status Distribution | Stacked horizontal bar | Black / #D1D1D1 / #E62429 / #EEE | Tooltip with count |
| W3 Monthly Trend | Line + dots | Single black line, #111 dots | Tooltip month+count |
| W5 Approval Aging | 3 horizontal bars | Black / dark gray / red-left-border | Highlight bar |
| W6 My Balance | Progress bar | Black fill on #F5F5F5 | Static |
| W10 Dept Load | Horizontal bars | Black bars | Highlight bar |

### Animation Map

| Target | Animation | Duration | Trigger |
|---|---|---|---|
| Top common area | Fade-in + slide-down 8px | 200ms ease-out | Page load |
| Section headers | Fade-in | 150ms, stagger 50ms | Page load |
| Table rows (W1, W8) | Row reveal | Stagger 80ms | IntersectionObserver |
| Approval cards (W4) | Card stagger | 100ms between cards | IntersectionObserver |
| Charts (W2, W3, W5, W10) | Draw-in from baseline | 600-800ms ease-out | IntersectionObserver |
| Progress bar (W6) | Fill left→right | 400ms ease-out | IntersectionObserver |
| Balance breakdown (W6) | Count-up | 600ms ease-out | IntersectionObserver |
| Low balance list (W7) | Fade-in | 300ms | IntersectionObserver |
| Calendar impact (W9) | Fade-in | 300ms | IntersectionObserver |
| Empty state | Fade-in centered | 300ms | On render |
| Loading skeletons | Pulse | Infinite | On mount |
| Permission reflow | CSS grid auto-fit snap | Instant (server-side render) | Page load |
| Approve/Reject buttons (W4) | Tactile scale(0.97) | 100ms | On click |
| Table row hover | Background #F9F9F9 | 150ms | On hover |
| Link hover | Underline appear | 150ms | On hover |

---

## 4. Self-Review (Deliverable 4)

### Stat card check

| Check | Result |
|---|---|
| Number-only KPI cards | **0** — bỏ hoàn toàn W5(Employee)/W6(Dept)/W7(Position) từ version cũ |
| "128 nhân viên" style cards | **0** — không tồn tại |
| EmployeeCount / DeptCount / PositionCount | **0** — không có trong widget catalog |
| Global meaningless metrics | **0** — mọi widget đều có scope note + insight |
| Role hardcoding | **0** — chỉ dùng permission strings |
| DB seed | **0** — không seed |
| Legacy LUC (Booking/Orders/Revenue) | **0** |
| Green/Blue/Yellow colors | **0** — monochrome + red only |
| Pie charts | **0** |
| Multiple dashboards by role | **0** — 1 shell, widgets toggle by permission |

### Widget insight check

| Widget | Has insight? | Has action? |
|---|---|---|
| W1 My Requests | ✅ Theo dõi trạng thái đơn | ✅ Xem chi tiết, tạo mới |
| W2 Status Distribution | ✅ Tình trạng luồng nghỉ phép | ✅ Filter theo status |
| W3 Monthly Trend | ✅ Xu hướng tăng/giảm | ✅ Lên kế hoạch |
| W4 Approval Queue | ✅ Đơn cần xử lý | ✅ Approve/Reject |
| W5 Approval Aging | ✅ Tồn đọng SLA | ✅ Ưu tiên đơn cũ |
| W6 My Balance | ✅ Ngày phép còn lại | ✅ Quyết định tạo đơn |
| W7 Low Balance | ✅ Rủi ro hết phép | ✅ Thông báo nhân sự |
| W8 Upcoming Days | ✅ Ảnh hưởng kế hoạch | ✅ Tránh trùng ngày nghỉ |
| W9 Calendar Impact | ✅ Đơn bị ảnh hưởng | ✅ Kiểm tra/điều chỉnh |
| W10 Dept Load | ✅ Tải phòng ban | ✅ Điều phối nhân lực |

### Backend status summary

| Widget | Backend Status |
|---|---|
| W1 | Pattern exists (`GetLeaveRequestsQueryHandler`). Needs simple filter. |
| W2 | **Needs backend audit later** — aggregate on scoped results |
| W3 | **Needs backend audit later** — time-series aggregate |
| W4 | **Needs backend audit later** — scoped JOIN ApproverAssignment |
| W5 | **Needs backend audit later** — computed from W4 + age |
| W6 | Exists (LeaveBalance for current user). Phase 2C.3 formula. |
| W7 | **Needs backend audit later** — low balance threshold query |
| W8 | **Needs backend audit later** — date filter on WorkCalendarDay (simple) |
| W9 | **Needs backend audit later** — calendar change tracking + cross-ref |
| W10 | **Needs backend audit later** — cross-table aggregate |

---

## 5. Guardrails

- [x] Không sửa C#/Controller/Application/Domain/Infrastructure
- [x] Không sửa DB/seed
- [x] Không sửa Auth/Keycloak
- [x] Không stage/commit/push
- [x] Không hardcode role name
- [x] Không stat/KPI cards
- [x] 1 desktop + 1 mobile screen (không tách theo role)
- [x] Dùng Stitch MCP edit_screens
- [x] UTF-8 BOM (file starts with ﻿)
