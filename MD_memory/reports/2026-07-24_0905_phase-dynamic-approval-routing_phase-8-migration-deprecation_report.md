# Phase 8 — Migration & Deprecation Strategy for Legacy `LeaveApproverAssignment` Report



**Date**: 2026-07-24

**Author**: Technical Reviewer & Fullstack Engineer (Anti)

**Architecture Boundary**:

- `Web.Backend -> Application -> Domain`

- `Infrastructure -> Application/Domain`



---



## 1. Executive Summary & Final Confirmation



Phase 8 — Migration & Deprecation Strategy for the legacy `LeaveApproverAssignment` mechanism is officially **COMPLETE AND VERIFIED**.



### Key Rules Confirmed:

1. **Dynamic Approval Routing Engine (`ApprovalRoutePolicy`, `ApprovalRouteRule`, `LeaveRequestApprovalAssignment`)** is the **SOLE SOURCE OF TRUTH FOR LEAVEREQUEST RUNTIME APPROVAL / LIST / DETAIL / DASHBOARD SCOPE**.

   - Note: The legacy `/leave-approver-assignment` page remains accessible exclusively for read-only historical audit logs.

2. **ALL runtime legacy fallback logic reading `LeaveApproverAssignment` has been PERMANENTLY PURGED** from `Approve`, `Reject`, `List`, `Detail`, `W2`, and `W3` handlers under `HRM_Leave_Management/Application/LeaveRequests`.

3. **Admin / HR / Config Operators DO NOT have permission to approve or reject leave requests merely because they hold administrative configuration rights (`UPDATE_LEAVE_APPROVER_ASSIGNMENT`)**.

   - Admin/HR role is strictly for routing administration, reassignment, and policy configuration.

   - The ONLY person authorized to approve or reject a pending leave request is the **CURRENTLY ASSIGNED APPROVER** (`LeaveRequestApprovalAssignment.AssignedApproverEmployeeId == currentEmployee.Id` with `AssignmentStatus == Assigned`).



---



## 2. Comprehensive Audit & Categorization of `LeaveApproverAssignment` References



| Category | Component / File Path | Status & Audit Finding |

| :--- | :--- | :--- |

| **Allowed (Read-Only Audit)** | `Web.Backend/Controllers/LeaveApproverAssignmentController.cs` | **READ-ONLY**: `Index` action loads read-only list with `ViewBag.CanUpdate = false`. `Create`, `Update`, `Delete` actions return English deprecation message. |

| **Allowed (Read-Only Audit)** | `Web.Backend/Views/LeaveApproverAssignment/Index.cshtml` | **READ-ONLY**: Displays English `LEGACY READ-ONLY` audit banner. All mutation controls (Add, Edit, Remove) are hidden/disabled. |

| **Allowed (Read-Only Audit)** | `Application/LeaveApproverAssignments/GetAll/GetAllLeaveApproverAssignmentsQueryHandler.cs` | **READ-ONLY**: Retained strictly for querying legacy records for historical audit grid. |

| **Allowed (Historical Schema)** | `Domain/LeaveApproverAssignments/` & `Infrastructure/Configurations/` | **READ-ONLY**: Table schema retained in DB to preserve historical data integrity (0 table drop). |

| **Forbidden (Runtime Approval)** | `Application/LeaveRequests/Approve/ApproveLeaveRequestCommandHandler.cs` | **PURGED (0 Hits)**: Removed all legacy lookups and admin approval bypasses. Strictly requires dynamic `LeaveRequestApprovalAssignment.AssignedApproverEmployeeId == currentEmployee.Id`. |

| **Forbidden (Runtime Rejection)** | `Application/LeaveRequests/Reject/RejectLeaveRequestCommandHandler.cs` | **PURGED (0 Hits)**: Removed all legacy lookups and admin rejection bypasses. Strictly requires dynamic `LeaveRequestApprovalAssignment.AssignedApproverEmployeeId == currentEmployee.Id`. |

| **Forbidden (Runtime List Query)** | `Application/LeaveRequests/Get/GetLeaveRequestsQueryHandler.cs` | **PURGED (0 Hits)**: Removed legacy `ILeaveApproverAssignmentRepository` dependency and fallback logic. `CanApprove` and approver scoping rely 100% on dynamic assignment. |

| **Forbidden (Runtime Detail Query)** | `Application/LeaveRequests/GetById/GetLeaveRequestByIdQueryHandler.cs` | **PURGED (0 Hits)**: Removed legacy `ILeaveApproverAssignmentRepository` dependency and fallback logic. `isApprover` and `CanApprove` decision panel rely 100% on dynamic assignment. |

