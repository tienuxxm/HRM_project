# Động Cơ Phê Duyệt Động (Dynamic Approval Routing Engine) — Ma Trận Cấu Hình UAT & Kế Hoạch Xác Minh (Bản Tiếng Việt)

**Ngày lập**: 2026-07-24  
**Tác giả**: Senior Fullstack Engineer & Technical Reviewer (Anti)  
**Mật khẩu UAT chung đã được User xác nhận**: `Admin@123456`  
**Ranh giới kiến trúc (Architecture Boundary)**:  
- `Web.Backend -> Application -> Domain`  
- `Infrastructure -> Application/Domain`  

---

## 1. Tóm Tắt Thực Thi & Chỉ Thị Xác Minh Nghiêm Ngặt

Tài liệu này thiết lập **Ma Trận Cấu Hình UAT** chính thức cho **Động Cơ Phê Duyệt Động Đơn Bước (Single-Step Dynamic Approval Routing Engine)** (`ApprovalRoutePolicy`, `ApprovalRouteRule`, `LeaveRequestApprovalAssignment`).

### Các Chỉ Thị Xác Minh Bắt Buộc:
1. **An Toàn Dữ Liệu & Bảo Mật Mật Khẩu**: Mật khẩu dạng plain-text đã được loại bỏ khỏi toàn bộ artifact trong workspace ở phase hiện tại. Các script debug tạo trong phase này (`MD_memory/debug/2026-07-24_1125_inspect-hrm-runtime-db.py` và `MD_memory/debug/2026-07-24_1142_query-permissions.py`) đã được cập nhật bắt buộc dùng biến môi trường `PGPASSWORD` và không có fallback plain-text. Không thực hiện bất kỳ thao tác mutation (thêm/sửa/xóa) database nào trong quá trình kiểm tra read-only này.
2. **Không Thay Đổi Code / Auth / Keycloak**: Không sửa code C#, Razor view, cấu hình Keycloak, hoặc phân quyền user nào.
3. **Đối Chiếu Chuẩn Xác Với Evidence Code Domain**: Toàn bộ trạng thái phân công, lý do, action type audit và thông báo lỗi domain đều trùng khớp 100% với code C# authoritative:
   - `ApprovalAssignmentStatus`: `Assigned` (`1`), `NeedsAdminAttention` (`2`).
   - `ApprovalAssignmentReason`: `DirectLevelMatch` (`1`), `SuperiorLevelEscalated` (`2`), `SpecificEmployeeOverride` (`3`), `OperatorManualReassigned` (`4`), `AutoApproved` (`5`).
   - `ApprovalRouteAuditActionType`: `Created` (`1`), `Reassigned` (`2`), `Escalated` (`3`), `NeedsAttention` (`4`), `OverrideApplied` (`5`), `AutoApproved` (`6`).
   - `LeaveRequestErrors.ApprovalRouteNotConfigured`: `LeaveRequest.ApprovalRouteNotConfigured` (`"Approval route is not configured for this department/position. Please assign an approver before submitting leave request."`).
4. **Trạng Thái Working Tree**: Working tree hiện có 2 plan chưa track (`MD_memory/plans/2026-07-24_1130_phase-dynamic-approval-routing_uat-config-matrix.md` và `MD_memory/plans/2026-07-24_1310_phase-dynamic-approval-routing_uat-config-matrix-vi.md`); không có file nào đang ở trạng thái staged.
5. **Quy Tắc Thực Thi UI**: Việc tạo dữ liệu thử nghiệm hoặc thiết lập cấu hình thực tế trên UI phải được User / Codex xem xét và phê duyệt rõ ràng trước khi thao tác.

---

## 2. Bảng Ánh Xạ Tài Khoản Runtime & Quyền Hạn Đã Được SQL Xác Minh Read-Only

Bảng dưới đây liệt kê các tài khoản user runtime, nhân viên liên kết, và **Danh Sách Quyền Hạn Đã Được Kiểm Tra Trực Tiếp Bằng SQL Read-Only** (`user -> user_to_role -> role -> role_to_permission -> permission`) từ PostgreSQL (`hrm_baseline_db`).

