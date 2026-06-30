# Báo cáo UAT - Phase 2C.3: Đơn Xin Nghỉ Phép (Leave Request)

- **Thời gian thực hiện**: 2026-06-26 17:05
- **Người thực hiện**: Antigravity (Senior .NET Fullstack Engineer)
- **Trạng thái**: **PASSED / CHỜ USER APPROVE** ⏳
- **Định dạng Encoding**: UTF-8 có BOM (Unicode Signature)

---

## 🔑 1. Thông Tin Cấu Hướng Xác Thực & Phân Quyền (Auth Setup)

- **Chế độ Auth**: Sử dụng Keycloak thật (`UseMockAuth: false` trong `appsettings.json`)
- **Keycloak Local Endpoint**: `http://localhost:8080` (Docker container `keycloak-hrm` hoạt động ổn định)
- **Realm**: `hrm` | **Client**: `hrm-web`
- **Tài khoản UAT**:
  - **Employee**: `employee` / `Admin@123456`
  - **Admin**: `admin@hrm.local` / `Admin@123456`

- **Phân quyền và Mapping dữ liệu**:
  - **User `employee`**:
    - ID Keycloak: `f598529c-53e0-4bd1-8bfe-509e30d04f6e`
    - Mapping DB: User ID `22222222-2222-2222-2222-222222222222` -> Employee Code `EMP002` (Nguyen Van Employee)
    - Role DB: `EMPLOYEE`
    - Quyền đã seed: `VIEW_LEAVE_REQUEST`, `CREATE_LEAVE_REQUEST`

---

## 🛠️ 2. Kết Quả Database Migration & Seeding

1. **Database Migration**:
   - Chạy lệnh `dotnet ef database update --project Infrastructure --startup-project Web.Backend` thành công.
   - Tạo bảng `leave_request` để lưu thông tin đơn nghỉ với các cột:
     - `id`, `employee_id`, `leave_type_id`, `start_date`, `end_date`, `start_session`, `end_session`, `duration` (decimal, hỗ trợ 0.5 ngày), `reason`, `status` (Pending, Approved, Rejected, Canceled), `created_at`, `updated_at`, `is_active`.
   
   - **Cấu hình bảng LeaveRequest** (Trích từ [LeaveRequestConfiguration.cs](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Infrastructure/Configurations/LeaveRequestConfiguration.cs)):
     - Khóa ngoại liên kết tới `Employee` và `LeaveType`.
     - Lưu trữ enum `LeaveRequestStatus` và `LeaveDayPart` dưới dạng string trong database.

---

## 🔍 3. Báo Cáo Phân Tích Tác Động GitNexus (Impact Analysis)

Sau khi chạy lại `node .gitnexus/run.cjs analyze` để cập nhật index mới nhất, kết quả phân tích blast radius của các thành phần LeaveRequest như sau:

*   **`LeaveRequestController`** (Controller UI chính):
    *   *Risk Level*: **LOW**
    *   *Impacted Count*: `0`
    *   *Mô tả*: GitNexus static impact LOW. Không phát hiện dependency legacy nào bị ảnh hưởng.
*   **`CreateLeaveRequestCommandHandler`** (Xử lý tạo đơn xin nghỉ):
    *   *Risk Level*: **LOW**
    *   *Impacted Count*: `1` (Direct caller: `LeaveRequestController` qua MediatR)
    *   *Mô tả*: Không ảnh hưởng đến các modules hay flows khác.
*   **`CancelLeaveRequestCommandHandler`** (Xử lý hủy đơn xin nghỉ):
    *   *Risk Level*: **LOW**
    *   *Impacted Count*: `1` (Direct caller: `LeaveRequestController` qua MediatR)
    *   *Mô tả*: Không ảnh hưởng đến các modules hay flows khác.

---

## 📂 4. Phân Tích Thay Đổi Git (Detect Changes)

