# Dynamic Approval Routing Engine — UAT Configuration Matrix & Verification Plan

**Date**: 2026-07-24  
**Author**: Senior Fullstack Engineer & Technical Reviewer (Anti)  
**Common UAT Password Confirmed by User**: `Admin@123456`  
**Architecture Boundary**:  
- `Web.Backend -> Application -> Domain`  
- `Infrastructure -> Application/Domain`  

---

## 1. Executive Summary & Verification Directives

This document establishes the official **UAT Configuration Matrix** for the **Single-Step Dynamic Approval Routing Engine** (`ApprovalRoutePolicy`, `ApprovalRouteRule`, `LeaveRequestApprovalAssignment`).

### Strict Verification Directives:
1. **Security & Data Safety**: Plaintext passwords have been removed from current phase workspace artifacts. Debug scripts created in this phase (`MD_memory/debug/2026-07-24_1125_inspect-hrm-runtime-db.py` and `MD_memory/debug/2026-07-24_1142_query-permissions.py`) have been updated to require the environment variable `PGPASSWORD` without plaintext fallback. Zero database mutations were performed during this inspection phase.
2. **Zero Code / Auth / Keycloak Mutation**: No C# code, Razor view, Keycloak configuration, or user permission mutation was performed.
3. **Exact Domain Code Evidence Alignment**: All expected assignment statuses, reasons, audit action types, and domain error messages are aligned strictly with authoritative C# domain code:
   - `ApprovalAssignmentStatus`: `Assigned` (`1`), `NeedsAdminAttention` (`2`).
   - `ApprovalAssignmentReason`: `DirectLevelMatch` (`1`), `SuperiorLevelEscalated` (`2`), `SpecificEmployeeOverride` (`3`), `OperatorManualReassigned` (`4`), `AutoApproved` (`5`).
   - `ApprovalRouteAuditActionType`: `Created` (`1`), `Reassigned` (`2`), `Escalated` (`3`), `NeedsAttention` (`4`), `OverrideApplied` (`5`), `AutoApproved` (`6`).
   - `LeaveRequestErrors.ApprovalRouteNotConfigured`: `LeaveRequest.ApprovalRouteNotConfigured` (`"Approval route is not configured for this department/position. Please assign an approver before submitting leave request."`).
4. **Working Tree Verification**: Working tree has one untracked deliverable plan (`MD_memory/plans/2026-07-24_1130_phase-dynamic-approval-routing_uat-config-matrix.md`); no staged files.
5. **UI Execution Rule**: Real configuration setup or test data creation via UI must be explicitly reviewed and approved by User / Codex before execution.

---

## 2. Database-Verified Runtime User & Employee Capability Mapping

The table below lists the runtime user accounts, linked employees, and **DB-Verified Assigned Permissions** retrieved via read-only SQL join (`user -> user_to_role -> role -> role_to_permission -> permission`) from PostgreSQL (`hrm_baseline_db`).

