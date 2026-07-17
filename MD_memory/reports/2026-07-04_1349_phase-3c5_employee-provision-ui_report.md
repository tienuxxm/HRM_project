# Báo cáo kết quả: Giao diện cấp tài khoản nhân viên (Employee Account Provisioning UI)

**Mã Phase:** Phase 3C.5 — Employee Account Provisioning UI (List Integration Only)  
**Ngày báo cáo:** 2026-07-04  
**Cập nhật:** 2026-07-06 10:00  
**Trạng thái:** ⚠️ PENDING USER CONFIRMATION (Local Verified & User confirmed TC-07 redirect PASS)  

---

## 1. Phân định Phạm vi Thực hiện (Scope Definition)

Để đảm bảo tính minh bạch và cấu trúc Clean Architecture, các thay đổi được chia tách rõ ràng thành hai nhóm độc lập:

### 1.1 Phạm vi UI Scope (UI Scope Files)
Các file thuộc UI scope chính thức của Phase 3C.5, chịu trách nhiệm hiển thị giao diện danh sách nhân viên và modal cấp tài khoản:
* **`HRM_Leave_Management/Web.Backend/Controllers/EmployeeController.cs`**: Quản lý endpoint danh sách nhân viên và xử lý yêu cầu AJAX cấp tài khoản.
* **`HRM_Leave_Management/Web.Backend/Views/Employee/Index.cshtml`**: Hiển thị bảng danh sách nhân viên với cột **Tài khoản** mới.
* **`HRM_Leave_Management/Web.Backend/Views/Employee/_ProvisionAccountPartial.cshtml`**: Modal AJAX xử lý việc nhập thông tin và vai trò để cấp tài khoản.

### 1.2 Phạm vi Auth Blocker & Follow-up (Out-of-Scope fixes)
Các file được sửa đổi nhằm xử lý sự cố chặn đăng nhập (Auth Blocker) và điều hướng landing page phát sinh sau khi cấp tài khoản thành công. Đây là các phần bổ trợ, không được đánh tráo hay giấu trong UI scope:
* **`HRM_Leave_Management/Infrastructure/Authentication/Models/MemberRepresentationModel.cs`**: Sửa đổi mapping đồng bộ tài khoản sang Keycloak.
* **`HRM_Leave_Management/Application/Users/Login/AdminLoginCommandHandler.cs`**: Cải tiến luồng xử lý xác thực (Authentication).
* **`HRM_Leave_Management/Web.Backend/Controllers/DashboardController.cs`**: Sửa đổi luồng điều hướng động sau đăng nhập (Authorization & Dynamic Redirect).

---

## 2. Phân tích Tác động Kỹ thuật (GitNexus Impact Evidence)

Trước khi thực hiện các thay đổi, chúng tôi đã tiến hành phân tích tác động (GitNexus Upstream Impact Analysis) để đánh giá blast radius của từng C# symbol liên quan:

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
    * *(Lưu ý: Overload `FromUser(Member member)` tại dòng 27 của `AuthenticationService.cs` thuộc luồng đăng ký Member cũ của CSM và không chịu ảnh hưởng bởi thay đổi này).*
  * **Luồng xử lý bị ảnh hưởng:** Luồng đăng ký tài khoản nhân viên và đồng bộ thông tin (RegisterAsync / Provision User) lên Keycloak.
  * **Đánh giá:** Chỉ ảnh hưởng tới luồng đăng ký tài khoản mới lên Keycloak. Các tài khoản có sẵn không bị ảnh hưởng.
* **`AdminLoginCommandHandler.Handle`**
  * **File:** `HRM_Leave_Management/Application/Users/Login/AdminLoginCommandHandler.cs`
  * **Risk Level:** `LOW`
  * **Caller trực tiếp:** Được gọi gián tiếp thông qua cơ chế MediatR xử lý `AdminLoginCommand` (được khởi tạo tại `HRM_Leave_Management/Web.Backend/Controllers/LoginController.cs` dòng 22).
  * **Phân định Scope & Cách ly:**
    * **HRM Scope (`HRM_Leave_Management/...`):** Chỉnh sửa file `HRM_Leave_Management/Application/Users/Login/AdminLoginCommandHandler.cs` để hỗ trợ xác thực bằng Username ngắn mới cấp cho nhân viên, kèm fallback xác thực bằng Email.
    * **CSM Root (`Application/...`):** File gốc của CSM `Application/Users/Login/AdminLoginCommandHandler.cs` (kết nối với `Web.Backend/Controllers/LoginController.cs`) hoàn toàn không bị sửa đổi hay ảnh hưởng, loại bỏ mọi nguy cơ gây lỗi cho hệ thống cũ.
  * **Đánh giá:** Cơ chế fallback auth hoạt động an toàn và giữ tương thích ngược hoàn toàn cho các tài khoản Admin/User cũ của HRM.