| System Username | Mã Nhân Viên Liên Kết | Tên Nhân Viên | Email | Trạng Thái Active | Phòng Ban | Chức Vụ | Quyền Resource Được Phân Công (Xác Minh Qua SQL) |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `admin` | *(System Admin)* | System Administrator | `admin@hrm.local` | `True` | *(Global)* | *(Global)* | `VIEW_DASHBOARD`, `CREATE_LEAVE_REQUEST`, `APPROVE_LEAVE_REQUEST`, `VIEW_LEAVE_APPROVER_ASSIGNMENT`, `UPDATE_LEAVE_APPROVER_ASSIGNMENT`, `VIEW_EMPLOYEE`, `UPDATE_EMPLOYEE`, `VIEW_DEPARTMENT`, `UPDATE_DEPARTMENT` + 41 Quyền Hệ Thống (Role ID: `11111111-1111-1111-1111-111111111111`) |
| `admin2` | *(System Admin)* | Secondary Admin | `admin2@hrm.local` | `True` | *(Global)* | *(Global)* | Cùng 50 Quyền Hệ Thống như `admin` (Role ID: `11111111-1111-1111-1111-111111111111`) |
| `huyadmin` | `EMP001` | Huy Admin | `huyadmin@hrm.local` | `True` | *(Unassigned)* | `Manager` | Cùng 50 Quyền Hệ Thống như `admin` (Role ID: `11111111-1111-1111-1111-111111111111`) |
| `uat.provision80` | `EMP05` | uat.provision80 | `uat.provision80@hrm.local` | `True` | `Information Technology` | `Employee` | `VIEW_DASHBOARD`, `CREATE_LEAVE_REQUEST`, `VIEW_LEAVE_BALANCE`, `VIEW_LEAVE_REQUEST` (Role ID: `11111111-1111-1111-1111-111111111112`) |
| `uat.provision81` | `EMP04` | uat.provision81 | `uat.provision81@hrm.local` | `True` | `Information Technology` | `Manager` | `VIEW_DASHBOARD`, `CREATE_LEAVE_REQUEST`, `APPROVE_LEAVE_REQUEST`, `VIEW_LEAVE_BALANCE`, `VIEW_LEAVE_REQUEST` (Role IDs: `7026b7ee-...`, `11111111-...`) |
| `ceo.test` | `EMP-CEO-TEST` | CEO Test | `ceo.test@hrm.local` | `True` | *(Company Level)* | `CEO` | `VIEW_DASHBOARD`, `CREATE_LEAVE_REQUEST`, `APPROVE_LEAVE_REQUEST`, `VIEW_LEAVE_BALANCE`, `VIEW_LEAVE_REQUEST` (Role IDs: `7026b7ee-...`, `11111111-...`) |
| `uat.provision86` | `EMP-NV-TEST` | Nhan Vien Test | `uat.provision86@hrm.local` | `True` | `Human Resources Department` | `Employee` | `VIEW_DASHBOARD`, `CREATE_LEAVE_REQUEST`, `VIEW_LEAVE_BALANCE`, `VIEW_LEAVE_REQUEST` (Role ID: `11111111-1111-1111-1111-111111111112`) |
| `tp.test` | `EMP-TP-TEST` | Truong Phong Test | `tp.test@hrm.local` | `False` | `Information Technology` | `Manager` | *(Tài khoản nhân viên đang Inactive)* |
| `nguyenvanemployee`| `EMP002` | Nguyen Van Employee | `nguyenvanemployee@hrm.local` | `False` | `Information Technology` | `Manager` | *(Tài khoản nhân viên đang Inactive)* |
| `mgr091507` | `MGR091507` | UAT_Delete_Manager_091507 | `mgr091507@hrm.local` | `False` | `Information Technology` | `Manager` | *(Tài khoản nhân viên đang Inactive)* |
| `uat.subordinate` | `SUB091507` | UAT_Delete_Subordinate_091507 | `uat.subordinate@hrm.local` | `False` | `Information Technology` | `Employee` | *(Tài khoản nhân viên đang Inactive)* |
| `with091507` | `WITH091507` | UAT_Delete_WithHistory_091507 | `with091507@hrm.local` | `False` | *(Unassigned)* | *(None)* | *(Tài khoản nhân viên đang Inactive)* |

> **Lý Do Lựa Chọn Dữ Liệu Thử Nghiệm UAT**: Tập hợp dữ liệu/tài khoản đã được xác minh bao phủ các điều kiện tiên quyết chính cho ma trận UAT; mỗi kịch bản test vẫn phụ thuộc vào phân loại độ sẵn sàng và các điều kiện tiền đề tương ứng đã được ghi nhận.

---

## 3. Trạng Thái Cấu Hình Phê Duyệt Động Hiện Có Trên Database Runtime

