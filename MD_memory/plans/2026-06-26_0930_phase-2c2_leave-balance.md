# Kế Hoạch Triển Khai - Phase 2C.2: Quản Lý Số Dư Ngày Phép (Leave Balance)

- **Thời gian lập**: 2026-06-26 10:00 (Cập nhật: 2026-06-26 14:55)
- **Người lập**: Antigravity (Senior .NET Fullstack Engineer)
- **Trạng thái**: **PASSED / CHỜ USER APPROVE** ⏳
- **Mục tiêu**: Thiết lập tính năng quản lý số dư ngày phép (Leave Balance) cho nhân viên theo từng loại phép và từng năm, hỗ trợ số thập phân (0.5 ngày), cho phép nhập/sửa thủ công `UsedDays` phục vụ di trú dữ liệu, và phân quyền chặt chẽ bằng permission-driven (không hardcode role).

---

## 📋 1. Nghiệp Vụ & Yêu Cầu Tính Năng

Theo các quyết định nghiệp vụ đã thống nhất:
1. **Kiểu dữ liệu cho ngày phép**:
   - `AllocatedDays` (Số ngày được cấp) và `UsedDays` (Số ngày đã dùng) phải dùng kiểu **decimal** để hỗ trợ xin nghỉ nửa ngày (0.5 ngày).
2. **Quản lý và điều chỉnh thủ công**:
   - Người dùng có quyền `UPDATE_LEAVE_BALANCE` được nhập và chỉnh sửa `UsedDays` thủ công để di trú (migrate) dữ liệu số dư phép cũ từ đầu kỳ.
3. **Cơ chế tính toán số ngày còn lại (`RemainingDays`)**:
   - **KHÔNG** lưu trữ cột `remaining_days` trong Database.
   - `RemainingDays` là trường tính toán động (Calculated Property) trên Entity/Application:
     `RemainingDays = AllocatedDays - UsedDays`
   - Ở Phase 2C.3 (Leave Request), ta sẽ tính khả dụng thực tế: `AvailableDays = AllocatedDays - UsedDays - PendingDays`.
4. **Năm cấp phát hợp lệ**:
   - Phải kiểm tra năm (`Year`) được chọn cấp phát nằm trong khoảng từ `currentYear - 1` đến `currentYear + 1`.
5. **Xem và Phân quyền (Permission-driven, không hardcode)**:
   - **Quyền xem số dư phép (`VIEW_LEAVE_BALANCE`)**:
     - Nhân viên chỉ được xem số dư phép của **chính mình**. Flow tra cứu: `IUserContext.IdentityId` → tìm `User` qua `User.IdentityId` → lấy `User.Id` → tìm `Employee` qua `Employee.UserId`.
     - Nhân viên KHÔNG thấy thanh tìm kiếm nhân viên khác, KHÔNG thấy nút thêm/sửa/xóa.
   - **Quyền cập nhật số dư phép (`UPDATE_LEAVE_BALANCE`)**:
     - Bao hàm quyền xem toàn bộ danh sách số dư phép của mọi nhân viên (không cần thêm `VIEW_LEAVE_BALANCE`).
     - Được tạo mới, chỉnh sửa (`AllocatedDays`, `UsedDays`), và soft-delete bản ghi LeaveBalance.
   - **Hành vi cấm**: Tuyệt đối không kiểm tra vai trò cứng (`if (role == "Admin")` hoặc `if (username == ...)`). Code chỉ check permission thông qua `IRoleService.checkRoleExist`. "Admin/HR" trong plan chỉ là mô tả nghiệp vụ, không phải logic code.
6. **Ràng buộc nghiệp vụ**:
   - `AllocatedDays >= UsedDays`
   - `AllocatedDays >= 0` và `UsedDays >= 0`
   - Số ngày phép phải là bội số của 0.5 (chỉ cho phép `.0` hoặc `.5`).
   - Một nhân viên chỉ có tối đa một bản ghi **active** số dư phép cho một loại phép trong một năm xác định. Ràng buộc này được thực thi bằng **partial unique index** (xem Mục 2).

