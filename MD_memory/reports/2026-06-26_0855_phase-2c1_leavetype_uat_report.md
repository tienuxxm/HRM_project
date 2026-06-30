# Báo Cáo UAT - Phase 2C.1: Quản Lý Loại Nghỉ Phép (Leave Type)

- **Thời gian thực hiện**: 2026-06-26 08:55
- **Người thực hiện**: Antigravity (Senior .NET Fullstack Engineer)
- **Trạng thái**: **PASSED** ✅
- **Định dạng Encoding**: UTF-8 có BOM (Unicode Signature)

---

## 🔑 1. Thông Tin Cấu Hình Xác Thực & Phân Quyền (Auth Setup)

- **Chế độ Auth**: Sử dụng Keycloak thật (`UseMockAuth: false` trong `appsettings.json`)
- **Keycloak Local Endpoint**: `http://localhost:8080` (Docker container `keycloak-hrm` hoạt động ổn định)
- **Realm**: `hrm` | **Client**: `hrm-web`
- **Tài khoản UAT**: 
  - Username: `admin`
  - Password: `Admin@123456`
- **Xác thực Role ADMIN**:
  - Bằng chứng thực tế truy vấn từ bảng `role` của database `hrm_baseline_db`:
    ```
    id                                   | resource_name | display_name
    -------------------------------------+---------------+---------------
    11111111-1111-1111-1111-111111111111 | ADMIN         | Administrator
    ```
    *(Xác nhận role ID `11111111-1111-1111-1111-111111111111` chính xác là role ADMIN).*
- **Ghi chú về cải tiến seeding**: 
  - Script seed tạm thời đang dùng cơ chế hardcode ID của ADMIN role. 
  - Phương án tối ưu và an toàn hơn sau này (cho môi trường Production/CI) là sử dụng câu lệnh truy vấn động (lookup) theo mã role (ví dụ: `SELECT id FROM role WHERE resource_name = 'ADMIN'`) thay vì gán cứng ID.
- **Permissions Seeded**:
  - `VIEW_LEAVE_TYPE` (ID: `cf0b0ef2-ef1e-4501-8b9a-4c28470aefb1`) - Gán cho role Admin
  - `UPDATE_LEAVE_TYPE` (ID: `cf0b0ef2-ef1e-4501-8b9a-4c28470aefb2`) - Gán cho role Admin

---

## 🛠️ 2. Kết Quả Database Migration & Seeding

1. **Database Migration**:
   - Chạy lệnh `dotnet ef database update --project Infrastructure --startup-project Web.Backend` thành công.
   - Tạo bảng `leave_type` và index tương ứng trên database PostgreSQL cục bộ (`hrm_baseline_db`).
2. **Seed SQL script**:
   - Đã chèn thành công các quyền `VIEW_LEAVE_TYPE`, `UPDATE_LEAVE_TYPE` và liên kết chúng với Admin Role trong bảng `role_to_permission`.
   - **⚠️ Lưu ý an toàn**: Script seed tạm thời tại `MD_memory/debug/2026-06-26_0850_seed-permissions.ps1` chứa thông tin mật khẩu PostgreSQL ở dạng plain-text (`12345@abc`). File này chỉ phục vụ mục đích **debug cục bộ (local testing)**, tuyệt đối không được đưa lên môi trường Production hoặc CI/CD pipeline. Script sẽ được giữ lại trong thư mục debug cho đến khi có cơ chế seed chính thức hoặc có xác nhận từ người dùng.

---

## 🔍 3. Báo Cáo Phân Tích Tác Động GitNexus (Impact Analysis)

Sau khi chạy lại `gitnexus analyze` để cập nhật index mới nhất, kết quả phân tích blast radius của các thành phần LeaveType như sau:

*   **`LeaveTypeController`** (Controller UI chính):
    *   *Risk Level*: **LOW**
    *   *Impacted Count*: `0`
    *   *Mô tả*: GitNexus chưa phát hiện dependency sang legacy flow, risk LOW theo static analysis. Không có component hoặc class legacy nào khác gọi trực tiếp đến lớp này (do là Endpoint nhận request từ browser).