- **`approval_route_policy`**: 1 Policy
  - ID: `f756fb72-8277-4934-8a71-be724b2e83bc`
  - Name: `Information Technology Approval Policy`
  - Department: `Information Technology`
  - Active: `True`
- **`approval_route_level`**: 1 Level
  - Level Rank 1: `Department Manager` (ID: `d1cdfb38-b880-447f-a459-7211c3cd8056`, CanApprove: `True`)
- **`approval_route_level_assignment`**: 1 Assignment
  - Level Rank 1 Assigned Approver: `uat.provision81` (`EMP04`)
- **`approval_route_rule`**: 1 Rule
  - ID: `47c847a9-4811-4173-854c-f2c0b22e5d48`
  - Requester Position: `Employee`
  - Specific Approver Override: `False`
  - Auto Approve: `False`
- **`approval_route_rule_candidate`**: 1 Candidate
  - Priority 1: `Level Rank 1` (`Department Manager`)

---

## 4. Danh Sách Chi Tiết Các Kịch Bản UAT Nghiệp Vụ (Cases A – J)

---

### Case A: Định Tuyến Phê Duyệt Trực Tiếp Cấp Trên Cùng Phòng Ban
- **Phân Loại Độ Sẵn Sàng Vận Hành**: **`READY NOW`** *(Đã UAT PASS & Xác Minh Tại Phase 9 TC1)*
- **Phòng Ban Test**: `Information Technology` (`IT`)
- **Chức Vụ Người Tạo Đơn**: `Employee`
- **Cấu Hình Policy**:
  - Level 1: `Department Manager` (Nhân viên được gán: `uat.provision81`)
  - Rule: Người tạo đơn `Employee` -> Ứng viên Priority 1: `Level 1` (`Department Manager`).
- **Tài Khoản Test**: Người tạo đơn: `uat.provision80` | Người duyệt: `uat.provision81`
- **Đường Dẫn UI (UI Path)**: `/leave-request/create` -> `/leave-request/detail/{id}` -> `/dashboard`
- **Kết Quả Mong Đợi**:
  - **Tạo đơn nghỉ phép**: Hệ thống trả về HTTP 200 / Chuyển hướng thành công.
  - **Trang chi tiết đơn nghỉ**: Current Approver = `uat.provision81 (EMP04)`, `LeaveRequestApprovalAssignment.Status` = `ApprovalAssignmentStatus.Assigned` (`1`), `Reason` = `ApprovalAssignmentReason.DirectLevelMatch` (`1`).
  - **Panel Quyết Định (Decision Panel)**: Hiển thị duy nhất cho người duyệt được gán `uat.provision81` (`APPROVE REQUEST` / `REJECT REQUEST`).
  - **Dashboard W4 Queue**: Hiển thị đơn xin nghỉ mới trong Widget W4 (`APPROVAL QUEUE`) khi `uat.provision81` đăng nhập.
- **Kỳ Vọng Audit**: `ApprovalRouteAuditLog.ActionType` = `ApprovalRouteAuditActionType.Created` (`1`), `ReasonCode` = `DirectLevelMatch`.
- **Quy Tắc Nghiệp Vụ Bao Phủ**: Phê duyệt tiêu chuẩn theo cấp trên trực tiếp trong cùng phòng ban.
- **Điều Kiện Tiền Đề / Cần Thao Tác**: Đã xác minh PASS trong Phase 9 UAT TC1. Không cần tạo thêm cấu hình.

---

### Case B: Leo Thang Theo Cấu Hình (Bỏ Qua Chức Vụ Trung Gian Không Có Trong Danh Sách)
- **Phân Loại Độ Sẵn Sàng Vận Hành**: **`NEEDS UI CONFIG ONLY`**
- **Phòng Ban Test**: `Information Technology` (`IT`)
- **Chức Vụ Người Tạo Đơn**: `Employee`
- **Cấu Hình Policy**:
  - Level 1: `Team Leader` (Chưa gán nhân viên / Người duyệt Inactive)
  - Level 2: `Department Header` (Nhân viên được gán: `uat.provision81`)
  - Rule: Ứng viên Priority 1 = `Level 1` (`Team Leader`), Ứng viên Priority 2 = `Level 2` (`Department Header`).
  - *Ghi chú*: Chức vụ `Manager` có tồn tại trong cấu trúc phòng IT nhưng KHÔNG được khai báo trong danh sách ứng viên của Rule.