*   **Trạng thái kiểm tra**: Đã thực thi lệnh `detect_changes` cho repository.
*   **Các file được tạo mới và sửa đổi cho Phase 2C.3**:
  - **Tạo mới**:
    - `HRM_Leave_Management/Domain/LeaveRequests/LeaveRequestId.cs`
    - `HRM_Leave_Management/Domain/LeaveRequests/LeaveRequestStatus.cs`
    - `HRM_Leave_Management/Domain/LeaveRequests/LeaveDayPart.cs`
    - `HRM_Leave_Management/Domain/LeaveRequests/LeaveRequest.cs`
    - `HRM_Leave_Management/Domain/LeaveRequests/ILeaveRequestRepository.cs`
    - `HRM_Leave_Management/Domain/LeaveRequests/LeaveRequestErrors.cs`
    - `HRM_Leave_Management/Application/LeaveRequests/Create/CreateLeaveRequestCommand.cs`
    - `HRM_Leave_Management/Application/LeaveRequests/Create/CreateLeaveRequestCommandHandler.cs`
    - `HRM_Leave_Management/Application/LeaveRequests/Cancel/CancelLeaveRequestCommand.cs`
    - `HRM_Leave_Management/Application/LeaveRequests/Cancel/CancelLeaveRequestCommandHandler.cs`
    - `HRM_Leave_Management/Application/LeaveRequests/Get/GetLeaveRequestsQuery.cs`
    - `HRM_Leave_Management/Application/LeaveRequests/Get/GetLeaveRequestsQueryHandler.cs`
    - `HRM_Leave_Management/Application/LeaveRequests/Get/LeaveRequestResponse.cs`
    - `HRM_Leave_Management/Infrastructure/Configurations/LeaveRequestConfiguration.cs`
    - `HRM_Leave_Management/Infrastructure/Repositories/LeaveRequestRepository.cs`
    - `HRM_Leave_Management/Infrastructure/Migrations/20260626085518_AddLeaveRequest.cs`
    - `HRM_Leave_Management/Infrastructure/Migrations/20260626085518_AddLeaveRequest.Designer.cs`
    - `HRM_Leave_Management/Web.Backend/Controllers/LeaveRequestController.cs`
    - `HRM_Leave_Management/Web.Backend/Views/LeaveRequest/Index.cshtml`
    - `HRM_Leave_Management/Web.Backend/Views/LeaveRequest/_CreateLeaveRequestPartial.cshtml`
    - `HRM_Leave_Management/Web.Backend/Views/LeaveRequest/_ConfirmCancelPartial.cshtml`
  - **Sửa đổi**:
    - `HRM_Leave_Management/Infrastructure/DependencyInjection.cs` (Đăng ký Repository)
    - `HRM_Leave_Management/Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs` (Cập nhật snapshot)
    - `HRM_Leave_Management/Web.Backend/Views/Shared/_Layout.cshtml` (Đổi sidebar route sang `leave-request`)

---

## ⚠️ 5. Báo Cáo Cảnh Báo Build (Build Warnings)

Thực hiện build toàn bộ project (`dotnet build`), kết quả xác nhận:
- **0 Error** (Không có lỗi biên dịch).
- Không phát sinh build warning mới liên quan đến module `LeaveRequest`.

---

## 🧪 6. Quy Trình Kiểm Thử UAT (Create & Cancel Leave Request)

### Kịch bản kiểm thử:
1. **Kiểm thử Tạo Đơn Nghỉ Phép (Tài khoản Employee)**:
   - Đăng nhập tài khoản `employee` (mật khẩu `Admin@123456`).
   - Truy cập `/leave-request`.
   - Click nút "Request Leave" ở góc trên bên phải để mở modal form tạo đơn nghỉ.
   - Nhập thông tin:
     - Leave Type: `Annual Leave (AL1)`
     - Start Date: `07/01/2026`
     - End Date: `07/02/2026`
     - Start Session: `Full Day`
     - End Session: `Full Day`
     - Reason: `Nghi phep ca nhan - Test Cancel Flow`
   - Click "Submit".
   - *Kết quả*: Đơn nghỉ được tạo thành công, xuất hiện ở dòng đầu tiên của danh sách với trạng thái **Pending**, thời gian nghỉ là **2 Days**, và có nút chức năng **Cancel** ở cột Actions.

2. **Kiểm thử Hủy Đơn Nghỉ Phép (Tài khoản Employee)**:
   - Click nút **Cancel** ở cột Actions của đơn nghỉ vừa tạo (`Nghi phep ca nhan - Test Cancel Flow`).
   - Xuất hiện modal Flowbite xác nhận: *"Bạn có chắc chắn muốn hủy đơn xin nghỉ phép Annual Leave (2 ngày, từ 01/07/2026 đến 02/07/2026)?"*
   - Click **Đồng ý**.
   - *Kết quả*: Trang web reload/ajax cập nhật lại danh sách, đơn nghỉ chuyển sang trạng thái **Canceled**, đồng thời nút **Cancel** biến mất khỏi cột Actions.

### Minh chứng hình ảnh (Screenshots):

#### Bước A: Điền Form Tạo Đơn Xin Nghỉ Phép
![Điền Form Tạo Đơn Xin Nghỉ](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/reports/screenshots/leave_request_create_form.png)

#### Bước B: Modal Xác Nhận Hủy Đơn Xin Nghỉ Phép (Flowbite Confirmation)
![Modal Xác Nhận Hủy Đơn](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/reports/screenshots/leave_request_confirm_cancel.png)

#### Bước C: Danh Sách Đơn Nghỉ Sau Khi Hủy Thành Công (Trạng thái chuyển sang Canceled)
![Danh Sách Sau Khi Hủy Đơn](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/reports/screenshots/leave_request_list_canceled.png)

---

## 📢 7. Khai Báo Thay Đổi & Sự Cố Keycloak Auth (Auth/User Change Disclosure)

- **Auth Mode**: Keycloak thật
- **UseMockAuth**: `false`
- **Tài khoản UAT**:
  - Employee: `employee` / `Admin@123456`
- **Permission Seeded**: `VIEW_LEAVE_REQUEST`, `CREATE_LEAVE_REQUEST`
- **Ghi nhận sự cố**: Không phát sinh sự cố thay đổi cấu hình hay tài khoản Keycloak trong suốt quá trình UAT Phase 2C.3. Mọi tính năng xác thực và phân quyền hoạt động đúng thiết kế.