*   **`DeleteLeaveTypeCommandHandler`** (Xử lý Command xóa):
    *   *Risk Level*: **LOW**
    *   *Impacted Count*: `1` (Direct caller: `LeaveTypeController.cs` thông qua MediatR)
    *   *Mô tả*: GitNexus chưa phát hiện dependency sang legacy flow, risk LOW theo static analysis. Không ảnh hưởng đến bất kỳ luồng xử lý hoặc module nghiệp vụ khác trong hệ thống.
*   **`GetAllLeaveTypesQueryHandler`** (Truy vấn danh sách):
    *   *Risk Level*: **LOW**
    *   *Impacted Count*: `1` (Direct caller: `LeaveTypeController.cs` thông qua MediatR)
    *   *Mô tả*: GitNexus chưa phát hiện dependency sang legacy flow, risk LOW theo static analysis.

---

## 📂 4. Phân Tích Thay Đổi Git (Detect Changes)

*   **Trạng thái kiểm tra**: Đã thực thi lệnh `detect_changes` cho repository `Customer_Management_System-Cao_Thanh_Huy_01212407665`.
*   **Giá trị tham khảo**: Do repository hiện tại đang ở nhánh `master` và ghi nhận `No commits yet` (chưa có commit HEAD ban đầu), tất cả các file đều ở dạng *Untracked files*. Vì thế kết quả của lệnh `detect_changes` chỉ mang tính chất tham khảo tĩnh, không thể so sánh sự thay đổi của workspace với lịch sử commit.
*   **Danh sách file Phase 2C.1 đã thực sự tạo mới hoặc sửa đổi thủ công**:
  - **Tạo mới**:
    - `HRM_Leave_Management/Domain/LeaveTypes/LeaveTypeId.cs`
    - `HRM_Leave_Management/Domain/LeaveTypes/LeaveType.cs`
    - `HRM_Leave_Management/Domain/LeaveTypes/ILeaveTypeRepository.cs`
    - `HRM_Leave_Management/Domain/LeaveTypes/LeaveTypeErrors.cs`
    - `HRM_Leave_Management/Application/LeaveTypes/Create/CreateLeaveTypeCommand.cs`
    - `HRM_Leave_Management/Application/LeaveTypes/Create/CreateLeaveTypeCommandHandler.cs`
    - `HRM_Leave_Management/Application/LeaveTypes/Delete/DeleteLeaveTypeCommand.cs`
    - `HRM_Leave_Management/Application/LeaveTypes/Delete/DeleteLeaveTypeCommandHandler.cs`
    - `HRM_Leave_Management/Application/LeaveTypes/Update/UpdateLeaveTypeCommand.cs`
    - `HRM_Leave_Management/Application/LeaveTypes/Update/UpdateLeaveTypeCommandHandler.cs`
    - `HRM_Leave_Management/Application/LeaveTypes/GetAll/GetAllLeaveTypesQuery.cs`
    - `HRM_Leave_Management/Application/LeaveTypes/GetAll/GetAllLeaveTypesQueryHandler.cs`
    - `HRM_Leave_Management/Infrastructure/Configurations/LeaveTypeConfiguration.cs`
    - `HRM_Leave_Management/Infrastructure/Repositories/LeaveTypeRepository.cs`
    - `HRM_Leave_Management/Infrastructure/Migrations/20260625100832_AddLeaveType.cs`
    - `HRM_Leave_Management/Infrastructure/Migrations/20260625100832_AddLeaveType.Designer.cs`
    - `HRM_Leave_Management/Web.Backend/Controllers/LeaveTypeController.cs`
    - `HRM_Leave_Management/Web.Backend/Views/LeaveType/Index.cshtml`
    - `HRM_Leave_Management/Web.Backend/Views/LeaveType/_CreateLeaveTypePartial.cshtml`
    - `HRM_Leave_Management/Web.Backend/Views/LeaveType/_UpdateLeaveTypePartial.cshtml`
  - **Sửa đổi**:
    - `HRM_Leave_Management/Infrastructure/DependencyInjection.cs` (Đăng ký `LeaveTypeRepository` vào DI container)
    - `HRM_Leave_Management/Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs` (Cập nhật snapshot cơ sở dữ liệu)
    - `HRM_Leave_Management/Web.Backend/Views/Shared/_Layout.cshtml` (Thêm mục "Leave Types" vào sidebar quản trị)

