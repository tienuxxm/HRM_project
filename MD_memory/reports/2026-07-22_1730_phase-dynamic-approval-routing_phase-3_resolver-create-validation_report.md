# Phase 3 Report — Application Resolver & Create Leave Validation (Specific Approver Option A Enforced & Untracked Scope Audited)



> **File Location**: `MD_memory/reports/2026-07-22_1730_phase-dynamic-approval-routing_phase-3_resolver-create-validation_report.md`

> **Phase**: `phase-dynamic-approval-routing` (Phase 3 Application Resolver & Create Leave Validation)

> **Date**: 2026-07-22

> **Status**: ✅ COMPLETED & BUILD PASSED (0 Build Errors)

> **Source of Truth**: [`MD_memory/plans/2026-07-22_1600_phase-dynamic-approval-routing-design_v5_proposal.md`](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/plans/2026-07-22_1600_phase-dynamic-approval-routing-design_v5_proposal.md)

> **Implementation Plan**: [`MD_memory/plans/2026-07-22_1615_phase-dynamic-approval-routing_implementation-plan.md`](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/plans/2026-07-22_1615_phase-dynamic-approval-routing_implementation-plan.md)

> **Architecture Boundary Preserved**:

> - `Web.Backend -> Application -> Domain`

> - `Infrastructure -> Application/Domain`



---



## 1. Phân Tích Mâu Thuẫn V5 Proposal & Giải Pháp Khóa Nghiệp Vụ (Option A Alignment)



### 1.1. Mâu Thuẫn Trong V5 Proposal Về Specific Approver Invalid

- Trong V5 Proposal (`MD_memory/plans/2026-07-22_1600_phase-dynamic-approval-routing-design_v5_proposal.md`), mục 1.5 vừa ghi:

  > *"Nếu SpecificApproverEmployeeId bị inactive/disabled/unlinked User/không có quyền, hệ thống KHÔNG tự động lặng lẽ fallback"*

  nhưng đoạn giả định luồng xử lý lại ghi tiếp về việc chuyển sang Rule Candidates.

- **Phân tích bản chất nghiệp vụ**: `SpecificApproverEmployeeId` là một cấu hình chỉ định đích danh (Intentional Override). Khi Admin/HR đã chủ động cấu hình một người duyệt đích danh cho vị trí này, nếu người đó không hợp lệ (mất quyền, inactive, hoặc tự duyệt đơn của mình), việc tự động lặng lẽ chuyển sang các Candidate khác sẽ làm sai ý định cấu hình ban đầu.



### 1.2. Quyết Định Thống Nhất (Option A Enforced For Create Flow)

- **Hành vi Option A được áp dụng**: Khi `rule.SpecificApproverEmployeeId != null`:

  - Nếu người duyệt chỉ định không hợp lệ (inactive, chưa gán User, không có quyền `APPROVE_LEAVE_REQUEST`, hoặc chính là người nộp đơn), Resolver **TRẢ VỀ LỖI NGAY LẬP TỨC** (`ApprovalRouteNotConfigured`).

  - **TUYỆT ĐỐI KHÔNG LẶNG LẼ FALLBACK** sang danh sách Rule Candidates.

  - Luồng duyệt Candidate (`Candidates` by `PriorityOrder`) chỉ được thực hiện khi Rule **không cấu hình Specific Approver** (`rule.SpecificApproverEmployeeId == null`).



---



## 2. Chi Tiết Thực Hiện Code & Structural Changes



1. **`ApprovalRouteResolverService.cs`**:

   ```csharp

   // 3. Option A: Check SpecificApprover if configured on Rule (Intentional Override)

   if (rule.SpecificApproverEmployeeId != null)

   {

       var specificApprover = await _dbContext.Set<Employee>()

           .FirstOrDefaultAsync(e => e.Id == rule.SpecificApproverEmployeeId, cancellationToken);



       if (specificApprover != null && await ValidateApproverAsync(specificApprover, requester, cancellationToken))

       {

           return ApprovalRouteResolutionResult.Success(

               specificApprover,

               policy.Id,

               rule.Id,

               candidateId: null,

               priorityOrder: 0,

               levelId: null,

               levelAssignmentId: null);

       }



       // Option A Enforced: SpecificApprover is an intentional override. If invalid, fail immediately.

       return ApprovalRouteResolutionResult.Failure(RouteNotConfiguredMessage);

   }

   ```



