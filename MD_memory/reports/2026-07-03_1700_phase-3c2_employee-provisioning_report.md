# Báo Cáo Phase 3C.2 — Employee Account Provisioning

**Ngày tạo:** 2026-07-03  
**Cập nhật lần cuối:** 2026-07-04 11:30  
**Trạng thái:** ✅ Technical readiness PASS — Chờ manual API UAT + git stage/commit  
**Boundary:** `Web.Backend -> Application -> Domain` | `Infrastructure -> Application/Domain`  
**Encoding scan sau update report:** PASS (19 files, 0 BOM failures, 0 mojibake)

---

## 1. Tóm Tắt Technical Readiness

| # | Bước | Kết quả | Evidence |
|---|---|---|---|
| 1 | Build | ✅ PASS | 0 Error(s), 30 Warning(s) |
| 2 | Encoding scan (BOM + mojibake) | ✅ PASS | 19 files, BOM failures 0, mojibake hits 0 |
| 3 | DB duplicate check employee.user_id | ✅ PASS | 0 rows trùng |
| 4 | Migration generated | ✅ PASS | `20260704024251_AddUniqueIndexEmployeeUserId` |
| 5 | Database update applied | ✅ PASS | `__EFMigrationsHistory` đã có record |
| 6 | Unique index verify | ✅ PASS | `IX_employee_user_id` — `CREATE UNIQUE INDEX "IX_employee_user_id" ON public.employee USING btree (user_id) WHERE (user_id IS NOT NULL)` |
| 7 | FK rename verify | ✅ PASS | `fk_leave_approver_assignment_employee_approver_id` — cosmetic rename, same column/table/onDelete |
| 8 | Manual API UAT | ⏳ Chờ thực hiện | |
| 9 | Git stage/commit | ⏳ Chờ UAT pass + user xác nhận | |

### Migration Scope

Migration `20260704024251_AddUniqueIndexEmployeeUserId` chứa:
- **Mục đích chính:** Drop index non-unique `ix_employee_user_id`, tạo unique filtered index `IX_employee_user_id`.
- **Kèm theo:** Cosmetic FK constraint rename trên `leave_approver_assignment` (chỉ đổi tên constraint, không đổi column/table/onDelete). Root cause: EF Core compare current model với snapshot cũ, FK name đã lệch từ Phase 3B. Thêm Employee unique index làm migration mới lộ ra diff này.

---

## 2. Danh Sách File Đã Thay Đổi/Tạo Mới (10 file code)

### Layer: Domain
1. **`Domain/Employees/Employee.cs`** — Thêm `LinkUser(UserId userId)` với guard ngăn ghi đè liên kết.
2. **`Domain/Employees/EmployeeErrors.cs`** — Thêm `AlreadyLinkedToUser` error.
3. **`Domain/Users/UserErrors.cs`** — Sửa typo mã lỗi `DuplicateUsername`.

### Layer: Infrastructure
4. **`Infrastructure/Authentication/Models/AuthenticationErrors.cs`** — Thêm `UsernameExisted`, `DeleteUserError`, `UserAlreadyExists`.
5. **`Infrastructure/Authentication/AuthenticationService.cs`** — Parse Keycloak 409 Conflict chi tiết. Thêm `DeleteUser` trả `Result`.
6. **`Infrastructure/Configurations/EmployeeConfiguration.cs`** — Unique filtered index `IX_employee_user_id`.

### Layer: Application
7. **`Application/Employees/ProvisionAccount/ProvisionEmployeeAccountCommand.cs`** *(Mới)* — Command record: `EmployeeId`, `Username`, `Email`, `Password`, `RoleIds`.
8. **`Application/Employees/ProvisionAccount/ProvisionEmployeeAccountCommandValidator.cs`** *(Mới)* — FluentValidation: NotEmpty, EmailAddress, MinLength(6), RoleIds no duplicates.
9. **`Application/Employees/ProvisionAccount/ProvisionEmployeeAccountCommandHandler.cs`** *(Mới)* — Handler: validate Employee, check duplicate email/username DB, validate RoleIds exist, register Keycloak, link User-Employee, compensating action rollback.

