# Báo Cáo Hoàn Thành Patch Phase A1 Dashboard Shell (1:1 Contract Alignment)

## I. Tổng Quan & Cấu Trúc Scope
- **Ngày thực hiện:** 2026-07-21
- **Mục tiêu:** Sửa các lệch hợp đồng (residual issues) của Phase A1 Dashboard Shell, bao gồm 1:1 Widget Contract, Swiss Sharp CSS, và chỉnh sửa các Action Link về đúng controller route hiện có.
- **Boundary giữ vững:**
  - Web.Backend -> Application -> Domain
  - Infrastructure -> Application/Domain
- **Scope File được tác động:**
  1. HRM_Leave_Management/Web.Backend/Models/DashboardViewModel.cs
  2. HRM_Leave_Management/Web.Backend/Views/Dashboard/Index.cshtml
  3. HRM_Leave_Management/Web.Backend/wwwroot/css/dashboard.css (untracked file)
- **File ngoài scope:** IWorkCalendarDayRepository.cs được giữ nguyên 100%, không bị checkout/restore/reset.

---

## II. Chi Tiết Các Thay Đổi Đã Patch

### 1. Sửa Action Links Lệch Route Controller (Index.cshtml)
Tất cả các link action trên 10 widget đã được điều chỉnh từ route không tồn tại về đúng 5 controller route hiện có:
- `/LeaveRequest/MyRequests` -> `/leave-request`
- `/LeaveRequest` -> `/leave-request`
- `/LeaveRequest/Create` -> `/leave-request`
- `/LeaveRequest/Approvals` -> `/leave-request`
- `/LeaveRequest/Approvals?filter=aging` -> `/leave-request`
- `/Employee` -> `/employee`
- `/Department` -> `/department`
- `/WorkCalendar` -> `/work-calendar`

### 2. Giữ Đúng Permission Gate 1:1 (W1-W10) (DashboardViewModel.cs)
- **W1 (My Leave Requests):** CanViewLeaveRequest
- **W2 (Status Distribution):** CanViewLeaveRequest
- **W3 (Monthly Trend):** CanViewLeaveRequest
- **W4 (Approval Queue):** CanApproveLeaveRequest
- **W5 (Approval Aging):** CanApproveLeaveRequest
- **W6 (My Leave Balance):** CanViewLeaveBalance
- **W7 (Low Balance Watchlist):** CanViewLeaveBalance && CanViewEmployee
- **W8 (Upcoming Holidays):** CanViewWorkCalendar
- **W9 (Calendar Impact Alerts):** CanViewWorkCalendar
- **W10 (Department Leave Load):** CanViewLeaveRequest && CanViewDepartment

### 3. Swiss Sharp Style CSS (dashboard.css - Untracked File)
- Standardized `border-radius`: `2px`
- Shadow: `box-shadow: none;`
- Hairline border: `1px solid var(--hrm-border-light)`

---

## III. Bằng Chứng Kiểm Tra (Verification Evidence)

1. **git status --short:**
   - M HRM_Leave_Management/Web.Backend/Controllers/DashboardController.cs (pre-existing)
   - M HRM_Leave_Management/Web.Backend/Models/DashboardViewModel.cs (patched)
   - M HRM_Leave_Management/Web.Backend/Views/Dashboard/Index.cshtml (patched)
   - ?? HRM_Leave_Management/Web.Backend/wwwroot/css/dashboard.css (untracked, chưa stage)

2. **Kiểm tra Untracked CSS dashboard.css bằng Token Audit:**
   - `box-shadow`: Không tồn tại / `box-shadow: none;`.
   - `border-radius`: Chỉ dùng `border-radius: 2px`.
   - Bảng màu cấm (`blue`, `green`, `yellow`, `amber`, `indigo`, `lime`): 0 kết quả trong các quy tắc CSS thực tế.
   - *Lưu ý:* Vì dashboard.css là untracked file (??), git diff --check chỉ kiểm tra các file đã được Git track (Index.cshtml, DashboardViewModel.cs).

3. **git diff --check (Tracked Files):**
   - Lệnh: git diff --check -- HRM_Leave_Management/Web.Backend/Views/Dashboard/Index.cshtml HRM_Leave_Management/Web.Backend/Models/DashboardViewModel.cs
   - Kết quả: Clean (0 lỗi khoảng trắng / xuống dòng).

4. **git diff --name-status (Tracked Files):**
   - M HRM_Leave_Management/Web.Backend/Models/DashboardViewModel.cs
   - M HRM_Leave_Management/Web.Backend/Views/Dashboard/Index.cshtml

5. **Dotnet Build:** PASSED (0 Error).

6. **GitNexus Symbol Assessment:**
   - Symbol risk: LOW (0 affected execution flows).
   - *Lưu ý:* Giao diện runtime dashboard vẫn cần thực hiện manual / browser UAT sau khi User yêu cầu.

---

## IV. Kết Luận
Dashboard Shell đã hoàn thành source-level alignment pass; runtime UAT pending. Báo cáo đạt chuẩn UTF-8 BOM và không chứa ký tự điều khiển (control characters) không hợp lệ.