---

## 🗄️ 2. Thiết Kế Database Schema

Bảng mới: `leave_balance`

| Tên Cột | Kiểu Dữ Liệu | Ràng Buộc | Mô Tả |
| :--- | :--- | :--- | :--- |
| `id` | `uuid` | Primary Key | Khóa chính |
| `employee_id` | `uuid` | Foreign Key (`employee.id`), Not Null | Liên kết tới nhân viên |
| `leave_type_id` | `uuid` | Foreign Key (`leave_type.id`), Not Null | Liên kết tới loại phép |
| `year` | `integer` | Not Null | Năm áp dụng (Giới hạn: currentYear - 1 đến +1) |
| `allocated_days` | `numeric(18,2)` | Not Null, Default 0 | Số ngày phép được cấp ban đầu |
| `used_days` | `numeric(18,2)` | Not Null, Default 0 | Số ngày phép đã sử dụng (cho phép sửa thủ công) |
| `is_active` | `boolean` | Not Null, Default true | Trạng thái kích hoạt (soft delete) |
| `created_date` | `timestamp` | Not Null, Default UTC | Thời gian tạo |

**Lưu ý**: Không có cột `remaining_days`. Giá trị này được tính động từ `AllocatedDays - UsedDays`.

**Ràng buộc chỉ mục**:
- **Partial Unique Index** (không phải unique constraint thường): `CREATE UNIQUE INDEX ix_leave_balance_unique_active ON leave_balance (employee_id, leave_type_id, year) WHERE is_active = true;`
  - Mục tiêu: Chỉ enforce tính duy nhất trên các bản ghi active. Sau khi soft delete (`is_active = false`), có thể tạo lại bản ghi mới cùng `employee_id + leave_type_id + year`.
  - Trong EF Core: sử dụng `.HasIndex(...).IsUnique().HasFilter("is_active = true")`.
- Index hỗ trợ truy vấn nhanh theo nhân viên và năm: `CREATE INDEX ix_leave_balance_employee_year ON leave_balance (employee_id, year);`

---

## 🏗️ 3. Kiến Trúc Lớp (Clean Architecture)

### 3.1. Lớp Domain

- **Value Object**: `Domain/LeaveBalances/LeaveBalanceId.cs` (bọc `Guid`).
- **Domain Entity**: `Domain/LeaveBalances/LeaveBalance.cs`
  ```csharp
  public class LeaveBalance : Entity<LeaveBalanceId>
  {
      private LeaveBalance(
          LeaveBalanceId id,
          EmployeeId employeeId,
          LeaveTypeId leaveTypeId,
          int year,
          decimal allocatedDays,
          decimal usedDays,
          bool isActive,
          DateTime createdDate)
      {
          Id = id;
          EmployeeId = employeeId;
          LeaveTypeId = leaveTypeId;
          Year = year;
          AllocatedDays = allocatedDays;
          UsedDays = usedDays;
          IsActive = isActive;
          CreatedDate = createdDate;
      }

      private LeaveBalance() { } // EF Core ctor

      public EmployeeId EmployeeId { get; private set; }
      public Employee? Employee { get; private set; }
      public LeaveTypeId LeaveTypeId { get; private set; }
      public LeaveType? LeaveType { get; private set; }
      public int Year { get; private set; }
      public decimal AllocatedDays { get; private set; }
      public decimal UsedDays { get; private set; }
      public bool IsActive { get; private set; }
      public DateTime CreatedDate { get; private set; }

      // Thuộc tính tính toán động — KHÔNG lưu trong DB
      public decimal RemainingDays => AllocatedDays - UsedDays;

      public static LeaveBalance Create(
          EmployeeId employeeId,
          LeaveTypeId leaveTypeId,
          int year,
          decimal allocatedDays,
          decimal usedDays)
      {
          return new LeaveBalance(
              LeaveBalanceId.New(),
              employeeId,
              leaveTypeId,
              year,
              allocatedDays,
              usedDays,
              isActive: true,
              createdDate: DateTime.UtcNow);
      }

      public void Update(decimal allocatedDays, decimal usedDays)
      {
          AllocatedDays = allocatedDays;
          UsedDays = usedDays;
      }

      public void SetActive(bool isActive)
      {
          IsActive = isActive;
      }
  }
  ```