---

## ⚠️ 5. Báo Cáo Cảnh Báo Build (Build Warnings)

Thực hiện build toàn bộ project (`dotnet build`), kết quả ghi nhận cảnh báo phát sinh liên quan đến module `LeaveType`:

1.  **Cảnh báo `CS8618` tại `Domain\LeaveTypes\LeaveType.cs` (Đã xử lý) ✅**:
    *   *Nội dung*: Cảnh báo các thuộc tính non-nullable `Name` và `Code` chưa được gán giá trị mặc định khi thoát khỏi constructor rỗng (EF Core constructor).
    *   *Hành động xử lý*: Đã xử lý warning bằng cách gán `Name = null!; Code = null!;` trong EF Core private constructor (tại `Domain/LeaveTypes/LeaveType.cs`).
    *   *Xác minh*: Kết quả build lại trong `build_output_2` xác nhận **không còn cảnh báo nào** phát sinh từ tệp `Domain/LeaveTypes/LeaveType.cs`.
2.  **Cảnh báo `MVC1000` tại `Views/LeaveType/Index.cshtml` (Chấp nhận và ghi nhận) ⚠️**:
    *   *Nội dung*: Cảnh báo việc sử dụng `IHtmlHelper.Partial` có thể gây deadlock, khuyên dùng Tag Helper `<partial>`.
    *   *Hành động*: Đây là warning từ file mới nhưng viết theo coding pattern cũ của dự án. Chúng tôi ghi nhận đây là **Technical Debt** và sẽ lên kế hoạch tối ưu hóa các phần giao diện Razor này sang Tag Helper trong các phase cleanup tiếp theo.

---

## 🧪 6. Quy Trình Kiểm Thử UAT (CRUD & Soft Delete)

### Kịch bản kiểm thử:
1. **Login**: Truy cập `/leave-type` -> Keycloak redirect đăng nhập -> Đăng nhập thành công với tài khoản `admin` -> Quay lại `/leave-type`.
2. **Create (Tạo mới)**: Thêm loại phép mới:
   - **Code**: `AL`
   - **Name**: `Annual Leave`
   - **Default Days**: `12`
   - **Description**: `Annual Leave for UAT`
   - *Kết quả*: Thêm thành công, hiển thị trên bảng.
3. **Update (Cập nhật)**: Click **Edit** dòng `Annual Leave` -> Sửa Name thành `Annual Leave (UAT)` -> Lưu thay đổi.
   - *Kết quả*: Cập nhật thành công, bảng hiển thị tên mới.
4. **Delete (Soft Delete)**: Click **Remove** dòng `Annual Leave (UAT)` -> Xác nhận.
   - *Kết quả*: Bản ghi được cập nhật `is_active = false` trong DB. Bảng danh sách lọc `IsActive == true` và không hiển thị bản ghi đã xóa nữa.

### Minh chứng hình ảnh (Screenshots):

#### Bước A: Đăng nhập và tạo mới thành công loại phép `Annual Leave`
![Bản ghi Annual Leave được tạo thành công](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/reports/assets/leavetype_created.png)

#### Bước B: Cập nhật thành công loại phép thành `Annual Leave (UAT)`
![Bản ghi được cập nhật thành công](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/reports/assets/leavetype_updated.png)

#### Video ghi lại toàn bộ phiên UAT:
[Xem Video UAT Session](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/reports/assets/leavetype_uat_session.webp)
