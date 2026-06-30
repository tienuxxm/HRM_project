---
name: luc-hrm-refactor-guard
description: Use when Codex/Anti refactors, reviews, plans, edits, or verifies the copied LUC root project into HRM Leave Management. Applies to changes under HRM_Leave_Management, architecture decisions, module removal, auth/permission work, EF/MediatR/Clean Architecture changes, and any request involving "baseline", "refactor", "HRM", "Employee", "Department", "Leave", "Keycloak", "permission", or "xóa module cũ".
---

# LUC HRM Refactor Guard

## Core Rule

Preserve a working baseline first. Build HRM modules beside the copied LUC modules, verify them, then remove old restaurant/loyalty modules in small batches.

When architecture facts are needed, read `references/root-architecture.md`.

## Mandatory Workflow

1. **Restate the phase.** Say whether the task is baseline setup, HRM module build, UI shell change, or cleanup.
2. **Check current evidence.** Read the relevant file(s), `MD_memory/baseline_report.md`, `MD_memory/hrm_refactor_mapping.md`, and the current code before proposing changes.
3. **Sync plan before new work.** Before starting a new phase, update stale checklist/status in `MD_memory/hrm_refactor_mapping.md` or the active phase plan. Do not code when the previous phase status is stale.
4. **Use GitNexus before C# symbol edits.** Run impact analysis for every function/class/method that will be changed. Report blast radius. Warn if HIGH/CRITICAL.
5. **Prefer additive HRM work.** Add `Employee`, `Department`, `LeaveType`, `LeaveRequest`, etc. beside old modules before deleting old modules.
6. **Keep auth and permission stable.** Do not replace Keycloak, `User`, `Role`, `Permission`, `UserToRole`, or `RoleToPermission` unless the user explicitly approves a dedicated auth redesign.
7. **Verify before completion.** At minimum run build; for runnable changes also verify login, dashboard, and any touched route.
8. **Document the result.** Add or update a concise `MD_memory/` report with what changed, what was verified, and what risk remains.

## Plan/Report Naming Rules

Use timestamped names for all new planning and reporting documents created after 2026-06-25:

- Plan: `MD_memory/plans/YYYY-MM-DD_HHMM_<phase>_<slug>.md`
- Report: `MD_memory/reports/YYYY-MM-DD_HHMM_<phase>_<slug>_report.md`
- Temporary debug/script file: `MD_memory/debug/YYYY-MM-DD_HHMM_<slug>.<ext>` when a new temporary file is needed.

Rules:

- `<phase>` must be short and explicit, such as `phase-2b`, `phase-2c`, or `cleanup-1`.
- `<slug>` must be lowercase, ASCII/no Vietnamese accents, and hyphen-separated, such as `hrm-sidebar`, `employee-uat`, or `leave-request-design`.
- When asked for the next phase, read `MD_memory/hrm_refactor_mapping.md`, state the next phase and entry conditions, then propose the exact timestamped plan filename.
- Do not create a new phase plan/report under a generic name like `plan.md`, `report.md`, `final.md`, or `new-plan.md`.

## Keycloak/Auth/UAT Rules

- Use real Keycloak for UAT. `UseMockAuth` must be `false`.
- Local Keycloak is `http://localhost:8080`, realm `hrm`, client `hrm-web`.
- Use only the agreed UAT account unless the user changes it: `admin` or `admin@hrm.local` with password `Admin@123456`.
- Do not guess other passwords, change Keycloak passwords, create/edit Keycloak users, enable mock auth, or edit `JwtService`, `JwtBearerOptionsSetup`, `UserContext`, or auth config unless the current task is explicitly an auth task.
- If login fails, check container `keycloak-hrm`, the well-known endpoint, `UseMockAuth: false`, appsettings auth config, and wrong-password/right-password behavior. Report concrete logs instead of changing auth.
- Before UAT of a new module, seed its permissions first. 403 after login is an authorization/permission problem until proven otherwise, not a login problem.
- Do not continue browser/subagent UAT by default. Only run browser UAT when the user or Codex explicitly asks for it. Otherwise create a manual UAT report with URL, account, prerequisites, step-by-step actions, expected results, and failure capture instructions for the user to execute.
- UAT reports must state auth mode, `UseMockAuth` value, account used, permissions seeded, and exact routes tested.

