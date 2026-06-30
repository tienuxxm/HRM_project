# Plan Tách Dự Án HRM / Leave Management

> Ngày tạo: 2026-06-23  
> Cập nhật: 2026-06-23 (v5 - viết lại có dấu, chốt chiến lược mini baseline trước, refactor từng lớp sau)  
> Trạng thái: Chờ xác nhận lại Phase 1 theo hướng v5  
> Tác giả: Anti / Codex phối hợp rà soát  
> Mục tiêu: Tạo một dự án HRM/Leave Management riêng bằng cách copy chọn lọc các phần nền móng của Customer Management & Loyalty System, chạy được thành một mini baseline trước, sau đó refactor từng lớp sang nghiệp vụ HRM.

---

## 1. Quyết Định Đã Xác Nhận

| Hạng mục | Quyết định |
|---|---|
| Thư mục dự án mới | `D:\Customer_Management_System-Cao_Thanh_Huy_01212407665\HRM_Leave_Management` |
| Database dự kiến | `hrm_db` |
| Postgres connection | `Host=localhost;Database=hrm_db;Username=postgres;Password=12345@abc;Port=5432` |
| Admin seed | Username: `admin`, Password: `Admin@123456` |
| .NET SDK | Máy đã có .NET SDK `8.0.128` |
| Repo gốc | Chỉ dùng làm reference/copy source, tuyệt đối không sửa/xóa/refactor nếu chưa có lệnh rõ ràng |
| Chiến lược chính | Copy chọn lọc phần nền móng, chạy mini baseline trước, refactor từng lớp sau |
| Không làm | Không raw copy toàn bộ repo gốc; không scaffold mới rồi tự viết HRM quá sớm |

---

## 2. Làm Rõ Chiến Lược

### 2.1 Điều Chúng Ta Không Muốn

**Không copy toàn bộ repo gốc.**  
Việc copy nguyên cả Customer Management & Loyalty System rồi xóa dần module thừa sẽ kéo theo nhiều code chết, migration cũ, dependency external service và nghiệp vụ nhà hàng/loyalty không cần thiết.

**Không viết mới HRM quá sớm.**  
Nếu scaffold một solution mới rồi tự viết entity/flow HRM ngay từ đầu, ta sẽ mất baseline để đối chiếu UI, auth, role/permission, dashboard và các pattern đã chạy ổn trong project gốc.

### 2.2 Điều Chúng Ta Muốn

Tạo một **mini baseline** trong `HRM_Leave_Management` bằng cách:

1. Copy chọn lọc những phần nền móng dự định refactor từ project gốc.
2. Giữ hành vi gần project gốc nhất có thể ở giai đoạn đầu.
3. Build/run/login được.
4. UI login/layout/dashboard shell không còn là template MVC trống.
5. Sau khi baseline ổn, mới refactor từng lớp sang HRM.

Nói ngắn gọn:

> Bê phần nền móng cần dùng ra trước, chạy được trước, rồi mới thay tên/thay nghiệp vụ/thay schema theo từng lớp.

---

## 3. Quy Tắc Bắt Buộc

1. **Không sửa repo gốc.**  
   Mọi thay đổi chỉ nằm trong `HRM_Leave_Management` hoặc tài liệu `MD_memory`.

2. **Không copy tràn lan.**  
   Chỉ copy phần có lý do rõ ràng:
   - Auth/login.
   - User/Role/Permission.
   - Clean Architecture foundation.
   - EF/repository pattern.
   - UI layout/login/dashboard shell.
   - Static assets cần cho UI.

3. **Không refactor quá sớm.**  
   Chưa đổi `User` thành `Employee` trên diện rộng nếu baseline chưa chạy ổn.

4. **Không đổi auth tùy tiện.**  
   Nếu Keycloak hoặc external config làm app không chạy local, phải debug và báo root cause trước khi thay bằng local JWT.

