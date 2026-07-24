# Phase 2 Report — Infrastructure / EF Core Mapping & Clean Migration



> **File Location**: `MD_memory/reports/2026-07-22_1640_phase-dynamic-approval-routing_phase-2_infrastructure_report.md`

> **Phase**: `phase-dynamic-approval-routing` (Phase 2 EF Core Mapping, Repositories & Clean Migration)

> **Date**: 2026-07-22

> **Status**: ✅ COMPLETED & FULLY SYNCHRONIZED MIGRATION (0 Build Errors)

> **Phase Scope Status**: Phase 2 migration scope verified (Repo working tree contains uncommitted work from earlier phases)

> **Migration File**: `Infrastructure/Migrations/20260722100601_AddApprovalRouting.cs`

> **Designer File**: `Infrastructure/Migrations/20260722100601_AddApprovalRouting.Designer.cs`

> **Model Snapshot**: `Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs`

> **Source of Truth**: [`MD_memory/plans/2026-07-22_1600_phase-dynamic-approval-routing-design_v5_proposal.md`](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/plans/2026-07-22_1600_phase-dynamic-approval-routing-design_v5_proposal.md)

> **Implementation Plan**: [`MD_memory/plans/2026-07-22_1615_phase-dynamic-approval-routing_implementation-plan.md`](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/plans/2026-07-22_1615_phase-dynamic-approval-routing_implementation-plan.md)

> **Evidence Directory**: [`MD_memory/evidence/2026-07-22_dynamic-approval-routing-phase2/`](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/evidence/2026-07-22_dynamic-approval-routing-phase2/)

> **Architecture Boundary Preserved**:

> - `Web.Backend -> Application -> Domain`

> - `Infrastructure -> Application/Domain`



---



## 1. Breakdown Phạm Vi Thay Đổi Working Tree Của Phase 2



### 1.1. Trạng Thái Working Tree Toàn Repo

- **Lưu ý quan trọng**: Working tree của repository hiện tại đang lưu giữ các file chưa committed từ các phase trước (Dashboard, WorkCalendar, Employee-User Link).

- **Phạm vi Phase 2**: **Phase 2 migration scope verified** — chỉ tập trung vào module `ApprovalRouting` và hạ tầng EF Core liên quan.



### 1.2. Phase 2 Tracked Diff Scope

Các file đã có sẵn trong git và được sửa đổi trực tiếp cho Phase 2:

- `HRM_Leave_Management/Infrastructure/ApplicationDbContext.cs`

- `HRM_Leave_Management/Infrastructure/DependencyInjection.cs`

- `HRM_Leave_Management/Infrastructure/Configurations/DepartmentConfiguration.cs`

- `HRM_Leave_Management/Infrastructure/Configurations/EmployeeConfiguration.cs`

- `HRM_Leave_Management/Infrastructure/Configurations/LeaveApproverAssignmentConfiguration.cs`

- `HRM_Leave_Management/Infrastructure/Configurations/LeaveRequestConfiguration.cs`

- `HRM_Leave_Management/Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs`



### 1.3. Phase 2 Untracked New Files List

Các file mới hoàn toàn được tạo dựng riêng cho Phase 2 (Được xác nhận qua `git status --short`):

- **Domain Entities & Value Objects** (`Domain/ApprovalRouting/`):

  - `ApprovalRoutePolicy.cs`, `ApprovalRoutePolicyId.cs`

  - `ApprovalRouteLevel.cs`, `ApprovalRouteLevelId.cs`

  - `ApprovalRouteLevelAssignment.cs`, `ApprovalRouteLevelAssignmentId.cs`

  - `ApprovalRouteRule.cs`, `ApprovalRouteRuleId.cs`

  - `ApprovalRouteRuleCandidate.cs`, `ApprovalRouteRuleCandidateId.cs`

  - `LeaveRequestApprovalAssignment.cs`, `LeaveRequestApprovalAssignmentId.cs`

  - `ApprovalRouteAuditLog.cs`, `ApprovalRouteAuditLogId.cs`

  - `IApprovalRoutePolicyRepository.cs`, `ILeaveRequestApprovalAssignmentRepository.cs`

- **Infrastructure Configurations**:

  - `Infrastructure/Configurations/ApprovalRoutePolicyConfiguration.cs`

  - `Infrastructure/Configurations/ApprovalRouteLevelConfiguration.cs`

  - `Infrastructure/Configurations/ApprovalRouteRuleConfiguration.cs`

  - `Infrastructure/Configurations/ApprovalRouteRuleCandidateConfiguration.cs`

  - `Infrastructure/Configurations/ApprovalRouteLevelAssignmentConfiguration.cs`

  - `Infrastructure/Configurations/LeaveRequestApprovalAssignmentConfiguration.cs`

  - `Infrastructure/Configurations/ApprovalRouteAuditLogConfiguration.cs`

- **Infrastructure Repositories**:

  - `Infrastructure/Repositories/ApprovalRoutePolicyRepository.cs`

  - `Infrastructure/Repositories/LeaveRequestApprovalAssignmentRepository.cs`

