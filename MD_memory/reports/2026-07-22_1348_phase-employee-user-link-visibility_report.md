# Report — Phase Employee-User Link Visibility



> **Report Location**: `MD_memory/reports/2026-07-22_1348_phase-employee-user-link-visibility_report.md`

> **Phase**: `phase-employee-user-link-visibility`

> **Date**: 2026-07-22

> **Status**: ✅ PASSED & VERIFIED (Build 0 errors, 0 whitespace issues, 0 mojibake)



---



## 1. Root Cause Analysis (RCA) & Data Capability Matrix



| View | Query Model | Field Availability | Frontend Implementation Status | Next C# Step (If Any) |

|---|---|---|---|---|

| **`/employee` (Employee List)** | `List<Domain.Employees.Employee>` | ✅ `emp.UserId` available on `Employee` entity | ✅ **COMPLETED**: Rendered monochrome badges `LINKED ACCOUNT` (with User ID) vs `NO SYSTEM ACCOUNT`. PROVISION button disabled/hidden when linked. | No C# changes needed. |

| **`/user` (User List)** | `GetAllUserPagedResponse` (`UserResponse`) | ❌ `UserResponse` currently has **NO `EmployeeCode` or `EmployeeFullName` field** | ⚠️ **FRONTEND READY**: Updated `User/Index.cshtml` to render `LINKED EMPLOYEE` badge when `row.employeeCode` is present, defaulting to `SYSTEM ONLY` badge. | Proposal below to enrich `UserResponse` with `EmployeeCode` & `EmployeeFullName` in Application layer. |



---



## 2. Technical Proposal for User List Employee Link Data (C# Application Layer)



To populate the `LINKED EMPLOYEE` badge with actual employee code and full name on `/user`:



1. **DTO Update**:

   - File: `Application/Users/GetOne/UserResponse.cs`

   - Add properties: `public string? EmployeeCode { get; init; }`, `public string? EmployeeFullName { get; init; }`

2. **Handler Update**:

   - File: `Application/Users/GetAllPaged/GetAllUserPagedCommandHandler.cs`

   - Inject `IEmployeeRepository` (or join `employees` table).

   - Look up active employee records matching `user.Id` (`e.UserId == user.Id`) to populate `EmployeeCode` & `EmployeeFullName`.

3. **GitNexus Impact Analysis**:

   - Upstream callers of `GetAllUserPagedQuery` / `UserResponse`: `UserController` (DataTable API), `RoleController`. Risk level: **LOW**.



---



## 3. UI Modifications & Swiss Visual Compliance



1. **Employee List (`/employee`)**:

   - Badges: `LINKED ACCOUNT` (Black background with white mono text) vs `NO SYSTEM ACCOUNT` (Light gray background with gray mono text).

   - Provision Button: Fully operational for unlinked employees (`PROVISION ACCOUNT`), rendered as disabled label `[LINKED]` for already linked employees to prevent duplicate account provisioning attempts.

   - Design: 100% Swiss Monochrome (Black/White/Gray). Zero green/blue/yellow/amber, zero emojis.



2. **User List (`/user`)**:

   - Added `LINKED EMPLOYEE` column to desktop DataTable and mobile card list.

   - Gracefully displays `SYSTEM ONLY` badge for system users without linked employee profiles.



---



## 4. Verification Results



| Check | Tool / Command | Result |

|---|---|---|

| **Git Scope Check** | `git diff --name-status` | ✅ **Only 2 files modified**: `Views/Employee/Index.cshtml`, `Views/User/Index.cshtml` |

| **Git Whitespace Check** | `git diff --check` | ✅ **0 whitespace errors** |

| **Dotnet Build** | `dotnet build Web.Backend/Web.Backend.csproj --no-restore` | ✅ **Build Succeeded (0 Errors)** |

| **Mojibake & UTF-8 BOM** | `python scan-mojibake.py --require-bom` | ✅ **0 failures, 0 mojibake** |



---



## 5. Manual UAT Execution Steps



### Test Case 1: Employee List Link Visibility (`/employee`)

1. Open browser and navigate to `http://localhost:5300/employee`.

2. Login with Keycloak account: `admin` / `Admin@123456`.

3. Observe the `SYSTEM ACCOUNT` column:

   - Employees with existing user accounts show badge `LINKED ACCOUNT` and shortened User ID. The action shows `[LINKED]` (disabled).

   - Employees without user accounts show badge `NO SYSTEM ACCOUNT`. The action shows `PROVISION ACCOUNT`.

4. Click `PROVISION ACCOUNT` on an unlinked employee to confirm modal opens.



### Test Case 2: User List System-Only vs Linked Employee (`/user`)

1. Navigate to `http://localhost:5300/user`.

2. Observe the `LINKED EMPLOYEE` column:

   - Currently displays `SYSTEM ONLY` badge for system accounts.

   - UI is ready to display `LINKED EMPLOYEE (EMP001)` as soon as `UserResponse` C# query enhancement is approved.