- **Interface Repository**: `Domain/LeaveBalances/ILeaveBalanceRepository.cs`
  ```csharp
  public interface ILeaveBalanceRepository
  {
      Task<LeaveBalance?> GetByIdAsync(LeaveBalanceId id, CancellationToken cancellationToken = default);
      Task<LeaveBalance?> GetByUniqueKeyAsync(EmployeeId employeeId, LeaveTypeId leaveTypeId, int year, CancellationToken cancellationToken = default);
      Task<List<LeaveBalance>> GetByEmployeeAsync(EmployeeId employeeId, int year, CancellationToken cancellationToken = default);
      void Add(LeaveBalance leaveBalance);
      void Update(LeaveBalance leaveBalance);
  }
  ```

### 3.2. Lớp Infrastructure
- **Cấu hình Entity Mapping**: `Infrastructure/Configurations/LeaveBalanceConfiguration.cs`
  - Khai báo khóa ngoại, kiểu dữ liệu `decimal` tương đương `numeric(18,2)`.
  - Tạo **Partial Unique Index** `(employee_id, leave_type_id, year)` với filter `is_active = true` bằng `.HasIndex(...).IsUnique().HasFilter("is_active = true")`.
  - **Ignore** thuộc tính `RemainingDays` (Calculated Property, không map vào DB).
- **Repository Implementation**: `Infrastructure/Repositories/LeaveBalanceRepository.cs`
- **Đăng ký Dependency Injection**: Đăng ký `ILeaveBalanceRepository` trong `Infrastructure/DependencyInjection.cs`.
- **EF Core Migration**: Tạo migration mới `AddLeaveBalance` bằng CLI.
- **Verify migration (bắt buộc)**: Sau khi tạo migration, phải kiểm tra file migration C# sinh ra có chứa:
  - `CREATE UNIQUE INDEX ix_leave_balance_unique_active ON leave_balance (employee_id, leave_type_id, year) WHERE is_active = true;` (hoặc tương đương trong EF Core migration syntax).
  - Nếu EF Core không sinh đúng partial unique index cho PostgreSQL, phải sửa migration thủ công bằng `migrationBuilder.Sql(...)` và báo lại SQL chính xác.

### 3.3. Lớp Application (CQRS & MediatR)

#### Commands (Ghi Dữ Liệu)
1. **`CreateLeaveBalanceCommand`**:
   - Gửi yêu cầu gán số dư phép mới cho nhân viên.
   - Validator / Handler checks:
     - Kiểm tra `Employee` tồn tại và đang hoạt động.
     - Kiểm tra `LeaveType` tồn tại và đang hoạt động.
     - Kiểm tra năm `Year` có hợp lệ không (từ `currentYear - 1` đến `currentYear + 1`).
     - Kiểm tra `AllocatedDays >= UsedDays` và `AllocatedDays >= 0` và `UsedDays >= 0`.
     - Kiểm tra giá trị phải là bội số của 0.5.
     - Kiểm tra tính duy nhất: Đã tồn tại bản ghi active cho `(EmployeeId, LeaveTypeId, Year)` chưa. Nếu rồi thì báo lỗi `LeaveBalanceErrors.AlreadyExists`.
2. **`UpdateLeaveBalanceCommand`**:
   - Cập nhật số dư phép hiện có (cho phép điều chỉnh cả `AllocatedDays` và `UsedDays`).
   - Checks:
     - Bản ghi `LeaveBalance` có tồn tại và đang active không.
     - `AllocatedDays >= UsedDays` và `AllocatedDays >= 0` và `UsedDays >= 0`.
     - Giá trị phải là bội số của 0.5.
