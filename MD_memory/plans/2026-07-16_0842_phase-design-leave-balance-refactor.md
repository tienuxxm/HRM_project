# Phase Design: Leave Balance UI Refactor

- **Created**: 2026-07-16 08:42
- **Scope type**: UI refactor planning only
- **Target route**: `/leave-balance`
- **Primary view files expected**:
  - `HRM_Leave_Management/Web.Backend/Views/LeaveBalance/Index.cshtml`
  - `HRM_Leave_Management/Web.Backend/Views/LeaveBalance/_CreateLeaveBalancePartial.cshtml`
  - `HRM_Leave_Management/Web.Backend/Views/LeaveBalance/_UpdateLeaveBalancePartial.cshtml`
- **Reference business plan**: `MD_memory/plans/2026-06-26_0930_phase-2c2_leave-balance.md`
- **Design source of truth**: approved Swiss International HR runtime patterns from Employee, Department, and Position screens

## Architecture Boundary

Preserve:

- `Web.Backend -> Application -> Domain`
- `Infrastructure -> Application/Domain`

This phase must not change backend business rules, auth, permissions, Keycloak, database schema, repositories, MediatR handlers, or C# domain/application behavior unless a separate impact-reviewed backend task is approved.

## Why This Screen Next

Leave Balance is the next best screen after Employee, Department, and Position because it is a core HRM data screen and reuses the same visual grammar:

- list/table on desktop
- card list on mobile
- search/filter controls
- create/update modal
- action links/buttons
- pagination or summary footer

It also validates important HRM business concepts before moving into more complex workflows such as Leave Request approval:

- employee
- leave type
- year
- allocated days
- used days
- remaining days
- self-view versus admin/HR update permissions

## Design Decision

Do **not** generate a brand-new Stitch design unless the current approved patterns cannot express a required workflow.

For this screen, Anti should map the existing Leave Balance UI onto approved patterns:

- Desktop shell/layout: follow Employee Directory / Department / Position Swiss International HR desktop pattern.
- Mobile layout: follow approved Department/Position mobile card-list pattern.
- Modal style: reuse existing standardized Employee/Department/Position modal shell.
- Visual language: Swiss International HR, monochrome, hairline borders, square corners, red only for destructive/critical states.

## Functional Requirements That Must Not Regress

Anti must preserve all existing functionality:

- `VIEW_LEAVE_BALANCE` and `UPDATE_LEAVE_BALANCE` permission-driven behavior.
- Self-view behavior: employees with view-only access see only their own balances.
- Admin/HR update behavior: users with `UPDATE_LEAVE_BALANCE` can view/manage all balances.
- Existing filters:
  - employee
  - leave type
  - year
- Create flow.
- Update flow.
- Delete/soft-delete flow.
- `RemainingDays` remains display-only/calculated from `AllocatedDays - UsedDays`; do not persist or edit it directly.
- Decimal day values must remain supported, including `0.5`.
- Validation/error feedback must use the project’s existing toast/modal/error pattern, not native `alert()`/`confirm()`.

## UI Refactor Goals

### Desktop

- Preserve global shell/sidebar/header already normalized.
- Use a dense Swiss data table:
  - Employee
  - Leave Type
  - Year
  - Allocated Days
  - Used Days
  - Remaining Days
  - Status
  - Actions
- Place filters above the table, following Employee/Department/Position spacing.
- Primary action button: `+ ADD BALANCE` or `+ CREATE BALANCE`.
- Use compact status badges and action text links consistent with existing refactored screens.

### Mobile

- No left sidebar.
- Use bottom nav global pattern.
- Use stacked cards instead of desktop table.
- Each card should expose the key scan data:
  - employee name/code
  - leave type
  - year
  - allocated / used / remaining
  - status
  - actions
- Ensure vertical page scroll works to the last card and footer/pagination.
- Bottom nav must not cover the last card or summary footer.

### Modals

- Reuse the approved square modal shell:
  - black header
  - red close button
  - hairline border
  - no rounded corners
  - centered overlay
  - dim backdrop
- Create/update modal must preserve field names and model binding.
- Remaining Days should not be editable.
- If helper/caution text is used, it must explain a real rule, for example:
  - `Remaining days are calculated automatically from allocated minus used days.`
  - Do not show generic caution text permanently if it does not represent a concrete rule.

## Required Pre-Code Audit

Before editing, Anti must inspect and report:

- current `LeaveBalanceController`
- `Index.cshtml`
- `_CreateLeaveBalancePartial.cshtml`
- `_UpdateLeaveBalancePartial.cshtml`
- current scripts inside the view
- current permission-dependent UI branches
- current submit/update/delete AJAX or form behavior
- reusable UI patterns from Employee/Department/Position screens

Anti must identify all existing functions/events/IDs/data attributes that must be preserved to avoid the previous `undefined value` and broken modal issues.

## Implementation Constraints

