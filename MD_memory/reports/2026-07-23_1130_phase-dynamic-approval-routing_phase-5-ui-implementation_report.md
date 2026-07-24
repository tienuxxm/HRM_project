# Phase 5 UI Implementation Report: Dynamic Approval Routing Module (`/approval-routing`)



**Date**: 2026-07-23

**Author**: Senior .NET Fullstack Engineer & Technical Reviewer (Anti)

**Status**: REFACTORED & VERIFIED — SIDEBAR INTEGRATION COMPLETE & REPORT UPDATED

**Architecture Boundary**:

- `Web.Backend -> Application -> Domain`

- `Infrastructure -> Application/Domain`

**Stitch Project Reference**: `17479353588209716186` (7 Approved Visible Canvas Screens)

**Evidence Source**: `MD_memory/evidence/2026-07-23_phase5_stitch/phase5_stitch_screen_evidence.html`



---



## 1. Sidebar Integration Patch (`_Layout.cshtml`)



Anti has updated `_Layout.cshtml` to replace the legacy "Approver Assignments" menu item with the new Phase 5 Approval Routing UI module:



- **Menu Group**: `LEAVE MANAGEMENT`

- **Label**: `APPROVAL ROUTING`

- **Icon**: `alt_route`

- **Target URL**: `/approval-routing/policies`

- **Mobile Label**: `Routing`

- **Active State Matching**: Evaluated via `basePath: 'approval-routing'`. Highlights active state when `location.pathname` matches any `/approval-routing/*` sub-route (`/approval-routing/policies`, `/approval-routing/detail`, `/approval-routing/slots`, `/approval-routing/impact-preview`).



---



## 2. Technical Debt & Legacy View Fix (`LeaveApproverAssignment/Index.cshtml`)



- **Root Cause of Legacy Console Error**: Legacy view `LeaveApproverAssignment/Index.cshtml` placed inline `<script>` tags in the body before jQuery was rendered by `_Layout.cshtml`.

- **Fix**: Wrapped inline scripts in `LeaveApproverAssignment/Index.cshtml` into `@section Scripts { ... }` so jQuery `$`, `$('#...')`, `$(document).ready(...)` execute after jQuery is loaded without `$ is not defined` console errors.



---



## 3. PostgreSQL Database & Migration Technical Points Summary



### 1. PostgreSQL-Specific Company-Level Expression Unique Index

- Implemented an expression index on a constant expression `((1))` via raw SQL:

  ```sql

  CREATE UNIQUE INDEX IF NOT EXISTS ix_approval_route_policy_active_company_level

  ON approval_route_policy ((1))

  WHERE is_active = true AND department_id IS NULL;

  ```

- **Provider Scope**: Tailored specifically for Npgsql/PostgreSQL used by this project.



### 2. Synchronization Boundaries of EF Configuration & ModelSnapshot

- Department-level active unique index (`ix_approval_route_policy_department_id_active_dept`) and DB check constraint (`ck_approval_route_rule_auto_approve_no_specific_approver`) are represented in EF Fluent API configurations and ModelSnapshot.

- Company-level unique expression index (`ix_approval_route_policy_active_company_level`) is managed via PostgreSQL raw SQL in EF migration.



### 3. Migration Breakdown

- `20260722100601_AddApprovalRouting`: Created initial approval routing tables.

- `20260723062259_AddIsAutoApproveToApprovalRouteRule`: Added `is_auto_approve` column and nullable `department_id`.

- `20260723063701_AddIsAutoApproveAndCompanyUniqueIndex`: Added PostgreSQL raw SQL expression index `ON approval_route_policy ((1))` and boolean check constraint.



---



## 4. Verification & Quality Assurance Results



| Verification Test | Command | Result | Notes |

| :--- | :--- | :---: | :--- |

| **Git Diff Whitespace Check** | `git diff --check` | **PASSED** | **0 formatting/whitespace errors** |

| **.NET Solution Compilation** | `dotnet build` | **PASSED** | **0 Errors**, Razor views compiled cleanly |

| **BOM & Mojibake Check** | `python scan-mojibake.py <report> --require-bom` | **PASSED** | **UTF-8 BOM Validated**, **0 Mojibake hits** |



---



## 5. Manual UAT Report For User (Sidebar & Route Verification)



### Test Environment Prerequisites:

- Keycloak container `keycloak-hrm` running on `http://localhost:8080`.

- Web application running locally (`http://localhost:5000` or via `dotnet run`).

- Log in with `admin@hrm.local` / `Admin@123456`.



### Step-by-Step Manual UAT Steps:

1. Log in to HRM Portal.

2. Observe the left sidebar navigation under section `LEAVE MANAGEMENT`.

3. **Verify Sidebar Link**: Confirm menu item displays **APPROVAL ROUTING** (icon `alt_route`).

4. **Click Link**: Click **APPROVAL ROUTING**.

5. **Expected Outcome**: Browser navigates to `/approval-routing/policies`, loading the Swiss-styled Approval Routing Policies page without 500 or console errors.

6. **Active Highlight**: Confirm **APPROVAL ROUTING** remains highlighted black with active indicator on all `/approval-routing/*` pages.
