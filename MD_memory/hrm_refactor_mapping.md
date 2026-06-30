# HRM Refactor Mapping — Phân loại Module & Kế hoạch Hành động

> Ngày tạo: 2026-06-24
> Cập nhật: 2026-06-24 (v3 — tiếng Việt có dấu, bổ sung Phase 2A Detailed Design)
> Trạng thái: Bản nháp — chờ xác nhận trước khi thực hiện
> Chiến lược: **Xây song song trước, xóa module cũ sau khi có module thay thế.**

---

## 1. Tổng quan Hiện trạng

Bản copy `HRM_Leave_Management` hiện là **bản sao nguyên xi** của Project LUC gốc. Chưa có bất kỳ refactor nghiệp vụ nào, chỉ sửa config (`appsettings.json`, `launchSettings.json`) và seed quyền.

### Thống kê module nghiệp vụ (không tính bin/obj):

| Tầng | Số lượng | Ghi chú |
|---|---|---|
| Domain | ~49 thư mục nghiệp vụ | Entity, Value Object, Repository Interface |
| Application | ~35 thư mục nghiệp vụ | Command, Query, Handler |
| Infrastructure/Repositories | 41 file | Kế thừa `Repository<TEntity, TEntityId>` base class |
| Infrastructure/Configurations | 48 file | EF Core `IEntityTypeConfiguration` |
| Web.Backend/Controllers | 23 file | MVC Controller + Razor Views |

### Kiến trúc hiện có:

- **Entity base:** `Entity<TEntityId> : IEntity` — hỗ trợ domain events (dòng 1-32, `Domain/Abstractions/Entity.cs`)
- **Id pattern:** Strongly-typed ID dạng `record UserId(Guid Value)` với `New()` factory
- **Repository base:** `Repository<TEntity, TEntityId>` — `GetEntitiesAsQueryable`, `GetAllPaged`, `GetByIdAsync`, `Add`, `Update`, `Remove` (105 dòng, `Infrastructure/Repositories/Repository.cs`)
- **DbContext:** Auto-scan configuration qua `ApplyConfigurationsFromAssembly`
- **Permission pattern:** Bảng `permission` với `ResourceName` (VD: `VIEW_BOOKING`) + `DisplayName`, gán qua `RoleToPermission`

---

## 2. Chiến lược Refactor

> [!IMPORTANT]
> **Không xóa module cũ trước.** Xây module HRM mới song song trên baseline đang chạy. Sau khi module HRM đã thay thế hoàn toàn và verify, mới xóa module cũ theo đợt.

### Lộ trình Phase:

| Phase | Nội dung | Điều kiện bắt đầu |
|---|---|---|
| **2A.1** | Department CRUD tối thiểu | Xác nhận design (tài liệu này) |
| **2A.2** | Employee CRUD tối thiểu (có FK → Department) | Phase 2A.1 verify xong |
| **2B** | Đổi sidebar sang HRM, ẩn menu cũ | Phase 2A.2 verify xong |
| **2C** | LeaveType + LeaveRequest + LeaveBalance | Phase 2B verify xong |
| **3** | Approval Flow + Notification + Dashboard HRM | Phase 2C verify xong |
| **Cleanup** | Xóa module gốc theo đợt dependency | Phase 2C verify xong |

---

## 3. Phân loại Module

### NHÓM A: GIỮ LẠI (nền tảng auth, layout, abstractions)

Users, Roles, Permissions, UserToRoles, RoleToPermissions, Auth (Login/Logout), Layout Shell, Dashboard Shell (chỉ giữ shell — nghiệp vụ booking/revenue hiện tại sẽ thay thế trong Phase 3), Domain Abstractions, Application Abstractions, Domain Shared, ApplicationDbContext, Outbox, Clock.

> **Phản biện Dashboard:** `DashboardController.cs` dòng 36-46 gọi `GetBookingReportCommand` + `GetRevenueCommand` — nghiệp vụ LUC thuần túy. Chỉ giữ shell/chart pattern/permission check, nội dung sẽ thay bằng thống kê HRM (Phase 3).