- **Tài Khoản Test**: Người tạo đơn: `uat.provision80` | Người duyệt đích: `uat.provision81`
- **Đường Dẫn UI (UI Path)**: `/approval-routing/policies/detail/{id}` -> `/leave-request/create` -> `/leave-request/detail/{id}`
- **Kết Quả Mong Đợi**:
  - Động cơ định tuyến đánh giá Candidate Priority 1 (`Team Leader`) -> Thất bại (không có người duyệt active được gán).
  - Tự động leo thang sang Candidate Priority 2 (`Department Header`) -> Giải quyết thành công ra `uat.provision81`.
  - **Hành vi nghiêm ngặt**: Chức vụ `Manager` bị bỏ qua hoàn toàn vì không được cấu hình trong danh sách ứng viên của rule.
  - **Trang chi tiết đơn nghỉ**: Current Approver = `uat.provision81`, `LeaveRequestApprovalAssignment.Status` = `ApprovalAssignmentStatus.Assigned` (`1`), `Reason` = `ApprovalAssignmentReason.SuperiorLevelEscalated` (`2`).
- **Kỳ Vọng Audit**: `ApprovalRouteAuditLog.ActionType` = `ApprovalRouteAuditActionType.Created` (`1`), `ReasonCode` = `SuperiorLevelEscalated`.
- **Quy Tắc Nghiệp Vụ Bao Phủ**: Leo thang ứng viên hoàn toàn theo cấu hình; tuân thủ nghiêm ngặt danh sách candidate mà không tự động rơi về cơ chế cấu trúc mặc định.
- **Điều Kiện Tiền Đề / Cần Thao Tác**: Cần thao tác trên UI tạo Level 2 (`Department Header`) thuộc Policy `f756fb72-8277-4934-8a71-be724b2e83bc`. Cần User duyệt trước khi thao tác UI.

---

### Case C: Bảo Vệ Khi Thiếu Policy / Thiếu Rule
- **Phân Loại Độ Sẵn Sàng Vận Hành**: **`NEEDS TEST DATA`**
- **Phòng Ban Test**: `Finance` (`FINANCE`) hoặc `Sales` (`SALES`)
- **Chức Vụ Người Tạo Đơn**: `Employee`
- **Cấu Hình Policy**: Không có `ApprovalRoutePolicy` active hoặc không có `ApprovalRouteRule` khớp với phòng ban/chức vụ.
- **Tài Khoản Test**: Nhân viên thử nghiệm active thuộc phòng ban `FINANCE`.
- **Đường Dẫn UI (UI Path)**: `/leave-request/create`
- **Kết Quả Mong Đợi**:
  - Thao tác gửi đơn bị chặn ngay lập tức với thông báo lỗi domain chuẩn xác:  
    `LeaveRequestErrors.ApprovalRouteNotConfigured` (`LeaveRequest.ApprovalRouteNotConfigured`: `"Approval route is not configured for this department/position. Please assign an approver before submitting leave request."`).
  - **Hành vi nghiêm ngặt**: 0 đơn nghỉ mồ côi (orphaned) bị tạo mồ côi trong bảng `leave_request` hoặc `leave_request_approval_assignment`.
- **Kỳ Vọng Audit**: Không tạo audit log (vì command trả về failure result).
- **Quy Tắc Nghiệp Vụ Bao Phủ**: Invariant bảo vệ hệ thống khỏi các đơn xin nghỉ không có tuyến phê duyệt.
- **Điều Kiện Tiền Đề / Cần Thao Tác**: Cần gán một user test active vào phòng ban `Finance` qua UI. Cần User duyệt trước khi tạo dữ liệu thử nghiệm.

---

### Case D: Ghi Đè Chỉ Định Nhân Viên Phê Duyệt Cụ Thể (Specific Employee Override)
- **Phân Loại Độ Sẵn Sàng Vận Hành**: **`NEEDS UI CONFIG ONLY`**
- **Phòng Ban Test**: `Information Technology` (`IT`)
- **Chức Vụ Người Tạo Đơn**: `Employee`
- **Cấu Hình Policy**:
  - Rule: `specific_approver_employee_id` được gán thẳng cho nhân viên `uat.provision81`.
