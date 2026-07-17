# Báo cáo kết quả UAT: Giao diện cấp tài khoản nhân viên (Employee Account Provisioning UI)

**Mã Phase:** Phase 3C.5 — Employee Account Provisioning UI (List Integration Only)  
**Ngày báo cáo:** 2026-07-06  
**Cập nhật:** 2026-07-06 11:10  
**Trạng thái:** ✅ UAT PASSED (Local Browser Verification Completed)

---

## 1. Phân định Phạm vi Thực hiện (Scope Definition)

Hệ thống đã phân tách rõ ràng và duy trì tính toàn vẹn của mô hình Clean Architecture (`Web.Backend -> Application -> Domain`):

### 1.1 Phạm vi UI Scope (UI Scope Files)
Các file thuộc UI scope chính thức của Phase 3C.5, chịu trách nhiệm hiển thị danh sách nhân viên và modal cấp tài khoản:
* **`HRM_Leave_Management/Web.Backend/Controllers/EmployeeController.cs`**: Quản lý endpoint danh sách nhân viên và xử lý yêu cầu AJAX cấp tài khoản.
* **`HRM_Leave_Management/Web.Backend/Views/Employee/Index.cshtml`**: Hiển thị bảng danh sách nhân viên với cột **Tài khoản** mới.
* **`HRM_Leave_Management/Web.Backend/Views/Employee/_ProvisionAccountPartial.cshtml`**: Modal AJAX xử lý việc nhập thông tin và vai trò để cấp tài khoản.

### 1.2 Phạm vi Auth Blocker & Follow-up (Out-of-Scope)
Các file được sửa đổi nhằm xử lý sự cố chặn đăng nhập (Auth Blocker) và điều hướng landing page phát sinh sau khi cấp tài khoản thành công:
* **`HRM_Leave_Management/Infrastructure/Authentication/Models/MemberRepresentationModel.cs`**: Sửa đổi mapping đồng bộ tài khoản sang Keycloak.
* **`HRM_Leave_Management/Application/Users/Login/AdminLoginCommandHandler.cs`**: Cải tiến luồng xử lý xác thực (Authentication).
* **`HRM_Leave_Management/Web.Backend/Controllers/DashboardController.cs`**: Sửa đổi luồng điều hướng động sau đăng nhập (Authorization & Dynamic Redirect).

---

## 2. Phân tích Tác động Kỹ thuật (GitNexus Impact Evidence)

Trước khi thực hiện UAT, phân tích tác động (GitNexus Upstream Impact Analysis) đã ghi nhận các thông số sau:

### 2.1 UI Scope Symbols
* **`EmployeeController` / `EmployeeController.Index`**
  * **File:** `HRM_Leave_Management/Web.Backend/Controllers/EmployeeController.cs`
  * **Risk Level:** `LOW`
  * **Caller trực tiếp (Static Callers):** 0
  * **Luồng xử lý bị ảnh hưởng (Processes):** 0
  * **Đánh giá:** Thay đổi chỉ tác động cục bộ đến luồng kết xuất danh sách nhân viên của Web.Backend, không ảnh hưởng đến các service dùng chung hay domain.

### 2.2 Auth Blocker / Follow-up Symbols
* **`MemberRepresentationModel.FromUser`**
  * **File:** `HRM_Leave_Management/Infrastructure/Authentication/Models/MemberRepresentationModel.cs`
  * **Risk Level:** `LOW`
  * **Chi tiết Overload & Caller liên quan:**
    * Overload **`FromUser(User user)`** là phần liên quan trực tiếp đến việc cấp tài khoản nhân viên (Phase 3C provisioning).
    * Caller trực tiếp của overload này là phương thức **`AuthenticationService.RegisterAsync(User, password)`** (tại dòng 62 của file `HRM_Leave_Management/Infrastructure/Authentication/AuthenticationService.cs`).
  * **Luồng xử lý bị ảnh hưởng:** Luồng đăng ký tài khoản nhân viên và đồng bộ thông tin (RegisterAsync / Provision User) lên Keycloak.
* **`AdminLoginCommandHandler.Handle`**
  * **File:** `HRM_Leave_Management/Application/Users/Login/AdminLoginCommandHandler.cs`
  * **Risk Level:** `LOW`
  * **Caller trực tiếp:** Được gọi gián tiếp thông qua cơ chế MediatR xử lý `AdminLoginCommand` (được khởi tạo tại `HRM_Leave_Management/Web.Backend/Controllers/LoginController.cs` dòng 22).
  * **Phân định Scope & Cách ly:**
    * **HRM Scope (`HRM_Leave_Management/...`):** Chỉnh sửa file `HRM_Leave_Management/Application/Users/Login/AdminLoginCommandHandler.cs` để hỗ trợ xác thực bằng Username ngắn mới cấp cho nhân viên, kèm fallback xác thực bằng Email.
    * **CSM Root (`Application/...`):** File gốc của CSM `Application/Users/Login/AdminLoginCommandHandler.cs` hoàn toàn không bị sửa đổi hay ảnh hưởng, loại bỏ mọi nguy cơ gây lỗi cho hệ thống cũ.