### NHÓM B: TẠM GIỮ THAM CHIẾU PATTERN

Members (CRUD pattern), Notifications (pattern thông báo — **cảnh báo:** Firebase chưa cấu hình local, `IFirebaseMessaging` đăng ký DI dòng 173 có thể lỗi runtime), Categories (CRUD đơn giản), Repository.cs base, DependencyInjection.cs, MediatR Behaviors.

### NHÓM C: LOẠI BỎ SAU KHI HRM ĐÃ THAY THẾ

30+ module nhà hàng/loyalty. Xóa theo 4 đợt dependency. Chi tiết xem mục 6.

### NHÓM D: XÂY MỚI CHO HRM

Employee, Department, LeaveType, LeaveBalance, LeaveRequest, LeaveApproval. Chi tiết Phase 2A bên dưới.

---

## 4. Phase 2A Detailed Design

### 4.1 Entity `Department`

```csharp
// Domain/Departments/DepartmentId.cs
public record DepartmentId(Guid Value)
{
    public static DepartmentId New() => new(Guid.NewGuid());
}

// Domain/Departments/Department.cs
public class Department : Entity<DepartmentId>
{
    public string Name { get; private set; }
    public string Code { get; private set; }           // VD: "IT", "HR", "FIN"
    public string? Description { get; private set; }
    public DepartmentId? ParentDepartmentId { get; private set; }  // Self-ref cho phòng ban cha
    public Department? ParentDepartment { get; private set; }       // Navigation property
    public bool IsActive { get; private set; }
    public DateTime CreatedDate { get; private set; }
}
```

**Quyết định thiết kế:**

| Thuộc tính | Quyết định | Lý do |
|---|---|---|
| `Name` | `string` (không dùng Value Object) | Department name không cần validation phức tạp như `Email` hay `PhoneNumber`. Dùng VO ở đây over-engineering. |
| `Code` | `string`, unique | Mã phòng ban ngắn để hiển thị/filter nhanh. Unique constraint trong Configuration. |
| `ParentDepartmentId` | **Nullable** FK self-reference | Cho phép cấu trúc phòng ban cha-con. `null` = phòng ban gốc (root). |
| `IsActive` | `bool` thay vì soft-delete | Đơn giản hơn `IsDeleted`, semantics rõ ràng hơn cho UI (active/inactive). |

> [!IMPORTANT]
> **Chốt: Department KHÔNG có `ManagerId`.** Lý do: nếu `Department.ManagerId → Employee` và `Employee.DepartmentId → Department` thì tạo vòng FK hai chiều. EF Core migration sẽ phức tạp (phải insert một bên trước với null rồi update sau). Thay vào đó, quan hệ "ai quản lý phòng ban" sẽ được giải quyết qua query: tìm Employee có `DepartmentId = X` và `Position = 'Manager'` hoặc qua `Employee.ManagerId` (self-ref). Đơn giản hơn, ít coupling hơn.

**EF Configuration dự kiến:**
```csharp
// Infrastructure/Configurations/DepartmentConfiguration.cs
builder.ToTable("department");
builder.HasKey(d => d.Id);
builder.Property(d => d.Id)
    .HasConversion(id => id.Value, value => new DepartmentId(value));
builder.Property(d => d.Name).IsRequired().HasMaxLength(200);
builder.Property(d => d.Code).IsRequired().HasMaxLength(20);
builder.HasIndex(d => d.Code).IsUnique();

// Nullable FK self-ref — pattern null-safe
builder.Property(d => d.ParentDepartmentId)
    .HasConversion(
        id => id == null ? (Guid?)null : id.Value,
        value => value.HasValue ? new DepartmentId(value.Value) : null);
builder.HasOne(d => d.ParentDepartment)
    .WithMany()
    .HasForeignKey(d => d.ParentDepartmentId)
    .OnDelete(DeleteBehavior.Restrict);  // Không cascade delete phòng ban con
```