- Prefer reusing existing Razor, Tailwind utility patterns, and shared modal behavior.
- No new NuGet or npm package.
- No Keycloak/auth edits.
- No C# symbol edits unless explicitly approved after GitNexus impact analysis.
- No DB/data mutation for design-only work.
- Do not stage, commit, or push.
- Do not run `git checkout`, `git restore`, `git reset`, or any command that reverts files unless the user explicitly approves the exact file list first.
- Do not rewrite or replace current global shell work. The mobile scroll, bottom nav, and desktop sidebar/header fixes were hard-won and must be treated as protected existing behavior.
- Do not run browser UAT unless explicitly approved by User/Codex; otherwise provide manual UAT steps.

## Strict Edit Scope For Anti

This phase is a **content-area and raw modal refactor only**.

Allowed runtime edit candidates:

- `HRM_Leave_Management/Web.Backend/Views/LeaveBalance/Index.cshtml`
- `HRM_Leave_Management/Web.Backend/Views/LeaveBalance/_CreateLeaveBalancePartial.cshtml`
- `HRM_Leave_Management/Web.Backend/Views/LeaveBalance/_UpdateLeaveBalancePartial.cshtml`

Protected runtime files unless the user opens a separate shell-specific task:

- `HRM_Leave_Management/Web.Backend/Views/Shared/_Layout.cshtml`
- `HRM_Leave_Management/Web.Backend/wwwroot/css/styles.css`
- `HRM_Leave_Management/Web.Backend/tailwind.config.js`
- any global JavaScript/CSS file
- any Employee, Department, or Position view that already passed UAT

Anti must not "fix" shell/sidebar/header/footer/mobile bottom-nav while working on Leave Balance.
If Anti discovers a shell issue while testing Leave Balance, it must report it as a separate finding and stop for approval.

Before any edit, Anti must print the exact file list it intends to modify. If the list contains a protected file, Anti must ask for explicit approval and explain why a Leave Balance content refactor cannot avoid that file.

## Protected Existing UI Scope

The Leave Balance refactor must not regress the global shell and responsive behavior that already passed user UAT:

- Desktop sidebar remains visible, fixed-width, and independently scrollable when its content exceeds viewport height.
- Desktop main content must not be covered by the sidebar.
- Mobile must not show the desktop left sidebar.
- Mobile uses the approved bottom navigation pattern.
- Mobile vertical page scroll must reach the last card, summary/footer, and pagination.
- Bottom navigation must not cover the last visible item; content needs adequate bottom padding.
- Mobile horizontal bottom navigation labels must remain clear and unique; do not reintroduce repeated labels such as multiple `Leave` items.
- Do not edit `HRM_Leave_Management/Web.Backend/Views/Shared/_Layout.cshtml` for this phase unless the user explicitly approves a shell-specific subtask.

Before coding Leave Balance, Anti must capture the current shell behavior as a baseline in the proposal:

- desktop sidebar behavior
- mobile bottom-nav behavior
- mobile vertical page scroll behavior
- exact files that are protected and will not be touched

## Git Safety For Anti

The workspace is expected to be dirty from previous phases. Anti must work with the dirty tree, not erase it.

Before coding:

- Run `git status --short`.
- Run `git branch --show-current`.
- Report only files relevant to this task and explicitly say the rest are pre-existing/out of scope.

Forbidden without explicit user approval:

- `git checkout`
- `git restore`
- `git reset`
- `git clean`
- deleting generated-looking files just to make status cleaner
- broad `git add .`

After coding:

- Run `git diff --check -- <changed LeaveBalance files>`.
- Run `git diff --name-status -- <changed LeaveBalance files>`.
- Do not stage, commit, or push.

Anti's report must not say "working tree clean" unless `git status --short` actually proves it.

## Verification Checklist

Minimum verification after code:

- `git diff --check -- <changed files>`
- Build Web.Backend using a method that avoids apphost lock if server is running.
- Confirm no JavaScript console errors on `/leave-balance`.
- Desktop manual/UAT:
  - list renders
  - filters still work
  - create modal opens and submits
  - update modal opens with correct values, no `undefined`
  - delete flow still works
  - permission-based buttons appear/hide correctly
- Mobile manual/UAT:
  - card list renders
  - vertical scroll reaches last item/footer
  - bottom nav does not cover final content
  - create/update modal fits viewport and scrolls internally if needed

## Out Of Scope

- Redesigning User/Role/Permission screens.
- Changing Leave Balance business rules.
- Changing permission names.
- Changing Leave Request availability calculation.
- Rebuilding the full design system.
- New Stitch exploration unless the approved pattern cannot cover a required interaction.

## Suggested Next Decision

If Leave Balance refactor passes UAT, proceed next to either:

1. `Leave Type` if the goal is to finish simple master-data screens first.
2. `Leave Request List` if the goal is to move into workflow screens. This one should go through Stitch review before runtime refactor because approval states and actions are more complex.