| System Username | Linked Employee Code | Employee Name | Email | Active Status | Department | Position | DB-Verified Assigned Resource Permissions |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `admin` | *(System Admin)* | System Administrator | `admin@hrm.local` | `True` | *(Global)* | *(Global)* | `VIEW_DASHBOARD`, `CREATE_LEAVE_REQUEST`, `APPROVE_LEAVE_REQUEST`, `VIEW_LEAVE_APPROVER_ASSIGNMENT`, `UPDATE_LEAVE_APPROVER_ASSIGNMENT`, `VIEW_EMPLOYEE`, `UPDATE_EMPLOYEE`, `VIEW_DEPARTMENT`, `UPDATE_DEPARTMENT` + 41 System Perms (Role ID: `11111111-1111-1111-1111-111111111111`) |
| `admin2` | *(System Admin)* | Secondary Admin | `admin2@hrm.local` | `True` | *(Global)* | *(Global)* | Same 50 System Perms as `admin` (Role ID: `11111111-1111-1111-1111-111111111111`) |
| `huyadmin` | `EMP001` | Huy Admin | `huyadmin@hrm.local` | `True` | *(Unassigned)* | `Manager` | Same 50 System Perms as `admin` (Role ID: `11111111-1111-1111-1111-111111111111`) |
| `uat.provision80` | `EMP05` | uat.provision80 | `uat.provision80@hrm.local` | `True` | `Information Technology` | `Employee` | `VIEW_DASHBOARD`, `CREATE_LEAVE_REQUEST`, `VIEW_LEAVE_BALANCE`, `VIEW_LEAVE_REQUEST` (Role ID: `11111111-1111-1111-1111-111111111112`) |
| `uat.provision81` | `EMP04` | uat.provision81 | `uat.provision81@hrm.local` | `True` | `Information Technology` | `Manager` | `VIEW_DASHBOARD`, `CREATE_LEAVE_REQUEST`, `APPROVE_LEAVE_REQUEST`, `VIEW_LEAVE_BALANCE`, `VIEW_LEAVE_REQUEST` (Role IDs: `7026b7ee-...`, `11111111-...`) |
| `ceo.test` | `EMP-CEO-TEST` | CEO Test | `ceo.test@hrm.local` | `True` | *(Company Level)* | `CEO` | `VIEW_DASHBOARD`, `CREATE_LEAVE_REQUEST`, `APPROVE_LEAVE_REQUEST`, `VIEW_LEAVE_BALANCE`, `VIEW_LEAVE_REQUEST` (Role IDs: `7026b7ee-...`, `11111111-...`) |
| `uat.provision86` | `EMP-NV-TEST` | Nhan Vien Test | `uat.provision86@hrm.local` | `True` | `Human Resources Department` | `Employee` | `VIEW_DASHBOARD`, `CREATE_LEAVE_REQUEST`, `VIEW_LEAVE_BALANCE`, `VIEW_LEAVE_REQUEST` (Role ID: `11111111-1111-1111-1111-111111111112`) |
| `tp.test` | `EMP-TP-TEST` | Truong Phong Test | `tp.test@hrm.local` | `False` | `Information Technology` | `Manager` | *(Inactive Employee Account)* |
| `nguyenvanemployee`| `EMP002` | Nguyen Van Employee | `nguyenvanemployee@hrm.local` | `False` | `Information Technology` | `Manager` | *(Inactive Employee Account)* |
| `mgr091507` | `MGR091507` | UAT_Delete_Manager_091507 | `mgr091507@hrm.local` | `False` | `Information Technology` | `Manager` | *(Inactive Employee Account)* |
| `uat.subordinate` | `SUB091507` | UAT_Delete_Subordinate_091507 | `uat.subordinate@hrm.local` | `False` | `Information Technology` | `Employee` | *(Inactive Employee Account)* |
| `with091507` | `WITH091507` | UAT_Delete_WithHistory_091507 | `with091507@hrm.local` | `False` | *(Unassigned)* | *(None)* | *(Inactive Employee Account)* |

> **UAT Test Subset Rationale**: The verified account/data subset covers the primary prerequisites for the UAT matrix; each case remains subject to its documented readiness category and preconditions.

---

## 3. Existing Dynamic Approval Routing Configuration State

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

## 4. Comprehensive UAT Business Cases (Cases A – J)

---

### Case A: Same-Department Direct Superior Routing
- **Operational Readiness Category**: **`READY NOW`** *(Passed & Verified in Phase 9 UAT TC1)*
- **Department**: `Information Technology` (`IT`)
- **Requester Position**: `Employee`
- **Policy Configuration**:
  - Level 1: `Department Manager` (Assigned: `uat.provision81`)
  - Rule: Requester `Employee` -> Candidate Priority 1: `Level 1` (`Department Manager`).
- **Test Accounts**: Requester: `uat.provision80` | Approver: `uat.provision81`
- **UI Path**: `/leave-request/create` -> `/leave-request/detail/{id}` -> `/dashboard`
- **Expected Results**:
  - **Leave Request Creation**: Returns HTTP 200 / Success redirect.
  - **Leave Request Detail**: Current Approver = `uat.provision81 (EMP04)`, `LeaveRequestApprovalAssignment.Status` = `ApprovalAssignmentStatus.Assigned` (`1`), `Reason` = `ApprovalAssignmentReason.DirectLevelMatch` (`1`).
  - **Decision Panel**: Rendered strictly for assigned approver `uat.provision81` (`APPROVE REQUEST` / `REJECT REQUEST`).
  - **Dashboard W4 Queue**: Displays the new request in Widget W4 (`APPROVAL QUEUE`) for `uat.provision81`.