5. **Dùng GitNexus khi đụng symbol C#.**  
   Trước khi sửa function/class/method/symbol đã có, phải chạy impact/context nếu GitNexus index được. Nếu GitNexus chưa index `HRM_Leave_Management`, phải báo rõ và xin phép dùng manual analysis tạm thời.

6. **Verify trước khi báo xong.**  
   Không được báo hoàn thành nếu chưa build/run/login/browser test.

---

## 4. Target Framework

### 4.1 Tình Trạng Môi Trường

Máy hiện có:

```text
dotnet --list-sdks:
7.0.410
8.0.128
```

### 4.2 Quyết Định

Dùng **.NET 8** cho giai đoạn tách dự án.

Lý do:

- Project gốc đang gần hệ sinh thái .NET 7, nâng lên .NET 8 ít rủi ro hơn so với nhảy thẳng lên .NET 10.
- Máy đã có SDK .NET 8.
- .NET 8 đủ ổn định để dựng mini baseline.

Ghi chú dài hạn:

- .NET 10 LTS có thể là mục tiêu nâng cấp sau khi dự án đã tách ổn định.
- Không đưa việc nâng cấp .NET 10 vào Phase 1.

---

## 5. Chiến Lược Auth

### 5.1 Hiện Trạng Project Gốc

Project gốc dùng pattern **JWT-in-Cookie**:

- `LoginController.Login()` gọi MediatR.
- `AdminLoginCommandHandler` xử lý login.
- `JwtService.GetAccessTokenAsync()` gọi Keycloak.
- Token được lưu vào cookie `X-Access-Token`.
- Middleware trong `Program.cs` đọc cookie rồi gắn vào `Authorization: Bearer {token}`.
- `JwtBearerOptionsSetup` validate token.

### 5.2 Chiến Lược Cho Mini Baseline

Trong mini baseline, ưu tiên:

1. Copy/giữ flow auth gốc nhiều nhất có thể.
2. Chỉ sửa tối thiểu để chạy được trong `HRM_Leave_Management`.
3. Không thay Keycloak bằng local JWT nếu chưa cần.
4. Nếu thiếu Keycloak/config khiến app không chạy, phải ghi rõ lỗi và đề xuất refactor auth riêng.

### 5.3 Refactor Auth Sau Baseline

Sau khi baseline chạy ổn, có thể refactor:

- Bỏ Keycloak.
- Dùng local JWT signing bằng HMAC-SHA256.
- Dùng `PasswordHasher<T>` để hash password.
- Validate JWT bằng symmetric key.
- Seed admin local trong `hrm_db`.

Đây là bước sau baseline, không làm vội nếu chưa cần.

---

## 6. Cấu Trúc Dự Án Mục Tiêu

```text
HRM_Leave_Management/
|-- HRM.sln
|
|-- HRM.Domain/
|   |-- Abstractions/           <- copy từ repo gốc
|   |-- Shared/                 <- copy phần cần thiết
|   |-- Helpers/                <- copy nếu cần
|   |-- Users/                  <- copy pattern gốc nếu cần baseline
|   |-- Roles/                  <- copy từ repo gốc
|   |-- Permissions/            <- copy từ repo gốc
|   |-- UserToRoles/            <- copy trước nếu cần baseline
|   |-- EmployeeToRoles/        <- refactor sau nếu đã chốt User -> Employee
|
|-- HRM.Application/
|   |-- Abstractions/           <- copy chọn lọc
|   |-- Behaviors/              <- copy nếu cần
|   |-- Exceptions/             <- copy nếu cần
|   |-- Auth/                   <- copy pattern gốc trước, refactor sau
|   |-- Users/                  <- copy nếu cần baseline
|   |-- Roles/                  <- copy nếu cần
|   |-- Permissions/            <- copy nếu cần
|
|-- HRM.Infrastructure/
|   |-- Authentication/         <- copy pattern gốc trước, refactor sau
|   |-- Data/                   <- copy pattern EF/ApplicationDbContext nếu cần
|   |-- Repositories/           <- copy chọn lọc
|   |-- Configurations/         <- copy chọn lọc
|   |-- DependencyInjection.cs  <- chỉ sửa tối thiểu để chạy baseline
|
|-- HRM.Web/
    |-- Controllers/            <- Auth/Login/Dashboard/Role/User nếu cần baseline
    |-- Views/
    |   |-- Shared/              <- copy layout gốc
    |   |-- Login/               <- copy login gốc
    |   |-- Dashboard/           <- copy dashboard shell gốc
    |-- wwwroot/                <- copy assets cần cho UI
    |-- tailwind.config.js      <- copy nếu UI cần
    |-- package.json            <- copy nếu cần build asset
```

