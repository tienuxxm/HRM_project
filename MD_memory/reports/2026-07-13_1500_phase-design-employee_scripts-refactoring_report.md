# TECHNICAL REPORT & UAT MANUAL INSTRUCTIONS: EMPLOYEE SCRIPT REFACTORING

## 1. Technical Context & Issue Resolution

On the `/employee` page, two main categories of runtime JavaScript errors/warnings were reported:
1. **`$ is not defined`**: Triggered because inline `<script>` blocks in partial views (`_CreateEmployeePartial.cshtml`, `_UpdateEmployeePartial.cshtml`) and the main view (`Index.cshtml`) were being parsed and executed during page rendering before jQuery was loaded. (jQuery is registered at the very bottom of the Layout body).
2. **Flowbite Warnings (`Modal with id... has not been initialized`)**: Occurred because modal elements without an active button trigger (e.g. `_ProvisionAccountPartial` containers for already-provisioned employees whose triggers are disabled/hidden) were rendered, leading to mismatches when Flowbite initialized triggers.

### Applied Fixes
- **Script Consolidation & Delegation**: Removed all inline `<script>` blocks from `_CreateEmployeePartial.cshtml` and `_UpdateEmployeePartial.cshtml`.
- **Global Script Injection**: Moved all JavaScript logic (creating employee, updating employee, provisioning account, directory filtering, and dynamic toast notifications) into the `@section Scripts` block at the bottom of `Employee/Index.cshtml`, ensuring it runs safely after jQuery loads.
- **Event Delegation**: Refactored individual update actions to use a single delegated click listener (`$(document).on('click', '.save-employee-btn', ...)`) mapped via the `data-employee-id` attribute, eliminating duplicate event listeners and minimizing script size.
- **Conditional Provision Modal Rendering**: Updated the modal container generation loop in `Index.cshtml` to only include `_ProvisionAccountPartial` if `emp.UserId == null`. Already provisioned employees no longer pollute the DOM with unused modals, eliminating Flowbite's uninitialized modal warnings.

---

## 2. Manual UAT Test Plan

> [!IMPORTANT]  
> All changes are local only. Verify on your local environment at `http://localhost:5300` using the following step-by-step instructions.

### Prerequisites
- **Authentication**: Keycloak local container must be running.
- **Credentials**: Log in using an authorized account (e.g., `admin` / `Admin@123456`).
- **Permissions**: Ensure your role has `VIEW_EMPLOYEE` and `UPDATE_EMPLOYEE` permissions seeded.

---

### Test Case 1: Page Load & Script Initialization (Clean Console)
1. Open Google Chrome or any modern browser.
2. Press `F12` to open the Developer Tools, and navigate to the **Console** tab.
3. Access the URL: `http://localhost:5300/employee`.
4. **Expected Result**: 
   - The employee directory loads successfully.
   - The console contains **no** `$ is not defined` errors.
   - The console contains **no** Flowbite warning about uninitialized modals (e.g., `Modal with id provisionAccountModal-... has not been initialized`).

---

### Test Case 2: Create Employee Action
1. On the Employee directory page, click the **[ADD EMPLOYEE]** action button.
2. In the modal, fill in:
   - **Employee Code**: `EMP-UAT-99`
   - **Full Name**: `UAT Script Tester`
   - **Join Date**: Select today's date.
3. Click **[CREATE EMPLOYEE]**.
4. **Expected Result**:
   - The AJAX post executes without dependency errors.
   - A success toast is displayed.
   - The page reloads and the new employee `EMP-UAT-99` is present in the list.

---

### Test Case 3: Update Employee Action
1. Find the newly created employee `EMP-UAT-99` in the list.
2. Click the **[EDIT]** button in their row to open the edit modal.
3. Modify the **Full Name** to `UAT Script Tester Updated`.
4. Click **[SAVE CHANGES]**.
5. **Expected Result**:
   - The edit modal closes.
   - A success toast is displayed.
   - The page reloads, showing the updated name in the table.
   - Verify that clicking close/cancel triggers Flowbite's native modal toggling correctly.

---

### Test Case 4: Account Provisioning Modal & Flowbite Integration
1. Find the employee `EMP-UAT-99` (who is not provisioned yet, `UserId` is null).
2. Click the **[PROVISION]** action link.
3. Fill in the credentials:
   - **Username**: `uattester`
   - **Email**: `uattester@hrm.local`
   - **Password**: `Admin@123456`
   - **Role**: Select at least one role (e.g. `Employee`).
4. Click **[PROVISION ACCOUNT]**.
5. **Expected Result**:
   - The account is provisioned successfully, showing a success toast.
   - The page reloads and the action status changes to `PROVISIONED` (disabled/grayed out).
   - Reload the page and verify no Flowbite modal registration error appears now that the modal container for this user is no longer rendered in the DOM.

---

## 3. Scope & Version Control Hygiene

- **Files Edited**:
  1. `HRM_Leave_Management/Web.Backend/Views/Employee/_CreateEmployeePartial.cshtml` (Removed inline scripts)
  2. `HRM_Leave_Management/Web.Backend/Views/Employee/_UpdateEmployeePartial.cshtml` (Removed inline scripts, added `data-employee-id` class hook)
  3. `HRM_Leave_Management/Web.Backend/Views/Employee/Index.cshtml` (Consolidated JS scripts into `@section Scripts`, added conditional partial rendering)
- **Git Checkpoint Status**: Clean working tree changes remain unstaged. No commits or pushes have been made to the remote `origin/main` branch in compliance with project rules.
