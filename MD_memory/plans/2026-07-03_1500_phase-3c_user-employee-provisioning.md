# Phase 3C Plan — User + Employee Provisioning Flow

**Ngày tạo:** 2026-07-03  
**Cập nhật:** 2026-07-03 16:15  
**Trạng thái:** APPROVED — 6 business decisions đã chốt  
**Boundary:** `Web.Backend -> Application -> Domain` | `Infrastructure -> Application/Domain`

---

## 1. Business Decision Confirmed

### BD-1 (Q-1): Backend 2 bước, không nhồi handler cũ ✅

- Tạo Employee trước bằng `CreateEmployeeCommand` (giữ nguyên).
- Cấp account bằng command riêng `ProvisionEmployeeAccountCommand`.
- UI có thể làm liền mạch (submit 1 lần), nhưng backend tách 2 command.
- **KHÔNG** nhồi toàn bộ Keycloak/User/Role vào `CreateEmployeeCommandHandler`.

### BD-2 (Q-2): Cho phép link Employee với User đã tồn tại ✅

- Guard bắt buộc:
  - Employee chưa có `UserId`.
  - User chưa được link với Employee khác (unique index `employee.user_id`).
  - Người thao tác phải có quyền `UPDATE_EMPLOYEE + UPDATE_USER`.

### BD-3 (Q-3): Disable User khi Employee nghỉ việc ✅

- Khi Employee nghỉ việc phải disable User để user đó không login được nữa.
- Không chỉ unlink rồi để User vẫn login được.
- **Hiện trạng code:** User entity **KHÔNG CÓ** `IsActive`/`IsDisabled` (đã xác minh bằng grep). `IAuthenticationService` **KHÔNG CÓ** method `DisableUser`.
- **Cần thiết kế thêm:** `DisableUser`/account status trong sub-phase 3C.4 hoặc sub-phase riêng, bao gồm:
  - Thêm `IsActive` flag trên User entity (migration).
  - Thêm `DisableUser(userId)` vào `IAuthenticationService` → gọi Keycloak PUT disable.
  - Impact analysis auth boundary bắt buộc trước khi code.

### BD-4 (Q-4): Không xóa Keycloak user khi delete/disable Employee ✅

- Chỉ disable login/account, không xóa trên Keycloak.
- `IAuthenticationService` hiện có `DeleteUser()` nhưng **KHÔNG** dùng cho flow này.
- Cần kiểm tra Keycloak có API disable user không; nếu có thì thêm `DisableKeycloakUser()` vào abstraction.

### BD-5 (Q-5): Không tạo LeaveBalance mặc định khi provision ✅

- LeaveBalance vẫn do HR phân bổ riêng trong LeaveBalance management (Phase 2C).

### BD-6 (Q-6): Bắt buộc chọn role khi cấp account ✅

- Không hardcode role mặc định trong backend.
- UI có thể gợi ý nhưng backend phải validate role input (không rỗng, role phải tồn tại).

---

## 2. Hiện Trạng Codebase (Evidence)

### 2.1 Bảng DB và EF Configuration

| Entity | Table name | File |
|---|---|---|
| Employee | `"employee"` | `EmployeeConfiguration.cs:14` |
| User | `"user"` | `UserConfiguration.cs:11` |

### 2.2 Employee entity hiện tại

- `Employee.UserId` — nullable `UserId?` — FK → `user.id` — `OnDelete(SetNull)`.
- Sẽ thêm Unique Index trên `employee.user_id` qua Fluent API.
- UI Create/Update hiện tại **không expose trường UserId**.

### 2.3 AuthenticationService — Method hiện có

| Method | Mô tả | Có sẵn? |
|---|---|---|
| `RegisterAsync(User, password)` | Tạo user trên Keycloak | ✅ Có |
| `DeleteUser(userId)` | Xóa user trên Keycloak | ✅ Có |
| `ResetPassword(password, userId)` | Reset password | ✅ Có |
| `ChangeEmail(email, userId)` | Đổi email trên Keycloak | ✅ Có |
| `DisableUser(userId)` | Disable user trên Keycloak | ❌ **CHƯA CÓ — cần thêm** |

### 2.4 User entity — Soft-delete / Disable

- **KHÔNG CÓ** `IsActive`, `IsDisabled`, `IsDeleted` trên User entity.
- Cần thêm `IsActive` flag + migration (Phase 3C.4).

### 2.5 Bug đã biết

- `UserErrors.DuplicateUsername` dùng nhầm error code `"User.DuplicateEmail"`.
- Keycloak 409 Conflict luôn trả `EmailExisted`, không phân biệt username/email.

---

## 3. Phase Breakdown

### Phase 3C.1 — Plan + Encoding + Business Confirmed

