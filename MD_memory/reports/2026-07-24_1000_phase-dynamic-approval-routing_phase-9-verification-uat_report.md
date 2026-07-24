# Phase 9 — Verification & UAT Strategy for Dynamic Approval Routing Engine Report



**Date**: 2026-07-24

**Author**: Technical Reviewer & Fullstack Engineer (Anti)

**Architecture Boundary**:

- `Web.Backend -> Application -> Domain`

- `Infrastructure -> Application/Domain`



---



## 1. Executive Summary & Final Verification Status



This document records the **Phase 9 Verification & User Acceptance Testing (UAT) Execution** for the **Single-Step Dynamic Superior Approval Routing Engine** (`ApprovalRoutePolicy`, `ApprovalRouteRule`, `LeaveRequestApprovalAssignment`).



### Key Status Summary:

1. **Phase 9 Group A UAT**: **100% PASSED & VERIFIED BY USER**.

   - **TC1 (IT Department Superior Approval Routing)**: **PASSED (USER VERIFIED)**.

   - **TC6 (Legacy LeaveApproverAssignment Read-Only Audit Mode)**: **PASSED (USER VERIFIED)**.

2. **Phase 9 Group B (DEFERRED)**: TC2, TC3, and TC4 remain deferred as documented (requiring missing test data or UI feature expansion).

3. **Phase 9 Group C (TECHNICAL CODE-REVIEWED)**: TC5 (Domain Rerouting & Audit Logs) and TC6 API Endpoint Blocking verified via code review and controller route inspection.

4. **Clean Architecture Isolation**: Preserves `Web.Backend -> Application -> Domain` and `Infrastructure -> Application/Domain`. Zero C# code, view, or DB mutations were performed in Phase 9.



---



## 2. Categorized UAT Test Matrix & Execution Summary



| Test Case | Scenario / Category | Account / Route | Target Result | Execution Status |

| :--- | :--- | :--- | :--- | :--- |

| **TC1** | IT Department Superior Approval Routing (Group A: READY NOW) | `uat.provision80` / `uat.provision81`<br>`/leave-request/detail/{id}` | Dynamic assignment to `uat.provision81` with decision panel on `/leave-request/detail/{id}`. | **PASSED (USER VERIFIED)** |

| **TC6 (UI)** | Legacy `LeaveApproverAssignment` Read-Only Audit Mode (Group A: READY NOW) | `admin`<br>`/leave-approver-assignment` | English read-only banner, historical grid intact, `Actions` column = `No Permission`, zero mutation controls. | **PASSED (USER VERIFIED)** |

| **TC2** | HRM Department Direct Header Routing (Group B: DEFERRED) | HRM Employee | Dynamic direct header routing. | **DEFERRED** (Missing test data) |

| **TC3** | Unconfigured Department Guard (Group B: DEFERRED) | Unconfigured Staff | Blocks leave creation with policy missing error message. | **DEFERRED** (Missing test data) |

| **TC4** | Specific Approver Override UI (Group B: DEFERRED) | `admin`<br>`/approval-routing/policies/detail` | Override candidate sequence mode with specific approver employee. | **DEFERRED** (UI scope) |

| **TC5** | Inactive Approver Rerouting & Audit Log (Group C: TECHNICAL) | `/approval-routing/levels/assignments` | Reroute success (`AssignmentStatus = Assigned`, Audit `Reassigned`); Reroute fail (`NeedsAdminAttention`, Audit `NeedsAttention`). | **TECHNICAL CODE-REVIEWED** |

| **TC6 (API)**| Legacy Mutation Endpoints Blocked Check (Group C: TECHNICAL) | `POST /leave-approver-assignment/*` | Returns HTTP 200 JSON `{ success: false, message: "This feature has been retired..." }`. | **TECHNICAL CODE-REVIEWED** |



---



## 3. Detailed UAT Execution Record



### TC1: IT Department Superior Approval Routing — PASSED

- **Verification Date**: 2026-07-24

- **Verified By**: User & Codex

- **Requester Account**: `uat.provision80` / `Admin@123456`

- **Assigned Approver Account**: `uat.provision81` / `Admin@123456`

- **Observed Instance Details**:

  - **Leave Type**: `Sick Leave`

  - **Date Range**: `28 JUL 2026` (Duration: `1 day`)

  - **Detail View Route**: `/leave-request/detail/a934199b-c467-4819-a69e-d77424a26d63`

