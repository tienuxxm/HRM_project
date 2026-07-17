# Phase 3C.2 — Impact Analysis Report (Final)

**Ngày:** 2026-07-03 16:50  
**Revision:** v3 — Tích hợp quyết định Q1/Q2/Q3 từ Codex  
**Tool:** GitNexus MCP repo `HRM_project` + grep code verification  
**Scope:** Tất cả symbols cần sửa/dùng cho Phase 3C.2 ProvisionEmployeeAccountCommand  
**Quyết định tổng:** GO — Tất cả LOW risk, 0 HIGH/CRITICAL

---

## 1. Impact Results

### 1.1 RegisterAsync(User) — Sửa error handling 409 Conflict

| Field | Value |
|---|---|
| **UID** | `Method:HRM_Leave_Management/Infrastructure/Authentication/AuthenticationService.cs:AuthenticationService.RegisterAsync#3~User,string,CancellationToken` |
| **File** | `HRM_Leave_Management/Infrastructure/Authentication/AuthenticationService.cs:51` |
| **Risk** | LOW |
| **GitNexus direct callers** | 0 |
| **Code-verified callers** | **1** (GitNexus stale cho relationship này) |
| **Processes affected** | 0 |

**GitNexus Stale Warning:** GitNexus trả 0 callers nhưng grep code xác nhận **1 caller thực tế**:

| Caller | File | Line |
|---|---|---|
| `CreateUserCommandHandler.Handle` | `HRM_Leave_Management/Application/Users/Create/CreateUserCommandHandler.cs` | 62 |

**Blast radius cho flow tạo User hiện tại:**
- `CreateUserCommandHandler` gọi `RegisterAsync(user, password)` và nhận `Result<string>`.
- Nếu RegisterAsync trả failure (email trùng hoặc username trùng), handler trả `Result.Failure<Guid>(identityIdResult.Error)`.
- Thay đổi của Phase 3C.2: Sửa **bên trong** method RegisterAsync để phân biệt 409 email vs 409 username. **Không đổi method signature.** Return type vẫn là `Result<string>`. Caller hiện tại (`CreateUserCommandHandler`) vẫn hoạt động đúng vì nó chỉ check `IsFailure` rồi forward error.
- **Kết luận:** LOW risk — không break caller hiện tại.

**Thêm caller mới:** `ProvisionEmployeeAccountCommandHandler` sẽ gọi `RegisterAsync(user, password)` theo cùng pattern với `CreateUserCommandHandler`.

**Quyết định:** GO

### 1.2 DeleteUser — Dùng cho compensating rollback ONLY

| Field | Value |
|---|---|
| **UID** | `Method:HRM_Leave_Management/Infrastructure/Authentication/AuthenticationService.cs:AuthenticationService.DeleteUser#2` |
| **File** | `HRM_Leave_Management/Infrastructure/Authentication/AuthenticationService.cs:108` |
| **Risk** | LOW |
| **Direct callers (d=1)** | 4 (2 trong HRM, 2 trong root CSM) |
| **Processes affected** | 0 |
| **Modules affected** | 1 ("Create") |

**Direct callers trong HRM:**

| Caller | File |
|---|---|
| `DeleteMyAccountCommandHandler.Handle` | `HRM_Leave_Management/Application/Members/DeleteMyAccount/...` |
| `DeleteUserCommandHandler.Handle` | `HRM_Leave_Management/Application/Users/Delete/...` |

**Phân biệt rõ mục đích sử dụng `DeleteUser` trong Phase 3C.2:**
- **CHỈ dùng cho compensating rollback:** Khi `ProvisionEmployeeAccountCommandHandler` tạo Keycloak user thành công nhưng DB SaveChanges fail, gọi `DeleteUser` để rollback Keycloak user vừa tạo.
- **KHÔNG dùng cho:** Employee nghỉ việc, delete Employee, hoặc bất kỳ nghiệp vụ HR nào. BD-4 đã chốt: chỉ disable Keycloak user khi Employee nghỉ việc, không xóa.
- **Thay đổi:** Fix error mapping sai (`ChangePasswordError` -> `DeleteUserError`). Không đổi signature.

