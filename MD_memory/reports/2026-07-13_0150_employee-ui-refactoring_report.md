# UAT Report: Employee UI Refactoring (Swiss International Style)

- **Date:** 2026-07-13
- **Phase:** Employee UI Refactor (DP Style 06 - Swiss International)
- **Status:** READY FOR USER UAT

---

## 1. Preserved Architecture Boundaries

The UI changes are strictly confined to the Presentation/Web layer. No backend C#, Domain, Application, or Infrastructure layers were modified:
- `Web.Backend -> Application -> Domain` (Preserved)
- `Infrastructure -> Application/Domain` (Preserved)

---

## 2. Refactored Component Status & Scope Clarity

### A. Component Status
*   **Employee Roster Content:** **DONE** (Refactored to Swiss International with responsive table on Desktop and stacked cards on Mobile).
*   **Create / Edit Modals:** **DONE** (Refactored `_CreateEmployeePartial.cshtml` and `_UpdateEmployeePartial.cshtml` with square corners, monochrome inputs, uppercase headings/labels, and black buttons).
*   **Provision Modal:** **DONE** (Refactored `_ProvisionAccountPartial.cshtml` with standard 0px corner styling and monochrome layout).
*   **Delete Modal:** **DONE** (Refactored shared `_ConfirmDeletePartial.cshtml` to Swiss style with a Swiss Red button).
*   **Global App Shell (Sidebar/Header/Topbar/Menu):** **PENDING** (Currently retains the legacy yellow/orange colors, "LOGO HERE", and old style layout. It has not been updated yet as it is shared globally across all pages).

### B. Verification of `_Layout.cshtml` Diff
*   The C# permission check (`RoleService.checkRoleExist(..., "VIEW_WORK_CALENDAR", ...)`) currently present in `_Layout.cshtml` is **completely outside the current Employee UI scope**. It was implemented as part of the previous **Phase 3D (Work Calendar UI)** to control menu item visibility.
*   No new authentication or permission logic has been or will be introduced in this phase.


---

## 3. Build Verification

- **Command executed:** `dotnet build HRM_Leave_Management/LUC.sln`
- **Status:** **PASS** (0 Errors, 217 Warnings - all warnings are pre-existing MVC1000/NU1701/NU1903/NETSDK1138 warnings)

---

## 4. Manual UAT Steps for the User

Please execute the following manual tests to verify the UI layout changes on desktop and mobile:

### Test Case 1: Desktop View Validation (Employee Roster & Modals)
1. **Access Portal**: Open the application, sign in with the admin account (`admin` / `Admin@123456`), and navigate to `/Employee`.
2. **Check Grid Layout**: Confirm that the table is displayed with 0px sharp corners, hairline gray borders (`#D1D1D1`), uppercase table headings, and solid styling.
3. **Add Employee**: Click the "Add Employee" button. Ensure the modal opens, all input boxes have square corners, labels are uppercase, and "SAVE EMPLOYEE" is a solid black button.
4. **Edit Employee**: Click "Edit" for an employee. Verify that the pre-populated update modal conforms to the same Swiss International style.
5. **Account Provisioning**: Click "Provision" for an unprovisioned employee. Verify the modal structure, select roles using CTRL, and click the black solid "PROVISION ACCOUNT" button.
6. **Confirm Delete**: Click "Remove" for an employee. Verify that the confirmation modal has a bold uppercase header and a Swiss Red "Confirm" button.

### Test Case 2: Mobile View Validation (Responsive Roster & Modals)
1. **Toggle Mobile Emulation**: Press F12 in your browser and switch to mobile view (e.g., iPhone 12/14/Pro width: ~390px).
2. **Check Card Layout**: Confirm that the desktop table is hidden, and the employee roster is displayed as a list of stacked white cards with grey hairline borders.
3. **Card Content**: Each card should clearly display the employee's No., Code, Full Name, Department, Position, Join Date, Status badge (Active/Inactive), and Account status badge (Provisioned/Provision).
4. **Modals on Mobile**: Tap "Provision", "Edit", or "Remove" on a card. Verify that the modals render correctly without breaking layout boundaries on mobile widths.