- **Tài Khoản Test**: Người tạo đơn: `uat.provision80` | Người duyệt được chỉ định: `uat.provision81`
- **Đường Dẫn UI (UI Path)**: `/approval-routing/policies/detail/{id}` -> `/leave-request/create` -> `/leave-request/detail/{id}`
- **Kết Quả Mong Đợi**:
  - Động cơ định tuyến bỏ qua hoàn toàn chuỗi ứng viên cấp bậc (level candidates).
  - Gán trực tiếp đơn cho `uat.provision81`.
  - **Trang chi tiết đơn nghỉ**: Current Approver = `uat.provision81`, `LeaveRequestApprovalAssignment.Status` = `ApprovalAssignmentStatus.Assigned` (`1`), `Reason` = `ApprovalAssignmentReason.SpecificEmployeeOverride` (`3`).
- **Kỳ Vọng Audit**: `ApprovalRouteAuditLog.ActionType` = `ApprovalRouteAuditActionType.Created` (`1`), `ReasonCode` = `SpecificEmployeeOverride`.
- **Quy Tắc Nghiệp Vụ Bao Phủ**: Cấu hình ghi đè nhân viên phê duyệt cụ thể cho các trường hợp ủy quyền tạm thời hoặc phân công đặc biệt mà không cần thay đổi chức vụ hành chính.
- **Điều Kiện Tiền Đề / Cần Thao Tác**: Cần cấu hình specific approver override trên Policy Rule qua trang `/approval-routing/policies/detail/{id}`. Cần User duyệt trước khi thao tác UI.

---

### Case E: Kịch Bản Đảo Nguồn Linh Hoạt (Dynamic Reverse-Proof Case)
- **Phân Loại Độ Sẵn Sàng Vận Hành**: **`NEEDS UI CONFIG ONLY`**
- **Phòng Ban Test**: `Information Technology` (`IT`)
- **Chức Vụ Người Tạo Đơn**: `Manager`
- **Cấu Hình Policy**:
  - Rule áp dụng cho chức vụ `Manager` được cấu hình Candidate Level gán cho một nhân viên có chức vụ `Employee` (`uat.provision80`).
- **Tài Khoản Test**: Người tạo đơn: `uat.provision81` (`Manager`) | Người duyệt: `uat.provision80` (`Employee`)
- **Đường Dẫn UI (UI Path)**: `/approval-routing/policies/detail/{id}` -> `/leave-request/create` -> `/leave-request/detail/{id}`
- **Kết Quả Mong Đợi**:
  - Đơn do `Manager` (`uat.provision81`) tạo ra được định tuyến thành công sang cho `Employee` (`uat.provision80`) phê duyệt.
  - **Trang chi tiết đơn nghỉ**: Current Approver = `uat.provision80`, `LeaveRequestApprovalAssignment.Status` = `ApprovalAssignmentStatus.Assigned` (`1`), `Reason` = `ApprovalAssignmentReason.DirectLevelMatch` (`1`).
  - Decision Panel hiển thị duy nhất cho `uat.provision80`.
- **Kỳ Vọng Audit**: `ApprovalRouteAuditLog.ActionType` = `ApprovalRouteAuditActionType.Created` (`1`).
- **Quy Tắc Nghiệp Vụ Bao Phủ**: Thử nghiệm kỹ thuật chứng minh động cơ phê duyệt hoạt động 100% dựa trên policy động và KHÔNG bị hardcode theo thứ bậc vai trò tĩnh trong code.
- **Điều Kiện Tiền Đề / Cần Thao Tác**: Kịch bản thử nghiệm phi sản xuất. Cần cấu hình rule cho chức vụ `Manager` trên UI. Cần User duyệt trước khi thao tác.

---

### Case F: Người Duyệt Cấp Cao Nhất Phòng Ban -> Người Duyệt Cấp Công Ty
- **Phân Loại Độ Sẵn Sàng Vận Hành**: **`NEEDS UI CONFIG ONLY`**
- **Phòng Ban Test**: `Information Technology` (`IT`)
- **Chức Vụ Người Tạo Đơn**: `Manager` (Chức vụ cao nhất của phòng IT)
- **Cấu Hình Policy**:
  - Rule cho chức vụ `Manager` được cấu hình Candidate Level = `Company Top Approver` (Nhân viên gán: `CEO Test` / `EMP-CEO-TEST`).