**Quyết định:** GO

### 1.3 UserErrors (class) — Fix DuplicateUsername error code

| Field | Value |
|---|---|
| **UID** | `Class:HRM_Leave_Management/Domain/Users/UserErrors.cs:UserErrors` |
| **File** | `HRM_Leave_Management/Domain/Users/UserErrors.cs` |
| **Risk** | LOW |
| **GitNexus direct callers** | 0 |
| **Code-verified callers** | 1: `CreateUserCommandHandler.cs:49` dùng `UserErrors.DuplicateUsername` |

**Bug hiện tại:** `DuplicateUsername` dùng sai error code `"User.DuplicateEmail"` thay vi `"User.DuplicateUsername"`.  
**Thay đổi:** Sửa error code string. Không đổi signature, không đổi tên field.

**Quyết định:** GO

### 1.4 AuthenticationErrors — Thêm `UsernameExisted`, `DeleteUserError`, `UserAlreadyExists`

| Field | Value |
|---|---|
| **File** | `HRM_Leave_Management/Infrastructure/Authentication/Models/AuthenticationErrors.cs` |
| **Risk** | LOW (thêm member mới, không sửa member cũ) |

**GitNexus không tìm thấy symbol** — có thể do namespace nested `Infrastructure.Authentication.Models`. Grep xác nhận 10 references trong HRM, tất cả trong `AuthenticationService.cs` và `JwtService.cs`.

**Thay đổi:** Thêm 3 static property mới:
- `UsernameExisted` — khi parse được Keycloak 409 là trùng username
- `DeleteUserError` — thay thế `ChangePasswordError` sai trong `DeleteUser`
- `UserAlreadyExists` — fallback trung tính khi không parse được Keycloak 409 body (Q2 đã chốt: không được fallback về `EmailExisted`)

Không sửa member cũ.

**Quyết định:** GO

### 1.5 EmployeeController — Thêm action provision-account

| Field | Value |
|---|---|
| **UID** | `Class:HRM_Leave_Management/Web.Backend/Controllers/EmployeeController.cs:EmployeeController` |
| **File** | `HRM_Leave_Management/Web.Backend/Controllers/EmployeeController.cs` |
| **Risk** | LOW |
| **Direct callers** | 0 |

**Thay đổi:** Thêm action `[HttpPost("provision-account")]` mới. Không sửa action cũ.

**Quyết định:** GO

### 1.6 EmployeeConfiguration — Thêm unique filtered index

| Field | Value |
|---|---|
| **UID** | `Class:HRM_Leave_Management/Infrastructure/Configurations/EmployeeConfiguration.cs:EmployeeConfiguration` |
| **File** | `HRM_Leave_Management/Infrastructure/Configurations/EmployeeConfiguration.cs` |
| **Risk** | LOW |
| **Direct callers** | 0 |

**Thay đổi:** Thêm `HasIndex(e => e.UserId).IsUnique().HasFilter("user_id IS NOT NULL")` để đảm bảo 1:1 Employee-User.

**Quyết định:** GO

### 1.7 Employee (entity) — Thêm domain method `LinkUser`

| Field | Value |
|---|---|
| **UID** | `Class:HRM_Leave_Management/Domain/Employees/Employee.cs:Employee` |
| **File** | `HRM_Leave_Management/Domain/Employees/Employee.cs` |
| **Risk** | LOW |
| **Direct callers (d=1)** | 1 (`Employee.Create`) |
| **d=2** | 1 (`CreateEmployeeCommandHandler.Handle`) |
| **Processes affected** | 0 |