---

### 4.2 Entity `Employee`

```csharp
// Domain/Employees/EmployeeId.cs
public record EmployeeId(Guid Value)
{
    public static EmployeeId New() => new(Guid.NewGuid());
}

// Domain/Employees/Employee.cs
public class Employee : Entity<EmployeeId>
{
    public UserId? UserId { get; private set; }             // FK -> User (nullable Phase 2A)
    public User? User { get; private set; }                  // Navigation
    public string FullName { get; private set; }
    public string EmployeeCode { get; private set; }         // VD: "NV001"
    public DepartmentId? DepartmentId { get; private set; }  // FK -> Department
    public Department? Department { get; private set; }       // Navigation
    public string? Position { get; private set; }            // VD: "Senior Developer"
    public DateTime JoinDate { get; private set; }
    public EmployeeId? ManagerId { get; private set; }       // Self-ref -> quản lý trực tiếp
    public Employee? Manager { get; private set; }           // Navigation
    public bool IsActive { get; private set; }
    public DateTime CreatedDate { get; private set; }
}
```

**Quyết định thiết kế quan trọng:**

#### 4.2.1 `Employee.UserId` → `User.Id`: Nullable (Phase 2A) → Required (Phase 2C)

**Vấn đề thực tế:** Hệ thống hiện tại tạo User qua flow:
1. Admin vào trang `/Users/Create` → gọi Keycloak API tạo account → insert bảng `user`
2. User record chỉ có: Name, Username, Email, PhoneNumber, IdentityId (Keycloak UUID)

**Khi tạo Employee, user Keycloak + bảng `user` được tạo ở đâu?**
- Nếu form Employee **chọn user có sẵn** (dropdown): cần User phải tồn tại trước → Required OK.
- Nếu form Employee **tạo user mới đồng thời**: phải gọi Keycloak API trong CreateEmployeeHandler → phức tạp, vượt scope Phase 2A.
- Nếu chưa có flow tạo user trong Employee form: **bắt buộc phải để nullable** để không block CRUD.

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| Required | Đảm bảo mọi employee có account. Join đơn giản. | Cần flow tạo User trước hoặc đồng thời. Phase 2A chưa có. |
| **Nullable (đề xuất Phase 2A)** | Tạo hồ sơ nhân viên trước, gán account sau. Không block CRUD. | Phải check null khi tạo LeaveRequest (Phase 2C). |

> [!IMPORTANT]
> **Chốt Phase 2A: Nullable.** Lý do:
> - Phase 2A tập trung CRUD cơ bản, chưa có flow tạo user Keycloak trong Employee form.
> - Form Employee sẽ có dropdown "Chọn tài khoản" (danh sách User có sẵn) — **không bắt buộc**.
> - Khi sang Phase 2C (LeaveRequest), sẽ thêm validation: chỉ cho tạo đơn nghỉ nếu `UserId != null`.
> - Nếu sau này cần enforce required, chỉ cần thêm migration `ALTER COLUMN SET NOT NULL` + update seed data.
>
> **Lộ trình chuyển Required:** Phase 2C hoặc Phase 3 — khi đã có flow gán user cho tất cả employee.

#### 4.2.2 `Employee.DepartmentId`: Nullable

**Đề xuất: Nullable.** Lý do: Nhân viên mới có thể chưa được phân phòng ban. Hoặc phòng ban bị tái cơ cấu. Không nên block tạo employee vì chưa có department.

#### 4.2.3 `Employee.ManagerId`: Self-reference

**Đề xuất: Nullable FK self-reference.** `null` = nhân viên cấp cao nhất (CEO/Director, không có quản lý). Dùng cho approval flow sau này: khi nhân viên tạo đơn nghỉ, hệ thống gửi cho `ManagerId` duyệt.