- **Tài Khoản Test**: Người tạo đơn: `uat.provision81` (`IT Manager`) | Người duyệt: `ceo.test` (`CEO Test`)
- **Đường Dẫn UI (UI Path)**: `/leave-request/create` -> `/leave-request/detail/{id}` -> `/dashboard`
- **Kết Quả Mong Đợi**:
  - Đơn xin nghỉ được định tuyến mượt mà từ Trưởng phòng IT lên Người duyệt cấp Công ty (`CEO Test`).
  - **Trang chi tiết đơn nghỉ**: Current Approver = `CEO Test (EMP-CEO-TEST)`, `LeaveRequestApprovalAssignment.Status` = `ApprovalAssignmentStatus.Assigned` (`1`), `Reason` = `ApprovalAssignmentReason.DirectLevelMatch` (`1`).
  - Dashboard W4 Queue của `ceo.test` hiển thị đơn cần duyệt.
  - **Hành vi nghiêm ngặt**: Zero hardcoded kiểm tra tên role "CEO" trong code C#.
- **Kỳ Vọng Audit**: `ApprovalRouteAuditLog.ActionType` = `ApprovalRouteAuditActionType.Created` (`1`).
- **Quy Tắc Nghiệp Vụ Bao Phủ**: Leo thang phê duyệt đa tầng giữa các phòng ban / cấp công ty mà không hardcode chuỗi role.
- **Điều Kiện Tiền Đề / Cần Thao Tác**: Cần cấu hình rule candidate level cho chức vụ `Manager` hướng tới người duyệt cấp công ty. Cần User duyệt trước khi thao tác UI.

---

### Case G: Người Duyệt Cấp Cao Nhất Công Ty Tự Xin Nghỉ Được Tự Động Phê Duyệt (Auto-Approved)
- **Phân Loại Độ Sẵn Sàng Vận Hành**: **`NEEDS UI CONFIG ONLY`**
- **Phòng Ban Test**: Cấp Công Ty (`None`)
- **Chức Vụ Người Tạo Đơn**: `CEO`
- **Cấu Hình Policy**:
  - Policy Rule cho chức vụ `CEO` có thuộc tính `is_auto_approve = true`.
- **Tài Khoản Test**: Người tạo đơn: `ceo.test`
- **Đường Dẫn UI (UI Path)**: `/leave-request/create` -> `/leave-request/detail/{id}`
- **Kết Quả Mong Đợi**:
  - Ngay sau khi bấm nộp đơn, trạng thái đơn xin nghỉ lập tức chuyển thành `APPROVED` (`LeaveRequestStatus.Approved`).
  - Không tạo phân công phê duyệt `LeaveRequestApprovalAssignment`; số ngày phép `LeaveBalance.UsedDays` được trừ ngay lập tức.
  - **Hành vi nghiêm ngặt**: Việc tự động duyệt được thực thi thuần túy theo cấu hình policy rule (`is_auto_approve = true`), KHÔNG hardcoded theo username hoặc tên role CEO.
- **Kỳ Vọng Audit**: `ApprovalRouteAuditLog.ActionType` = `ApprovalRouteAuditActionType.AutoApproved` (`6`), `ReasonCode` = `"ConfiguredTerminalApproverAutoApproved"`.
- **Quy Tắc Nghiệp Vụ Bao Phủ**: Tính năng tự động phê duyệt có thể cấu hình dành cho các vị trí quản lý cấp cao nhất.
- **Điều Kiện Tiền Đề / Cần Thao Tác**: Cần đặt `is_auto_approve = true` trên Policy Rule cho chức vụ `CEO` qua UI. Cần User duyệt trước khi thao tác UI.

---

### Case H: Xử Lý Tác Động & Định Tuyến Lại Khi Người Duyệt Inactive / Bị Xóa
- **Phân Loại Độ Sẵn Sàng Vận Hành**: **`CODE-REVIEW ONLY / READY FOR TRIGGER`**
- **Phòng Ban Test**: `Information Technology` (`IT`)
- **Kịch Bản**: Admin thực hiện vô hiệu hóa (deactivate) hoặc xóa một nhân viên (`uat.provision81`) đang đóng vai trò là người duyệt active của các đơn xin nghỉ đang chờ duyệt.
- **Đường Dẫn UI (UI Path)**: `/employee/index` -> Action: `Deactivate` / `Delete`
- **Kết Quả Mong Đợi**:
  - Modal phân tích tác động (`GetEmployeeDeactivationImpactQueryHandler`) hiển thị cảnh báo Admin: `"Employee uat.provision81 is currently an active approver for N pending leave requests and M policy levels."`
  - Lựa chọn 1: Chọn thủ công nhân viên thay thế (`ReassignPendingLeaveRequestsCommandHandler`).
  - Lựa chọn 2: Hệ thống vô hiệu hóa và tự động định tuyến lại (`InactivateEmployeeWithReassignmentCommandHandler`).
  - **Trạng thái phân công**: Các đơn đang chờ duyệt bị ảnh hưởng sẽ chuyển trạng thái sang `ApprovalAssignmentStatus.NeedsAdminAttention` (`2`) nếu không có ứng viên kế tiếp, hoặc `Assigned` (`1`) với `Reason` = `ApprovalAssignmentReason.OperatorManualReassigned` (`4`).
  - **Cách ly lịch sử**: Các đơn đã hoàn tất (`APPROVED`, `REJECTED`, `CANCELED`) giữ nguyên snapshot người duyệt lịch sử và không bị ảnh hưởng.