- **EF Core Migration Files**:

  - `Infrastructure/Migrations/20260722100601_AddApprovalRouting.cs`

  - `Infrastructure/Migrations/20260722100601_AddApprovalRouting.Designer.cs`



---



## 2. GitNexus Impact Analysis & Change Detection Evidence



Toàn bộ dữ liệu phân tích tác động đã được lưu trữ làm bằng chứng trực tiếp tại workspace với chuẩn UTF-8 BOM:

- **Impact Analysis Evidence**: [`MD_memory/evidence/2026-07-22_dynamic-approval-routing-phase2/impact_analysis_results.md`](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/evidence/2026-07-22_dynamic-approval-routing-phase2/impact_analysis_results.md)

- **Change Detection Evidence**: [`MD_memory/evidence/2026-07-22_dynamic-approval-routing-phase2/detect_changes_results.md`](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/evidence/2026-07-22_dynamic-approval-routing-phase2/detect_changes_results.md)



### 2.1. Dynamic Symbol Impact Matrix & Disambiguation



> **Ghi chú về Disambiguation Symbol**:

> Thư viện codebase có chứa nhiều lớp `ApplicationDbContext` (ví dụ `Persistence/ApplicationDbContext.cs` của root project). Để đảm bảo phân tích chính xác file thuộc Phase 2, target đã được chỉ định tường minh theo symbol ID: `Class:HRM_Leave_Management/Infrastructure/ApplicationDbContext.cs:ApplicationDbContext`.



| Symbol / Class Config | Upstream Blast Radius | Risk Level | Process Affected | Direct Callers Broken | Evidence File Link |

| :--- | :--- | :--- | :--- | :--- | :--- |