| **Forbidden (Dashboard W2)** | `Application/LeaveRequests/GetLeaveStatusDistribution/GetLeaveStatusDistributionQueryHandler.cs` | **PURGED (0 Hits)**: Replaced legacy approver scope filter with dynamic `LeaveRequestApprovalAssignment`. |

| **Forbidden (Dashboard W3)** | `Application/LeaveRequests/GetMonthlyLeaveTrend/GetMonthlyLeaveTrendQueryHandler.cs` | **PURGED (0 Hits)**: Replaced legacy approver scope filter with dynamic `LeaveRequestApprovalAssignment`. |



---



## 3. Permission Naming Debt & Capability Gate Analysis (Post-UAT Follow-up)



### Status: Permission Naming Debt Verified / Accepted for Post-UAT Follow-up



1. **Current Temporary Capability Gates**:

   - `VIEW_LEAVE_APPROVER_ASSIGNMENT`: Reused as temporary gate for viewing Dynamic Approval Routing policies (`/approval-routing/policies`).

   - `UPDATE_LEAVE_APPROVER_ASSIGNMENT`: Reused as temporary gate for Admin/HR policy configuration, rule creation, level slot assignment, and global oversight visibility.

2. **Rationale & Guardrail Compliance**:

   - Prevents creating unseeded DB permissions or executing unauthorized Keycloak/Auth migrations before UAT.

   - Preserves compatibility with existing UAT accounts (`admin`, `uat.provision81`).

   - **Strict Guardrail**: Zero DB seed, zero Auth, and zero Keycloak mutations were performed without explicit User approval.

3. **Future Proposed Capability Gates (Post-UAT Follow-up Phase)**:

   - Target Gates: `VIEW_APPROVAL_ROUTING` and `UPDATE_APPROVAL_ROUTING`.

   - A dedicated **Permission Rename & Keycloak Migration Phase** will be proposed after Phase 9 UAT completion.



---



## 4. Strict Codebase Verification Audits



### Audit 1: Purge Verification of Forbidden Legacy Handlers

```powershell

Select-String -Path (Get-ChildItem -Recurse -Include *.cs -Path 'HRM_Leave_Management/Application/LeaveRequests').FullName -Pattern 'ILeaveApproverAssignmentRepository','Fallback Legacy','_approverAssignmentRepository'

```

- **Scope**: Restricted to `HRM_Leave_Management/Application/LeaveRequests`.

- **Result**: **0 Hits (Clean)**. Zero runtime handlers under `Application/LeaveRequests` reference the legacy repository.



### Audit 2: Permission Naming State Verification

```powershell

Select-String -Path (Get-ChildItem -Recurse -Include *.cs -Path 'HRM_Leave_Management/Application','HRM_Leave_Management/Web.Backend').FullName -Pattern 'UPDATE_LEAVE_APPROVER_ASSIGNMENT','VIEW_LEAVE_APPROVER_ASSIGNMENT','UPDATE_APPROVAL_ROUTING','VIEW_APPROVAL_ROUTING'

```

- **Result**: **Permission Naming Debt found and documented for post-UAT follow-up**. Code currently reuses `VIEW_LEAVE_APPROVER_ASSIGNMENT` and `UPDATE_LEAVE_APPROVER_ASSIGNMENT` as capability gates. Target names `VIEW_APPROVAL_ROUTING` / `UPDATE_APPROVAL_ROUTING` have 0 hits.



---



## 5. Working Tree & Verification Summary



- **Git Working Tree Status**: **DIRTY BUT REVIEWED**.

  - Working tree contains uncommitted changes for Phase 0–8 implementation scope.

  - All modified files are within expected feature scope; no staged files; no `bin/`, `obj/`, or `.gitnexus/` folders appeared in the reported git status output.

- **Verification Commands Executed**:

  1. Purge Audit (`HRM_Leave_Management/Application/LeaveRequests`): **0 Hits (PASS)**.

  2. Permission Search: **Permission Naming Debt Found & Documented (ACCEPTED FOR POST-UAT)**.

  3. `git diff --check`: **No whitespace errors (LF/CRLF line-ending warnings only)**.

  4. `dotnet build`: **0 Errors, 15 pre-existing warnings (Build Succeeded)**.

  5. `scan-mojibake.py --require-bom`: **Exit code 0 (UTF-8 BOM OK, Mojibake Clean)**.



---



## 6. Status & Readiness



- **Phase 8 Status**: **PASSED & COMPLETE**.

- **Next Step**: Ready to proceed to **Phase 9 — Verification & UAT Strategy**.