### Layer: Web.Backend
10. **`Web.Backend/Controllers/EmployeeController.cs`** — Endpoint `POST /employee/provision-account`, permission check `UPDATE_EMPLOYEE + UPDATE_USER`.

### Migration (3 file)
11. `Infrastructure/Migrations/20260704024251_AddUniqueIndexEmployeeUserId.cs`
12. `Infrastructure/Migrations/20260704024251_AddUniqueIndexEmployeeUserId.Designer.cs`
13. `Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs`

---

## 3. Endpoint API

```
POST /employee/provision-account
Authorization: UPDATE_EMPLOYEE + UPDATE_USER
Content-Type: application/x-www-form-urlencoded
```

### Input

| Field | Type | Validation |
|---|---|---|
| `EmployeeId` | `Guid` | NotEmpty |
| `Username` | `string` | NotEmpty, MaxLength(50) |
| `Email` | `string` | NotEmpty, EmailAddress, MaxLength(150) |
| `Password` | `string` | NotEmpty, MinimumLength(6) |
| `RoleIds` | `List<Guid>` | NotEmpty, no duplicates |

### Handler Flow

```
1. Get Employee by ID → fail: Employee.NotFound
2. Guard employee.UserId == null → fail: Employee.AlreadyLinkedToUser
3. Check Email unique trong DB → fail: User.DuplicateEmail
4. Check Username unique trong DB → fail: User.DuplicateUsername
5. Validate RoleIds exist (.Distinct()) → fail: Role.NotFound
6. Create User entity
7. RegisterAsync(User) Keycloak → fail: Username.Existed / Email.Existed / User.AlreadyExists / Server.Error
8. SetIdentityId, LinkUser → fail: Employee.AlreadyLinkedToUser + Keycloak rollback
9. Add User, UserToRoles, Update Employee
10. SaveChangesAsync → fail: Keycloak rollback + throw
11. Return { result: true, message: "Cấp tài khoản nhân viên thành công." }
```

---

## 4. UAT Checklist — Manual API Test

> **Lưu ý quan trọng:**
> - TC-01 (happy path) có mutation DB + Keycloak. **Không tự chạy nếu chưa có user approval rõ ràng.**
> - TC-12 (permission test) không được tự tạo/sửa user/role/permission trong Keycloak hoặc DB nếu chưa được user xác nhận.
> - TC-13 (compensating action) là **optional/controlled technical test**, không bắt buộc cho baseline UAT.
> - Auth mode: **Keycloak thật**, `UseMockAuth: false`.

### Điều kiện tiên quyết (Prerequisites)

| # | Điều kiện | Cách verify |
|---|---|---|
| P1 | Docker container `keycloak-hrm` đang chạy | `docker ps \| findstr keycloak` |
| P2 | HRM app đang chạy | `dotnet run --project HRM_Leave_Management/Web.Backend` |
| P3 | `UseMockAuth = false` | Kiểm tra `appsettings.json` — mục `Authentication:UseMockAuth` |
| P4 | Login thành công với `admin` / `Admin@123456` | Truy cập `/employee`, không bị redirect |
| P5 | Account test có cả `UPDATE_EMPLOYEE` và `UPDATE_USER` | `SELECT p.name FROM role_to_permission rp JOIN permission p ON rp.permission_id = p.id WHERE rp.role_id IN (SELECT role_id FROM user_to_role WHERE user_id = '<admin_user_id>') AND p.name IN ('UPDATE_EMPLOYEE','UPDATE_USER');` |
| P6 | Có Employee chưa có `user_id` | `SELECT id, full_name, user_id FROM employee WHERE user_id IS NULL LIMIT 1` |
| P7 | Biết ít nhất 1 `role.id` hợp lệ | `SELECT id, name FROM role LIMIT 5` |
| P8 | Migration đã apply | `SELECT * FROM "__EFMigrationsHistory" WHERE "MigrationId" LIKE '%AddUniqueIndexEmployeeUserId%'` — phải 1 row |

**Tool test:** Postman / curl / Browser DevTools (POST form data). Chưa có UI form Razor cho provision.

---

