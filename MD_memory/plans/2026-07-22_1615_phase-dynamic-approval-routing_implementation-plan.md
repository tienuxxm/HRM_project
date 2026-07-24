# Implementation Plan — Micro-Phased Roadmap: Single-Step Dynamic Superior Approval Routing Engine



> **File Location**: `MD_memory/plans/2026-07-22_1615_phase-dynamic-approval-routing_implementation-plan.md`

> **Phase**: `phase-dynamic-approval-routing`

> **Date**: 2026-07-22

> **Status**: 📋 IMPLEMENTATION PLAN (Kế hoạch triển khai Micro-Phase — KHÔNG CODE SOURCE / KHÔNG DB MIGRATION TRONG PHASE NÀY)

> **Source of Truth Proposal**: `MD_memory/plans/2026-07-22_1600_phase-dynamic-approval-routing-design_v5_proposal.md`

> **Architecture Boundary Preserved**:

> - `Web.Backend -> Application -> Domain`

> - `Infrastructure -> Application/Domain`



---



## Executive Summary & Source of Truth Confirmation



Tài liệu này định nghĩa lộ trình thực thi theo từng Micro-Phase (từ Phase 0 đến Phase 9) cho tính năng **Single-Step Dynamic Superior Approval Routing Engine**.



Tất cả các thành viên phát triển và agent **bắt buộc tuân thủ V5 Proposal** (`MD_memory/plans/2026-07-22_1600_phase-dynamic-approval-routing-design_v5_proposal.md`) làm **DUY NHẤT một Source of Truth**.



---



## Phase 0 — Plan Reconciliation & Deprecation List



### 0.1. Đối Chiếu Khái Niệm Đã Deprecated / Bị Bỏ Khỏi V5 Source of Truth

- ❌ **Đã Bỏ Multi-step Sequential Approval**: Bỏ hoàn toàn khái niệm duyệt qua nhiều bước tuần tự (`ApprovalStep`, `ApprovalStepInstance`, workflow Leader -> Manager -> Header).

- ❌ **Đã Bỏ Global Runtime Default Fallback**: Bỏ tự động fallback về Global Policy khi phòng ban chưa cấu hình Policy riêng. Nếu Department chưa có active Policy -> Chặn tạo đơn nghỉ phép.

- ❌ **Đã Bỏ Admin Proxy Approval**: Bỏ tư duy cho phép Admin/HR duyệt thay đơn của nhân viên khác. Admin/HR chỉ thực hiện cấu hình routing / gán lại người duyệt.

- ❌ **Đã Bỏ Single Mutable Employee ID On Level**: Bỏ việc lưu `AssignedEmployeeId` trực tiếp trên `ApprovalRouteLevel` mà không lưu vết. Thay bằng `ApprovalRouteLevelAssignment` để audit lịch sử.

- ❌ **Đã Bỏ Auto-Deduction of Position Hierarchy**: Bỏ việc tự suy luận cấp bậc ngoài config. Routing chỉ leo cấp theo đúng `ApprovalRouteRuleCandidate.PriorityOrder`.

- ❌ **Đã Bỏ Vận Hành Trên `LeaveApproverAssignment`**: Màn hình `/leave-approver-assignment` cũ chính thức **DEPRECATED**. Xây dựng module UI mới `/approval-routing`.



---



## Phase 1 — Domain Model



### 1.1. Các Domain Entities & Value Objects Cần Tạo

- **Entities mới**:

  - `Domain/ApprovalRoutePolicies/ApprovalRoutePolicy.cs` (Aggregate Root)

  - `Domain/ApprovalRoutePolicies/ApprovalRouteLevel.cs`

  - `Domain/ApprovalRoutePolicies/ApprovalRouteLevelAssignment.cs` (Audit khung thời gian gán level)

  - `Domain/ApprovalRoutePolicies/ApprovalRouteRule.cs`

  - `Domain/ApprovalRoutePolicies/ApprovalRouteRuleCandidate.cs` (Child table candidate)

  - `Domain/LeaveRequestApprovalAssignments/LeaveRequestApprovalAssignment.cs` (1:1 với LeaveRequest)

  - `Domain/ApprovalRouteAudits/ApprovalRouteAuditLog.cs` (Lịch sử audit luân chuyển)



- **Enums mới**:

  - `Domain/LeaveRequestApprovalAssignments/AssignmentStatus.cs` (`Assigned`, `NeedsAdminAttention`, `Reassigned`)

  - `Domain/LeaveRequestApprovalAssignments/AssignmentReason.cs` (`DirectLevelMatch`, `SuperiorLevelEscalated`, `SpecificEmployeeOverride`, `AdminManualReassigned`)



### 1.2. Các Ràng Buộc Domain Invariants