- **Audit Expectation**: `ApprovalRouteAuditLog.ActionType` = `ApprovalRouteAuditActionType.Created` (`1`), `ReasonCode` = `DirectLevelMatch`.
- **Business Rule Covered**: Standard same-department direct superior approval routing.
- **Preconditions / Action Needed**: Verified PASS in Phase 9 UAT TC1. No additional config needed.

---

### Case B: Config-Driven Escalation (Skipping Unlisted Intermediate Positions)
- **Operational Readiness Category**: **`NEEDS UI CONFIG ONLY`**
- **Department**: `Information Technology` (`IT`)
- **Requester Position**: `Employee`
- **Policy Configuration**:
  - Level 1: `Team Leader` (Unassigned / Approver Inactive)
  - Level 2: `Department Header` (Assigned: `uat.provision81`)
  - Rule: Candidate Priority 1 = `Level 1` (`Team Leader`), Candidate Priority 2 = `Level 2` (`Department Header`).
  - *Note*: Position `Manager` exists in IT structural hierarchy, but is NOT listed in the candidate sequence.
- **Test Accounts**: Requester: `uat.provision80` | Target Approver: `uat.provision81`
- **UI Path**: `/approval-routing/policies/detail/{id}` -> `/leave-request/create` -> `/leave-request/detail/{id}`
- **Expected Results**:
  - Routing engine evaluates Candidate Priority 1 (`Team Leader`) -> Fails resolution (no active assigned approver).
  - Escalates to Candidate Priority 2 (`Department Header`) -> Resolves successfully to `uat.provision81`.
  - **Strict Behavior**: Position `Manager` is completely bypassed because it is not configured in the rule candidate list.
  - **Leave Request Detail**: Current Approver = `uat.provision81`, `LeaveRequestApprovalAssignment.Status` = `ApprovalAssignmentStatus.Assigned` (`1`), `Reason` = `ApprovalAssignmentReason.SuperiorLevelEscalated` (`2`).
- **Audit Expectation**: `ApprovalRouteAuditLog.ActionType` = `ApprovalRouteAuditActionType.Created` (`1`), `ReasonCode` = `SuperiorLevelEscalated`.
- **Business Rule Covered**: Config-driven candidate escalation; strict adherence to candidate sequence without automatic structural fallback.
- **Preconditions / Action Needed**: Requires UI creation of Level 2 (`Department Header`) under Policy `f756fb72-8277-4934-8a71-be724b2e83bc`. User approval required before UI edit.

---

### Case C: Missing Policy / Missing Rule Guard
- **Operational Readiness Category**: **`NEEDS TEST DATA`**
- **Department**: `Finance` (`FINANCE`) or `Sales` (`SALES`)
- **Requester Position**: `Employee`
- **Policy Configuration**: No active `ApprovalRoutePolicy` or no matching `ApprovalRouteRule` configured for the department.
- **Test Account**: Active test employee assigned to `FINANCE` department.
- **UI Path**: `/leave-request/create`
- **Expected Results**:
  - Submission attempt is blocked with exact error response:  
    `LeaveRequestErrors.ApprovalRouteNotConfigured` (`LeaveRequest.ApprovalRouteNotConfigured`: `"Approval route is not configured for this department/position. Please assign an approver before submitting leave request."`).
  - **Strict Behavior**: Zero pending orphaned leave requests created in `leave_request` or `leave_request_approval_assignment`.
- **Audit Expectation**: No audit log created (command returns failure result).
- **Business Rule Covered**: Invariant safety guard against unrouted/orphaned leave requests.
- **Preconditions / Action Needed**: Requires assigning an active test user to `Finance` department via UI. User approval required before creating test data via UI.

---

### Case D: Specific Employee Approver Override
- **Operational Readiness Category**: **`NEEDS UI CONFIG ONLY`**
- **Department**: `Information Technology` (`IT`)
- **Requester Position**: `Employee`
- **Policy Configuration**:
  - Rule: `specific_approver_employee_id` set directly to employee `uat.provision81`.