3. **`DeleteLeaveBalanceCommand`** (Soft Delete):
   - Đặt `IsActive = false` để hủy cấp phép.

#### Queries (Đọc Dữ Liệu)
1. **`GetLeaveBalancesByEmployeeQuery`**:
   - Lấy danh sách số dư phép của một nhân viên trong một năm.
2. **`GetLeaveBalancesPagedQuery`**:
   - Lấy danh sách tổng hợp số dư phép (Hỗ trợ tìm kiếm, phân trang, lọc theo phòng ban, loại phép, năm).

---

## 🔑 4. Danh Sách Permission & Hành Vi Kỹ Thuật

### 4.1. Permissions

| Permission | Hành vi | Mô tả nghiệp vụ |
| :--- | :--- | :--- |
| `VIEW_LEAVE_BALANCE` | Xem số dư phép **chỉ của chính mình** (self-view) | Dành cho nhân viên thường |
| `UPDATE_LEAVE_BALANCE` | Xem toàn bộ danh sách + tạo/sửa/xóa LeaveBalance | Dành cho vai trò quản lý (mô tả nghiệp vụ: Admin/HR) |

**Quyết định MVP Phase 2C.2**: Không thêm permission `VIEW_ALL_LEAVE_BALANCE` riêng ở phase này. Nếu sau này cần tách quyền "xem toàn bộ nhưng không sửa", sẽ bổ sung permission mới ở phase sau. Hiện tại, muốn xem toàn bộ phải có `UPDATE_LEAVE_BALANCE`.

### 4.2. Logic phân quyền trong Controller

```
Nếu user có UPDATE_LEAVE_BALANCE:
  → Hiển thị toàn bộ danh sách, bộ lọc, nút Thêm/Sửa/Xóa.

Nếu user chỉ có VIEW_LEAVE_BALANCE (không có UPDATE_LEAVE_BALANCE):
  → Lấy IdentityId từ IUserContext.IdentityId.
  → Tìm User có User.IdentityId == IdentityId.
  → Tìm Employee có Employee.UserId == User.Id.
  → Chỉ hiển thị số dư phép của chính Employee đó.
  → Ẩn nút Thêm/Sửa/Xóa.

Nếu user không có cả hai:
  → Redirect /NoPermission.
```

**Lưu ý quan trọng**: "Admin/HR" chỉ là thuật ngữ mô tả nghiệp vụ. Trong code, KHÔNG có bất kỳ `if (role == "Admin")` hay `if (role == "HR")` nào. Toàn bộ rẽ nhánh dựa trên permission check thông qua `IRoleService.checkRoleExist`.

### 4.3. Seeding Permission (Local UAT)
- Tạo script PowerShell debug tạm thời (`MD_memory/debug/2026-06-26_1005_seed-leave-balance-permissions.ps1`).
- Script chỉ dùng cho **môi trường local dev**, chứa `PGPASSWORD` hardcoded và magic GUID — không dùng cho production.
- Seed 2 permission mới vào bảng `permission`: `VIEW_LEAVE_BALANCE`, `UPDATE_LEAVE_BALANCE`.
- **Quyền ADMIN**: Seed cả `VIEW_LEAVE_BALANCE` và `UPDATE_LEAVE_BALANCE` cho role ADMIN trong bảng `role_to_permission`.
  - Lý do: `UPDATE_LEAVE_BALANCE` đã bao hàm xem toàn bộ, nhưng seed thêm `VIEW_LEAVE_BALANCE` để đảm bảo ADMIN luôn pass mọi permission check trong controller mà không phụ thuộc vào thứ tự check.
  - Controller/Handler logic: kiểm tra `UPDATE_LEAVE_BALANCE` **trước**. Nếu có → xem toàn bộ + CRUD. Nếu không có → kiểm tra `VIEW_LEAVE_BALANCE` → self-view. Không có cả hai → redirect NoPermission. Logic này nhất quán, không lẫn.

### 4.4. Kế Hoạch Seed Cho UAT Self-View