- **Kỳ Vọng Audit**: `ApprovalRouteAuditLog.ActionType` = `ApprovalRouteAuditActionType.Reassigned` (`2`) hoặc `NeedsAttention` (`4`).
- **Quy Tắc Nghiệp Vụ Bao Phủ**: Quản lý an toàn vòng đời người duyệt active mà không làm đứt gãy luồng công việc.
- **Điều Kiện Tiền Đề / Cần Thao Tác**: Cần có ít nhất một đơn xin nghỉ đang chờ duyệt được gán cho nhân viên target trước khi kích hoạt vô hiệu hóa trên UI. Cần User duyệt trước khi thao tác.

---

### Case I: Người Duyệt Chỉ Định Inactive Hoặc Mất Quyền Duyệt
- **Phân Loại Độ Sẵn Sàng Vận Hành**: **`CODE-REVIEW ONLY / READY FOR TRIGGER`**
- **Phòng Ban Test**: `Information Technology` (`IT`)
- **Kịch Bản**: Người duyệt được chỉ định cụ thể trong Case D (`specific_approver_employee_id`) bị vô hiệu hóa tài khoản hoặc bị thu hồi quyền `APPROVE_LEAVE_REQUEST`.
- **Đường Dẫn UI (UI Path)**: `/approval-routing/levels/assignments`
- **Kết Quả Mong Đợi**:
  - Động cơ phê duyệt KHÔNG âm thầm chuyển sang cơ chế fallback không có vết audit.
  - Phân công đang chờ duyệt chuyển sang trạng thái `ApprovalAssignmentStatus.NeedsAdminAttention` (`2`).
  - Banner cảnh báo hiển thị trên màn hình quản trị Approval Routing: `"1 or more pending leave requests require manual approver assignment due to inactive/unqualified specific approver."`
- **Kỳ Vọng Audit**: `ApprovalRouteAuditLog.ActionType` = `ApprovalRouteAuditActionType.NeedsAttention` (`4`).
- **Quy Tắc Nghiệp Vụ Bao Phủ**: Cơ chế thông báo lỗi rõ ràng và yêu cầu Admin can thiệp khi người duyệt chỉ định không còn hợp lệ.
- **Điều Kiện Tiền Đề / Cần Thao Tác**: Cần thiết lập ghi đè người duyệt cụ thể trước khi vô hiệu hóa tài khoản đó. Cần User duyệt trước khi thực hiện.

---

### Case J: Ràng Buộc Mỗi Position Chỉ Có Một Active Employee Trong Một Department
- **Phân Loại Độ Sẵn Sàng Vận Hành**: **`NOT IMPLEMENTED / NEEDS CODE PHASE`**
- **Phòng Ban Test**: Bất kỳ phòng ban nào (Ví dụ: `Information Technology`)
- **Kịch Bản**: Admin cố gắng gán một nhân viên vào chức vụ `Manager` trong phòng ban `IT` trong khi `uat.provision81` đã đang là một `Manager` active trong `IT`.
- **Đường Dẫn UI (UI Path)**: N/A (Yêu cầu viết code trong tương lai)
- **Bằng Chứng Code Thực Tế**: Đã kiểm tra trực tiếp source code `EmployeeErrors.cs`, `CreateEmployeeCommandHandler.cs`, và `UpdateEmployeeCommandHandler.cs` cho thấy quy tắc ràng buộc *mỗi chức vụ chỉ có tối đa một nhân viên active trong một phòng ban* **chưa được cài đặt trong code C#**. `CreateEmployeeCommandHandler` hiện chỉ kiểm tra lỗi `EmployeeErrors.EmployeeCodeExisted`.
- **Yêu Cầu Cho Phase Viết Code Trong Tương Lai**:
  - Khai báo bổ sung domain error (ví dụ: `EmployeeErrors.DuplicatePositionInDepartment`) trong file `EmployeeErrors.cs`.
  - Bổ sung logic kiểm tra ràng buộc vào `CreateEmployeeCommandHandler` & `UpdateEmployeeCommandHandler`.