1. Một `LeaveRequest` ở trạng thái `Pending` chỉ có **duy nhất 1 `AssignedApproverEmployeeId`**.

2. Một `ApprovalRouteLevelId` trong một Department **không được có 2 Active Assignment bị chồng lấn khung thời gian (`EffectiveFrom` -> `EffectiveTo`)**.

3. Ưu tiên Specific Approver (`SpecificApproverEmployeeId`). Nếu Specific Approver inactive/mất quyền -> Quét Candidate Levels theo `PriorityOrder`.

4. Cấm tự duyệt đơn của chính mình (`ApproverEmployeeId != RequesterEmployeeId`).



### 1.3. GitNexus Impact Targets Cần Phân Tích Trước Khi Code Phase 1

- `LeaveRequest`

- `LeaveRequestStatus`

- `Employee`

- `Department`



---



## Phase 2 — Infrastructure & EF Core Mapping



### 2.1. DbSets & Configurations Cần Thêm Trong `Infrastructure`

- Thêm các `DbSet<T>` tương ứng vào `ApplicationDbContext.cs`.

- Tạo các class Configuration trong `Infrastructure/Data/Configurations/`:

  - `ApprovalRoutePolicyConfiguration.cs` (Unique Index: `DepartmentId` cho Policy active)

  - `ApprovalRouteLevelConfiguration.cs`

  - `ApprovalRouteLevelAssignmentConfiguration.cs` (Index: `ApprovalRouteLevelId` + `IsActive`)

  - `ApprovalRouteRuleConfiguration.cs`

  - `ApprovalRouteRuleCandidateConfiguration.cs` (Unique Index: `ApprovalRouteRuleId` + `PriorityOrder`)

  - `LeaveRequestApprovalAssignmentConfiguration.cs` (Unique Index: `LeaveRequestId`, Index: `AssignedApproverEmployeeId` + `AssignmentStatus`)

  - `ApprovalRouteAuditLogConfiguration.cs`



### 2.2. Migration & Strategy

- **Tên Migration dự kiến**: `AddSingleStepDynamicApprovalRoutingEngine`

- **Quy tắc**: Không chạy migration tự động convert dữ liệu cũ ngây thơ. `LeaveApproverAssignment` cũ được giữ nguyên dạng read-only.



---



## Phase 3 — Application Resolver & Create Leave Request Validation



### 3.1. Các Components Cần Xây Dựng Trong `Application`

- **Service**: `Application/Abstractions/ApprovalRouting/IApprovalRouteResolverService.cs`

- **Implementation**: `Infrastructure/Services/ApprovalRouteResolverService.cs`

- **Tích hợp vào `CreateLeaveRequestCommandHandler.cs`**:

  1. Gọi `ApprovalRouteResolverService.ResolveApproverAsync(employeeId, positionId, departmentId)`.

  2. Nếu trả về `Failure` -> CHẶN TẠO ĐƠN và ném lỗi:

     `"Approval route is not configured for this department. Please assign an approver before submitting leave request."`

  3. Nếu trả về `Success` -> Tạo `LeaveRequest` (Status: Pending) + Tạo `LeaveRequestApprovalAssignment` (snapshot metadata) + Ghi `ApprovalRouteAuditLog`.



### 3.2. GitNexus Impact Targets

- `CreateLeaveRequestCommandHandler`

- `ILeaveRequestRepository`



---



## Phase 4 — Lifecycle Impact Preview & Reassignment Commands



### 4.1. Application Commands & Queries

- Query Preview Impact: `GetEmployeeDeactivationImpactQuery` (Đếm số đơn Pending bị ảnh hưởng + Danh sách Level Slot bị trống + Đề xuất Candidates thay thế).

- Command Reassignment:

  - `InactivateEmployeeWithReassignmentCommand`

  - `UnassignApprovalLevelCommand`

- Quy tắc: Đơn đã `Approved`, `Rejected`, `Canceled` được GIỮ NGUYÊN 100%. Đơn `Pending` được luân chuyển theo lựa chọn của Admin/HR.



---



## Phase 5 — Approval Routing UI Module (`/approval-routing`)



### 5.1. Các Route & Controller Views Trực Thuộc `Web.Backend`

- Menu chính: **Approval Routing** (`/approval-routing`)

- Các Màn hình:

  1. `Views/ApprovalRouting/Index.cshtml` (Danh sách Policy theo Department)

  2. `Views/ApprovalRouting/Detail.cshtml` (Cấu hình Level, Candidates, Specific Approver)

  3. `Views/ApprovalRouting/LevelAssignments.cshtml` (Gán Nhân viên giữ Slot duyệt kèm chống overlap date)

  4. `Views/ApprovalRouting/_ImpactModal.cshtml` (Modal hiển thị tác động & lựa chọn Reassignment khi inactive Approver)

  5. `Views/ApprovalRouting/MigrationTool.cshtml` (Công cụ hỗ trợ chuyển đổi từ cấu hình cũ)