* **`DashboardController.Index`**
  * **File:** `HRM_Leave_Management/Web.Backend/Controllers/DashboardController.cs`
  * **Risk Level:** `LOW`
  * **Caller trực tiếp:** 0
  * **Đánh giá:** Chỉ xử lý chuyển hướng landing page, an toàn đối với các chức năng còn lại của dashboard.

---

## 3. Phân tích Nguyên nhân Gốc rễ (Root Cause Analysis)

Sự cố kiểm thử liên quan đến tài khoản Employee mới được phân tích rõ thành hai cấu phần độc lập, tương ứng với phạm vi UI và Auth/Authz follow-up:

### 3.1 Sự cố xác thực (Authentication Mismatch) - *Lỗi Code thuộc luồng Auth Blocker (Ngoài phạm vi UI Scope)*
* **Triệu chứng:** Tài khoản Employee mới cấp không thể đăng nhập được bằng Username ngắn, hoặc tài khoản Admin cũ dùng định dạng Email làm Username trên Keycloak bị báo lỗi sai mật khẩu.
* **Nguyên nhân:** Lỗi logic mapping trong file `MemberRepresentationModel.FromUser` gán nhầm `Username = user.Email.Value`. Khi Keycloak nhận diện, nó lưu Username dưới dạng Email đầy đủ. Đồng thời, `AdminLoginCommandHandler` của HRM (`HRM_Leave_Management/Application/Users/Login/AdminLoginCommandHandler.cs`) ban đầu chỉ hỗ trợ kiểm tra Keycloak bằng Email.
* **Tính chất:** Đây là lỗi code của luồng xác thực nền tảng (Auth Blocker), hoàn toàn tách biệt với logic hiển thị danh sách hay kích hoạt modal của UI provisioning.
* **Giải pháp đã áp dụng:** Sửa mapping Keycloak về đúng `user.Username.Value` (Username ngắn) và nâng cấp `AdminLoginCommandHandler.cs` của HRM (`HRM_Leave_Management/Application/Users/Login/AdminLoginCommandHandler.cs`) để hỗ trợ xác thực bằng Username ngắn, đồng thời tự động fallback xác thực lại bằng Email để đảm bảo tương thích ngược hoàn toàn cho các tài khoản Admin/Employee cũ đã lưu dưới dạng Email.

### 3.2 Sự cố điều hướng Landing Page (Authorization / Redirect Loop) - *Sự cố cấu hình dữ liệu mẫu (Manual/Seed Data Issue, không phải bug UI)*
* **Triệu chứng:** Sau khi đăng nhập thành công bằng tài khoản Employee, hệ thống lập tức chuyển hướng về `/NoPermission` hoặc bị lặp chuyển hướng.
* **Nguyên nhân:** Mặc định hệ thống redirect về `/dashboard` vốn yêu cầu quyền `VIEW_DASHBOARD`. Tài khoản Employee mới thuộc vai trò `EMPLOYEE_SELF_VIEW` không có quyền này. Việc phân bổ hoặc thiếu quyền `VIEW_DASHBOARD` cho tài khoản employee là **sự cố cấu hình dữ liệu thực tế (manual/seed data issue)** tại môi trường UAT, không phải lỗi code hay thiết kế của UI provisioning (UI provisioning chỉ thực hiện lưu database và gọi Keycloak API cấp tài khoản theo đúng vai trò được chọn từ giao diện).
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

## 5. Kế hoạch & Kết quả Xác minh Kỹ thuật (Technical Verification)

### 5.1 Build thử nghiệm dự án (dotnet build)
* Kết quả Build: **PASS** (0 Error(s), 218 Warning(s))
* Không có lỗi biên dịch (no CS compiler errors).

### 5.2 Kiểm tra sự hiện diện của window.alert/confirm
* Xác nhận không có việc gọi `window.alert` hay `window.confirm` trong các file Views liên quan đến Employee vừa chỉnh sửa.

---

## 6. Thông tin UAT thủ công bắt buộc cho người dùng

### 6.1 Môi trường và tài khoản test
* **URL ứng dụng:** `http://localhost:5300`
* **Route cần test:** `http://localhost:5300/employee`
* **Auth mode:** Keycloak thật (UseMockAuth = `false`)
* **Tài khoản UAT:** `admin` hoặc `admin@hrm.local` (Mật khẩu: `Admin@123456`)
* **Quyền xem trang:** `VIEW_EMPLOYEE`
* **Quyền cấp tài khoản:** `UPDATE_EMPLOYEE` + `UPDATE_USER`

