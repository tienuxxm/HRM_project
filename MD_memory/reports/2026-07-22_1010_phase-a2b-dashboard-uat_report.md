# Báo cáo UAT Dashboard Phase A2B — Approval Widgets (W4 & W5)

> **Ngày UAT**: 2026-07-22
> **Môi trường**: Local Development (`http://localhost:5300`)
> **Chế độ xác thực**: Keycloak thật (`UseMockAuth: false`, Keycloak container `:8080`, Realm `hrm`)
> **Tuân thủ quy tắc**: Không code patch rủi ro ngoài scope, không seed DB, không chỉnh sửa Keycloak/Auth/Permission.

---

## 1. Tóm tắt kết quả UAT Matrix

| Test Case | Mô tả | Account test | Expected Behavior | Actual Observed | Kết luận |
|---|---|---|---|---|---|
| **TC1** | Admin global approval view | `admin` / `Admin@123456` | W4/W5 hiện 3 đơn pending của active employees | W4/W5 render đúng **3 đơn** (`Nhan Vien Test`, `uat.provision80` x2). W5 Overdue = **3**. | ✅ **PASS** |
| **TC2** | HR global approval view | *N/A* | Defer theo yêu cầu User | Không thực hiện (Không tự seed DB) | ⏸️ **DEFERRED** |
| **TC3** | CEO scoped approval view | `ceo.test` / `Admin@123456` | W4/W5 chỉ hiện 0 đơn (Do 7 đơn khớp position target thuộc nhân viên inactive) | W4/W5 render 0 đơn, khớp chính xác 100% với DB query | ✅ **PASS** |
| **TC4** | Manager scoped approval view | *N/A* | Defer theo yêu cầu User | Không thực hiện (Không tự đoán account) | ⏸️ **DEFERRED** |
| **TC5** | Employee no approval permission | `nv.test` / `Admin@123456` | W4/W5 không render, không có card rỗng gây lệch layout | W4/W5 **hoàn toàn ẩn**, layout đan xếp chuẩn Swiss UI, 0 UI gap | ✅ **PASS** |
| **TC6** | Console & UX Compliance | Tất cả accounts | 0 JS errors, Swiss UI empty states chuẩn | 0 console JS errors trên tất cả các trang | ✅ **PASS** |

---

## 2. Bằng chứng kiểm thử chi tiết & Workspace Evidence Relative Paths

### TC1 — Admin global approval view
- **Account used**: `admin` / `Admin@123456`
- **Identity / Capabilities observed**:
  - `CanViewDashboard`: `true`
  - `CanApproveLeaveRequest`: `true`
  - `CanViewAllApprovals` (`UPDATE_LEAVE_APPROVER_ASSIGNMENT`): `true` (Global Approval Capability)
- **URL tested**: `http://localhost:5300/dashboard`
- **W4 Result Count**: 3 items (`Nhan Vien Test`, `uat.provision80` x2)
- **W5 Bucket Counts**: Today: 0, 1-2 Days: 0, Overdue (3+ days): 3
- **Console JS Errors**: `0`
- **Workspace Evidence Screenshot**: [TC1_post_patch_admin_dashboard.png](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/evidence/2026-07-22_dashboard-a2b/TC1_post_patch_admin_dashboard.png)
- **Trạng thái**: ✅ **PASS**

### TC3 — CEO / Approver scoped view
- **Account used**: `ceo.test` / `Admin@123456`
- **Identity / Capabilities observed**:
  - `CanViewDashboard`: `true`
  - `CanApproveLeaveRequest`: `true`
  - `CanViewAllApprovals`: `false` (Scoped Approver)
- **URL tested**: `http://localhost:5300/dashboard`
- **W4 Result Count**: 0 items
- **W5 Bucket Counts**: Today: 0, 1-2 Days: 0, Overdue: 0
- **Bằng chứng DB Scoped Query**:
  - `ceo.test` có 1 assignment active target position `a9c8b7f6-6c5d-4e3d-2b1a-0f9e8d7c6b5a`.
  - Có 7 đơn pending mang position `a9c8b7f6...`, nhưng TẤT CẢ 7 đơn này thuộc về `Nguyen Van Employee` (`is_active = false`).
  - Đối với nhân viên Active, `ceo.test` có 0 đơn pending trong scope.
  - Kết quả 0 đơn trên UI hoàn toàn chính xác theo DB query logic.
