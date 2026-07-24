# Phase 0/1 Report — Plan Reconciliation & Domain Model Development (Strict Position-Rule — No Fallback)



> **File Location**: `MD_memory/reports/2026-07-22_1620_phase-dynamic-approval-routing_phase-0-1_domain_report.md`

> **Phase**: `phase-dynamic-approval-routing` (Phase 0 & Phase 1 Strict Position Lock)

> **Date**: 2026-07-22

> **Status**: ✅ COMPLETED & STRICT POSITION RULE LOCKED (0 Build Errors)

> **Source of Truth**: `MD_memory/plans/2026-07-22_1600_phase-dynamic-approval-routing-design_v5_proposal.md`

> **Implementation Plan**: `MD_memory/plans/2026-07-22_1615_phase-dynamic-approval-routing_implementation-plan.md`

> **Architecture Boundary Preserved**:

> - `Web.Backend -> Application -> Domain`

> - `Infrastructure -> Application/Domain`



---



## 1. Phase 0 — Plan Reconciliation & Source of Truth Audit



Xác nhận chính thức:

- **V5 Proposal** (`MD_memory/plans/2026-07-22_1600_phase-dynamic-approval-routing-design_v5_proposal.md`) và **Implementation Plan** (`MD_memory/plans/2026-07-22_1615_phase-dynamic-approval-routing_implementation-plan.md`) là **SOURCE OF TRUTH DUY NHẤT**.

- Các đề xuất cũ V1/V2/V3/V4 mâu thuẫn đã chính thức bị **DEPRECATED**.



### Tóm Tắt Nguyên Tắc Bắt Buộc Theo Phase 0:

1. **Single-Step Dynamic Superior Routing**: Mỗi đơn nghỉ phép `Pending` chỉ có duy nhất 1 `AssignedApproverEmployeeId` hiện hành. Không duyệt tuần tự nhiều bước.

2. **Department-Specific Policy Only**: Mỗi phòng ban có Policy riêng. Không dùng Global Default Policy làm runtime fallback. Nếu phòng chưa có Policy active -> Chặn nộp đơn.

3. **No Operator Proxy Approval**: Admin/HR không được duyệt thay đơn của nhân viên khác. Admin/HR chỉ cấu hình routing và xử lý luân chuyển khi mất người duyệt.

4. **Strict Config-Driven Candidates**: Routing leo cấp thuần theo `ApprovalRouteRuleCandidate.PriorityOrder`. Không tự suy luận cấp bậc ngoài cấu hình.

5. **Clean Domain State**: `LeaveRequest.Status = Pending` được giữ nguyên sạch sẻ. Trạng thái routing được quản lý độc lập tại `LeaveRequestApprovalAssignment.AssignmentStatus`.



---



## 2. Dynamic Approval Routing Domain Lock — Strict Position Rules (NO FALLBACK)



Theo quyết định nghiệp vụ mới nhất từ User: **TUYỆT ĐỐI KHÔNG DÙNG DEFAULT / FALLBACK RULE TRONG SYSTEM**.



### 2.1. Invariant 1: Bắt Buộc `RequesterPositionId != null` (Khóa Trực Tiếp Trên Domain)

- **Tình huống cấm**: Không cho phép tạo Rule với `RequesterPositionId == null`.

- **Khóa trong code**:

  - `ApprovalRouteRule.Create(...)` và `ApprovalRoutePolicy.AddRule(...)` kiểm tra `if (requesterPositionId == null)` và ném `ArgumentNullException`:

    `"RequesterPositionId is required for approval route rule. Default/fallback rules are not supported."`

  - Đảm bảo thuộc tính `ApprovalRouteRule.RequesterPositionId` mang kiểu non-nullable `PositionId`.

  - Mỗi active rule trong Policy bắt buộc phải gắn duy nhất với một `RequesterPositionId` cụ thể (`_rules.Any(r => r.IsActive && r.RequesterPositionId == requesterPositionId)`).



### 2.2. Xử Lý Khi Vị Trí Nhân Viên Chưa Được Cấu Hình Rule

- Nếu một Nhân viên thuộc một Vị trí (Position) chưa được cấu hình Rule riêng trong Policy của Phòng ban:

  - **Không tự động fallback**: Hệ thống sẽ **KHÔNG** tự đoán hay route cho bất kỳ ai.

  - **Chặn tại Application Phase (Resolver)**: Khi nộp đơn, Handler sẽ chặn và trả về thông báo lỗi nghiệp vụ rõ ràng:

    > `Approval route is not configured for this department/position. Please assign an approver before submitting leave request.`



### 2.3. Invariant 2: Candidate Level Belonging To Same Policy