| # | Việc | Trạng thái |
|---|---|---|
| 1 | Viết plan Phase 3C | ✅ Done |
| 2 | User review và chốt 6 BD | ✅ Done |
| 3 | Encoding scan PASS (BOM + no mojibake) | ⏳ Chờ user chạy script BOM |

### Phase 3C.2 — Account Provisioning Command

| # | Việc | Layer | Trạng thái |
|---|---|---|---|
| 1 | Fix typo `UserErrors.DuplicateUsername` | Domain | ✅ Done |
| 2 | Tách Keycloak error 409: parse response body | Infrastructure | ✅ Done |
| 3 | Thêm `AuthenticationErrors.UsernameExisted` | Domain | ✅ Done |
| 4 | Migration: Unique filtered index `employee.user_id` | Infrastructure | ⏳ Chờ chạy migration |
| 5 | `ProvisionEmployeeAccountCommand` + Validator | Application | ✅ Done |
| 6 | `ProvisionEmployeeAccountCommandHandler` | Application | ✅ Done |
| 7 | Rollback: nếu DB fail → gọi `DeleteUser` trên Keycloak | Application | ✅ Done |
| 8 | Controller action `POST /employees/{id}/provision-account` | Web.Backend | ✅ Done |

**Impact analysis bắt buộc trước khi code:**
- `UserErrors.DuplicateUsername` — kiểm tra callers.
- `AuthenticationService.RegisterAsync` — kiểm tra callers.
- `AuthenticationService.DeleteUser` — kiểm tra callers (dùng cho rollback là usage pattern mới).

### Phase 3C.3 — Link Existing User to Employee

| # | Việc | Layer |
|---|---|---|
| 1 | `LinkUserToEmployeeCommand` + Validator | Application |
| 2 | `LinkUserToEmployeeCommandHandler` | Application |
| 3 | Guard: Employee chưa có UserId | Application |
| 4 | Guard: User chưa link Employee khác (unique index chặn) | DB + Application |
| 5 | Guard: Permission `UPDATE_EMPLOYEE + UPDATE_USER` | Application |
| 6 | Controller action `POST /employees/{id}/link-user` | Web.Backend |

### Phase 3C.4 — Disable User / Prevent Login

| # | Việc | Layer | Lưu ý |
|---|---|---|---|
| 1 | Thêm `IsActive` flag trên User entity | Domain | Migration mới |
| 2 | Thêm `DisableUser(userId)` vào `IAuthenticationService` | Application (interface) | Auth boundary change |
| 3 | Implement `DisableUser` trong `AuthenticationService` | Infrastructure | Gọi Keycloak API PUT disable |
| 4 | `DisableEmployeeAccountCommand` + Handler | Application | Set User.IsActive=false + disable Keycloak |
| 5 | Cập nhật login flow: check `IsActive` trước khi cho login | Infrastructure | **Impact analysis bắt buộc** |
| 6 | Controller action | Web.Backend | |

**⚠️ Auth-sensitive:** Phase này thay đổi auth boundary (`IAuthenticationService`). Bắt buộc:
- Chạy impact analysis trên `IAuthenticationService` trước khi code.
- Kiểm tra Keycloak Admin REST API có endpoint disable user không.
- Xin user approval nếu impact HIGH/CRITICAL.

### Phase 3C.5 — UI Employee Integrated Flow

| # | Việc |
|---|---|
| 1 | Employee Create modal: checkbox "Tạo tài khoản hệ thống" |
| 2 | Khi check: hiển thị Username, Password, Confirm Password, Role (dropdown) |
| 3 | Employee Edit modal: hiển thị trạng thái account (đã có / chưa có) |
| 4 | Nút "Cấp phát tài khoản" cho Employee chưa có account |
| 5 | Nút "Liên kết User đã có" → dropdown chọn User chưa link |
| 6 | Hiển thị trạng thái Active/Disabled cho Employee đã có account |
| 7 | Dùng toast/modal, **KHÔNG** `window.alert()` |

### Phase 3C.6 — Error Handling + UAT Report

| # | Việc |
|---|---|
| 1 | Parse Keycloak 409: phân biệt username vs email conflict |
| 2 | Hiển thị lỗi cụ thể trên UI |
| 3 | Rollback compensation: log warning khi rollback fail |
| 4 | Tạo manual UAT report (không chạy browser UAT tự động) |

---

## 4. Data Model Changes

### 4.1 Unique Index cho `employee.user_id` (Phase 3C.2)

```sql
CREATE UNIQUE INDEX IX_employee_user_id
ON employee (user_id)
WHERE user_id IS NOT NULL;
```

### 4.2 Thêm `IsActive` cho User (Phase 3C.4)

```sql
ALTER TABLE "user" ADD COLUMN is_active BOOLEAN NOT NULL DEFAULT TRUE;
```

### 4.3 Không tạo LeaveBalance mặc định (BD-5)

---

## 5. Permission