Điểm quan trọng:

- Có thể giữ tên `User` trong baseline nếu đổi sang `Employee` quá sớm làm vỡ flow.
- Việc đổi tên/mapping sang HRM là bước refactor sau.
- Không copy các module nhà hàng/loyalty nếu không phục vụ baseline.

---

## 7. Phần Nên Copy Chọn Lọc

| Phần từ repo gốc | Hành động |
|---|---|
| `Domain/Abstractions/` | Copy nguyên nếu không kéo dependency thừa |
| `Domain/Shared/` | Copy phần cần thiết như Email, PhoneNumber; tránh kéo value object không dùng |
| `Domain/Helpers/` | Copy nếu có code nền móng cần dùng |
| `Domain/Users/` | Copy pattern nếu cần baseline login/role |
| `Domain/Roles/` | Copy |
| `Domain/Permissions/` | Copy |
| `Domain/UserToRoles/` | Copy trước nếu cần baseline, đổi sang EmployeeToRoles sau |
| `Domain/RoleToPermissions/` | Copy |
| `Application/Abstractions/` | Copy chọn lọc |
| `Application/Behaviors/` | Copy nếu pipeline MediatR cần |
| `Application/Exceptions/` | Copy nếu handler/controller phụ thuộc |
| `Application/Users/Login/` | Copy nếu giữ login flow gốc |
| `Infrastructure/Authentication/` | Copy pattern gốc trước, refactor sau |
| `Infrastructure/Data/` | Copy pattern hoặc file cần thiết |
| `Infrastructure/Repositories/` | Copy repo liên quan User/Role/Permission nếu cần |
| `Web.Backend/Controllers/LoginController.cs` | Copy nếu giữ route login gốc |
| `Web.Backend/Controllers/AuthController.cs` | Copy phần cần cho user info/logout |
| `Web.Backend/Controllers/DashboardController.cs` | Copy shell nếu cần dashboard giống gốc |
| `Web.Backend/Views/Shared/` | Copy layout/partial cần cho UI |
| `Web.Backend/Views/Login/` | Copy login UI |
| `Web.Backend/Views/Dashboard/` | Copy dashboard shell |
| `Web.Backend/wwwroot/` | Copy assets cần cho layout/login/dashboard |
| `tailwind.config.js`, `package.json` | Copy nếu cần asset/style giống gốc |

---

## 8. Phần Không Nên Copy Trong Baseline

Không copy tràn lan các module sau nếu chưa chứng minh cần cho foundation:

- Booking.
- Orders.
- Vouchers.
- Promotions.
- Products.
- Restaurants.
- Menu Items.
- Membership classes.
- Member point histories.
- Loyalty-specific workflows.
- Payment/VnPay.
- Firebase.
- eSMS.
- AWS S3 nếu chưa có upload trong HRM MVP.
- Migrations cũ chứa schema nhà hàng/loyalty.

Nếu file foundation phụ thuộc vào một module thừa, Anti phải báo dependency đó và đề xuất cách xử lý tối thiểu.

---

## 9. Database Và Migration

### 9.1 Mini Baseline