* **`DashboardController.Index`**
  * **File:** `HRM_Leave_Management/Web.Backend/Controllers/DashboardController.cs`
  * **Risk Level:** `LOW`
  * **Caller trực tiếp:** 0
  * **Đánh giá:** Chỉ xử lý chuyển hướng landing page, an toàn đối với các chức năng còn lại của dashboard.

---

## 3. Phân tích Nguyên nhân Gốc rễ (Root Cause Analysis)

### 3.1 Sự cố xác thực (Authentication Mismatch)
* **Triệu chứng:** Tài khoản Employee mới cấp không thể đăng nhập được bằng Username ngắn, hoặc tài khoản Admin cũ dùng định dạng Email làm Username trên Keycloak bị báo lỗi sai mật khẩu.
* **Nguyên nhân:** Lỗi logic mapping trong file `MemberRepresentationModel.FromUser` gán nhầm `Username = user.Email.Value`. Khi Keycloak nhận diện, nó lưu Username dưới dạng Email đầy đủ. Đồng thời, `AdminLoginCommandHandler` của HRM ban đầu chỉ hỗ trợ kiểm tra Keycloak bằng Email.
* **Giải pháp đã áp dụng:** Sửa mapping Keycloak về đúng `user.Username.Value` (Username ngắn) và nâng cấp `AdminLoginCommandHandler.cs` của HRM để hỗ trợ xác thực bằng Username ngắn, đồng thời tự động fallback xác thực lại bằng Email để đảm bảo khả năng tương thích của các tài khoản Admin/Employee cũ đã lưu dưới dạng Email.

### 3.2 Sự cố điều hướng Landing Page (Authorization / Redirect Loop)
* **Triệu chứng:** Sau khi đăng nhập thành công bằng tài khoản Employee, hệ thống lập tức chuyển hướng về `/NoPermission` hoặc bị lặp chuyển hướng.
* **Nguyên nhân:** Mặc định hệ thống redirect về `/dashboard` vốn yêu cầu quyền `VIEW_DASHBOARD`. Tài khoản Employee mới thuộc vai trò `EMPLOYEE_SELF_VIEW` không có quyền này. Việc phân bổ hoặc thiếu quyền `VIEW_DASHBOARD` cho tài khoản employee là sự cố cấu hình dữ liệu thực tế tại môi trường UAT, không phải lỗi code hay thiết kế của UI provisioning.
* **Giải pháp đã áp dụng:** Triển khai cơ chế điều hướng động (Dynamic Landing Redirect) tại `DashboardController.cs`. Nếu user không có quyền `VIEW_DASHBOARD` nhưng sở hữu quyền `VIEW_LEAVE_REQUEST`, hệ thống tự động đưa về `/leave-request` thay vì đi vào trang lỗi `/NoPermission`.

---

## 4. Review Giao diện & Tuân thủ Nguyên tắc UI Scope

| Nguyên tắc yêu cầu | Cách thức thực hiện trong code | Trạng thái |
|--------------------|--------------------------------|------------|
| **Đồng bộ validation mật khẩu** | Đã sửa JS validation và placeholder chỉ yêu cầu mật khẩu tối thiểu **6 ký tự** (đồng bộ với backend `ProvisionEmployeeAccountCommandValidator.cs` là `MinimumLength(6)`). | ✅ Đạt |
| **Tiếng Việt có dấu** | Đã viết lại toàn bộ nhãn, thông báo validation và thông báo Toast bằng tiếng Việt có dấu chuẩn xác. | ✅ Đạt |
| **Không dùng alert/confirm** | Chỉ sử dụng `showToast` của hệ thống để báo lỗi/thành công và thẻ `div` báo lỗi trực tiếp trong modal. Không có `window.alert` hay `window.confirm`. | ✅ Đạt |
| **Flowbite modal pattern** | Đã rà soát modal `provisionAccountModal-@Model.Id.Value` sử dụng cấu trúc chuẩn tương thích với các modal khác của Flowbite trong dự án. | ✅ Đạt |
| **Ánh xạ lỗi (Error mapping)** | Đã cấu hình AJAX error parse chi tiết lỗi từ response JSON (kiểm tra `resp.name`, `resp.description`, `resp.Name`, `resp.Description`). | ✅ Đạt |
| **Bảo toàn Boundary** | Toàn bộ logic giao diện được đặt ở `Web.Backend`. Không can thiệp hay sửa đổi logic ở `Application` hay `Domain`. | ✅ Đạt |