- Candidate Level bắt buộc phải là một Level hợp lệ thuộc cùng Policy.

- **Khóa trong code**: Khởi tạo candidate thông qua method `ApprovalRoutePolicy.AddRuleCandidate(ruleId, levelId, priorityOrder)`. Policy sẽ trực tiếp xác minh `_levels.FirstOrDefault(l => l.Id == levelId)` trước khi tạo candidate. Nếu level ID thuộc policy khác, quăng `InvalidOperationException`.

- Đồng thời ở Phase 2, EF Core Mapping & Application Validation sẽ tiếp tục bổ sung FK / Unique Index composite constraint.



### 2.4. Invariant 3: Helper Method Overlap Enforcement Wording

- Helper method `ApprovalRouteLevelAssignment.OverlapsWith(...)` hiện nằm trong Domain để phục vụ công cụ kiểm tra overlap.

- Việc cưỡng chế Invariant (Enforcement) sẽ được thực hiện tại **Application Handler (Phase 5)** khi tạo/sửa assignment và **Database EXCLUDE/Index Strategy (Phase 2)**. Domain không tự nhận là đã enforce 100% DB constraint ở Phase 1.



### 2.5. Rút Gọn `ApprovalAssignmentStatus` & Luồng `Reassign`

- Trạng thái assignment hiện tại chỉ gồm 2 state rõ ràng: `Assigned = 1`, `NeedsAdminAttention = 2`.

- Khi gọi `Reassign(...)`, `AssignmentStatus` duy trì là `Assigned`, đồng thời cập nhật `AssignmentReason = OperatorManualReassigned` và ghi log `ApprovalRouteAuditLog` với `ActionType = Reassigned`.

- **Lợi ích**: Dashboard W4/W5 query cực đơn giản: `WHERE AssignmentStatus == ApprovalAssignmentStatus.Assigned`.



### 2.6. Trích Xuất FK `LeaveRequestApprovalAssignmentId` Trong Audit Log

- `ApprovalRouteAuditLog` có thuộc tính `LeaveRequestApprovalAssignmentId` hỗ trợ trace vết re-route.



### 2.7. Enum Neutral Name (`OperatorManualReassigned`)

- Đã đổi tên `AdminManualReassigned` -> `OperatorManualReassigned` để loại bỏ role semantics khỏi Domain.



---



## 3. Danh Sách Files Trong `Domain/ApprovalRouting/`



1. **Enums**:

   - `ApprovalAssignmentStatus.cs` (`Assigned`, `NeedsAdminAttention`)

   - `ApprovalAssignmentReason.cs` (`DirectLevelMatch`, `SuperiorLevelEscalated`, `SpecificEmployeeOverride`, `OperatorManualReassigned`)

   - `ApprovalRouteAuditActionType.cs` (`Created`, `Reassigned`, `Escalated`, `NeedsAttention`, `OverrideApplied`)

2. **Typed IDs (`ApprovalRoutingIds.cs`)**:

   - `ApprovalRoutePolicyId`, `ApprovalRouteLevelId`, `ApprovalRouteLevelAssignmentId`, `ApprovalRouteRuleId`, `ApprovalRouteRuleCandidateId`, `LeaveRequestApprovalAssignmentId`, `ApprovalRouteAuditLogId`.

3. **Domain Entities**:

   - `ApprovalRoutePolicy.cs` (Aggregate Root)

   - `ApprovalRouteLevel.cs`

   - `ApprovalRouteLevelAssignment.cs`

   - `ApprovalRouteRule.cs` (Non-nullable `RequesterPositionId`)

   - `ApprovalRouteRuleCandidate.cs`

   - `LeaveRequestApprovalAssignment.cs`

   - `ApprovalRouteAuditLog.cs`

4. **Repository Interfaces**:

   - `IApprovalRoutePolicyRepository.cs`

   - `ILeaveRequestApprovalAssignmentRepository.cs`



---



## 4. GitNexus Impact & Verification Summary



```bash

# 1. Scope Diff Check (Chỉ riêng module ApprovalRouting)

git diff --check -- HRM_Leave_Management/Domain/ApprovalRouting

# Output: 0 whitespace / syntax errors.



# 2. Build Check

dotnet build HRM_Leave_Management/Web.Backend/Web.Backend.csproj --no-restore

# Output: Build succeeded. 0 Errors.

```



- **Mojibake Check**: `python MD_memory/debug/2026-06-26_1430_scan-mojibake.py MD_memory/reports/2026-07-22_1620_phase-dynamic-approval-routing_phase-0-1_domain_report.md --require-bom` -> ✅ **PASS (BOM OK, 0 failures, 0 mojibake)**.

- **Scope Diff Status**: Clean cho module Domain/ApprovalRouting.
