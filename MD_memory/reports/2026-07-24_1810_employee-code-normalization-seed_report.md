# Employee Code Normalization Seed Report

Date: 2026-07-24

Architecture boundary preserved:
- `Web.Backend -> Application -> Domain`
- `Infrastructure -> Application/Domain`

## Scope

This seed only normalized `employee.employee_code` values in the local runtime database.

Out of scope:
- No `user` table changes.
- No Keycloak/Auth changes.
- No role/permission changes.
- No leave request, approval routing, balance, department, or position mutation.
- No source code mutation in this seed step.

## Numbering Rule

Employee codes were assigned by:

1. `employee.created_date` ascending.
2. `employee.id` ascending as a deterministic tie-breaker.

Reason:
- `CreatedDate` is the closest available technical attribute for record creation order.
- `JoinDate` is a business date and may be earlier/later than actual record creation.
- `Id` alone is stable but not meaningful as a business ordering signal.

## Applied Mapping

| Old Code | New Code | Employee | Active |
|---|---|---|---|
| EMP001 | EMP001 | Huy Admin | true |
| EMP002 | EMP002 | Nguyen Van Employee | false |
| EMP-CEO-TEST | EMP003 | CEO Test | true |
| EMP-NV-TEST | EMP004 | Nhan Vien Test | true |
| EMP-TP-TEST | EMP005 | Truong Phong Test | false |
| EMP04 | EMP006 | uat.provision81 | true |
| EMP05 | EMP007 | uat.provision80 | true |
| MGR091507 | EMP008 | UAT_Delete_Manager_091507 | false |
| SUB091507 | EMP009 | UAT_Delete_Subordinate_091507 | false |
| WITH091507 | EMP010 | UAT_Delete_WithHistory_091507 | false |
| UAT-TC03-0927 | EMP011 | UAT_Delete_WithHistory_0927 | false |
| EMP005 | EMP012 | uaT.provision82 | true |

## Risk Assessment

Risk level: Medium-low for local UAT database.

Risk details:
- Low data integrity risk because leave requests and approval routing reference employees by `EmployeeId`, not by `EmployeeCode`.
- Medium UAT visibility risk because screenshots/runbooks mentioning old codes such as `EMP04`, `EMP05`, or `EMP005` are now stale.
- Unique constraint risk was controlled by a two-step transaction: temporary `TMP###` codes first, then final `EMP###` codes.

## Verification

- Employee count after seed: 12.
- Duplicate employee codes after seed: 0.
- Max normalized sequence after seed: 12.
- Next generated code expected from create flow: `EMP013`.
- `git diff --check`: pass.
- `dotnet build HRM_Leave_Management/Web.Backend/Web.Backend.csproj --no-restore`: build succeeded with 0 errors and 15 pre-existing warnings.