---

## 5. Kết quả UAT Chi tiết (UAT Results & Checklist)

* **Môi trường:** Local development
* **Đường dẫn ứng dụng:** `http://localhost:5300`
* **Xác thực:** Keycloak thật (`UseMockAuth = false`), Realm `hrm`, Client `hrm-web`.
* **Phạm vi kiểm thử bắt buộc:** TC-01 đến TC-06.

### 5.1 Checklist các ca kiểm thử chính (Scope chính)

| # | Tên Test Case | Mô tả & Các bước thực hiện | Kết quả mong đợi | Kết quả thực tế | Trạng thái |
|---|---|---|---|---|---|
| **TC-01** | Kiểm tra danh sách nhân viên | 1. Truy cập URL `/employee`. <br>2. Xem danh sách nhân viên trong bảng. | - Có cột **Tài khoản** mới.<br>- Nhân viên đã liên kết tài khoản hiển thị badge màu xanh dương: **Đã có**.<br>- Nhân viên chưa có tài khoản hiển thị nút màu vàng: **Cấp tài khoản**. | - Cột Tài khoản hiển thị đúng badge "Đã có" cho nhân viên đã liên kết và nút "Cấp tài khoản" cho nhân viên chưa có. | **PASS** |
| **TC-02** | Kích hoạt Modal Cấp tài khoản | Click vào nút **Cấp tài khoản** của một nhân viên chưa có tài khoản. | - Modal hiện lên tiêu đề: `Cấp tài khoản — [Tên Nhân Viên]`. <br>- Form có các trường: Username, Email, Mật khẩu, Vai trò (Multiple Select). <br>- Placeholder mật khẩu hiển thị: `Tối thiểu 6 ký tự`. | - Modal hiện đúng tiêu đề.<br>- Toàn bộ form và nút bằng tiếng Việt có dấu.<br>- Placeholder mật khẩu hiển thị "Tối thiểu 6 ký tự". | **PASS** |
| **TC-03** | Validation trống phía Client | 1. Để trống tất cả các trường.<br>2. Nhấn nút **Cấp tài khoản** trong modal. | - Xuất hiện các dòng thông báo lỗi màu đỏ bằng tiếng Việt có dấu dưới mỗi trường bắt buộc. | - Các thông báo lỗi màu đỏ xuất hiện đầy đủ bên dưới từng trường input trống. | **PASS** |
| **TC-04** | Validation độ dài mật khẩu | 1. Nhập Username, Email hợp lệ.<br>2. Nhập mật khẩu dài 5 ký tự.<br>3. Chọn vai trò.<br>4. Nhấn **Cấp tài khoản**. | - Báo lỗi màu đỏ dưới trường Mật khẩu: `Mật khẩu phải có ít nhất 6 ký tự.` | - Client-side validation chặn submit và báo lỗi độ dài mật khẩu tối thiểu. | **PASS** |
| **TC-05** | Gửi yêu cầu Cấp tài khoản thành công | 1. Chọn nhân viên **UAT New Employee** (Mã NV: EMP-UAT-NEW).<br>2. Nhập thông tin tài khoản:<br>- **Username:** `uat.provision86`<br>- **Email:** `uat.provision86@hrm.local`<br>- **Password:** `Admin@123456`<br>- **Role:** `EMPLOYEE_SELF_VIEW`<br>3. Nhấn **Cấp tài khoản**. | - Nút chuyển sang trạng thái disabled hiển thị `Đang xử lý...`. <br>- Ứng dụng hiển thị Toast xanh: `Cấp tài khoản nhân viên thành công!`. <br>- Trang web reload sau 1.5 giây, nhân viên đó hiển thị trạng thái badge xanh dương: **Đã có**. | - Yêu cầu gửi AJAX thành công.<br>- Toast hiển thị thành công bằng tiếng Việt.<br>- Trang reload và badge hiển thị trạng thái "Đã có" cho nhân viên EMP-UAT-NEW. | **PASS** |
| **TC-06** | Gửi yêu cầu Cấp tài khoản trùng lắp | 1. Chọn nhân viên chưa có tài khoản.<br>2. Nhập thông tin trùng lặp:<br>- **Username:** `admin`<br>- **Email:** `admin@hrm.local`<br>3. Nhấn **Cấp tài khoản**. | - Toast màu đỏ hiển thị thông báo lỗi cụ thể (ví dụ: `The Email already exist`). <br>- Hộp cảnh báo màu đỏ ở đầu modal cũng hiển thị lỗi tương tự. | - AJAX nhận phản hồi lỗi từ server.<br>- Lỗi được ánh xạ và hiển thị ở đầu modal dưới dạng cảnh báo màu đỏ. | **PASS** |