- **Chuẩn Style**: Swiss HR UI (Tone đen/trắng/xám, đỏ `#E62429` chỉ cho hành động destructive, không alert/confirm native).



---



## Phase 6 — Leave Request Detail / List Integration



### 6.1. Tích Hợp UI Trên Màn `/leaverequest`

- Cột **Approver**: Hiển thị tên & mã nhân viên của `AssignedApproverEmployeeId` hiện hành.

- Badge cảnh báo: Nếu `AssignmentStatus == NeedsAdminAttention`, hiển thị badge `NEEDS ROUTING ATTENTION` (Monochrome outline/Red tag).

- Đơn đã Approved/Rejected/Canceled hiển thị người xử lý thực tế (`ProcessedBy`).



---



## Phase 7 — Dashboard W4/W5 Integration



### 7.1. Cập Nhật Handlers Dashboard W4/W5

- Sửa `GetPendingApprovalsQueryHandler.cs` (W4) & `GetApprovalAgingQueryHandler.cs` (W5):

  - **Scoped Approver View**: Filter `AssignedApproverEmployeeId == currentEmployee.Id` AND `AssignmentStatus == Assigned` AND `LeaveRequest.Status == Pending`.

  - **Admin / HR Global View**: Tách làm 2 Queue: Operational Queue (Thông tin) và Needs Attention Queue (Cần gán lại người duyệt).

  - Khai báo Permission capability `VIEW_APPROVAL_ROUTING` và `UPDATE_APPROVAL_ROUTING` (không seed DB).



---



## Phase 8 — Migration & Deprecation Strategy for `LeaveApproverAssignment`



1. **Giai đoạn 1**: Đánh dấu `/leave-approver-assignment` là Legacy/Read-Only. Hiển thị banner khuyến cáo chuyển sang `/approval-routing`.

2. **Giai đoạn 2**: Cung cấp công cụ dry-run mapping tool cho HR/Admin preview và xác nhận policy mới cho từng phòng ban.

3. **Giai đoạn 3**: Deprecate hoàn toàn menu cũ sau khi UAT phase mới PASS.



---



## Phase 9 — Verification & UAT Strategy



### 9.1. Lệnh Verification Kỹ Thuật

- Build check: `dotnet build HRM_Leave_Management/Web.Backend/Web.Backend.csproj --no-restore`

- GitNexus diff check: `detect_changes({scope: "compare", base_ref: "main"})`

- Mojibake check: `python MD_memory/debug/2026-06-26_1430_scan-mojibake.py <report-path> --require-bom`



### 9.2. Ma Trận Kịch Bản UAT Browser (Manual Step-by-Step)

- **TC1 (IT Chain)**: Employee IT nộp đơn -> Leader IT duyệt. Leader IT inactive -> Route sang Header IT (bỏ qua Manager).

- **TC2 (HRM Chain)**: Employee HRM nộp đơn -> Route trực tiếp Header HRM.

- **TC3 (Thiếu Policy)**: Department chưa tạo Policy -> Chặn nộp đơn và ném lỗi rõ.

- **TC4 (Specific Approver)**: Gán specific approver -> Specific approver duyệt.

- **TC5 (Inactive Approver Modal)**: Inactive Leader IT -> Pop up Impact Modal yêu cầu chọn phương án xử lý đơn Pending.



### 9.3. Đường Dẫn Lưu Evidence

- Evidence screenshot/log: `MD_memory/evidence/2026-07-22_phase-dynamic-approval-routing/`

- Báo cáo UAT tổng hợp: `MD_memory/reports/2026-07-22_HHMM_phase-dynamic-approval-routing_report.md`



---



## Target GitNexus Impact Symbols (Dự Kiến Chạy Trước Khi Code)



1. `LeaveRequest` (Domain Entity)

2. `CreateLeaveRequestCommandHandler` (Application Handler)

3. `GetPendingApprovalsQueryHandler` (Dashboard W4 Handler)

4. `GetApprovalAgingQueryHandler` (Dashboard W5 Handler)

5. `ApproveLeaveRequestCommandHandler` (Approve Handler)

6. `EmployeeRepository` (Infrastructure Repository)



---



## Open Questions For Blocking Alignment



> [!NOTE]

> Không còn câu hỏi blocking nào. Tất cả 10 quy tắc nghiệp vụ đã được làm rõ và thống nhất 100% trong V5 Proposal & Implementation Plan này. Hệ thống đã sẵn sàng cho bước lập lịch code từng Micro-Phase.