**EF Configuration dự kiến:**
```csharp
// Infrastructure/Configurations/EmployeeConfiguration.cs
builder.ToTable("employee");
builder.HasKey(e => e.Id);
builder.Property(e => e.Id)
    .HasConversion(id => id.Value, value => new EmployeeId(value));

// FK -> User (nullable Phase 2A, chuyển required Phase 2C) — pattern null-safe
builder.Property(e => e.UserId)
    .HasConversion(
        id => id == null ? (Guid?)null : id.Value,
        value => value.HasValue ? new UserId(value.Value) : null);
builder.HasIndex(e => e.UserId).IsUnique();
builder.HasOne(e => e.User)
    .WithMany()
    .HasForeignKey(e => e.UserId)
    .OnDelete(DeleteBehavior.Restrict);

// FK -> Department (nullable) — pattern null-safe
builder.Property(e => e.DepartmentId)
    .HasConversion(
        id => id == null ? (Guid?)null : id.Value,
        value => value.HasValue ? new DepartmentId(value.Value) : null);
builder.HasOne(e => e.Department)
    .WithMany()
    .HasForeignKey(e => e.DepartmentId)
    .OnDelete(DeleteBehavior.SetNull);

// Self-ref -> Manager (nullable) — pattern null-safe
builder.Property(e => e.ManagerId)
    .HasConversion(
        id => id == null ? (Guid?)null : id.Value,
        value => value.HasValue ? new EmployeeId(value.Value) : null);
builder.HasOne(e => e.Manager)
    .WithMany()
    .HasForeignKey(e => e.ManagerId)
    .OnDelete(DeleteBehavior.Restrict);  // Không cascade delete nhân viên khi xóa manager

builder.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(20);
builder.HasIndex(e => e.EmployeeCode).IsUnique();
builder.Property(e => e.FullName).IsRequired().HasMaxLength(200);
builder.Property(e => e.Position).HasMaxLength(100);
```

> [!NOTE]
> **Về nullable FK conversion:** Tất cả snippet trên đã dùng pattern null-safe (`id == null ? (Guid?)null : id.Value`). Nếu muốn đơn giản hơn, có thể dùng `Guid?` thay `DepartmentId?` cho FK nullable — trade-off type-safety vs đơn giản.

---

### 4.3 Permission cần seed

Dựa trên pattern hiện có trong `SeedDb/Program.cs` (dòng 10-44), permission theo cặp VIEW/UPDATE:

```
Permissions mới cần thêm:
  ("VIEW_EMPLOYEE",    "Xem nhân viên")
  ("UPDATE_EMPLOYEE",  "Quản lý nhân viên")
  ("VIEW_DEPARTMENT",  "Xem phòng ban")
  ("UPDATE_DEPARTMENT","Quản lý phòng ban")
```

**Cách seed:** Bổ sung vào script `SeedDb/Program.cs` hiện có. Script dùng `ON CONFLICT DO UPDATE` nên chạy lại an toàn (idempotent).

**Gán cho role Admin:** Thêm `role_to_permission` record liên kết 4 permission mới với role Admin (ID `11111111-1111-1111-1111-111111111111`).

---

### 4.4 Route / Controller / View dự kiến

| Controller | Route | Action | Permission | Ghi chú |
|---|---|---|---|---|
| `EmployeeController` | `/Employee` | Index (list + search + pagination) | `VIEW_EMPLOYEE` | Pattern theo `MemberController` |
| | `/Employee/Create` | Create (GET form + POST) | `UPDATE_EMPLOYEE` | |
| | `/Employee/Edit/{id}` | Edit (GET form + POST) | `UPDATE_EMPLOYEE` | |
| | `/Employee/Detail/{id}` | Detail (read-only) | `VIEW_EMPLOYEE` | |
| `DepartmentController` | `/Department` | Index (list) | `VIEW_DEPARTMENT` | Pattern theo `CategoriesController` |
| | `/Department/Create` | Create | `UPDATE_DEPARTMENT` | |
| | `/Department/Edit/{id}` | Edit | `UPDATE_DEPARTMENT` | |