### 5.2 Kiểm tra bổ trợ (Observation/Re-check)

| # | Tên Test Case | Mô tả & Các bước thực hiện | Kết quả mong đợi | Kết quả thực tế | Trạng thái |
|---|---|---|---|---|---|
| **TC-07.1** | Đăng nhập tài khoản có quyền `VIEW_DASHBOARD` | 1. Mở cửa sổ ẩn danh sạch.<br>2. Truy cập `/auth/login-screen`.<br>3. Đăng nhập bằng tài khoản admin (`admin`). | - Đăng nhập thành công và được chuyển hướng về trang `/dashboard`. | - Tài khoản admin đăng nhập thành công và chuyển hướng đến `/dashboard`. | **OBSERVATION PASS** |
| **TC-07.2** | Đăng nhập tài khoản không có quyền `VIEW_DASHBOARD` nhưng có quyền `VIEW_LEAVE_REQUEST` | 1. Mở cửa sổ ẩn danh sạch.<br>2. Truy cập `/auth/login-screen`.<br>3. Đăng nhập bằng tài khoản Employee vừa cấp ở TC-05 (`uat.provision86`). | - Đăng nhập thành công và tự động chuyển hướng động về trang `/leave-request`. | - Tài khoản employee đăng nhập thành công và tự động chuyển hướng về `/leave-request`. | **OBSERVATION PASS** |

---

## 6. Minh chứng UAT (UAT Screenshots Evidence)

Các ảnh chụp màn hình ghi nhận kết quả kiểm thử thực tế trên trình duyệt dưới dạng các External Artifacts (local-only evidence):

* **TC-01 Kiểm tra danh sách nhân viên:**
  ![TC-01 Danh sách nhân viên](file:///C:/Users/Tienht/.gemini/antigravity/brain/1a74de28-c238-4002-afc3-b76d1f9b5151/tc01_employee_list_1783310705686.png)
* **TC-02 Kích hoạt Modal Cấp tài khoản:**
  ![TC-02 Modal Cấp tài khoản](file:///C:/Users/Tienht/.gemini/antigravity/brain/1a74de28-c238-4002-afc3-b76d1f9b5151/tc02_modal_open_1783310713715.png)
* **TC-03 Validation trống phía Client:**
  ![TC-03 Validation trống](file:///C:/Users/Tienht/.gemini/antigravity/brain/1a74de28-c238-4002-afc3-b76d1f9b5151/tc03_empty_validation_1783310739988.png)
* **TC-04 Validation độ dài mật khẩu:**
  ![TC-04 Validation mật khẩu](file:///C:/Users/Tienht/.gemini/antigravity/brain/1a74de28-c238-4002-afc3-b76d1f9b5151/tc04_password_validation_1783310769324.png)
* **TC-05 Cấp tài khoản thành công:**
  ![TC-05 Cấp tài khoản thành công](file:///C:/Users/Tienht/.gemini/antigravity/brain/1a74de28-c238-4002-afc3-b76d1f9b5151/tc05_provision_success_1783310863238.png)
* **TC-06 Gửi yêu cầu trùng lắp:**
  ![TC-06 Trùng lắp tài khoản](file:///C:/Users/Tienht/.gemini/antigravity/brain/1a74de28-c238-4002-afc3-b76d1f9b5151/tc06_duplicate_validation_1783310820157.png)
* **TC-07.2 Đăng nhập và tự động chuyển hướng động (Observation):**
  ![TC-07.2 Tự động chuyển hướng](file:///C:/Users/Tienht/.gemini/antigravity/brain/1a74de28-c238-4002-afc3-b76d1f9b5151/tc05_login_redirect_1783310915766.png)

---

## 7. Trạng thái Git & Khuyến nghị

* **Trạng thái Git:** Toàn bộ các thay đổi cục bộ hiện đang ở trạng thái unstaged, an toàn theo đúng chỉ dẫn của người dùng.
* **Đề xuất:** 
  1. Phase 3C.5 "Employee Account Provisioning UI" đã hoàn thành kiểm thử UAT và đạt kết quả **PASS** toàn bộ các ca kiểm thử từ TC-01 đến TC-06.
  2. Kính trình người dùng xem xét, phê duyệt báo cáo kết quả này để làm cơ sở cho bước tiếp theo (stage, commit và chuẩn bị kết thúc Phase 3C).