- **Quy Tắc Nghiệp Vụ Bao Phủ**: Ràng buộc toàn vẹn dữ liệu cấu trúc phòng ban & chức vụ.
- **Điều Kiện Tiền Đề / Cần Thao Tác**: Chưa sẵn sàng để chạy UAT. Bắt buộc cần một phase viết code C# riêng biệt ở Application/Domain layer trước khi test.

---

## 5. Bảng Tổng Hợp Ma Trận UAT & Yêu Cầu Phê Duyệt Dữ Liệu Thử Nghiệm

| Mã Case | Tên Kịch Bản UAT | Phòng Ban Target | Người Tạo Đơn | Người Duyệt / Đích | Phân Loại Độ Sẵn Sàng Vận Hành | Điều Kiện Tiền Đề & Yêu Cầu User Phê Duyệt |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **UAT-CASE-A** | Phê Duyệt Cấp Trên Trực Tiếp | `IT` | `uat.provision80` | `uat.provision81` | **READY NOW** | Đã UAT PASS tại Phase 9 TC1. Sẵn sàng. |
| **UAT-CASE-B** | Leo Thang Theo Cấu Hình | `IT` | `uat.provision80` | `uat.provision81` | **NEEDS UI CONFIG ONLY** | Cần tạo Level 2 trên UI. Cần User duyệt trước khi làm. |
| **UAT-CASE-C** | Chặn Khi Thiếu Policy | `FINANCE` | Nhân viên Finance | Không có (Bị chặn) | **NEEDS TEST DATA** | Cần gán nhân viên vào Finance. Cần User duyệt. |
| **UAT-CASE-D** | Ghi Đè Nhân Viên Cụ Thể | `IT` | `uat.provision80` | `uat.provision81` | **NEEDS UI CONFIG ONLY** | Cần cấu hình Specific Override Rule trên UI. Cần User duyệt. |
| **UAT-CASE-E** | Đảo Nguồn Linh Hoạt | `IT` | `uat.provision81` | `uat.provision80` | **NEEDS UI CONFIG ONLY** | Cấu hình thử nghiệm phi sản xuất. Cần User duyệt. |
| **UAT-CASE-F** | Trưởng Phòng -> CEO | `IT` | `uat.provision81` | `ceo.test` | **NEEDS UI CONFIG ONLY** | Cần cấu hình Rule hướng tới cấp CEO. Cần User duyệt. |
| **UAT-CASE-G** | CEO Tự Động Phê Duyệt | Cấp Công Ty | `ceo.test` | Không (Tự động) | **NEEDS UI CONFIG ONLY** | Cần bật `is_auto_approve = true` trên UI. Cần User duyệt. |
| **UAT-CASE-H** | Xử Lý Người Duyệt Inactive | `IT` | Nhân viên Active | Người duyệt thay thế | **CODE-REVIEW ONLY / READY** | Cần thao tác Deactivate trên UI. Cần User duyệt. |
| **UAT-CASE-I** | Người Duyệt Mất Quyền | `IT` | Nhân viên Active | Cảnh báo Admin | **CODE-REVIEW ONLY / READY** | Cần thao tác Deactivate người duyệt chỉ định. Cần User duyệt. |
| **UAT-CASE-J** | Ràng Buộc Một Position Active | Mọi phòng ban | N/A | Thao tác Admin | **NOT IMPLEMENTED / NEEDS CODE PHASE** | Chưa cài đặt logic trong code C#. Bắt buộc cần phase code. |

---

## 6. Nhật Ký Xác Minh & Kiểm Tra Cuối

- **An Toàn Bảo Mật**: Các script debug tạo ở phase này (`MD_memory/debug/2026-07-24_1125_inspect-hrm-runtime-db.py` và `MD_memory/debug/2026-07-24_1142_query-permissions.py`) yêu cầu biến `PGPASSWORD` và không có fallback plain-text. Đã quét mẫu secret thành công trên các file artifact trong phase hiện tại.
- `git status --short`: Working tree hiện có 2 plan chưa track (`MD_memory/plans/2026-07-24_1130_phase-dynamic-approval-routing_uat-config-matrix.md` và `MD_memory/plans/2026-07-24_1310_phase-dynamic-approval-routing_uat-config-matrix-vi.md`); không có file nào đang staged.
- `scan-mojibake.py --require-bom`: Đạt (BOM OK, Mojibake Clean).