| Action | Permission | Ghi chú |
|---|---|---|
| Xem Employee | `VIEW_EMPLOYEE` | Đã có |
| Tạo/sửa Employee | `UPDATE_EMPLOYEE` | Đã có |
| Cấp phát account | `UPDATE_EMPLOYEE` + `UPDATE_USER` | Kết hợp |
| Link existing User | `UPDATE_EMPLOYEE` + `UPDATE_USER` | Kết hợp |
| Disable account | `UPDATE_EMPLOYEE` + `UPDATE_USER` | Kết hợp |

**Rủi ro tách permission:** Nếu sau này cần phân quyền hẹp hơn, có thể tách `PROVISION_EMPLOYEE_ACCOUNT` ở phase sau.

---

## 6. Rủi Ro

| # | Rủi ro | Mức độ | Giảm thiểu |
|---|---|---|---|
| R-1 | Keycloak user tạo OK nhưng DB fail → orphan | HIGH | Compensating rollback `DeleteUser` |
| R-2 | Rollback `DeleteUser` cũng fail → orphan tồn tại | MEDIUM | Log warning, manual cleanup |
| R-3 | Sửa `AuthenticationService` error handling ảnh hưởng caller | MEDIUM | Impact analysis bắt buộc |
| R-4 | Race condition provision cùng Employee | LOW | Unique index chặn DB level |
| R-5 | `DeleteUser` dùng cho rollback là usage pattern mới | MEDIUM | Impact analysis + user approval |
| R-6 | Thêm `DisableUser` thay đổi auth boundary | HIGH | Impact analysis, user approval trước code |
| R-7 | Keycloak API có thể không hỗ trợ disable user | MEDIUM | Kiểm tra REST API docs trước khi code |
| R-8 | Thêm `IsActive` trên User ảnh hưởng login flow | HIGH | Impact analysis login pipeline |
| R-9 | Migration unique index fail nếu data trùng | LOW | Kiểm tra data trước migration |
| R-10 | Không xóa Keycloak user → tài khoản vẫn tồn tại trên IdP | INFO | Chấp nhận: chỉ disable, không xóa (BD-4) |

---

## 7. UAT Checklist (Manual — không browser tự động)

| # | Test case | Kết quả mong đợi |
|---|---|---|
| 1 | Tạo Employee không tạo account | Employee tạo thành công, UserId = NULL |
| 2 | Provision account cho Employee đã tồn tại | User + Keycloak tạo OK, Employee.UserId được gán |
| 3 | Provision account: bắt buộc chọn role | Backend reject nếu role rỗng |
| 4 | Provision account: Employee đã có account | Backend reject, lỗi "Employee đã có tài khoản" |
| 5 | Link existing User cho Employee | Employee.UserId gán đúng User |
| 6 | Link User đã bị link Employee khác | Backend reject, lỗi unique constraint |
| 7 | Link User: Employee đã có UserId | Backend reject |
| 8 | Disable Employee → User bị disable | User.IsActive = false, Keycloak disabled |
| 9 | User bị disable không login được | Login fail với message phù hợp |
| 10 | Provision account: rollback khi DB fail | Keycloak user bị xóa (compensating) |
| 11 | Không tạo LeaveBalance khi provision | LeaveBalance table không có row mới |
| 12 | Duplicate username khi provision | Lỗi "Username đã tồn tại" (không generic) |
| 13 | Duplicate email khi provision | Lỗi "Email đã tồn tại" |
| 14 | Permission check: user không có `UPDATE_USER` | 403 Forbidden |

---

## 8. Out of Scope

- Không sửa User Management form hiện có.
- Không xóa Keycloak user khi disable Employee (BD-4).
- Không tạo LeaveBalance tự động (BD-5).
- Không hardcode role mặc định (BD-6).
- Không chạy migration / mutate DB trong phase planning.
- Không thao tác Keycloak.
- Không browser UAT tự động.
- Không stage / commit / push.
- Employee bulk import.
- Employee self-service portal.

---

## 9. Verification Plan (Áp dụng khi bắt đầu code)

```bash
# Trước khi code
git status --short
git branch --show-current

# Sau mỗi sub-phase
dotnet build HRM_Leave_Management/HRM_Leave_Management.sln
python MD_memory/debug/2026-06-26_1430_scan-mojibake.py HRM_Leave_Management/**/*.cs --require-bom

# Trước khi báo xong
git diff --stat
git diff --name-status
git status --short
```

---

## 10. Auth Compliance

- `UseMockAuth` = `false` — KHÔNG ĐƯỢC ĐỔI.
- Keycloak local: `http://localhost:8080`, realm: `hrm`, client: `hrm-web`.
- Tài khoản UAT: `admin` / `Admin@123456`.
- KHÔNG được tự ý đổi password, tạo/sửa user Keycloak, hoặc bật mock auth.
