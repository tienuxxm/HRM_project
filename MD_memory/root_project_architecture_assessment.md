# Báo cáo đánh giá kiến trúc root project LUC

> Ngày lập: 2026-06-24  
> Phạm vi: root project `Customer_Management_System-Cao_Thanh_Huy_01212407665` và bản copy baseline `HRM_Leave_Management`  
> Mục tiêu: tổng hợp ưu điểm, nhược điểm, cơ chế vận hành và các nguyên tắc phải giữ khi refactor sang HRM Leave Management.

---

## 1. Kết luận ngắn

Root project có nền kiến trúc khá tốt để học và tái sử dụng: solution tách thành `Domain`, `Application`, `Infrastructure`, `Web.Backend`; nghiệp vụ đi qua MediatR; persistence đi qua repository/DbContext; auth tách qua Keycloak/JWT; phân quyền dựa trên `User -> Role -> Permission`; domain event được gom vào Outbox.

Tuy vậy, đây chưa phải một Clean Architecture "sạch tuyệt đối". Dự án có nhiều coupling thực tế: `Infrastructure/DependencyInjection.cs` đăng ký quá nhiều external service và repository; controller còn chứa nhiều logic điều phối; permission check lặp trong từng action; một số handler gọi external service như AWS S3 ngay trong bước map response; cấu hình local thiếu external dependency dễ gây lỗi 500.

Khuyến nghị refactor HRM: giữ các pattern nền móng tốt, nhưng xây module HRM mới song song trên baseline, rồi dọn module cũ theo từng đợt nhỏ sau khi đã có thay thế. Không xóa hàng loạt ngay.

---

## 2. Kiến trúc tổng thể

Solution có 4 project chính:

| Project | Vai trò | Bằng chứng |
|---|---|---|
| `Domain` | Entity, value object, domain event, repository interface, `Result/Error`, abstraction nền | `Domain/Abstractions/Entity.cs`, `Domain/Abstractions/Result.cs` |
| `Application` | Use case qua command/query handler, MediatR pipeline, validation/logging | `Application/DependencyInjection.cs`, `Application/Users/Login/AdminLoginCommandHandler.cs` |
| `Infrastructure` | EF Core, repository implementation, Keycloak, AWS S3, Firebase, VnPay, Quartz, Outbox | `Infrastructure/DependencyInjection.cs`, `Infrastructure/ApplicationDbContext.cs` |
| `Web.Backend` | ASP.NET Core MVC/Razor, controller, view, static assets, middleware cookie-to-bearer | `Web.Backend/Program.cs`, `Web.Backend/Controllers/*` |

Mô hình này gần Clean Architecture vì dependency đi theo hướng:

`Web.Backend -> Application -> Domain`  
`Infrastructure -> Application/Domain`

`Domain` không phụ thuộc Web/Infrastructure. Đây là điểm rất đáng giữ.

---

## 3. Luồng dữ liệu hệ thống

### 3.1 Luồng request MVC thông thường

1. Người dùng mở route trong `Web.Backend/Controllers`.
2. Controller kiểm tra `[Authorize]` và quyền bằng `IRoleService.checkRoleExist(...)`.
3. Controller tạo command/query và gửi qua `ISender.Send(...)` của MediatR.
4. Handler trong `Application` gọi repository hoặc Dapper/SQL factory.
5. Repository dùng `ApplicationDbContext`.
6. Handler trả `Result<T>` hoặc lỗi domain.
7. Controller render Razor view, JSON hoặc redirect.

Ví dụ: `BookingController.Index` kiểm tra `VIEW_BOOKING`, gửi `GetAllBookingPagedCommand`, rồi render view.

### 3.2 Luồng login

1. `LoginController.Login` nhận `LoginViewModel`.
2. Gửi `AdminLoginCommand`.
3. `AdminLoginCommandHandler` tìm user nội bộ theo `Username`.
4. Handler lấy email của user rồi gọi `IJwtService.GetAccessTokenAsync(email, password)`.
5. `JwtService` gọi Keycloak token endpoint bằng password grant.
6. Nếu thành công, controller set cookie `X-Access-Token`.
7. Middleware trong `Program.cs` đọc cookie và thêm header `Authorization: Bearer ...`.
8. JwtBearer middleware validate token bằng metadata/issuer/audience Keycloak.

Ý nghĩa: Keycloak xác thực "bạn là ai"; database app quyết định "bạn có quyền gì".

### 3.3 Luồng phân quyền

Quyền nằm trong DB app:

`user -> user_to_role -> role -> role_to_permission -> permission`

`RoleService.checkRoleExist(identityId, permission)` query user theo `IdentityId`, include role/permission, rồi so `ResourceName` với permission cần kiểm tra.