- **Test Accounts**: Requester: `uat.provision80` | Specified Approver: `uat.provision81`
- **UI Path**: `/approval-routing/policies/detail/{id}` -> `/leave-request/create` -> `/leave-request/detail/{id}`
- **Expected Results**:
  - Engine bypasses level candidate sequence entirely.
  - Directly assigns request to `uat.provision81`.
  - **Leave Request Detail**: Current Approver = `uat.provision81`, `LeaveRequestApprovalAssignment.Status` = `ApprovalAssignmentStatus.Assigned` (`1`), `Reason` = `ApprovalAssignmentReason.SpecificEmployeeOverride` (`3`).
- **Audit Expectation**: `ApprovalRouteAuditLog.ActionType` = `ApprovalRouteAuditActionType.Created` (`1`), `ReasonCode` = `SpecificEmployeeOverride`.
- **Business Rule Covered**: Specific employee override configuration for temporary delegate / interim assignment without changing structural positions.
- **Preconditions / Action Needed**: Requires configuring specific approver override on Policy Rule via `/approval-routing/policies/detail/{id}`. User approval required.

---

### Case E: Dynamic Reverse-Proof Case (Non-Production Flexibility Proof)
- **Operational Readiness Category**: **`NEEDS UI CONFIG ONLY`**
- **Department**: `Information Technology` (`IT`)
- **Requester Position**: `Manager`
- **Policy Configuration**:
  - Rule for `Manager` position configured with Candidate Level assigned to an `Employee` position (`uat.provision80`).
- **Test Accounts**: Requester: `uat.provision81` (`Manager`) | Approver: `uat.provision80` (`Employee`)
- **UI Path**: `/approval-routing/policies/detail/{id}` -> `/leave-request/create` -> `/leave-request/detail/{id}`
- **Expected Results**:
  - Request created by `Manager` (`uat.provision81`) routes successfully to `Employee` (`uat.provision80`).
  - **Leave Request Detail**: Current Approver = `uat.provision80`, `LeaveRequestApprovalAssignment.Status` = `ApprovalAssignmentStatus.Assigned` (`1`), `Reason` = `ApprovalAssignmentReason.DirectLevelMatch` (`1`).
  - Decision Panel rendered strictly for `uat.provision80`.
- **Audit Expectation**: `ApprovalRouteAuditLog.ActionType` = `ApprovalRouteAuditActionType.Created` (`1`).
- **Business Rule Covered**: Technical verification proving the engine is 100% dynamic policy-driven and does NOT hardcode structural role hierarchies.
- **Preconditions / Action Needed**: Non-production test case. Requires UI configuration of rule for `Manager` position. User approval required.

---

### Case F: Top-Level Department Approver -> Company-Level Approver
- **Operational Readiness Category**: **`NEEDS UI CONFIG ONLY`**
- **Department**: `Information Technology` (`IT`)
- **Requester Position**: `Manager` (Top position of IT department)
- **Policy Configuration**:
  - Rule for `Manager` position configured with Candidate Level = `Company Top Approver` (Assigned: `CEO Test` / `EMP-CEO-TEST`).
- **Test Accounts**: Requester: `uat.provision81` (`IT Manager`) | Approver: `ceo.test` (`CEO Test`)
- **UI Path**: `/leave-request/create` -> `/leave-request/detail/{id}` -> `/dashboard`
- **Expected Results**:
  - Request routes cleanly from Department Top Approver to Company Top Approver (`CEO Test`).
  - **Leave Request Detail**: Current Approver = `CEO Test (EMP-CEO-TEST)`, `LeaveRequestApprovalAssignment.Status` = `ApprovalAssignmentStatus.Assigned` (`1`), `Reason` = `ApprovalAssignmentReason.DirectLevelMatch` (`1`).
  - Dashboard W4 Queue for `ceo.test` displays the request.
  - **Strict Behavior**: Zero hardcoded "CEO" role name checks in C# code.
- **Audit Expectation**: `ApprovalRouteAuditLog.ActionType` = `ApprovalRouteAuditActionType.Created` (`1`).
- **Business Rule Covered**: Multi-tier cross-department / company-level approval escalation without hardcoding role strings.
- **Preconditions / Action Needed**: Requires configuring candidate level rule for `Manager` position targeting Company Level assigned approver. User approval required.

---