**Vấn đề Codex nêu:** `Employee.UserId` có `private set` (dòng 42). Method `Update()` (dòng 74-90) có gán `UserId` nhưng yêu cầu 7 tham số (fullName, employeeCode, departmentId, userId, positionId, joinDate, managerId). Dùng `Update()` để chỉ link user là SAI vì:
1. Phải truyền tất cả field khác — coupling cao, dễ sai.
2. Vi phạm Single Responsibility — provision chỉ cần gán UserId.

**Giải pháp (Q1 đã chốt — có guard, không cho overwrite):** Thêm domain method mới với guard:

```csharp
public Result LinkUser(UserId userId)
{
    if (UserId is not null)
        return Result.Failure(EmployeeErrors.AlreadyLinkedToUser);
    UserId = userId;
    return Result.Success();
}
```

Method này:
- Chỉ gán `UserId` nếu Employee CHƯA có UserId.
- Nếu đã có UserId, trả `Result.Failure` (không throw exception — dùng Result/Error pattern của project).
- Lý do bắt buộc có guard (Codex giải thích): nếu cho overwrite thì lịch sử nghỉ phép, approver assignment, permission theo user cũ có thể bị "đổi chủ" âm thầm.
- Không đổi signature của `Create()` hoặc `Update()`.
- Không ảnh hưởng caller hiện tại của Employee.

**Impact của việc thêm `LinkUser`:**
- Thêm method mới vào class — không break bất kỳ caller nào.
- `CreateEmployeeCommandHandler` và `UpdateEmployeeCommandHandler` vẫn gọi `Create()`/`Update()` bình thường.
- Cần thêm `EmployeeErrors.AlreadyLinkedToUser` — xem mục 1.8 bên dưới.

**Quyết định:** GO — Employee.cs vào scope sửa (thêm method mới).

### 1.8 EmployeeErrors — Thêm `AlreadyLinkedToUser`

| Field | Value |
|---|---|
| **UID** | `Class:HRM_Leave_Management/Domain/Employees/EmployeeErrors.cs:EmployeeErrors` |
| **File** | `HRM_Leave_Management/Domain/Employees/EmployeeErrors.cs` |
| **Risk** | LOW |
| **GitNexus direct callers** | 0 |
| **Code-verified callers** | 3 handlers dùng `EmployeeErrors.NotFound`, `.EmployeeCodeExisted`, `.HasSubordinates` |
| **Processes affected** | 0 |

**Hiện có 3 errors:**
- `NotFound` — dùng bởi `UpdateEmployeeCommandHandler`, `DeleteEmployeeCommandHandler`
- `EmployeeCodeExisted` — dùng bởi `UpdateEmployeeCommandHandler`, `CreateEmployeeCommandHandler`
- `HasSubordinates` — dùng bởi `DeleteEmployeeCommandHandler`

**Thay đổi:** Thêm 1 static field mới: `AlreadyLinkedToUser`. Không sửa member cũ.

```csharp
public static Error AlreadyLinkedToUser = new(
    "Employee.AlreadyLinkedToUser",
    "Employee already has a linked user account");
```

**Quyết định:** GO

---

## 2. Bugs Phát Hiện

### Bug 1: `UserErrors.DuplicateUsername` sai error code

```csharp
// UserErrors.cs:19-21
public static Error DuplicateUsername = new(
    "User.DuplicateEmail",     // SAI — phải là "User.DuplicateUsername"
    "The Username already exist");
```

### Bug 2: `DeleteUser` dùng sai error type

```csharp
// AuthenticationService.cs:112-114
return result.StatusCode == HttpStatusCode.NoContent
    ? Result.Success()
    : Result.Failure(AuthenticationErrors.ChangePasswordError); // SAI — phải là DeleteUserError
```

### Bug 3: Tất cả 409 Conflict đều trả `EmailExisted`

```csharp
// AuthenticationService.cs:72-77
HttpStatusCode.Conflict => Result.Failure<string>(AuthenticationErrors.EmailExisted),
// Keycloak trả 409 cho cả username conflict — cần parse response body
```