### TC-01: Happy Path — Tạo tài khoản thành công

> **⚠️ Test này có mutation DB + Keycloak.** Chỉ thực hiện khi user đã approve.

**Cách lấy dữ liệu test:**

1. Lấy `EmployeeId` (Employee chưa link user):
```sql
SELECT id, full_name, user_id FROM employee WHERE user_id IS NULL LIMIT 1;
-- Kết quả ví dụ: id = 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx'
```

2. Lấy `RoleId` hợp lệ:
```sql
SELECT id, name FROM role LIMIT 5;
-- Chọn 1 id từ kết quả
```

**Endpoint:**
```
POST /employee/provision-account
Content-Type: application/x-www-form-urlencoded
```

**Form fields (x-www-form-urlencoded):**
```
EmployeeId=<GUID từ bước 1>
&Username=employee.test01
&Email=employee.test01@hrm.local
&Password=Test@123456
&RoleIds[0]=<GUID từ bước 2>
```

**Cách gọi bằng curl (tham khảo):**
```powershell
# Không dùng trực tiếp vì cần auth cookie. Gợi ý dùng Postman hoặc DevTools.
# Đăng nhập bằng browser > mở DevTools > Console:
fetch('/employee/provision-account', {
  method: 'POST',
  headers: {'Content-Type': 'application/x-www-form-urlencoded'},
  body: 'EmployeeId=<GUID>&Username=employee.test01&Email=employee.test01@hrm.local&Password=Test@123456&RoleIds[0]=<ROLE_GUID>'
}).then(r => r.json()).then(console.log);
```

**Expected response:**
- HTTP 200
- Body: `{ "result": true, "message": "Cấp tài khoản nhân viên thành công." }`

**Verify sau khi pass (5 bước):**
1. Keycloak user được tạo: Keycloak Admin Console → Users → tìm `employee.test01`
2. DB `user` có record mới: `SELECT id, username, email, identity_id FROM "user" WHERE username = 'employee.test01'`
3. `employee.user_id` được link: `SELECT id, user_id FROM employee WHERE id = '<EmployeeId>'`
4. `user_to_role` có role: `SELECT * FROM user_to_role WHERE user_id = '<user_id từ bước 2>'`
5. API trả success: `result: true`, message phù hợp

---

### TC-02: Validation — EmployeeId rỗng
**Input:** `EmployeeId: (trống hoặc Guid.Empty)` | **Expected:** HTTP 400

### TC-03: Validation — Username rỗng
**Input:** `Username: (trống)` | **Expected:** HTTP 400

### TC-04: Validation — Email sai format
**Input:** `Email: not-an-email` | **Expected:** HTTP 400

### TC-05: Validation — Password quá ngắn
**Input:** `Password: 123` | **Expected:** HTTP 400 (MinimumLength 6)

### TC-06: Validation — RoleIds rỗng
**Input:** `RoleIds: (trống)` | **Expected:** HTTP 400, `"Danh sách vai trò không được để trống."`

### TC-07: Validation — RoleIds trùng lặp
**Input:** `RoleIds[0]: <same-guid>`, `RoleIds[1]: <same-guid>` | **Expected:** HTTP 400, `"Danh sách vai trò không được chứa các giá trị trùng lặp."`

---

### TC-08: Business — Employee đã có tài khoản
**Điều kiện:** Dùng Employee đã link user (ví dụ từ TC-01)  
**Expected:** HTTP 400, error code `Employee.AlreadyLinkedToUser`

### TC-09: Business — Email trùng trong DB
**Input:** `Email: <email đã tồn tại trong bảng user>`  
**Expected:** HTTP 400, error code `User.DuplicateEmail`

### TC-10: Business — Username trùng trong DB
**Input:** `Username: <username đã tồn tại>`  
**Expected:** HTTP 400, error code `User.DuplicateUsername`

### TC-11: Business — RoleId không tồn tại
**Input:** `RoleIds[0]: 00000000-0000-0000-0000-000000000099`  
**Expected:** HTTP 400, error code `Role.NotFound`

---