Ưu điểm: linh hoạt, permission tách khỏi Keycloak, dễ quản lý quyền trong app.

Nhược điểm: check quyền đang lặp ở controller, permission name là string literal, dễ sai chính tả và khó audit.

### 3.4 Luồng domain event và Outbox

`Entity<TEntityId>` có danh sách domain events. `ApplicationDbContext.SaveChanges/SaveChangesAsync` gọi `AddDomainEventsAsOutboxMessages()` trước khi lưu. Domain events được serialize vào `OutboxMessage`. Quartz job xử lý outbox sau.

Ưu điểm: không gọi side effect trực tiếp ngay trong transaction chính, có nền để xử lý bất đồng bộ.

Nhược điểm: nếu background jobs chứa logic cũ như loyalty/notification/Firebase thì local dễ lỗi và HRM phải rà soát trước khi giữ.

---

## 4. Điểm mạnh kiến trúc

### 4.1 Tách lớp tương đối rõ

Domain không phụ thuộc Web. Application chứa use case. Infrastructure chứa implementation. Đây là nền tốt để refactor từng lớp mà không phá toàn bộ.

### 4.2 CQRS nhẹ bằng MediatR

Command/query handler giúp module hóa use case. Pipeline behaviors cho logging và validation được đăng ký trong `Application/DependencyInjection.cs`.

Điểm đáng học:
- Mỗi use case có command/query riêng.
- Controller không trực tiếp thao tác DbContext.
- Validation có thể đặt ở Application.

### 4.3 Domain model có abstraction và value object

`Entity<TEntityId>`, `Result<T>`, `Error`, domain event là nền tốt. Nhiều entity dùng strongly typed id/value object, giúp code có ngữ nghĩa hơn primitive obsession.

### 4.4 Persistence tập trung

`ApplicationDbContext` dùng `ApplyConfigurationsFromAssembly`, repository base cung cấp các hàm chung như `GetEntitiesAsQueryable`, `GetByIdAsync`, `Add`, `Update`, `Remove`, paging.

### 4.5 Auth/permission có thể tái sử dụng cho HRM

Keycloak + JWT cookie + User/Role/Permission là nền phù hợp cho hệ quản trị HRM. Không nên đập bỏ sớm.

### 4.6 Baseline chạy độc lập được

Bản copy `HRM_Leave_Management` đã chạy với DB riêng, Keycloak local, admin local và seed quyền. Đây là mốc tốt để refactor có kiểm soát.

---

## 5. Điểm yếu và rủi ro

### 5.1 Infrastructure DI quá lớn

`Infrastructure/DependencyInjection.cs` đăng ký hàng chục repository và external services. Khi xóa module cũ, rất dễ quên xóa đăng ký DI tương ứng.

Quy tắc refactor: mỗi lần xóa module phải rà:
- interface repository trong Domain,
- implementation repository trong Infrastructure,
- EF configuration,
- DbContext scan,
- DI registration,
- controller/view/handler liên quan.

### 5.2 External service gây lỗi runtime

AWS S3, Firebase, VnPay, eSMS, SendGrid, Keycloak đều là dependency ngoài. Baseline đã gặp lỗi 500 ở các route gọi `_awsS3Service.GetUrlPresign(...)` khi AWS config trống.

Quy tắc refactor: không kết luận module lỗi nghiệp vụ nếu chưa tách lỗi external config khỏi lỗi business logic.

### 5.3 Permission check lặp và dùng string literal

Ví dụ `"VIEW_BOOKING"`, `"UPDATE_USER"`, `"VIEW_DASHBOARD"` được viết trực tiếp trong controller. Điều này chạy được nhưng dễ lệch khi rename module.

Quy tắc refactor: khi thêm HRM permission, tạo danh sách permission tập trung hoặc ít nhất tài liệu hóa rõ. Không rải string tùy tiện.

### 5.4 Controller còn nhiều orchestration logic

Một số controller như `BookingController` không chỉ gọi use case mà còn parse form, sort column, map view model, gọi nhiều query phụ, điều phối command. Điều này làm controller dày.

Quy tắc refactor: HRM controller nên mỏng hơn, logic nghiệp vụ để trong Application handler.

### 5.5 Query mapping có thể gọi external service

Một số handler map response đồng thời gọi AWS S3 để ký URL ảnh. Đây là coupling giữa query và external storage.

Quy tắc refactor: với HRM, mapping response không nên crash chỉ vì storage ngoài chưa cấu hình. Nếu cần file/avatar, bọc service bằng fallback hoặc đặt ở layer rõ ràng.

