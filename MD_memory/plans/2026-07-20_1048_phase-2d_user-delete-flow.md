# Plan: User Delete Flow UI Refactoring

## 1. Context & Scope
- **Objective:** Refactor the UI/UX for the Delete User confirmation modal and flow to align with Swiss International HR Style 06.
- **Allowed File to Modify:** `HRM_Leave_Management/Web.Backend/Views/User/Index.cshtml`
- **Boundary Constraint:** Strictly preserve Clean Architecture. No modifications to `UserController.cs`, Application, Domain, DB migrations, or Keycloak settings.

## 2. Technical Details & Audit
- **Removal Button:** The delete action is triggered by clicking the "Remove" link in the Actions column of the DataTable or mobile card, which calls `toggleDeleteModal('${row.id}')`.
- **Modal Generation:** The HTML modal structure is currently generated dynamically using `getDeleteModal` defined in `site.js`.
- **Action Endpoint:** The post request is handled via the `deleteAction` JS function which hits `/User/Delete/${id}`.
- **Preserved Contract:** Keep the parameters of `deleteAction` and `toggleDeleteModal` intact.

## 3. UI Refactoring Details
- Introduce `getUserDeleteModal(...)` function locally in `Index.cshtml` to replace `getDeleteModal(...)`. This overrides the shared modal layout specifically for the User Management module.
- **Swiss Style Customizations:**
  - Flat structure: `rounded-none`, `shadow-none`.
  - Border: Hairline border `#D1D1D1`.
  - Color scheme: Destructive red `#E62429` header with bold white text.
  - Buttons: Cancel button as outline (black border, white background), Remove button as solid destructive red.
  - Centering & Viewport safety: Ensure flex layout with `items-center justify-center` and higher `z-index` values (`z-100` for modal, `z-90` for backdrop) to avoid mobile navigation overlap.

## 4. Technical Debt Acknowledgment
- **Employee Orphan Risk:** Currently, deleting a Keycloak user does not cascade delete or nullify the associated `Employee.UserId` property in the local application DB, which may cause database orphan records. This issue is outside the current UI scope and must not be touched during this phase.

## 5. Verification Plan
- `git status --short`
- `git branch --show-current`
- `git diff --check`
- `dotnet build HRM_Leave_Management/Web.Backend/Web.Backend.csproj --no-restore`
- Manual/Browser UAT.