- **Verified UI Attributes**:

  1. **Dashboard W4 Queue**: Widget W4 `APPROVAL QUEUE` for `uat.provision81` displays the new request from `uat.provision80`.

  2. **Detail Approval Routing Panel**:

     - Current Approver: `uat.provision81 (EMP04)`

     - Routing Status: `ASSIGNED`

     - Assignment Reason: `DirectLevelMatch`

     - Snapshot Audit Trace: Rendered cleanly with Policy ID & Rule ID.

  3. **Official Decision Panel**:

     - Rendered strictly for assigned approver `uat.provision81` on `/leave-request/detail/a934199b-c467-4819-a69e-d77424a26d63`.

     - `APPROVE REQUEST` button: Present and active.

     - `REJECT REQUEST` button: Present and active.

- **Evidence Reference**:

  `Conversation screenshot evidence observed from User; no local evidence file captured in workspace.`



---



### TC6: Legacy `LeaveApproverAssignment` Read-Only Audit Mode — PASSED

- **Verification Date**: 2026-07-24

- **Verified By**: User & Codex

- **Route**: `http://localhost:5300/leave-approver-assignment`

- **User Account**: `admin`

- **Verified UI Elements**:

  1. **Page Title**: `APPROVAL CONFIGURATIONS (LEGACY)`

  2. **Banner Badge**: `LEGACY READ-ONLY`

  3. **Banner Message**: `LEGACY APPROVER ASSIGNMENTS - READ-ONLY AUDIT MODE`

  4. **CTA Link**: `GO TO DYNAMIC APPROVAL ROUTING POLICIES` (`/approval-routing/policies`) -> Functioning correctly.

  5. **Historical Grid**: Legacy assignment rows render intact for historical audit purposes.

  6. **Actions Column**: Displays `No Permission` text badge.

  7. **Mutation Controls**: Zero Add / Edit / Remove mutation controls rendered.

- **Evidence Reference**:

  `Conversation screenshot evidence observed from User; no local evidence file captured in workspace.`



---



## 4. Technical Inspection Record (Group C)



### TC5: Inactive Approver Rerouting & Audit Log Handling — TECHNICAL CODE-REVIEWED

- **Inspection Finding**: Verified via handler logic code-review:

  - Route: `/approval-routing/levels/assignments`

  - Handler: `RerouteApprovalAssignmentCommandHandler`

  - **Successful Reroute**: `AssignmentStatus` remains `Assigned` (`1`), while `ApprovalRouteAuditLog.ActionType` records `Reassigned` (`2`).

  - **Unresolvable Reroute** (no replacement candidate): `AssignmentStatus` transitions to `NeedsAdminAttention` (`2`), while `ApprovalRouteAuditLog.ActionType` records `NeedsAttention` (`4`).

  - Table schemas verified: `leave_request_approval_assignment` and `approval_route_audit_log`.



### TC6 (API Part): Legacy Mutation Endpoints Blocked Check — TECHNICAL CODE-REVIEWED

- **Inspection Finding**: Verified via `LeaveApproverAssignmentController.cs` route analysis:

  - Endpoints: `POST /leave-approver-assignment/create`, `POST /leave-approver-assignment/update`, `POST /leave-approver-assignment/delete`.

  - Response: HTTP 200 JSON `{ success: false, message: "This feature has been retired (LEGACY READ-ONLY)..." }`.



---



## 5. Strict Verification Checkpoint



### 5.1. Purge & Permission Code Scans

- **Purge Check (`HRM_Leave_Management/Application/LeaveRequests`)**: **0 Hits (Clean)**.

- **Permission Naming Audit**: **Permission Naming Debt Found & Documented (ACCEPTED FOR POST-UAT)**. Temporary gates `VIEW_LEAVE_APPROVER_ASSIGNMENT` and `UPDATE_LEAVE_APPROVER_ASSIGNMENT` verified.



### 5.2. Git Working Tree & Solution Build Audit

- `git status --short`: **Working tree is dirty but reviewed; no staged files; no `bin/`, `obj/`, or `.gitnexus/` appeared in the reported git status output**.

- `git diff --name-status`: Verified.

- `git diff --check`: **No whitespace errors (LF/CRLF line-ending warnings only)**.

- **Solution Build Check**: **BUILD SUCCESSFUL (0 Errors, 15 pre-existing warnings)** *(verified from previous build checkpoint)*.

- `scan-mojibake.py --require-bom`: **Exit code 0 (UTF-8 BOM OK, Mojibake Clean)**.