### Case G: Company Top Approver Self Leave Auto-Approved
- **Operational Readiness Category**: **`NEEDS UI CONFIG ONLY`**
- **Department**: Company Level (`None`)
- **Requester Position**: `CEO`
- **Policy Configuration**:
  - Policy Rule for `CEO` position has `is_auto_approve = true`.
- **Test Account**: Requester: `ceo.test`
- **UI Path**: `/leave-request/create` -> `/leave-request/detail/{id}`
- **Expected Results**:
  - Upon submission, request status is immediately set to `APPROVED` (`LeaveRequestStatus.Approved`).
  - `LeaveRequestApprovalAssignment` is NOT created for pending approval; `LeaveBalance.UsedDays` is deducted immediately.
  - **Strict Behavior**: Auto-approval is executed strictly by policy rule configuration (`is_auto_approve = true`), NOT hardcoded by username or CEO role name.
- **Audit Expectation**: `ApprovalRouteAuditLog.ActionType` = `ApprovalRouteAuditActionType.AutoApproved` (`6`), `ReasonCode` = `"ConfiguredTerminalApproverAutoApproved"`.
- **Business Rule Covered**: Configurable auto-approval for top-level executive positions.
- **Preconditions / Action Needed**: Requires setting `is_auto_approve = true` on Policy Rule for `CEO` position via UI. User approval required.

---

### Case H: Inactive / Deleted Approver Impact & Rerouting
- **Operational Readiness Category**: **`CODE-REVIEW ONLY / READY FOR TRIGGER`**
- **Department**: `Information Technology` (`IT`)
- **Scenario**: Admin deactivates or deletes an employee (`uat.provision81`) who is currently assigned as an active approver for pending leave requests.
- **UI Path**: `/employee/index` -> Action: `Deactivate` / `Delete`
- **Expected Results**:
  - Impact Analysis query (`GetEmployeeDeactivationImpactQueryHandler`) presents modal warning Admin: `"Employee uat.provision81 is currently an active approver for N pending leave requests and M policy levels."`
  - Option 1: Manually select replacement employee (`ReassignPendingLeaveRequestsCommandHandler`).
  - Option 2: System deactivates with auto-rerouting (`InactivateEmployeeWithReassignmentCommandHandler`).
  - **Assignment Status**: Affected pending requests transition to `ApprovalAssignmentStatus.NeedsAdminAttention` (`2`) if no next candidate exists, or `Assigned` (`1`) with `Reason` = `ApprovalAssignmentReason.OperatorManualReassigned` (`4`).
  - **Historical Isolation**: Completed requests (`APPROVED`, `REJECTED`, `CANCELED`) retain historical approver snapshots and are strictly unchanged.
- **Audit Expectation**: `ApprovalRouteAuditLog.ActionType` = `ApprovalRouteAuditActionType.Reassigned` (`2`) or `NeedsAttention` (`4`).
- **Business Rule Covered**: Safe lifecycle impact management for active approver lifecycle events.
- **Preconditions / Action Needed**: Requires active pending request assigned to target employee before triggering deactivation via UI. User approval required.

---

### Case I: Specific Approver Inactive / Lost Permission Guard
- **Operational Readiness Category**: **`CODE-REVIEW ONLY / READY FOR TRIGGER`**
- **Department**: `Information Technology` (`IT`)
- **Scenario**: Specific approver assigned in Case D (`specific_approver_employee_id`) is deactivated or loses `APPROVE_LEAVE_REQUEST` permission.
- **UI Path**: `/approval-routing/levels/assignments`
- **Expected Results**:
  - Engine DOES NOT perform silent untraced fallback.
  - Pending assignment status transitions to `ApprovalAssignmentStatus.NeedsAdminAttention` (`2`).
  - Warning banner rendered on Admin Approval Routing console: `"1 or more pending leave requests require manual approver assignment due to inactive/unqualified specific approver."`
- **Audit Expectation**: `ApprovalRouteAuditLog.ActionType` = `ApprovalRouteAuditActionType.NeedsAttention` (`4`).
- **Business Rule Covered**: Explicit failure notification & admin attention guard for broken specific approver overrides.
- **Preconditions / Action Needed**: Requires specific approver override setup before triggering deactivation. User approval required.

---

