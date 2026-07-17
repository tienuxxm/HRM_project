# Phase Design: Leave Request List UI Refactor & Pagination

- **Created**: 2026-07-16 14:00
- **Scope type**: UI refactor planning only
- **Target route**: `/leave-request`
- **Primary view files expected**:
  - `HRM_Leave_Management/Web.Backend/Views/LeaveRequest/Index.cshtml`
  - `HRM_Leave_Management/Web.Backend/Views/LeaveRequest/_CreateLeaveRequestPartial.cshtml`
  - `HRM_Leave_Management/Web.Backend/Views/LeaveRequest/_ConfirmCancelPartial.cshtml`
- **Reference business plan**: `MD_memory/plans/2026-06-26_1448_phase-2c3_leave-request.md`
- **Design source of truth**: approved Swiss International HR runtime patterns from Employee, Department, and Position screens

## Architecture Boundary

Preserve:
- `Web.Backend -> Application -> Domain`
- `Infrastructure -> Application/Domain`

This phase must not change backend business rules, auth, permissions, Keycloak, database schema, repositories, MediatR handlers, or C# domain/application behavior unless a separate impact-reviewed backend task is approved.

## Why This Screen Next

Leave Request List is the first primary workflow screen we are refactoring to the Swiss International HR design language. It integrates:
- Dense Swiss data table with status badges and action links.
- Conditional render paths (Employees see only their requests; Admins/HR/Approvers see all employees and filter options).
- Server-side pagination footer with showing numbers and PREV/NEXT buttons.
- Create request modal and cancel confirmation modal matching the Swiss modal pattern (no "SYSTEM: LIVE" labels, black headers, right-aligned action group).

## Design Decision

Do **not** generate a brand-new Stitch design unless the current approved patterns cannot express a required workflow.

For this screen, Anti will map the existing Leave Request UI onto approved patterns:
- **Desktop shell/layout**: follow Employee Directory / Department / Position Swiss International HR desktop pattern.
- **Mobile layout**: follow approved Department/Position mobile card-list pattern, structured specifically for Leave Requests (employee code, status badge, date range, duration, reason, details/cancel links).
- **Modal style**: reuse existing standardized Employee/Department/Position modal shell (black header, red close button, no "SYSTEM: LIVE" label, cancel & submit action buttons right-aligned).
- **Visual language**: Swiss International HR, monochrome, hairline borders, square corners, red only for destructive/critical states.

## Functional Requirements That Must Not Regress

Anti must preserve all existing functionality:
- `VIEW_LEAVE_REQUEST`, `CREATE_LEAVE_REQUEST`, and `APPROVE_LEAVE_REQUEST` permission-driven behavior.
- Self-view behavior: employees see only their own requests, employee filter dropdown is hidden.
- Admin/HR/Approver view behavior: users with `APPROVE_LEAVE_REQUEST` can view all requests and filter by Employee.
- Existing filters:
  - Employee (conditional)
  - Leave Type
  - Status
- Create flow: `createLeaveRequestForm` submit handler using AJAX post to `/leave-request/create`, showing toast, reloading on success.
- Cancel flow: `cancelLeaveRequest(id)` helper function using AJAX post to `/leave-request/cancel`, showing toast, reloading on success.
- Statuses: Pending, Approved, Rejected, Canceled must be displayed with corresponding styled Swiss badges.
- Toast feedback and modal triggers must work without JS errors.

## UI Refactor Goals

### Desktop

- Dense Swiss data table:
  - Headers: No | Employee | Leave Type | Period | Duration | Reason | Status | Process Info | Actions
  - Hairline borders (`border border-[#D1D1D1]`) and `#F5F5F5` header background.
  - Spacing/alignment matching standard Swiss tables.
- Filter controls above the table wrapped in a clean container with monospaced labels.
- Primary action button: `+ REQUEST LEAVE` or `+ CREATE REQUEST`.
- Compact status badges (using flat borders and clear contrasting readable text).
- Action links: compact underline decoration (`underline decoration-[#cfc4c5] underline-offset-4`).

### Mobile

- No left sidebar.
- Use bottom nav global pattern.
- Stacked cards layout instead of desktop table.
- Each card exposes:
  - Employee Code & Name
  - Status Badge (top right)
  - Leave Type
  - Period & Duration
  - Reason (truncated)
  - Actions & Process Info
- Vertical scroll clearance for bottom navigation footer.

### Modals

- Standard square modals:
  - Black header with uppercase white bold title.
  - Red close button (`#E62429` block, white close text/icon).
  - Hairline border, zero rounded corners.
  - Action group in footer right-aligned: `CANCEL` and primary action (`SUBMIT` or `CONFIRM`).
  - No `SYSTEM: LIVE` or irrelevant label in footer.

## Required Pre-Code Audit

Before editing, Anti must inspect and report:
- `LeaveRequestController` action parameters and return types (already completed: returns `PagedList<LeaveRequestResponse>`).
- Existing HTML structures, element IDs, and attributes of `Index.cshtml`.
- Modal forms: field names, input validation, scripts for AJAX submit and cancel handlers.
- Reusable UI styles from `LeaveBalance` pagination.

## Implementation Constraints

- Re-use existing Razor, Tailwind utility patterns.
- No new NuGet or npm packages.
- No Keycloak/auth edits.
- No C# modifications during the UI phase (since C# changes were already implemented).
- Do not stage, commit, or push.
- Do not use `git checkout`, `git restore`, `git reset`, or `git clean`.

## Verification Checklist

Minimum verification after code:
- Build successfully without errors.
- `/leave-request` renders correctly with dense Swiss styles.
- Filtering by Status, Leave Type, and Employee works and preserves pagination context.
- Pagination buttons (PREV/NEXT) work correctly, show correct `PAGE X OF Y` state, and hover/active states are completely readable.
- Mobile view renders correctly, scrolls cleanly, bottom nav does not overlap last elements.
- Submit modal and Cancel modal open/close correctly, forms submit successfully via AJAX, toasts show correct messages.