| `Class:HRM_Leave_Management/Infrastructure/ApplicationDbContext.cs:ApplicationDbContext` | 0 | **LOW** | 0 | 0 | [impact_analysis_results.md](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/evidence/2026-07-22_dynamic-approval-routing-phase2/impact_analysis_results.md#1-symbol-hrm_leave_managementinfrastructureapplicationdbcontextcs) |

| `DependencyInjection` | 0 | **LOW** | 0 | 0 | [impact_analysis_results.md](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/evidence/2026-07-22_dynamic-approval-routing-phase2/impact_analysis_results.md#2-symbol-dependencyinjection) |

| `DepartmentConfiguration` | 0 | **LOW** | 0 | 0 | [impact_analysis_results.md](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/evidence/2026-07-22_dynamic-approval-routing-phase2/impact_analysis_results.md#3-symbol-departmentconfiguration) |

| `EmployeeConfiguration` | 0 | **LOW** | 0 | 0 | [impact_analysis_results.md](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/evidence/2026-07-22_dynamic-approval-routing-phase2/impact_analysis_results.md#4-symbol-employeeconfiguration) |

| `LeaveApproverAssignmentConfiguration` | 0 | **LOW** | 0 | 0 | [impact_analysis_results.md](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/evidence/2026-07-22_dynamic-approval-routing-phase2/impact_analysis_results.md#5-symbol-leaveapproverassignmentconfiguration) |

| `LeaveRequestConfiguration` | 0 | **LOW** | 0 | 0 | [impact_analysis_results.md](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/evidence/2026-07-22_dynamic-approval-routing-phase2/impact_analysis_results.md#6-symbol-leaverequestconfiguration) |



*Kết luận Impact*: Không phát hiện rủi ro **HIGH** hay **CRITICAL**. Mọi thay đổi đều ở cấp độ hạ tầng an toàn.



### 2.2. Hạn Chế Kỹ Thuật Của `detect_changes` & Untracked Scope Coverage

- Lệnh `detect_changes` của GitNexus chỉ đo lường các thay đổi trên những file đã được Git track.

- Đối với 25 untracked files mới tạo trong Phase 2 (`Domain/ApprovalRouting/*`, `Infrastructure/Configurations/*`, Repositories & Migrations), bằng chứng phạm vi được xác minh bổ sung tường minh qua `git status --short` (Chi tiết tại [`detect_changes_results.md`](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/evidence/2026-07-22_dynamic-approval-routing-phase2/detect_changes_results.md#2-phase-2-untracked-new-files-scope-git-status---short)).



### 2.3. Giải Trình Lệnh `git checkout ApplicationDbContextModelSnapshot.cs`

- Lệnh `git checkout ApplicationDbContextModelSnapshot.cs` được thực hiện **duy nhất để revert file snapshot tạm thời do Anti scaffold thử nghiệm trước đó trong lượt này**.

- **Không revert bất kỳ công việc nào của User**. Bản snapshot hiện tại được scaffold hoàn chỉnh từ EF Core và khớp 100% với file migration.



---



## 3. Chi Tiết Kỹ Thuật & Business Invariants Phase 2



### 3.1. Synchronized Migration, Designer & ModelSnapshot Alignment

- Đã khóa cứng `.HasConstraintName(...)` tường minh cho 6 mối quan hệ cũ và 11 mối quan hệ mới.

- **Zero Constraint-Name Drift**: `ApplicationDbContextModelSnapshot.cs`, `20260722100601_AddApprovalRouting.Designer.cs`, và `20260722100601_AddApprovalRouting.cs` đồng bộ hoàn hảo. File migration `.cs` **chỉ chứa `CreateTable` và `CreateIndex` cho 7 bảng mới**.



### 3.2. Ngữ Nghĩa Audit Log (Option A)

- `ApprovalRouteAuditLog` **chỉ audit các Leave Request đã được tạo thành công** và có `LeaveRequestId` hợp lệ.

- Blocked submit **không tạo `LeaveRequest` và không ghi `ApprovalRouteAuditLog`**. Application handler trả về Business Error Response trực tiếp cho client (`Approval route is not configured for this department/position...`).



### 3.3. Date Range Overlap Wording

- Logic chống giao thoa khoảng thời gian (`EffectiveFrom` - `EffectiveTo`) của `ApprovalRouteLevelAssignment` **CHƯA được DB enforce** tại Phase 2. Application Handler / Validator tại Phase 5 **bắt buộc phải validate và enforce** trước khi cho phép tạo / cập nhật assignment.



### 3.4. Bảo Tồn Nguyên Tắc Nghiệp Vụ Chốt (Business Invariants)

- **Department-specific policy only**: Mỗi policy thuộc 1 Department cụ thể.

- **No global/default/fallback rule**: Cấm fallback/default rule (cấm requesterPositionId = null).

- **Strict config-driven PriorityOrder**: Thứ tự candidate được sắp xếp strictly theo `PriorityOrder`.

- **One current approver per pending leave request**: Mỗi đơn nghỉ Pending chỉ gán 1 nguời duyệt active duy nhất tại một thời điểm.

- **Admin/HR role separation**: Admin/HR chỉ cấu hình route và re-route, không duyệt thay.



---



## 4. Verification Commands & Execution Results



```bash

# 1. Verification 1: Confirm Constraint Name Alignment Across ModelSnapshot & Designer

rg -n "fk_department_department_parent_department|fk_employee_department_department|fk_employee_employee_manager|fk_employee_positions_position|fk_leave_approver_assignment_positions|fk_leave_request_employee_employee" HRM_Leave_Management/Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs HRM_Leave_Management/Infrastructure/Migrations/20260722100601_AddApprovalRouting.Designer.cs

# Output: Constraint names match 100% with baseline (Zero temp_id increment churn!).



# 2. Verification 2: Confirm Zero Unexpected Operations in Migration .cs Code

rg -n "DropForeignKey|AddForeignKey|AlterColumn|AddColumn|DropColumn|Rename|DropIndex" HRM_Leave_Management/Infrastructure/Migrations/20260722100601_AddApprovalRouting.cs

# Output: No results found. (Contains ONLY CreateTable and CreateIndex for 7 new tables!)



# 3. Verification 3: Confirm Zero Bad HasFilter / Value! Hits

rg -n "is_active = 1|id => id\.Value!" HRM_Leave_Management/Infrastructure/Configurations

# Output: No results found.



# 4. Verification 4: Targeted Git Diff Name Status & Check

git diff --name-status -- HRM_Leave_Management/Infrastructure HRM_Leave_Management/Domain/ApprovalRouting

git diff --check -- HRM_Leave_Management/Infrastructure HRM_Leave_Management/Domain/ApprovalRouting



# 5. Build Check

dotnet build HRM_Leave_Management/Web.Backend/Web.Backend.csproj --no-restore

# Output: 0 whitespace errors, Build succeeded with 0 Errors.

```



- **Mojibake Check & BOM**: `python MD_memory/debug/2026-06-26_1430_scan-mojibake.py MD_memory/reports/2026-07-22_1640_phase-dynamic-approval-routing_phase-2_infrastructure_report.md MD_memory/evidence/2026-07-22_dynamic-approval-routing-phase2/impact_analysis_results.md MD_memory/evidence/2026-07-22_dynamic-approval-routing-phase2/detect_changes_results.md --require-bom` -> ✅ **PASS (BOM OK, 0 failures, 0 mojibake)**.



---



## 5. Status Working Tree & Đề Xuất Phase 3



- **Trạng thái Repo**: **Phase 2 migration scope verified**. Working tree repo đang lưu giữ công việc uncommitted của các phase trước (Dashboard, WorkCalendar, Employee Link) theo đúng quy định.

- **Đề xuất Phase 3 (Application Resolver & Create Leave Validation)**:

  - Thêm interface & service `IApprovalRouteResolverService`.

  - Phân giải approver theo `RequesterPositionId` và `PriorityOrder` tăng dần.

  - Tích hợp Resolver vào `CreateLeaveRequestCommandHandler`.

  - Block nộp đơn và trả về lỗi nghiệp vụ khi không cấu hình route hoặc không tìm thấy approver:

    > `Approval route is not configured for this department/position. Please assign an approver before submitting leave request.`