### 5.6 Outbox/background jobs có thể mang nghiệp vụ cũ

Quartz jobs và event handlers của order/loyalty có thể tiếp tục chạy dù module UI đã bị ẩn. Nếu không kiểm tra, refactor HRM có thể bị lỗi nền.

### 5.7 Thiếu boundary rõ cho bounded context

Các module nhà hàng, loyalty, member, order, promotion sống chung trong cùng solution và DI. Refactor cần cẩn thận vì dependency chéo lớn.

---

## 6. Có phải Clean Architecture không?

Câu trả lời chính xác: **có hướng Clean Architecture, nhưng chưa clean tuyệt đối**.

Đúng tinh thần Clean Architecture:
- Có tách `Domain`, `Application`, `Infrastructure`, `Web`.
- Domain có abstraction, entity, domain event.
- Application không phụ thuộc Web.
- Infrastructure triển khai persistence/external service.
- Web gọi Application qua MediatR.

Chưa sạch tuyệt đối:
- Controller còn chứa nhiều orchestration logic.
- Infrastructure DI quá tập trung.
- External service được gọi trong query mapping.
- Permission string rải ở controller.
- Một số background job/external dependency khởi động cùng app và gây lỗi local.

Vì vậy khi refactor HRM, nên giữ khung Clean Architecture nhưng siết lại boundary.

---

## 7. Nguyên tắc refactor HRM phải tuân thủ

1. **Giữ baseline chạy được.** Không xóa module gốc hàng loạt trước khi module HRM thay thế đã chạy.
2. **Xây HRM song song trước.** Bắt đầu với `Employee` và `Department`, tham chiếu pattern từ `Members` và `Categories`.
3. **Không rename `User` thành `Employee`.** Tạo `Employee` riêng FK tới `User` để tách auth khỏi nghiệp vụ nhân sự.
4. **Không phá auth/permission.** Giữ Keycloak, `User`, `Role`, `Permission`, `UserToRole`, `RoleToPermission` cho tới khi có thiết kế thay thế được xác minh.
5. **Mọi C# symbol edit phải có GitNexus impact trước.** Nếu risk HIGH/CRITICAL thì báo user trước khi sửa.
6. **Mỗi bước nhỏ phải verify.** Build, run, login, dashboard, route mới.
7. **External dependency phải được phân loại.** S3/Firebase/VnPay/eSMS không phải lỗi HRM nếu module đó sẽ bỏ.
8. **Không commit secret/debug script như production.** `appsettings.json` local, Keycloak client secret, DB password trong `MD_memory/debug` chỉ dùng local.
9. **Không tính `bin/obj` là module nghiệp vụ.** Khi thống kê/refactor chỉ xét source.
10. **Tài liệu phải có dấu và UTF-8 đọc được.** Không để mojibake trong `MD_memory`.

---

## 8. Gợi ý thứ tự refactor tiếp theo

### Phase R1: Thiết kế HRM nền

- Tạo tài liệu schema `Employee`, `Department`.
- Xác định quan hệ `Employee.UserId -> User.Id`.
- Xác định permission HRM: `VIEW_EMPLOYEE`, `UPDATE_EMPLOYEE`, `VIEW_DEPARTMENT`, `UPDATE_DEPARTMENT`.

### Phase R2: Xây module HRM đầu tiên

- Domain: `Employees`, `Departments`.
- Application: CRUD command/query.
- Infrastructure: repository + EF configuration.
- Web: controller + views dùng layout hiện tại.
- Seed permission admin.

### Phase R3: Điều chỉnh UI shell

- Đổi sidebar sang HRM.
- Giữ route cũ không ưu tiên hoặc ẩn, chưa xóa source.
- Dashboard đổi từ booking/order sang HRM cards khi có dữ liệu.

### Phase R4: Leave core

- `LeaveType`, `LeaveBalance`, `LeaveRequest`.
- Kiểm tra overlap ngày nghỉ, half-day, số ngày còn lại.

### Phase R5: Approval flow

- `LeaveApproval`, audit trail, notification nếu cần.

### Phase Cleanup

- Xóa module nhà hàng/loyalty theo đợt nhỏ.
- Mỗi đợt: GitNexus impact -> sửa -> build -> run -> login/dashboard -> ghi report.

---

## 9. Skill tạo kèm

Skill project-local đã được tạo tại:

`MD_memory/skills/luc-hrm-refactor-guard/SKILL.md`

Skill này dùng để nhắc Anti/Codex luôn refactor theo guardrail của root project: giữ baseline, dùng GitNexus impact, xây HRM song song, không xóa module cũ quá sớm, verify trước khi báo hoàn thành.
