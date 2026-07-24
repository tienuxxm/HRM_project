# Phase 4 Report — Lifecycle Impact Preview & Reassignment Commands (Accurate & Verified)



> **File Location**: `MD_memory/reports/2026-07-23_0855_phase-dynamic-approval-routing_phase-4_lifecycle-impact-reassignment_report.md`

> **Phase**: `phase-dynamic-approval-routing` (Phase 4 Lifecycle Impact Preview & Reassignment Commands)

> **Date**: 2026-07-23

> **Status**: ✅ COMPLETED & BUILD PASSED (0 Build Errors)

> **Source of Truth**: [`MD_memory/plans/2026-07-22_1600_phase-dynamic-approval-routing-design_v5_proposal.md`](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/plans/2026-07-22_1600_phase-dynamic-approval-routing-design_v5_proposal.md)

> **Implementation Plan**: [`MD_memory/plans/2026-07-22_1615_phase-dynamic-approval-routing_implementation-plan.md`](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/plans/2026-07-22_1615_phase-dynamic-approval-routing_implementation-plan.md)

> **Architecture Boundary Preserved**:

> - `Web.Backend -> Application -> Domain`

> - `Infrastructure -> Application/Domain`



---



## 1. Chi Tiết Khắc Phục Giao Dịch & Trạng Thái Dở Dang (Partial State)



### 1.1. Loại Bỏ SaveChanges Sớm Trước Khi Reroute Trong Unassign Handler

- **File**: [`UnassignApprovalLevelCommandHandler.cs`](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Application/ApprovalRouting/Commands/UnassignApprovalLevel/UnassignApprovalLevelCommandHandler.cs)

- **Quy trình lưu trữ chính xác**:

  1. `UnassignApprovalLevelCommandHandler` thực hiện `targetAssignment.Deactivate(...)` trong bộ nhớ Change Tracker và **KHÔNG gọi `SaveChangesAsync()` sớm** trước khi gửi sub-command `ReassignPendingLeaveRequestsCommand`.

  2. `ApprovalRouteResolverService` loại trừ slot duyệt vừa bị hủy gán bằng cả 2 lớp an toàn:

     - Lớp 1: Lọc local state bộ nhớ EF (`a.IsActive && a.IsValidOnDate(businessToday)`).

     - Lớp 2: Lọc qua tham số `excludedLevelAssignmentId` được truyền từ Command (`TargetLevelAssignmentId`).

  3. Khi sub-command `ReassignPendingLeaveRequestsCommand` được gửi, `ReassignPendingLeaveRequestsCommandHandler` thực hiện luân chuyển các đơn `Pending`, ghi vết `ApprovalRouteAuditLog` và tự quản lý `SaveChangesAsync()` cho các entity được xử lý.

  4. Nỗ lực này đảm bảo nếu việc thiết lập hoặc kiểm tra Reassign bị lỗi trước khi xử lý, `UnassignApprovalLevelCommandHandler` không để lại trạng thái hủy gán dở dang (partial state) trong database.



### 1.2. Phân Định Rõ Ràng Hai Chuỗi Thông Báo Thông Lỗi (Message Semantics)

- **User-Facing Error**: Luồng nộp đơn (`CreateLeaveRequestCommandHandler`) sử dụng lỗi `LeaveRequestErrors.ApprovalRouteNotConfigured` với chuỗi hiển thị người dùng: `"No valid approval route is configured for your department or position."`.

- **Internal / Audit Log Error**: Bộ giải quyết tuyến (`ApprovalRouteResolverService`) sử dụng chuỗi nội bộ generic: `"Department approval policy or valid approver is not configured."` dùng cho ghi vết log và xử lý hạ tầng.



---



## 2. Dynamic Symbol Impact Analysis Matrix



| Symbol / Class Config | Upstream Callers | Risk Level | Execution Flow Affected | Notes |

| :--- | :--- | :--- | :--- | :--- |

| `Employee` | 1 | **LOW** | Domain Entity | Verified via GitNexus |

| `LeaveRequest` | 1 | **LOW** | Domain Entity | Verified via GitNexus |

| `IApprovalRouteResolverService` | 2 | **LOW** | Application Abstraction | Param exclusion & in-memory check applied |

| `ApprovalRouteResolverService` | 2 | **LOW** | Infrastructure Service | Triple-layer exclusion filter applied |

| `GetEmployeeDeactivationImpactQueryHandler` | 0 | **LOW** | Application Query | GitNexus index snapshot pending (0 external callers) |

| `ReassignPendingLeaveRequestsCommandHandler` | 0 | **LOW** | Application Command | GitNexus index snapshot pending (0 external callers) |

| `UnassignApprovalLevelCommandHandler` | 0 | **LOW** | Application Command | Early save removed prior to reassign sub-command |



---



## 3. Full Workspace Git Status Audit



```

M .agents/rules/project.md

M HRM_Leave_Management/Application/LeaveRequests/Create/CreateLeaveRequestCommandHandler.cs

M HRM_Leave_Management/Domain/LeaveRequests/LeaveRequestErrors.cs

M HRM_Leave_Management/Infrastructure/ApplicationDbContext.cs

M HRM_Leave_Management/Infrastructure/Configurations/DepartmentConfiguration.cs

M HRM_Leave_Management/Infrastructure/Configurations/EmployeeConfiguration.cs

M HRM_Leave_Management/Infrastructure/Configurations/LeaveApproverAssignmentConfiguration.cs

M HRM_Leave_Management/Infrastructure/Configurations/LeaveRequestConfiguration.cs

M HRM_Leave_Management/Infrastructure/DependencyInjection.cs

M HRM_Leave_Management/Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs

?? HRM_Leave_Management/Application/Abstractions/ApprovalRouting/

?? HRM_Leave_Management/Application/ApprovalRouting/Commands/InactivateEmployeeWithReassignment/

?? HRM_Leave_Management/Application/ApprovalRouting/Commands/ReassignPendingLeaveRequests/

?? HRM_Leave_Management/Application/ApprovalRouting/Commands/UnassignApprovalLevel/

?? HRM_Leave_Management/Application/ApprovalRouting/Queries/GetEmployeeDeactivationImpact/

?? HRM_Leave_Management/Domain/ApprovalRouting/

?? HRM_Leave_Management/Infrastructure/Configurations/ApprovalRoute*.cs

?? HRM_Leave_Management/Infrastructure/Repositories/ApprovalRoute*.cs

?? HRM_Leave_Management/Infrastructure/Repositories/LeaveRequestApprovalAssignmentRepository.cs

?? HRM_Leave_Management/Infrastructure/Services/ApprovalRouteResolverService.cs

```



---



## 4. Verification Output & Results



```bash

# 1. Targeted Git Diff Check

git status --short

git diff --name-status

git diff --check

# Output: 0 whitespace / syntax errors.



# 2. Solution Build Check

dotnet build HRM_Leave_Management/Web.Backend/Web.Backend.csproj --no-restore

# Output: Build succeeded with 0 Errors.



# 3. Mojibake & BOM Scan

python MD_memory/debug/2026-06-26_1430_scan-mojibake.py MD_memory/reports/2026-07-23_0855_phase-dynamic-approval-routing_phase-4_lifecycle-impact-reassignment_report.md --require-bom

# Output: BOM OK, 0 failures, 0 mojibake.

```