- Dùng database riêng `hrm_db` để không đụng database gốc.
- Nếu cần schema tối thiểu để login/role chạy, tạo migration riêng trong HRM.
- Không copy migration cũ vào HRM final nếu migration đó chứa nhiều bảng nhà hàng/loyalty.

### 9.2 Sau Baseline

Khi bắt đầu refactor sang HRM:

- Thiết kế schema sạch cho HRM.
- Chỉ giữ bảng cần thiết.
- Tạo migration mới.
- Seed admin/role/permission rõ ràng.
- Không dùng mock auth nguy hiểm.

---

## 10. External Services

| Service | Quyết định |
|---|---|
| Keycloak | Giữ pattern gốc nếu baseline cần; thay local JWT sau nếu cần offline dev |
| AWS S3 | Không bắt buộc trong giai đoạn đầu; nếu gây crash phải debug và tách khỏi flow không cần upload |
| Firebase | Không copy nếu chưa cần |
| VnPay | Không copy nếu chưa cần |
| eSMS | Không copy nếu chưa cần |
| SendGrid/Email | Để sau, khi có notification leave approval |

Nguyên tắc:

- Không hardcode secret.
- Không commit config thật.
- Nếu service external làm app crash, phải có root cause trước khi bypass.

---

## 11. Lộ Trình Phase

### Phase 1: Mini Baseline Foundation

**Mục tiêu:**  
Tạo mini baseline trong `HRM_Leave_Management`, copy chọn lọc phần nền móng từ project gốc, chạy được login/dashboard/UI shell gần gốc.

**Scope:**

1. Kiểm tra lại SDK .NET 8.
2. Đánh giá trạng thái hiện tại của `HRM_Leave_Management` so với plan v5.
3. Xác định phần nào đã refactor quá sớm.
4. Copy bổ sung phần nền móng còn thiếu từ project gốc.
5. Ưu tiên copy UI shell/login/dashboard/assets để localhost không còn là template trắng.
6. Giữ route/auth/cookie gần flow gốc nhất có thể, chỉ sửa tối thiểu để chạy.
7. Không đổi `User` sang `Employee` trên diện rộng nếu baseline chưa ổn.
8. Không làm CRUD Employee/Department.
9. Không làm Leave Management nghiệp vụ.
10. Verify build/run/login/browser.

**Không làm trong Phase 1:**

- Employee CRUD.
- Department CRUD.
- LeaveType/LeaveRequest/LeaveBalance.
- Approval flow.
- Reports.
- Refactor namespace/entity trên diện rộng.
- Copy module nhà hàng/loyalty không phục vụ foundation.

**Tiêu chí hoàn thành:**

- `dotnet build` pass.
- `dotnet run` khởi động được.
- Mở localhost thấy login UI gần project gốc.
- Đăng nhập được bằng admin seed hoặc tài khoản test đã xác định.
- Redirect về dashboard.
- Dashboard/layout/sidebar có style gần project gốc, không phải MVC template mặc định.
- Không sửa repo gốc.

---

### Phase 1.5: Refactor Từng Lớp Sau Baseline

Chỉ bắt đầu sau khi Phase 1 chạy ổn.

**Scope:**

1. Refactor UI menu sang HRM nhưng giữ layout/style gốc.
2. Refactor auth nếu cần: Keycloak -> local JWT.
3. Refactor `User` -> `Employee` nếu đã chốt.
4. Dọn dependency/module thừa.
5. Tạo schema/migration HRM sạch.
6. Verify sau từng lớp refactor.

**Tiêu chí hoàn thành:**

- Mỗi lớp refactor đều build/run được.
- Login không hỏng.
- UI không mất style.
- Không còn dependency thừa gây crash.

---

### Phase 2: Employee Và Department CRUD

**Scope:**

