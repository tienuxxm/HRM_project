# Báo cáo kết quả UAT Employee CRUD (Phase 2A.2)

Báo cáo này tài liệu hóa quá trình kiểm thử chấp nhận người dùng (UAT) đối với tính năng Employee CRUD của hệ thống HRM / Leave Management.

---

## 1. Thông tin chung
* **Môi trường UAT**: Local Development (Windows)
* **Cơ chế xác thực (Authentication)**: Keycloak thật (`UseMockAuth: false`)
  * **Realm**: `hrm`
  * **Client**: `hrm-web`
* **Tài khoản UAT**:
  * **Username**: `admin@hrm.local`
  * **Password**: `Admin@123456`

---

## 2. Giải trình lỗi 403 Forbidden ban đầu
Trong quá trình chạy UAT lần đầu, hệ thống đã trả về mã lỗi **403 Forbidden** khi tài khoản `admin@hrm.local` truy cập vào endpoint `/employee`.

Qua phân tích và trace hệ thống, lỗi này xảy ra **không phải do cơ chế Authentication (đăng nhập)** mà do **thiếu phân quyền trong cơ sở dữ liệu**:
1. Trong cấu trúc controller `EmployeeController.cs`, phương thức `Index` yêu cầu kiểm tra quyền `VIEW_EMPLOYEE` của người dùng.
2. Tại thời điểm chạy thử nghiệm ban đầu, bảng `permission` chưa chứa bản ghi cho quyền `VIEW_EMPLOYEE` và `UPDATE_EMPLOYEE`, đồng thời chưa gán các quyền này cho vai trò `ADMIN` của người dùng đăng nhập.
3. Khi người dùng đăng nhập thành công qua Keycloak, bộ lọc phân quyền kiểm tra cơ sở dữ liệu và phát hiện người dùng thiếu quyền hạn, dẫn đến việc chuyển hướng sang trang lỗi hoặc chặn truy cập.

---

## 3. Các bước khắc phục & chuẩn bị
Để xử lý lỗi trên và chuẩn bị cho UAT, các bước sau đã được thực hiện thành công:
1. **Khởi động Docker & Keycloak**: Khởi chạy Docker Desktop và container `keycloak-hrm` trên port `8080`.
2. **Khôi phục mật khẩu UAT**: Đăng nhập Keycloak admin console và thiết lập lại mật khẩu cho tài khoản `admin@hrm.local` về đúng mật khẩu thiết kế là `Admin@123456` (không dùng cờ temporary) để phục vụ kiểm thử.
3. **Seed Permission**: Thực thi đoạn script SQL để nạp quyền vào cơ sở dữ liệu PostgreSQL `hrm_baseline_db`:
   * Chèn quyền `VIEW_EMPLOYEE` và `UPDATE_EMPLOYEE` vào bảng `permission`.
   * Gán 2 quyền này cho vai trò `ADMIN` (ID: `11111111-1111-1111-1111-111111111111`) thông qua bảng trung gian `role_to_permission`.
4. **Sửa lỗi múi giờ PostgreSQL (UTC)**:
   * **Vấn đề**: Khi lưu nhân viên mới, PostgreSQL báo lỗi do trường `join_date` (thuộc kiểu `timestamp with time zone`) nhận giá trị dạng `DateTimeKind.Unspecified`.
   * **Khắc phục**: Cập nhật cả hai lớp Handler `CreateEmployeeCommandHandler` và `UpdateEmployeeCommandHandler` để chuẩn hóa múi giờ sang UTC thông qua hàm `DateTime.SpecifyKind(request.JoinDate, DateTimeKind.Utc)`.

---

## 4. Nhật ký thực hiện UAT CRUD
Dưới đây là tiến trình chạy thử nghiệm bằng Browser Subagent:

1. **Truy cập /employee**: Trình duyệt mở `http://localhost:5300/employee` và được chuyển hướng sang trang đăng nhập Keycloak.
2. **Đăng nhập**: Điền thông tin `admin@hrm.local` / `Admin@123456` và nhấn **Sign In**. Đăng nhập thành công và chuyển hướng ngược lại `/employee`.
3. **UAT Create & List**:
   * Click vào nút **Add Employee** trên giao diện danh sách.
   * Nhập thông tin: `EMP001` / `Nguyen Van A` / `Developer` / `06/25/2026`.
   * Nhấp nút **Create**. Nhân viên mới được tạo thành công và hiển thị tức thì trên danh sách.
4. **UAT Edit**:
   * Click nút **Edit** của hàng `EMP001`.
   * Thay đổi `FullName` thành `Nguyen Van A Edited` và `Position` thành `Senior Developer`.
   * Nhấn nút **Save**. Trang reload và hiển thị thông tin cập nhật thành công.
5. **UAT Delete**:
   * Click nút **Remove** của hàng `EMP001`.
   * Trong Confirm Modal, nhấn nút **Đồng ý**.
   * Trang reload và hàng `EMP001` biến mất hoàn toàn khỏi bảng.

---

## 5. Kết quả & Minh chứng UAT thành công

### 5.1. Bảng trạng thái UAT CRUD

| Chức năng | Trạng thái UAT | Auth Mode | Kết quả mong đợi | Kết quả thực tế |
| :--- | :--- | :--- | :--- | :--- |
| **Create** | **PASS** | Keycloak thật (`UseMockAuth: false`) | Lưu thành công nhân viên mới | Đạt yêu cầu |
| **List** | **PASS** | Keycloak thật (`UseMockAuth: false`) | Hiển thị danh sách nhân viên | Đạt yêu cầu |
| **Edit** | **PASS** | Keycloak thật (`UseMockAuth: false`) | Cập nhật thông tin nhân viên | Đạt yêu cầu |
| **Delete** | **PASS** | Keycloak thật (`UseMockAuth: false`) | Xóa nhân viên khỏi hệ thống | Đạt yêu cầu |

### 5.2. Minh chứng hình ảnh

* **Minh chứng Edit thành công (Thông tin đổi sang Nguyen Van A Edited / Senior Developer)**:
  ![Giao diện sau khi cập nhật thành công](file:///C:/Users/Tienht/.gemini/antigravity/brain/ee1a97e1-fbcb-416a-b265-61cc09b43319/artifacts/employee_edit_success.png)

* **Minh chứng Delete thành công (Bảng danh sách trống)**:
  ![Giao diện sau khi xóa nhân viên](file:///C:/Users/Tienht/.gemini/antigravity/brain/ee1a97e1-fbcb-416a-b265-61cc09b43319/artifacts/employee_delete_success.png)

---

## 6. Lưu ý về Git và Phạm vi kiểm tra
* Công cụ GitNexus `detect_changes` không thể thực hiện được trong phiên kiểm thử này do repository cục bộ chưa có commit khởi tạo (chưa có HEAD). Vì vậy, báo cáo này không sử dụng GitNexus `detect_changes` để đối chiếu và cam kết tính sạch sẽ của scope thay đổi git trong lần này.

### Đánh giá trạng thái UAT chung: **ĐẠT (PASSED)**
Employee Create/List/Edit/Delete pass trong UAT local với Keycloak thật.