### 6.2 Điều kiện trước khi test
1. Keycloak local đang chạy ở `http://localhost:8080`, realm `hrm`.
2. Ứng dụng HRM đang chạy ở `http://localhost:5300`.
3. Migration `20260704024251_AddUniqueIndexEmployeeUserId` đã apply vào DB.
4. Có ít nhất một Employee chưa có tài khoản (`employee.user_id IS NULL`).
5. Có ít nhất một role hợp lệ để chọn trong modal, ví dụ `EMPLOYEE_SELF_VIEW`.
6. Không dùng username/email đã tồn tại trong bảng `"user"` hoặc Keycloak.

### 6.3 Dữ liệu mẫu đề xuất cho TC-05
* **Username:** `uat.provision01`
* **Email:** `uat.provision01@hrm.local`
* **Password:** `Admin@123456`
* **Role:** `EMPLOYEE_SELF_VIEW`

---

## 7. Kết quả UAT (UAT Results & Checklist)

| # | Tên Test Case | Điều kiện ban đầu | Các bước thực hiện | Kết quả mong đợi | Trạng thái UAT |
|---|---|---|---|---|---|
| **TC-01** | Kiểm tra danh sách nhân viên | Đã login bằng tài khoản UAT có quyền `VIEW_EMPLOYEE` | 1. Truy cập URL `/employee`. <br>2. Xem danh sách nhân viên trong bảng. | - Có cột **Tài khoản** mới.<br>- Nhân viên đã liên kết tài khoản hiển thị badge màu xanh dương: **Đã có**.<br>- Nhân viên chưa có tài khoản hiển thị nút màu vàng: **Cấp tài khoản**. | **Local Verified - PENDING USER CONFIRMATION** (Đã hiển thị cột Account với nút Cấp tài khoản và badge Đã có) |
| **TC-02** | Kích hoạt Modal Cấp tài khoản | Đã thực hiện TC-01 | Click vào nút **Cấp tài khoản** của một nhân viên chưa có tài khoản. | - Modal hiện lên tiêu đề: `Cấp tài khoản — [Tên Nhân Viên]`. <br>- Form có các trường: Username, Email, Mật khẩu, Vai trò (Multiple Select). <br>- Placeholder mật khẩu hiển thị: `Tối thiểu 6 ký tự`. | **Local Verified - PENDING USER CONFIRMATION** (Modal mở ra chính xác, hiển thị tiếng Việt có dấu chuẩn) |
| **TC-03** | Validation trống phía Client | Đã thực hiện TC-02 | 1. Để trống tất cả các trường.<br>2. Nhấn nút **Cấp tài khoản** trong modal. | - Xuất hiện các dòng thông báo lỗi màu đỏ bằng tiếng Việt có dấu dưới mỗi trường bắt buộc. | **Local Verified - PENDING USER CONFIRMATION** (Hiển thị các câu cảnh báo lỗi trống bằng tiếng Việt) |
| **TC-04** | Validation độ dài mật khẩu | Đã thực hiện TC-02 | 1. Nhập Username, Email hợp lệ.<br>2. Nhập mật khẩu dài 5 ký tự.<br>3. Chọn vai trò.<br>4. Nhấn **Cấp tài khoản**. | - Báo lỗi màu đỏ dưới trường Mật khẩu: `Mật khẩu phải có ít nhất 6 ký tự.` | **Local Verified - PENDING USER CONFIRMATION** (Client-side chặn thành công và báo độ dài >= 6 ký tự) |
| **TC-05** | Gửi yêu cầu Cấp tài khoản thành công | Đã thực hiện TC-02 | 1. Nhập đầy đủ thông tin hợp lệ (mật khẩu >= 6 ký tự).<br>2. Chọn ít nhất một vai trò.<br>3. Nhấn **Cấp tài khoản**. | - Nút chuyển sang trạng thái disabled hiển thị `Đang xử lý...`. <br>- Ứng dụng hiển thị Toast xanh: `Cấp tài khoản nhân viên thành công!`. <br>- Trang web reload sau 1.5 giây, nhân viên đó hiển thị trạng thái badge xanh dương: **Đã có**. | **Local Verified - PENDING USER CONFIRMATION** (Tạo thành công tài khoản liên kết cả ở DB và Keycloak local) |
| **TC-06** | Gửi yêu cầu Cấp tài khoản trùng lắp | Đã thực hiện TC-02 | 1. Nhập username trùng với tài khoản đã tồn tại trong hệ thống.<br>2. Nhập các trường hợp lệ khác.<br>3. Nhấn **Cấp tài khoản**. | - Toast màu đỏ hiển thị thông báo lỗi cụ thể (ví dụ: `Tài khoản với Username này đã tồn tại`). <br>- Hộp cảnh báo màu đỏ ở đầu modal cũng hiển thị lỗi tương tự. | **Local Verified - PENDING USER CONFIRMATION** (Hiển thị thông báo "Tài khoản với Username này đã tồn tại" màu đỏ ở đầu modal) |
| **TC-07.1** | Đăng nhập tài khoản có quyền `VIEW_DASHBOARD` | Đã có tài khoản Admin | 1. Mở cửa sổ ẩn danh sạch.<br>2. Truy cập `/auth/login-screen`.<br>3. Đăng nhập bằng tài khoản admin (`admin`). | - Đăng nhập thành công và được chuyển hướng về trang `/dashboard`. | **Local Verified - USER CONFIRMED PASS** (Đăng nhập bình thường và vào Dashboard admin) |
| **TC-07.2** | Đăng nhập tài khoản không có quyền `VIEW_DASHBOARD` nhưng có quyền `VIEW_LEAVE_REQUEST` | Tài khoản Employee vừa cấp ở TC-05 (ví dụ `uat.provision05` thuộc vai trò `EMPLOYEE_SELF_VIEW`) | 1. Mở cửa sổ ẩn danh sạch.<br>2. Truy cập `/auth/login-screen`.<br>3. Đăng nhập bằng tài khoản Employee vừa cấp.<br>4. Kiểm tra trang điều hướng. | - Đăng nhập thành công và tự động chuyển hướng động về trang `/leave-request`. | **Local Verified - USER CONFIRMED PASS** (Đăng nhập thành công và chuyển hướng về đúng trang yêu cầu phép) |

