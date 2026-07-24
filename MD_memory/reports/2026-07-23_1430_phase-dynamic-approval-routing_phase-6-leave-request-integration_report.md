# Phase 6 Implementation Report: Leave Request List & Detail Integration (`/leave-request`)



**Date**: 2026-07-23

**Author**: Senior .NET Fullstack Engineer & Technical Reviewer (Anti)

**Status**: COMPLETED & VERIFIED

**Architecture Boundary**:

- `Web.Backend -> Application -> Domain`

- `Infrastructure -> Application/Domain`



---



## 1. Executive Summary & Objective



Phase 6 successfully integrates Dynamic Approval Routing information into the existing Leave Request List (`/leave-request`) and Detail (`/leave-request/detail/{id}`) views.



### Scope Control & Boundaries Maintained:

- **Header / Sidebar / Layout Shell**: `_Layout.cshtml`, header, navigation menus, and global breadcrumbs remain 100% untouched.

- **Database Schema & Auth**: No EF migrations, no DB schema changes, no Keycloak/Auth configuration modifications.

- **In-place Decision Security**: Admin/HR users cannot approve or reject leave requests in-place from the Detail view unless they are explicitly assigned as the active approver for the request.



---



## 2. Technical Implementation Details



### 1. Application Layer (`Application/LeaveRequests/`)

- **`LeaveRequestResponse.cs`**:

  Added Dynamic Approval Routing assignment properties:

  - `AssignedApproverEmployeeId` (`Guid?`)

  - `AssignedApproverName` (`string?`)

  - `AssignedApproverCode` (`string?`)

  - `AssignmentStatus` (`string?`)

  - `AssignmentReason` (`string?`)

  - `NeedsRoutingAttention` (`bool`)

  - `SnapshotPolicyId` (`Guid?`)

  - `SnapshotRuleId` (`Guid?`)



- **`GetLeaveRequestsQueryHandler.cs`**:

  - Injected `ILeaveRequestApprovalAssignmentRepository` to load active routing assignments for the leave request batch.

  - Mapped assigned approver details (`AssignedApproverName`, `AssignedApproverCode`).

  - Evaluated `NeedsRoutingAttention = (AssignmentStatus == ApprovalAssignmentStatus.NeedsAdminAttention)`.

  - Updated `CanApprove` condition: First checks active `LeaveRequestApprovalAssignment`, falling back to legacy `LeaveApproverAssignment` when no dynamic assignment exists.



- **`GetLeaveRequestByIdQueryHandler.cs`**:

  - Loaded `LeaveRequestApprovalAssignment` for single leave request view.

  - Mapped routing details, status, snapshot audit IDs, and `NeedsRoutingAttention` flag.



### 2. Frontend Layer (`Web.Backend/Views/LeaveRequest/`)

- **`Index.cshtml` (List View)**:

  - **Desktop Table & Mobile Stacked Cards**:

    - For `Pending` leave requests:

      - If `NeedsRoutingAttention` is `true`: Displays Swiss-styled warning badge:

        `<span class="px-2 py-0.5 font-mono text-[9px] font-bold bg-[#E62429] text-white uppercase border border-[#E62429] inline-block">NEEDS ROUTING ATTENTION</span>`

      - If `AssignedApproverName` is available: Displays `Assigned Approver: Name (Code)`.

      - Non-pending requests continue displaying historical processed info.



- **`Detail.cshtml` (Detail View)**:

  - Added **Approval Routing Panel** card on the Left Rail:

    - Current Assigned Approver Name & Code (or `UNASSIGNED (ATTENTION)` warning).

    - Assignment Status & Assignment Reason.

    - Snapshot Audit Policy/Rule IDs (if available).

  - If `NeedsRoutingAttention` is `true`: Renders Swiss red warning banner directing HR/Admin to `/approval-routing/policies`.

  - **Official Decision Panel**: Remains strictly gated by `Model.Status == "Pending" && Model.CanApprove`.



---



## 3. Quality Assurance & Verification Results



| Verification Test | Lệnh / Công cụ | Kết Quả | Chi Tiết |

| :--- | :--- | :---: | :--- |

| **Git Working Tree Check** | `git status --short` | **VERIFIED** | Scope scoped to Application & LeaveRequest Views |

| **Git Diff Whitespace Check** | `git diff --check` | **PASSED** | **0 formatting/whitespace errors** |

| **C# Solution Build** | `dotnet build Application & Infrastructure` | **PASSED** | **0 Errors** |

| **BOM & Mojibake Check** | `python scan-mojibake.py <report> --require-bom` | **PASSED** | **UTF-8 BOM Validated**, **0 Mojibake hits** |



---



## 4. Manual UAT Instructions For User



### Environment Prerequisites:

- Keycloak container running on `http://localhost:8080` (`UseMockAuth = false`).

- Web Backend running locally.

- Account: `admin@hrm.local` / `Admin@123456`.



### Step-by-Step Testing Procedure:

1. **Access Leave Request List (`/leave-request`)**:

   - Log in as `admin@hrm.local`.

   - Open `/leave-request`.

   - Observe the table column `Process Info / Approver`:

     - For `Pending` requests with valid routing: Confirm `Assigned Approver: <Name> (<Code>)` is displayed.

     - For `Pending` requests with missing approvers: Confirm badge `NEEDS ROUTING ATTENTION` (red Swiss style) is displayed.



2. **Access Leave Request Detail (`/leave-request/detail/{id}`)**:

   - Click **Details** on any `Pending` leave request.

   - Observe the Left Rail under Employee Particulars.

   - Confirm card **Approval Routing** displays:

     - Current Approver

     - Routing Status

     - Assignment Reason

   - If the request has `NeedsRoutingAttention`, confirm red warning banner renders directing HR/Admin to `/approval-routing/policies`.

   - Confirm layout shell, sidebar (`APPROVAL ROUTING -> /approval-routing/policies`), and header remain completely unchanged.