**View structure dự kiến:**
```
Views/
  Employee/
    Index.cshtml          (danh sách + search/filter)
    Create.cshtml         (form tạo mới)
    Edit.cshtml           (form chỉnh sửa)
    Detail.cshtml         (xem chi tiết)
    _EmployeeForm.cshtml  (partial form dùng chung Create/Edit)
  Department/
    Index.cshtml
    Create.cshtml
    Edit.cshtml
```

---

### 4.5 Migration Strategy

**Tạo migration riêng cho từng sub-phase:**

| Sub-phase | Migration name | Nội dung |
|---|---|---|
| 2A.1 | `AddDepartment` | Bảng `department` + index unique `Code` |
| 2A.2 | `AddEmployee` | Bảng `employee` + FK → `user`, `department`, self-ref `manager_id` |

> [!NOTE]
> **Kiểm tra trước khi tạo migration:** Chạy `dotnet ef migrations list` để verify migration history trong DB khớp với thư mục `Infrastructure/Migrations/`. Nếu lệch, cần resolve trước.
> 
> **Phương án đề xuất: Thêm migration** (`dotnet ef migrations add AddDepartment`). EF sẽ tạo migration chỉ chứa bảng mới. Schema cũ giữ nguyên.

---

### 4.6 Cấu trúc file cần tạo (Phase 2A)

```
Domain/
  Departments/
    Department.cs
    DepartmentId.cs
    IDepartmentRepository.cs
    DepartmentErrors.cs
  Employees/
    Employee.cs
    EmployeeId.cs
    IEmployeeRepository.cs
    EmployeeErrors.cs

Application/
  Departments/
    GetAllPaged/     (GetAllDepartmentPagedCommand + Handler)
    Create/          (CreateDepartmentCommand + Handler + Validator)
    Update/          (UpdateDepartmentCommand + Handler + Validator)
    GetOne/          (GetOneDepartmentCommand + Handler)
  Employees/
    GetAllPaged/     (GetAllEmployeePagedCommand + Handler)
    Create/          (CreateEmployeeCommand + Handler + Validator)
    Update/          (UpdateEmployeeCommand + Handler + Validator)
    GetOne/          (GetOneEmployeeCommand + Handler)

Infrastructure/
  Configurations/
    DepartmentConfiguration.cs
    EmployeeConfiguration.cs
  Repositories/
    DepartmentRepository.cs
    EmployeeRepository.cs

Web.Backend/
  Controllers/
    EmployeeController.cs
    DepartmentController.cs
  Views/
    Employee/  (Index, Create, Edit, Detail)
    Department/ (Index, Create, Edit)
```

**Đăng ký trong DependencyInjection.cs:** Thêm 2 dòng vào `AddPersistence()`:
```csharp
services.AddScoped<IDepartmentRepository, DepartmentRepository>();
services.AddScoped<IEmployeeRepository, EmployeeRepository>();
```

---

### 4.7 Verify Checklist

**Phase 2A.1 (Department):**
- [x] `dotnet build` pass
- [x] `dotnet ef migrations add AddDepartment` thành công
- [x] `dotnet ef database update` tạo bảng `department`
- [x] Seed 2 permission: `VIEW_DEPARTMENT`, `UPDATE_DEPARTMENT`
- [x] `dotnet run` khởi động không lỗi
- [x] Login admin → Dashboard bình thường
- [x] Truy cập `/Department` → danh sách trống
- [x] Tạo phòng ban → thành công
- [x] Route đã test: `/auth/login-screen`, `/dashboard`, `/department` (login, dashboard, department list)
- [x] **Department CRUD UAT pass với Keycloak thật** (2026-06-25)
- [x] Login sai password → bị Keycloak từ chối ("Incorrect Username or Password")
- [x] Login đúng password (`admin` / `Admin@123456`) → thành công qua Keycloak
- [x] Create / Edit / Delete Department → tất cả hoạt động bình thường
- [x] `UseMockAuth: false` đã xác nhận trong `appsettings.json`