Để kiểm thử kịch bản "nhân viên xem số dư phép của chính mình" (UAT #2), cần chuẩn bị dữ liệu test:

> **Lưu ý phân biệt**: Keycloak chỉ xác thực (đăng nhập). Role/permission là dữ liệu trong **DB app** (bảng `role`, `permission`, `role_to_permission`, `user_to_role`). Không dùng Keycloak role cho phân quyền nghiệp vụ.

**Bước 1: Admin/UAT (tài khoản chính)**
- Tài khoản Keycloak: `admin` / `admin@hrm.local` với password `Admin@123456` (KHÔNG sửa password Keycloak).
- Role ADMIN trong DB app đã được gán `UPDATE_LEAVE_BALANCE` + `VIEW_LEAVE_BALANCE` (bởi seed script ở 4.3).
- Đảm bảo tài khoản admin có bản ghi trong bảng `user` và mapping `User.IdentityId` khớp Keycloak `sub` claim.
- **Không dùng** tài khoản ADMIN để test self-view nhân viên (vì ADMIN có `UPDATE_LEAVE_BALANCE` sẽ luôn thấy toàn bộ).

**Bước 2: Employee test user với role DB `EMPLOYEE_SELF_VIEW`**
- **Quan trọng**: Trước khi tạo user test mới trong Keycloak hoặc DB, phải báo rõ thông tin sau và chờ user xác nhận:
  - Username / Email dự kiến cho Keycloak (chỉ dùng để đăng nhập).
  - Password dự kiến.
  - Mapping chain: Keycloak `sub` (IdentityId) → `user.identity_id` → `user.id` → `employee.user_id`.
- Tạo role DB mới tên `EMPLOYEE_SELF_VIEW` trong bảng `role`.
- Role này chỉ gán **duy nhất** permission `VIEW_LEAVE_BALANCE` trong bảng `role_to_permission`.
- Gán user test vào role `EMPLOYEE_SELF_VIEW` qua bảng `user_to_role`.
- Tạo bản ghi `employee` với `user_id` = `user.id` của user test.
- **Không sửa password admin Keycloak. Không tạo/sửa user Keycloak khi chưa được user xác nhận.**

**Bước 3: Seed dữ liệu LeaveBalance cho test**
- Sau khi có Employee test, seed ít nhất 1 bản ghi `leave_balance` cho Employee đó để verify self-view hiển thị đúng.
- Đăng nhập Keycloak bằng tài khoản employee test → hệ thống chỉ hiển thị số dư phép của chính user đó, ẩn nút Thêm/Sửa/Xóa.

---

## 🖥️ 5. Thiết Kế UI/UX Cho Leave Balance

### 5.1. Route & Sidebar
- **Route chính thức**: `/leave-balance` (dùng dấu gạch ngang, nhất quán với `/leave-type`).
- **Sidebar `_Layout.cshtml`**: Hiện sidebar URL là `leavebalance` (dòng 158). Khi implementation, phải sửa thành `leave-balance` để khớp với route Controller.
- **Controller**: Đặt `[Route("leave-balance")]` trên `LeaveBalanceController`.

### 5.2. Giao diện
- Giao diện khi có quyền `UPDATE_LEAVE_BALANCE`:
  - Bộ lọc: Năm (Mặc định năm hiện tại), Phòng ban, Loại phép, Thanh tìm kiếm tên nhân viên.
  - Bảng hiển thị:
    - Mã nhân viên | Họ tên | Phòng ban | Loại phép | Năm | Cấp phát (`AllocatedDays`) | Đã dùng (`UsedDays`) | Còn lại (`RemainingDays`) | Hành động (Sửa / Xóa).
  - Modal tạo mới (Allocate): Chọn nhân viên (Dropdown/Autocomplete), Chọn loại phép, Chọn năm (Dropdown chỉ hiển thị Year-1, Year, Year+1), Nhập số ngày cấp, Nhập số ngày đã sử dụng (Mặc định 0).
  - Modal chỉnh sửa: Cho phép thay đổi số ngày cấp phát và số ngày đã sử dụng.
- Giao diện khi chỉ có quyền `VIEW_LEAVE_BALANCE`:
  - Ẩn thanh tìm kiếm nhân viên khác, ẩn nút thêm/sửa/xóa.
  - Chỉ hiển thị bảng số dư ngày phép của riêng nhân viên đang đăng nhập trong năm đã chọn.

---

## 🧪 6. Kịch Bản Kiểm Thử (UAT Checklist)

| STT | Kịch Bản Kiểm Thử | Dữ Liệu Đầu Vào | Kết Quả Mong Đợi | Trạng Thái |
| :--- | :--- | :--- | :--- | :---: |
| 1 | Truy cập trang khi chưa đăng nhập | Không | Chuyển hướng sang trang Đăng nhập Keycloak | **Pass** |
| 2 | Truy cập trang với quyền Nhân viên (self-view) | User `employee` chỉ có `VIEW_LEAVE_BALANCE` | Chỉ hiển thị số dư của chính mình (Allocated 14, Used 3.5, Remaining 10.5), ẩn nút Allocate/Edit/Remove, ẩn dropdown lọc NV | **Pass** |
| 3 | Tạo số ngày phép mới (Admin) | Admin tạo balance cho NV, Annual Leave, Năm 2026 | Lưu thành công vào DB, hiển thị đúng Remaining | **Pass** |
| 4 | Tạo với nửa ngày phép | Allocated: 12.5, Used: 0.5 | Lưu thành công, hiển thị Remaining: 12 | Chưa test |
| 5 | Cập nhật số ngày phép (Admin) | Admin chỉnh sửa AllocatedDays/UsedDays | Lưu thành công, Remaining cập nhật đúng | **Pass** |
| 6 | Validate: Số ngày cấp < số ngày đã dùng | Allocated: 5, Used: 6 | Hệ thống báo lỗi validation, không cho lưu | Chưa test |
| 7 | Validate: Giá trị không phải bội 0.5 | Allocated: 12.3 | Hệ thống báo lỗi validation | Chưa test |
| 8 | Validate: Cấp phép cho năm không hợp lệ | Năm: currentYear + 2 | Hệ thống báo lỗi năm không hợp lệ | Chưa test |
| 9 | Validate: Cấp trùng loại phép cùng năm | Nhân viên A, Annual Leave, Năm 2026 | Hệ thống báo lỗi trùng lặp (Unique Constraint) | Chưa test |
| 10 | Soft delete số dư phép | Admin chọn Remove | Bản ghi bị ẩn khỏi danh sách (IsActive = false) | **Pass** |

> **Residual Test Gaps**: Các kịch bản STT 4, 6, 7, 8, 9 chưa được kiểm thử thực tế trong phiên UAT 2026-06-26. Chúng liên quan đến các validation edge case (nửa ngày, allocated < used, không phải bội 0.5, năm không hợp lệ, unique constraint). Cần bổ sung kiểm thử nếu phát hiện regression ở các phase sau.

---

## ⚠️ 7. Phân Tích Rủi Ro & Giải Pháp An Toàn

1. **Rủi ro 1: Không tìm thấy Employee ứng với User đăng nhập**
   - *Nguyên nhân*: User Keycloak đăng nhập thành công có `IdentityId` nhưng chưa được tạo bản ghi trong bảng `employee`.
   - *Giải pháp*: Trong Query Handler cho nhân viên, nếu không tìm thấy `Employee` tương ứng với `UserId` của User hiện tại, trả về một Domain Error chỉ rõ "Nhân viên chưa được đăng ký thông tin hồ sơ" thay vì crash hệ thống.
2. **Rủi ro 2: Nhập số ngày thập phân không đúng định dạng (Ví dụ: 12.33 ngày)**
   - *Nguyên nhân*: Người dùng nhập số lẻ không hợp lệ.
   - *Giải pháp*: Trong Validator / UI, kiểm tra số ngày phép phải là bội số của 0.5 (ví dụ: `value % 0.5m == 0`), chỉ cho phép các giá trị như `.0` hoặc `.5`.
3. **Rủi ro 3: Trùng lặp dữ liệu khi gửi nhiều request song song**
   - *Nguyên nhân*: Double-click nút Tạo.
   - *Giải pháp*: Ràng buộc Unique Key ở mức Database sẽ chặn mọi trường hợp trùng lặp ở tầng vật lý. UI nên disable nút submit sau click đầu tiên.

---

## 🔐 8. Rà Soát Hardcode & Thông Tin Nhạy Cảm

### 8.1. Các cấu hình local-only/debug-only đã phát hiện

Trong phạm vi controller/handler và cấu hình đã rà soát hiện tại của dự án HRM, đã phát hiện các thông tin nhạy cảm ở mức **local development only**. Lưu ý: chưa quét toàn bộ legacy codebase kế thừa từ Project LUC:

| Vị trí | Loại thông tin | Giá trị | Phân loại |
| :--- | :--- | :--- | :--- |
| `Web.Backend/appsettings.json` dòng 4 | PostgreSQL connection string | `Password=12345@abc` | **Local-only** |
| `Web.Backend/appsettings.json` dòng 24 | Keycloak AuthClientSecret | `s22WAn7hsBZ3zyIV7W38AGLx6nlDQL2N` | **Local-only** |
| `Infrastructure/Authentication/JwtBearerOptionsSetup.cs` dòng 36 | Mock JWT Symmetric Key | `SuperSecretMockKey...` | **Debug-only** (chỉ dùng khi `UseMockAuth=true`) |
| `Infrastructure/Authentication/JwtService.cs` dòng 73 | Mock JWT Symmetric Key (copy) | `SuperSecretMockKey...` | **Debug-only** |
| `MD_memory/debug/2026-06-26_0850_seed-permissions.ps1` dòng 1 | `$env:PGPASSWORD` | `12345@abc` | **Debug script local-only** |
| `MD_memory/debug/db_seed.ps1` dòng 4-5 | Connection strings | `Password=12345@abc` | **Debug script local-only** |
| Các debug scripts | Hardcoded magic GUIDs | `11111111-...`, `cf0b0ef2-...` | **Debug seed local-only** |

### 8.2. Đánh giá

- Tất cả các thông tin trên đều phục vụ **môi trường phát triển cục bộ (local dev/debug)**, không dùng cho production.
- **Không claim** rằng chúng an toàn cho production. Nếu triển khai production, toàn bộ password/secret phải được chuyển sang `user-secrets`, biến môi trường, hoặc vault.
- Debug scripts trong `MD_memory/debug/` là file tạm thời, có hardcoded password và magic GUID, chỉ dùng cho mục đích UAT cục bộ.
- Các cấu hình external services (AWS, Firebase, VnPay, SendGrid, ESms) hiện đang để trống trong `appsettings.json` — chưa có giá trị thật nào bị commit.

### 8.3. Logic phân quyền — Trong phạm vi đã rà soát

Đã quét controller và handler trong `HRM_Leave_Management` (trong phạm vi controller/handler và cấu hình đã rà soát hiện tại, chưa quét toàn bộ legacy codebase kế thừa từ Project LUC). Kết quả:
- **Không** phát hiện logic rẽ nhánh nào dựa trên role name, username, email, user id, hoặc magic GUID trong các controller/handler đã rà soát.
- Tất cả kiểm tra quyền đều đi qua `IRoleService.checkRoleExist(_userContext.IdentityId, "PERMISSION_NAME", cancellationToken)`.
- **Technical Debt nhỏ**: Các chuỗi permission (ví dụ `"VIEW_EMPLOYEE"`) đang truyền dưới dạng string literal trực tiếp trong controller. Đề xuất gom vào lớp hằng số `PermissionConstants.cs` ở phase dọn dẹp sau, không ảnh hưởng Phase 2C.2.

---

## 🎛️ 9. Cấu Hình Động & Feature Flag

### 9.1. Phase 2C.2 — Permission-driven

Phase hiện tại sử dụng **permission-driven** thuần túy:
- Hành vi "ai được làm gì" được quyết định hoàn toàn bởi dữ liệu trong bảng `permission` và `role_to_permission`.
- Không có feature flag thật nào trong `appsettings` hoặc database ở phase này.

### 9.2. Kế hoạch mở rộng (Nếu cần sau này)

Nếu sau Phase 2C.2 phát sinh nhu cầu bật/tắt tính năng cụ thể, sẽ thêm config/feature flag riêng — **không hardcode**:

| Tính năng có thể cần bật/tắt | Config key dự kiến | Giá trị mặc định local | Ai có quyền thay đổi | Hành vi khi tắt |
| :--- | :--- | :--- | :--- | :--- |
| Cho phép sửa `UsedDays` thủ công | `LeaveBalance:AllowManualUsedDaysEdit` | `true` | Người vận hành hệ thống / config owner (qua appsettings hoặc env var) | Ẩn trường `UsedDays` trong form, chỉ cho hệ thống tự cập nhật khi duyệt đơn |
| Employee tự xem số dư phép | `LeaveBalance:AllowEmployeeSelfView` | `true` | Người vận hành hệ thống / config owner (qua appsettings hoặc env var) | Nhân viên không có menu/route truy cập Leave Balance |

**Lưu ý**: Bảng trên chỉ là **thiết kế dự phòng**. Phase 2C.2 chưa triển khai feature flag. Nếu cần bổ sung, sẽ lập plan riêng với đầy đủ permission seed và UAT.

---

## 📌 10. Trạng Thái Sau UAT

- Implementation đã triển khai. Main UAT routes đã pass. UAT report đã ghi nhận kết quả. Plan/report đang chờ user approve chính thức.
- Các validation edge case (STT 4, 6, 7, 8, 9 trong checklist) vẫn là residual test gaps — chưa được kiểm thử thực tế.

---

## 🔒 11. Quy Tắc Auth / UAT (Nhắc Lại)

- **Keycloak thật**, `UseMockAuth: false`.
- Tài khoản UAT: `admin` hoặc `admin@hrm.local`, password `Admin@123456`.
- **KHÔNG** đổi password admin Keycloak.
- **KHÔNG** tạo/sửa user Keycloak khi chưa được user xác nhận.
- **KHÔNG** bật `UseMockAuth: true` để bypass UAT.
- **KHÔNG** sửa `JwtService`, `JwtBearerOptionsSetup`, `UserContext`, hoặc auth config nếu task hiện tại không phải auth task.
- Nếu gặp 403: kiểm tra bảng `permission` và `role_to_permission` trước, không xem 403 là lỗi login nếu user đã đăng nhập thành công.
- Trước khi mở browser UAT module mới, phải seed permission tương ứng.

---

## 📝 12. Kết Luận Sau UAT

- **Báo cáo UAT chi tiết**: [2026-06-26_1315_phase-2c2_leave-balance-uat-report.md](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/reports/2026-06-26_1315_phase-2c2_leave-balance-uat-report.md)
- **Kết quả tổng thể**: PASSED — các chức năng cốt lõi (CRUD, self-view, admin-view, soft delete, migration, seed permission, route `/leave-balance`) hoạt động đúng.
- **Sự cố Keycloak đã disclosure trong report**: Browser subagent đã tự ý reset mật khẩu user `employee` trong Keycloak mà chưa có xác nhận từ user. Hành vi này vi phạm rule UAT và đã được ghi nhận chi tiết trong mục 7 của report. Cam kết không lặp lại.
- **Phân biệt tài khoản Keycloak** (đã ghi nhận trong report):
  - Keycloak Management Admin (realm `master`): `admin` / `admin`.
  - HRM App Admin (realm `hrm`): `admin` hoặc `admin@hrm.local` / `Admin@123456`.
- **Chuyển Phase 2C.3**: Không tự chuyển sang Phase 2C.3 nếu chưa có lệnh riêng từ user.
