# Root architecture facts for LUC -> HRM refactor

Use this reference when refactoring `HRM_Leave_Management`, reviewing an Anti plan, or deciding whether to keep/remove a module.

## Solution shape

- `Domain`: entities, value objects, domain events, repository interfaces, `Result/Error`.
- `Application`: MediatR command/query handlers, validation/logging behaviors, use-case orchestration.
- `Infrastructure`: EF Core `ApplicationDbContext`, repository implementations, Keycloak, AWS S3, Firebase, VnPay, eSMS, SendGrid, Quartz, Outbox.
- `Web.Backend`: ASP.NET Core MVC/Razor controllers, views, static assets, middleware.

## Core flow

MVC controller -> permission check via `IRoleService` -> MediatR command/query -> repository/DbContext or Dapper -> `Result<T>` -> Razor/JSON/redirect.

Login flow:

`LoginController.Login` -> `AdminLoginCommandHandler` -> internal `UserRepository` lookup -> `JwtService` password grant to Keycloak -> cookie `X-Access-Token` -> middleware adds `Authorization: Bearer ...` -> JwtBearer validation.

Permission flow:

`user -> user_to_role -> role -> role_to_permission -> permission`. Keycloak authenticates identity; DB permissions authorize actions.

## Keep for HRM

- `User`, `Role`, `Permission`, `UserToRole`, `RoleToPermission`.
- Keycloak/JWT cookie auth unless user explicitly changes auth strategy.
- `ApplicationDbContext`, repository base, `Result/Error`, domain event/outbox pattern.
- Layout shell and dashboard pattern, but not booking/revenue business meaning.
- MediatR behaviors and validators.

## Refactor warnings

- Do not rename `User` to `Employee`; create `Employee` with FK to `User`.
- Do not delete restaurant/loyalty modules before HRM replacements exist.
- Do not edit C# symbols before GitNexus impact analysis.
- Do not start a new phase when `MD_memory/hrm_refactor_mapping.md` or the active phase checklist is stale. Update the plan/checklist first.
- Do not create new generic memory files. Use timestamped names:
  - Plan: `MD_memory/plans/YYYY-MM-DD_HHMM_<phase>_<slug>.md`
  - Report: `MD_memory/reports/YYYY-MM-DD_HHMM_<phase>_<slug>_report.md`
- Do not trust HTTP 200 as CRUD verified; distinguish render OK from business UAT.
- Do not treat S3/Firebase/VnPay/eSMS failures as HRM logic failures without runtime evidence.
- Do not commit local secrets from `appsettings.json` or debug seed scripts.
- Do not change the agreed auth/UAT setup during non-auth tasks: Keycloak real mode, `UseMockAuth: false`, realm `hrm`, client `hrm-web`, account `admin` or `admin@hrm.local`, password `Admin@123456`.
- Do not treat 403 as a login failure after successful authentication. Check `permission` and `role_to_permission` first.
- Seed module permissions before browser UAT: Department (`VIEW_DEPARTMENT`, `UPDATE_DEPARTMENT`), Employee (`VIEW_EMPLOYEE`, `UPDATE_EMPLOYEE`), Leave (`VIEW_LEAVE`, `CREATE_LEAVE`, `APPROVE_LEAVE`).

## Known weak points

- `Infrastructure/DependencyInjection.cs` is large and must be cleaned carefully.
- Permission names are string literals in controllers.
- Some query handlers call `_awsS3Service.GetUrlPresign(...)` while mapping DTOs.
- Background jobs may contain old LUC/loyalty logic.
- Controllers can be thick; prefer moving HRM business rules to Application handlers.

## Recommended next refactor order

1. Design `Employee` and `Department`.
2. Add HRM CRUD module beside old modules.
3. Seed HRM permissions.
4. Verify build/run/login/dashboard/new pages.
5. Replace sidebar/dashboard meaning.
6. Add leave modules.
7. Cleanup old modules in small batches.