## Architecture Guardrails

- Treat the root project as Clean Architecture inspired, not perfectly clean.
- Keep `Domain`, `Application`, `Infrastructure`, and `Web.Backend` boundaries.
- Put business rules in Application handlers/domain methods, not thick MVC controllers.
- Use MediatR command/query patterns already present in the codebase.
- Use EF configuration and repository patterns consistently.
- Use `Result<T>`/domain errors for business failures where the existing pattern supports it.
- Do not call external services from HRM DTO mapping unless a fallback/error boundary is designed.

## HRM Design Rules

- Do **not** rename `User` to `Employee`.
- Create `Employee` as a business entity with optional/required FK to `User` depending on the confirmed workflow.
- Keep `User` for login/auth/role/permission.
- Start with `Employee + Department` before leave workflows.
- Add HRM permissions deliberately, for example `VIEW_EMPLOYEE`, `UPDATE_EMPLOYEE`, `VIEW_DEPARTMENT`, `UPDATE_DEPARTMENT`.
- Reuse patterns from `Members` for rich CRUD/pagination and `Categories` for simple CRUD.
- Reuse dashboard/layout as shell only; do not keep booking/order business meaning.

## Dynamic Feature and Permission Rules

- Do not hardcode business capability by role name, username, email, user id, or magic GUID in application code.
- Prefer permission/config/feature-flag driven behavior for questions like "Can HR allocate leave?", "Can employees view their own balance?", or "Is import enabled?".
- Keep permission names consistent unless the user explicitly approves a different naming convention:
  - View: `VIEW_<RESOURCE>`
  - Create/update/manage resource data: `UPDATE_<RESOURCE>`
  - Do not invent `MANAGE_*`, `EDIT_*`, or `CREATE_*` when the active plan already uses `UPDATE_*`.
- For each configurable feature in a plan, state: config key/permission, local default, who can change it, seed requirement, and behavior when disabled.
- Before editing a module, scan the touched area for hardcoded role/user/permission/config values. Report suspected hardcodes and either remove them or document why they remain local-only/debug-only.

### LeaveBalance Phase 2C.2 Confirmed Defaults

- Use `decimal` for leave day quantities to support half-day values such as `0.5`.
- Admin/HR may manually enter and edit `UsedDays` to migrate starting balances.
- Do not persist `RemainingDays` when it can be calculated. Display it from `AllocatedDays - UsedDays`; later LeaveRequest availability uses `AllocatedDays - UsedDays - PendingDays`.
- Use `VIEW_LEAVE_BALANCE` and `UPDATE_LEAVE_BALANCE` unless the user explicitly changes the permission scheme.
- Employees can view their own leave balance; Admin/HR can view/manage according to permissions.
- Valid allocation year defaults to current year - 1 through current year + 1 unless the user changes the rule.

## Cleanup Rules

Do not delete old modules first. Remove them only after HRM replacements run.

For each cleanup batch:

1. List modules and dependencies.
2. Run GitNexus impact for edited/deleted symbols.
3. Remove Domain/Application/Infrastructure/Web parts together.
4. Clean DI registrations.
5. Clean EF configurations and migrations strategy.
6. Build and run.
7. Verify login/dashboard still work.
8. Update `MD_memory/` with results.

## Red Flags

Stop and ask or challenge the plan if you see:

- A request to delete many modules at once.
- A plan to rename `User` into `Employee`.
- A claim that a page is "OK" based only on HTTP 200 while CRUD was not tested.
- A claim that S3/Firebase/VnPay errors are business bugs without runtime evidence.
- A change to Keycloak/auth/permission without explicit approval.
- A config/debug file containing secrets being treated as production-ready.
- Mojibake or no-accent Vietnamese in project reports when the user requested Vietnamese with accents.

## Response Discipline

Always be willing to disagree with the user or a previous assistant if the code proves otherwise. Give file/line/log evidence. If the requirement is ambiguous, ask before making irreversible changes. Never say "complete" until verification evidence exists.