- **Console JS Errors**: `0`
- **Workspace Evidence Screenshot**: [TC3_post_patch_ceo_dashboard.png](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/evidence/2026-07-22_dashboard-a2b/TC3_post_patch_ceo_dashboard.png)
- **Trạng thái**: ✅ **PASS**

### TC5 — Employee no approval permission
- **Account used**: `nv.test` / `Admin@123456`
- **Identity / Capabilities observed**:
  - `CanViewDashboard`: `true`
  - `CanApproveLeaveRequest`: `false` (Không có quyền duyệt)
- **URL tested**: `http://localhost:5300/dashboard`
- **W4 Rendered**: ❌ No (Đã ẩn)
- **W5 Rendered**: ❌ No (Đã ẩn)
- **Console JS Errors**: `0`
- **Workspace Evidence Screenshot**: [TC5_post_patch_employee_dashboard.png](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/evidence/2026-07-22_dashboard-a2b/TC5_post_patch_employee_dashboard.png)
- **Trạng thái**: ✅ **PASS**

### TC6 — Console Evidence Log
- **Workspace Log Path**: [TC6_console_clean.txt](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/evidence/2026-07-22_dashboard-a2b/TC6_console_clean.txt)
- **Trạng thái**: ✅ **PASS**

---

## 3. Báo cáo Phân loại Git Scope Chính xác

### 3.1 Modified Tracked Files
- `.agents/rules/project.md`
- `HRM_Leave_Management/Domain/WorkCalendars/IWorkCalendarDayRepository.cs`
- `HRM_Leave_Management/Web.Backend/Controllers/DashboardController.cs`
- `HRM_Leave_Management/Web.Backend/Models/DashboardViewModel.cs`
- `HRM_Leave_Management/Web.Backend/Views/Dashboard/Index.cshtml`
- `HRM_Leave_Management/Web.Backend/Views/WorkCalendar/Index.cshtml`
- `HRM_Leave_Management/Web.Backend/Views/WorkCalendar/Preview.cshtml`
- `HRM_Leave_Management/Web.Backend/Views/WorkCalendar/Summary.cshtml`

### 3.2 Untracked New Application Query Folders/Files (Phase A2B Implementation Scope)
- `HRM_Leave_Management/Application/LeaveRequests/GetPendingApprovals/GetPendingApprovalsQuery.cs`
- `HRM_Leave_Management/Application/LeaveRequests/GetPendingApprovals/GetPendingApprovalsQueryHandler.cs`
- `HRM_Leave_Management/Application/LeaveRequests/GetPendingApprovals/PendingApprovalItem.cs`
- `HRM_Leave_Management/Application/LeaveRequests/GetApprovalAging/GetApprovalAgingQuery.cs`
- `HRM_Leave_Management/Application/LeaveRequests/GetApprovalAging/GetApprovalAgingQueryHandler.cs`
- `HRM_Leave_Management/Application/LeaveRequests/GetApprovalAging/ApprovalAgingResult.cs`

### 3.3 New Evidence / Report / Debug Files
- `MD_memory/evidence/2026-07-22_dashboard-a2b/TC1_admin_dashboard_w4_w5.png`
- `MD_memory/evidence/2026-07-22_dashboard-a2b/TC1_post_patch_admin_dashboard.png`
- `MD_memory/evidence/2026-07-22_dashboard-a2b/TC3_ceo_dashboard_w4_w5.png`
- `MD_memory/evidence/2026-07-22_dashboard-a2b/TC3_post_patch_ceo_dashboard.png`
- `MD_memory/evidence/2026-07-22_dashboard-a2b/TC5_employee_dashboard_no_w4_w5.png`
- `MD_memory/evidence/2026-07-22_dashboard-a2b/TC5_post_patch_employee_dashboard.png`
- `MD_memory/evidence/2026-07-22_dashboard-a2b/TC6_console_clean.txt`
- `MD_memory/reports/2026-07-22_1010_phase-a2b-dashboard-uat_report.md`
- `MD_memory/debug/2026-07-22_1010_db_check2.bat` (Sanitized)
- `MD_memory/debug/2026-07-22_1010_db_check3.bat` (Sanitized)
- `MD_memory/debug/2026-07-22_1010_db_evidence_query.bat` (Sanitized)
- `MD_memory/debug/2026-07-22_1010_generate_report.py`