---

## 3. Revised Exact File Scope

### 3.1 Sửa file hiện có (chỉ trong `HRM_Leave_Management/`) — **7 file**

| # | File | Thay đổi | Domain/Repo/Auth? |
|---|---|---|---|
| 1 | `Domain/Users/UserErrors.cs` | Fix DuplicateUsername error code | Domain (error code only) |
| 2 | `Domain/Employees/Employee.cs` | Thêm method `LinkUser(UserId userId)` với guard | Domain entity |
| 3 | `Domain/Employees/EmployeeErrors.cs` | Thêm `AlreadyLinkedToUser` | Domain (error) |
| 4 | `Infrastructure/Authentication/Models/AuthenticationErrors.cs` | Thêm `UsernameExisted`, `DeleteUserError`, `UserAlreadyExists` | Infrastructure |
| 5 | `Infrastructure/Authentication/AuthenticationService.cs` | Parse 409 body, fix DeleteUser error | Infrastructure (auth service) |
| 6 | `Infrastructure/Configurations/EmployeeConfiguration.cs` | Thêm unique filtered index | Infrastructure (EF config) |
| 7 | `Web.Backend/Controllers/EmployeeController.cs` | Thêm action provision-account | Web |

### 3.2 Tạo file mới

| # | File | Nội dung |
|---|---|---|
| 7 | `Application/Employees/ProvisionAccount/ProvisionEmployeeAccountCommand.cs` | Command record |
| 8 | `Application/Employees/ProvisionAccount/ProvisionEmployeeAccountCommandHandler.cs` | Handler + rollback |
| 9 | `Application/Employees/ProvisionAccount/ProvisionEmployeeAccountCommandValidator.cs` | FluentValidation |
| 10 | Migration mới (tên do EF generate) | Unique filtered index `employee.user_id` |

### 3.3 KHÔNG dùng

- Phase 3C.4 (Disable User) — auth-sensitive, cần impact + xác nhận riêng.
- `IAuthenticationService.cs` — không thêm method mới trong phase này.
- `IEmployeeRepository.cs` — không đổi interface, dùng `GetByIdAsync` sẵn có.
- File ngoài `HRM_Leave_Management/`.
- File dirty ngoài scope.

### 3.4 Auth boundary

- `AuthenticationService.cs` ĐƯỢC SỬA: chỉ thay đổi internal error parsing cho 409, và fix DeleteUser error mapping.
- KHÔNG thêm method mới vào `IAuthenticationService`.
- KHÔNG đổi Keycloak config/JwtService/auth middleware.
- ProvisionCommand gọi `RegisterAsync` và `DeleteUser` (rollback) — cả hai đều là method CÓ SẴN trên interface.

---

## 4. Working Tree Status (User báo)

Các file dirty **ngoài scope** — KHÔNG ĐƯỢC stage/commit:

| File | Trạng thái |
|---|---|
| `.agents/*` | Modified |
| `appsettings.json` | Modified |
| `MD_memory/gap_analysis.md` | Modified |
| `MD_memory/hrm_refactor_mapping.md` | Modified |
| `MD_memory/project_architecture_analysis.md` | Modified |
| `Web.Backend/appsettings.json` | Modified |
| `MD_memory/plans/2026-07-03_1500_phase-3c_user-employee-provisioning.md` | Untracked |

**Lưu ý:** Terminal bị block toàn session — không chạy được `git status --short`. User cần chạy thủ công để xác nhận trạng thái hiện tại.

---

## 5. UTF-8 BOM Status

File report này được tạo với UTF-8 BOM (byte `EF BB BF` đầu file).  
**User cần verify:** `python -c "print(open(r'MD_memory/reports/2026-07-03_1635_phase-3c2_impact-analysis_report.md','rb').read(3))"`  
**Kết quả mong đợi:** `b'\xef\xbb\xbf'`