### TC-12: Permission — Thiếu quyền
**Điều kiện:** Login với user KHÔNG có `UPDATE_EMPLOYEE` hoặc KHÔNG có `UPDATE_USER`  
**Expected:** Redirect `/NoPermission`

> **Lưu ý:** Không tự tạo/sửa user, role, hoặc permission trong Keycloak/DB để phục vụ test này nếu chưa được user xác nhận. Nếu cần tài khoản test thiếu quyền, hỏi user trước.

---

### TC-13: Compensating Action — Keycloak Rollback *(Optional/Controlled)*
**Loại:** Optional technical test — không bắt buộc cho baseline UAT.  
**Điều kiện:** Giả lập lỗi DB sau khi Keycloak đã tạo user (ví dụ: tạm sửa ràng buộc DB hoặc đổi constraint).  
**Expected:** Handler bắt exception, gọi `DeleteUser` xóa user khỏi Keycloak, không có user "mồ côi".  
**Ghi chú:** Yêu cầu thay đổi DB constraint hoặc simulate lỗi — chỉ thực hiện trong môi trường controlled và có plan rollback rõ ràng.

---

## 5. Git Scope

### Được phép stage (16 file):

**Code (10 file):**
1. `HRM_Leave_Management/Domain/Employees/Employee.cs`
2. `HRM_Leave_Management/Domain/Employees/EmployeeErrors.cs`
3. `HRM_Leave_Management/Domain/Users/UserErrors.cs`
4. `HRM_Leave_Management/Infrastructure/Authentication/Models/AuthenticationErrors.cs`
5. `HRM_Leave_Management/Infrastructure/Authentication/AuthenticationService.cs`
6. `HRM_Leave_Management/Infrastructure/Configurations/EmployeeConfiguration.cs`
7. `HRM_Leave_Management/Web.Backend/Controllers/EmployeeController.cs`
8. `HRM_Leave_Management/Application/Employees/ProvisionAccount/ProvisionEmployeeAccountCommand.cs`
9. `HRM_Leave_Management/Application/Employees/ProvisionAccount/ProvisionEmployeeAccountCommandHandler.cs`
10. `HRM_Leave_Management/Application/Employees/ProvisionAccount/ProvisionEmployeeAccountCommandValidator.cs`

**Migration (3 file):**
11. `HRM_Leave_Management/Infrastructure/Migrations/20260704024251_AddUniqueIndexEmployeeUserId.cs`
12. `HRM_Leave_Management/Infrastructure/Migrations/20260704024251_AddUniqueIndexEmployeeUserId.Designer.cs`
13. `HRM_Leave_Management/Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs`

**Tài liệu (3 file):**
14. `MD_memory/plans/2026-07-03_1500_phase-3c_user-employee-provisioning.md`
15. `MD_memory/reports/2026-07-03_1635_phase-3c2_impact-analysis_report.md`
16. `MD_memory/reports/2026-07-03_1700_phase-3c2_employee-provisioning_report.md`

### KHÔNG được stage:
- `.agents/*`
- `HRM_Leave_Management/Web.Backend/appsettings.json`
- `Web.Backend/appsettings.json`
- `MD_memory/debug/*`
- `MD_memory/gap_analysis.md`
- `MD_memory/hrm_refactor_mapping.md`
- `MD_memory/project_architecture_analysis.md`

---

## 6. Trạng Thái Cuối Cùng

**Technical readiness:** ✅ PASS (build 0 errors, encoding 19 files PASS, migration applied, DB verify PASS).  
**CHƯA claim UAT PASS.** **CHƯA ready to commit.**

Điều kiện chuyển sang commit:
1. User thực hiện manual API UAT (ít nhất TC-01 happy path) và xác nhận kết quả.
2. Git stage dùng explicit file list (16 file trong scope).
3. Không stage: `.agents/*`, `MD_memory/debug/*`, `appsettings.json`, `MD_memory/gap_analysis.md`, `MD_memory/hrm_refactor_mapping.md`, `MD_memory/project_architecture_analysis.md`.
4. Không push `origin/main` cho tới khi user xác nhận push.

**Boundary đang giữ:**
- `Web.Backend -> Application -> Domain`
- `Infrastructure -> Application/Domain`