2. **Loại Bỏ CEO Auto-Approve Hardcode**:

   - `CreateLeaveRequestCommandHandler` đã gỡ bỏ hoàn toàn kiểm tra `Position.Code == "CEO"`. CEO cũng như mọi vị trí khác bắt buộc phải qua `ApprovalRouteResolverService`.



3. **Kiểm Tra Quyền `APPROVE_LEAVE_REQUEST` & Duplicate Level Slot Guard**:

   - Tích hợp `IRoleService.checkRoleExist(identityId, "APPROVE_LEAVE_REQUEST", cancellationToken)`.

   - Guard `activeAssignments.Count == 1` bắt buộc đúng 1 người đảm nhận Level slot active trong ngày.



---



## 3. Untracked Source Files Audit (Danh Sách File Nguồn Runtime Chưa Stage)



Dưới đây là các file/thư mục mới thuộc scope Phase 3 & Phase 2 mà sau này cần stage chính thức (`git add`):



### 3.1. Phase 3 Dynamic Routing Application & Domain Abstractions

- `HRM_Leave_Management/Application/Abstractions/ApprovalRouting/IApprovalRouteResolverService.cs`

- `HRM_Leave_Management/Domain/ApprovalRouting/IApprovalRouteAuditLogRepository.cs`



### 3.2. Phase 3 Dynamic Routing Infrastructure Implementation

- `HRM_Leave_Management/Infrastructure/Services/ApprovalRouteResolverService.cs`

- `HRM_Leave_Management/Infrastructure/Repositories/ApprovalRouteAuditLogRepository.cs`

- `HRM_Leave_Management/Infrastructure/Repositories/ApprovalRoutePolicyRepository.cs`

- `HRM_Leave_Management/Infrastructure/Repositories/LeaveRequestApprovalAssignmentRepository.cs`



### 3.3. Phase 2 Infrastructure EF Configurations & Migration

- `HRM_Leave_Management/Infrastructure/Configurations/ApprovalRoutePolicyConfiguration.cs`

- `HRM_Leave_Management/Infrastructure/Configurations/ApprovalRouteLevelConfiguration.cs`

- `HRM_Leave_Management/Infrastructure/Configurations/ApprovalRouteLevelAssignmentConfiguration.cs`

- `HRM_Leave_Management/Infrastructure/Configurations/ApprovalRouteRuleConfiguration.cs`

- `HRM_Leave_Management/Infrastructure/Configurations/ApprovalRouteRuleCandidateConfiguration.cs`

- `HRM_Leave_Management/Infrastructure/Configurations/LeaveRequestApprovalAssignmentConfiguration.cs`

- `HRM_Leave_Management/Infrastructure/Configurations/ApprovalRouteAuditLogConfiguration.cs`

- `HRM_Leave_Management/Infrastructure/Migrations/20260722100601_AddApprovalRouting.cs`

- `HRM_Leave_Management/Infrastructure/Migrations/20260722100601_AddApprovalRouting.Designer.cs`



---



## 4. Verification Output & Results



```bash

# 1. Full Git Status Output

git status --short

# Output: Tracked changes & explicit untracked Phase 3 files audited.



# 2. Targeted Git Diff Check

git diff --name-status

git diff --check

# Output: 0 whitespace / syntax errors.



# 3. Solution Build Check

dotnet build HRM_Leave_Management/Web.Backend/Web.Backend.csproj --no-restore

# Output: Build succeeded with 0 Errors.



# 4. Mojibake & BOM Scan

python MD_memory/debug/2026-06-26_1430_scan-mojibake.py MD_memory/reports/2026-07-22_1730_phase-dynamic-approval-routing_phase-3_resolver-create-validation_report.md --require-bom

# Output: BOM OK, 0 failures, 0 mojibake.

```