1. Thiết kế `Department`.
2. Mở rộng `Employee`: Department, Position, JoinDate, Manager.
3. Employee CRUD.
4. Department CRUD.
5. Gán role cho employee.
6. Sidebar HRM hoàn chỉnh hơn.

---

### Phase 3: Leave Management Core

**Scope:**

1. LeaveType CRUD.
2. LeaveBalance.
3. LeaveRequest: tạo đơn, xem đơn, hủy đơn.
4. Validate ngày quá khứ.
5. Validate trùng đơn.
6. Tính số ngày nghỉ, hỗ trợ nửa ngày.
7. Kiểm tra số dư phép.

---

### Phase 4: Approval Flow

**Scope:**

1. LeaveApproval audit trail.
2. Approve/Reject.
3. Cập nhật LeaveBalance khi approve.
4. Danh sách đơn chờ duyệt.
5. Permission-based approval.

---

### Phase 5: Dashboard Và Reports

**Scope:**

1. Dashboard widgets.
2. Leave calendar.
3. Báo cáo sử dụng phép.
4. Thống kê theo employee/department/month.

---

## 12. Checklist Trước Khi Tiếp Tục

- [x] Có .NET SDK 8.0.128.
- [x] Dự án mới nằm trong `HRM_Leave_Management`.
- [x] Database dự kiến: `hrm_db`.
- [x] Admin seed dự kiến: `admin / Admin@123456`.
- [x] Đã chỉnh lại chiến lược plan sang mini baseline trước, refactor sau.
- [ ] Anti đọc lại plan v5.
- [ ] Anti so sánh `HRM_Leave_Management` hiện tại với plan v5.
- [ ] Anti báo phần nào đang lệch plan.
- [ ] Anti đề xuất cách đưa về mini baseline mà không xóa code nếu chưa được xác nhận.
- [ ] User xác nhận hành động tiếp theo.

---

## 13. Rủi Ro Đã Nhận Diện

| Rủi ro | Mức | Giảm thiểu |
|---|---|---|
| Copy quá nhiều code cũ | Trung bình | Chỉ copy phần nền móng đã chốt |
| Refactor quá sớm làm mất baseline | Cao | Chạy mini baseline trước, refactor sau |
| UI bị trắng/default | Cao | Copy đủ layout/login/dashboard/assets từ project gốc |
| Auth phụ thuộc Keycloak | Trung bình | Debug root cause, chỉ thay local JWT nếu cần |
| GitNexus chưa index HRM | Trung bình | Re-index hoặc báo rõ manual analysis tạm thời |
| Migration cũ chứa nhiều bảng thừa | Cao | Không copy migration cũ vào HRM final |
| External service gây crash | Trung bình | Tách khỏi flow chưa dùng hoặc cấu hình development fallback |

---

## 14. Prompt Cho Anti Trước Khi Làm Tiếp

```text
Anti, hãy đọc lại MD_memory/hrm_leave_management_tach_du_an_plan.md bản v5.

Chiến lược đã đổi:
- Không raw copy toàn bộ repo.
- Không scaffold mới rồi tự viết HRM quá sớm.
- Copy chọn lọc các phần nền móng của Customer Management & Loyalty System để tạo mini baseline chạy được trước.
- Sau khi baseline build/run/login/UI ổn, mới refactor từng lớp sang HRM.

Yêu cầu:
1. So sánh HRM_Leave_Management hiện tại với plan v5.
2. Chỉ ra phần nào đang đúng plan.
3. Chỉ ra phần nào đã refactor quá sớm.
4. Chỉ ra phần nào còn thiếu copy từ project gốc.
5. Đề xuất cách đưa về mini baseline mà không xóa code nếu chưa có xác nhận.
6. Nếu nhận định của tôi hoặc Codex sai, hãy phản biện bằng file/log cụ thể.
7. Nếu yêu cầu chưa rõ, hỏi lại trước khi sửa.

Chưa sửa code cho tới khi báo lại plan hành động.
```