**Lưu ý:** Do terminal bị block, tôi không chạy được encoding scan. User cần chạy:
```
python MD_memory/debug/2026-06-26_1430_scan-mojibake.py HRM_Leave_Management --require-bom
```

---

## 6. Pre-Code Checklist (Revised)

| # | Điều kiện | Trạng thái |
|---|---|---|
| 1 | Impact analysis cho tất cả 8 symbol | Done — tất cả LOW |
| 2 | Code-verified callers (không chỉ dựa vào GitNexus) | Done — RegisterAsync có 1 caller, ghi rõ stale |
| 3 | Employee.UserId access pattern làm rõ | Done — thêm `LinkUser()` với guard (Q1 chốt) |
| 4 | DeleteUser mục đích phân biệt rõ | Done — chỉ rollback, không dùng cho HR |
| 5 | Không dùng Phase 3C.4 | Xác nhận |
| 6 | Exact file scope liệt kê đầy đủ | Done — **7 sửa + 4 mới** |
| 7 | Auth boundary xác nhận | Done — chỉ sửa internal, không đổi interface |
| 8 | Không DB mutation/Keycloak/browser UAT | Xác nhận |
| 9 | Không stage/commit/push nếu chưa xác nhận | Xác nhận |
| 10 | Q1/Q2/Q3 đã được Codex trả lời và tích hợp | Done |

---

## 7. Quyết Định Kỹ Thuật Đã Chốt (Q1/Q2/Q3)

### Q1: LinkUser guard — ĐÃ CHỐT

**Quyết định:** Có guard. Không cho overwrite `Employee.UserId`. Nếu Employee đã có UserId thì return `Result.Failure(EmployeeErrors.AlreadyLinkedToUser)`. Dùng Result/Error pattern, không throw exception.

**Lý lý:** Cho overwrite sẽ gây rủi ro "đổi chủ" âm thầm cho lịch sử nghỉ phép, approver assignment, permission.

**Tác động scope:** Thêm `EmployeeErrors.AlreadyLinkedToUser` vào `EmployeeErrors.cs` (file đã tồn tại, LOW risk).

### Q2: 409 Fallback — ĐÃ CHỐT

**Quyết định:** Không fallback về `EmailExisted` khi không parse được Keycloak 409 body. Fallback phải là lỗi trung tính `UserAlreadyExists` hoặc `ServerError`.

**Logic parse:**
- Parse được `"username"` trong error message → `UsernameExisted`
- Parse được `"email"` trong error message → `EmailExisted`
- Không parse được → `UserAlreadyExists` (trung tính, không báo sai nguyên nhân)

**Tác động scope:** Thêm `UserAlreadyExists` vào `AuthenticationErrors.cs` (tăng từ 2 lên 3 member mới).

### Q3: Migration — ĐÃ CHỐT

**Quyết định:** Ưu tiên `dotnet ef migrations add`. Không tạo migration tay trừ khi terminal bị block VÀ user xác nhận riêng.

**Lệnh:** `dotnet ef migrations add AddUniqueIndexEmployeeUserId --project Infrastructure --startup-project Web.Backend`

**Tác động scope:** User chạy migration từ terminal. Nếu terminal vẫn block, báo lại để user xác nhận có cho tạo tay không.

---

## 8. Final Scope Summary

| Loại | Số lượng | Chi tiết |
|---|---|---|
| File sửa | **7** | UserErrors, Employee, EmployeeErrors, AuthenticationErrors, AuthenticationService, EmployeeConfiguration, EmployeeController |
| File mới | **3** | ProvisionEmployeeAccountCommand, Handler, Validator |
| Migration | **1** | Do EF generate (user chạy lệnh) |
| **Tổng** | **11** | |

**Không có câu hỏi nào còn mở. Report này là FINAL. Sẵn sàng code sau khi user xác nhận.**