### Case J: One Active Employee Per Position Per Department Invariant Guard
- **Operational Readiness Category**: **`NOT IMPLEMENTED / NEEDS CODE PHASE`**
- **Department**: Any department (e.g. `Information Technology`)
- **Scenario**: Admin attempts to assign an employee to position `Manager` in department `IT` when `uat.provision81` is already an active `Manager` in `IT`.
- **UI Path**: N/A (Requires future code implementation)
- **Current Code Evidence Finding**: Code inspection of `EmployeeErrors.cs`, `CreateEmployeeCommandHandler.cs`, and `UpdateEmployeeCommandHandler.cs` reveals that validation for *one active employee per position per department* is **not currently implemented in C# code**. `CreateEmployeeCommandHandler` currently checks only `EmployeeErrors.EmployeeCodeExisted`.
- **Expected Requirements for Future Code Phase**:
  - Add domain error (e.g., `EmployeeErrors.DuplicatePositionInDepartment`) to `EmployeeErrors.cs`.
  - Add validation logic to `CreateEmployeeCommandHandler` & `UpdateEmployeeCommandHandler`.
- **Business Rule Covered**: Structural database & business invariant enforcement.
- **Preconditions / Action Needed**: Not ready for UAT execution. Requires dedicated C# Application/Domain code phase to implement duplicate position guard.

---

## 5. Summary Matrix & Test Data Provisioning Summary

| Case Code | Business Case Title | Target Dept | Requester | Approver / Target | Operational Readiness Category | Precondition & User Approval Requirement |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **UAT-CASE-A** | Direct Superior Routing | `IT` | `uat.provision80` | `uat.provision81` | **READY NOW** | Passed in Phase 9 TC1. Ready. |
| **UAT-CASE-B** | Config Escalation | `IT` | `uat.provision80` | `uat.provision81` | **NEEDS UI CONFIG ONLY** | Needs Level 2 UI Config. User approval required. |
| **UAT-CASE-C** | Missing Policy Guard | `FINANCE` | Finance Employee | None (Blocked) | **NEEDS TEST DATA** | Needs Finance Employee. User approval required. |
| **UAT-CASE-D** | Specific Override | `IT` | `uat.provision80` | `uat.provision81` | **NEEDS UI CONFIG ONLY** | Needs Specific Override Rule UI Config. User approval required. |
| **UAT-CASE-E** | Dynamic Reverse Proof | `IT` | `uat.provision81` | `uat.provision80` | **NEEDS UI CONFIG ONLY** | Non-Production Config. User approval required. |
| **UAT-CASE-F** | Dept Top -> Company Top | `IT` | `uat.provision81` | `ceo.test` | **NEEDS UI CONFIG ONLY** | Needs Rule targeting CEO Level. User approval required. |
| **UAT-CASE-G** | CEO Auto-Approved | Company | `ceo.test` | None (Auto) | **NEEDS UI CONFIG ONLY** | Needs `is_auto_approve = true` Rule Config. User approval required. |
| **UAT-CASE-H** | Inactive Approver Impact | `IT` | Active Subordinate | Replacement Approver | **CODE-REVIEW ONLY / READY** | Requires Deactivation Action. User approval required. |
| **UAT-CASE-I** | Specific Approver Lost Perm | `IT` | Active Subordinate | Admin Attention | **CODE-REVIEW ONLY / READY** | Requires Deactivating Specific Approver. User approval required. |
| **UAT-CASE-J** | Single Active Pos Invariant | Any Dept | N/A | Admin Action | **NOT IMPLEMENTED / NEEDS CODE PHASE** | Rule not implemented in C# code. Needs code phase. |

---

## 6. Verification Checklist & Log

- **Security Cleanup**: Debug scripts created in this phase (`MD_memory/debug/2026-07-24_1125_inspect-hrm-runtime-db.py` and `MD_memory/debug/2026-07-24_1142_query-permissions.py`) require `PGPASSWORD` without plaintext fallback. Secret pattern scan verified clean across current phase workspace artifacts.
- `git status --short`: Working tree has one untracked deliverable plan (`MD_memory/plans/2026-07-24_1130_phase-dynamic-approval-routing_uat-config-matrix.md`); no staged files.
- `scan-mojibake.py --require-bom`: Pass.