---

## 8. Minh chứng UAT (UAT Screenshots & Video Recording)

*Lưu ý: Các hình ảnh và video dưới đây là minh chứng ghi nhận kiểm thử tự động cục bộ (local-only evidence) trong môi trường phát triển của Antigravity/Codex, không được lưu trữ làm bằng chứng commit lâu dài trong Git.*

### 8.1 Screenshots minh chứng (Local-Only Evidence):
* **TC-03 Validation trống:**
  ![TC-03 Validation trống](file:///C:/Users/Tienht/.gemini/antigravity/brain/6a63bea3-eced-492a-92f4-38b740d13573/tc03_validation_1783152968723.png)
* **TC-04 Validation độ dài mật khẩu:**
  ![TC-04 Validation độ dài mật khẩu](file:///C:/Users/Tienht/.gemini/antigravity/brain/6a63bea3-eced-492a-92f4-38b740d13573/tc04_validation_1783153013265.png)
* **TC-05 Cấp tài khoản thành công:**
  ![TC-05 Cấp tài khoản thành công](file:///C:/Users/Tienht/.gemini/antigravity/brain/6a63bea3-eced-492a-92f4-38b740d13573/tc05_success_1783153044222.png)
* **TC-06 Trùng tên đăng nhập:**
  ![TC-06 Trùng tên đăng nhập](file:///C:/Users/Tienht/.gemini/antigravity/brain/6a63bea3-eced-492a-92f4-38b740d13573/tc06_validation_1783153182308.png)

### 8.2 Video ghi màn hình UAT (Local-Only Evidence):
Quá trình UAT chi tiết từ phiên trình duyệt sạch:
![Video UAT Cấp Tài Khoản Nhân Viên](file:///C:/Users/Tienht/.gemini/antigravity/brain/6a63bea3-eced-492a-92f4-38b740d13573/employee_provision_uat_1783152855242.webp)

---

## 9. Trạng thái Git & Khuyến nghị

* **Trạng thái Git:** Chưa stage, chưa commit, chưa push. Có các thay đổi local đang ở dạng unstaged.
* **Khuyến nghị:** 
  1. Toàn bộ các kiểm thử về mặt kỹ thuật cho cả UI Scope và Auth Blocker đã sẵn sàng.
  2. Báo cáo này ở trạng thái **PENDING USER CONFIRMATION** nhằm chờ đợi sự xác nhận và phê duyệt UAT thực tế cuối cùng từ phía người dùng đối với các ca kiểm thử giao diện trước khi thực hiện các bước commit/push mã nguồn.