**Phase 2A.2 (Employee):**
- [x] `dotnet ef migrations add AddEmployee` thành công
- [x] `dotnet ef database update` tạo bảng `employee`
- [x] Seed 2 permission: `VIEW_EMPLOYEE`, `UPDATE_EMPLOYEE`
- [x] Truy cập `/Employee` → danh sách trống
- [x] Tạo nhân viên (chọn phòng ban, tùy chọn gán user) → thành công
- [ ] Route cũ vẫn hoạt động (cần verify lại đầy đủ, thực tế mới kiểm thử các route: `/employee`, `/NoPermission`, `/dashboard`, `/auth/login-screen` trong UAT)
- [ ] GitNexus: `detect_changes()` trước khi commit (không khả dụng do repo cục bộ chưa có HEAD/commit khởi tạo)

---

## 5. Dependency đặc biệt

| File | Vấn đề | Hành động |
|---|---|---|
| `Infrastructure/DependencyInjection.cs` (327 dòng) | Đăng ký 40+ repository. Khi xóa module phải xóa dòng đăng ký. | Phase 2A: chỉ thêm, không xóa |
| `ApplicationDbContext.cs` | Auto-scan Configuration. Xóa entity mà quên xóa Configuration → lỗi runtime. | Phase 2A: chỉ thêm |
| `DailyJob` + `YearEndJob` | Phụ thuộc `IMemberRepository` (nghiệp vụ loyalty). | Chưa động, xóa trong Phase Cleanup |
| `ConfigureAws()` | AWS S3 chưa cấu hình → 7 route lỗi 500. | Chưa động, xóa trong Phase Cleanup |
| `IFirebaseMessaging` (DI dòng 173) | Firebase chưa cấu hình local. | Cảnh báo rủi ro runtime, chưa xóa |

---

## 6. Thứ tự xóa Nhóm C (Phase Cleanup)

Chỉ bắt đầu sau Phase 2C verify xong.

```
Đợt 1 (không FK phức tạp):
  News, Partners, QrCode, PhoneValidationCheck, Districts, Provinces, Wards,
  FreeServices, MemberPointRules

Đợt 2 (entity con trước):
  MemberVouchers, MemberActivities, MemberDeviceTokens, MemberNotifications,
  MemberPointHistories, MembershipBenefits, OrderFees, PaymentDetails,
  InvoiceDetails, InvoiceFees, PromotionToRestaurants, ProductOfRestaurants,
  RestaurantMenuProducts

Đợt 3 (entity cha):
  Vouchers, Promotions, MembershipClasses, RestaurantAreas, RestaurantMenus,
  Invoices, Orders + Deliveries, Products, Restaurants

Đợt 4 (hub entity + external services):
  Members, Bookings, ConfigureAws, ConfigureVnPay, SmsServices,
  Firebase (nếu không cần), DailyJob, YearEndJob
```

---

## 7. Hành động tiếp theo

| Bước | Nội dung | Điều kiện |
|---|---|---|
| 1 | **Xác nhận Phase 2A design v4** trong tài liệu này | Chờ user review |
| 2 | Phase 2A.1: Tạo Department entity + repo + config + migration | Sau khi design được duyệt |
| 3 | Phase 2A.1: Tạo Application handlers + Controller + Views | Sau bước 2 build pass |
| 4 | Phase 2A.1: Seed permissions + verify checklist 2A.1 | Sau bước 3 |
| 5 | Phase 2A.2: Tạo Employee entity + repo + config + migration | Sau 2A.1 verify pass |
| 6 | Phase 2A.2: Tạo Application handlers + Controller + Views | Sau bước 5 build pass |
| 7 | Phase 2A.2: Seed permissions + verify checklist 2A.2 | Sau bước 6 |
| 8 | GitNexus `detect_changes()` + báo cáo | Trước khi commit |
