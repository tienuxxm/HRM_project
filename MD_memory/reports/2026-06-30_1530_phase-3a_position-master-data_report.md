# Manual UAT Report — Phase 3A: Position Master Data & Employee PositionId Migration

- **Date:** 2026-06-30
- **Phase:** Phase 3A (Position Master Data + Employee PositionId migration)
- **Status:** Built successfully, Database migrated. Ready for manual UAT.

---

## 1. Prerequisites (Điều kiện trước khi test)
1. **Docker Containers:** Đảm bảo container cơ sở dữ liệu Postgres và Keycloak (`keycloak-hrm`) đang chạy.
2. **Database Migration:** Đã được apply thành công (Lệnh `dotnet ef database update` trả về `Database is already up to date`).
3. **Application Build:** Dự án build thành công không lỗi (`dotnet build` -> 0 Errors).
4. **Auth Mode:** Sử dụng Keycloak thật (`UseMockAuth: false` trong `appsettings.Development.json`).

---

## 2. Test Account (Tài khoản Test)
- **Username:** `admin` hoặc `admin@hrm.local`
- **Password:** `Admin@123456`

---

## 3. Step-by-Step Test Cases (Các bước thao tác chi tiết)

### Test Case 1: Verification of Database Seed Data & Sidebar Link
* **Steps:**
  1. Chạy ứng dụng Web bằng lệnh: `dotnet run --project Web.Backend`
  2. Mở trình duyệt và truy cập `http://localhost:5000` (hoặc URL dev server hiển thị trên terminal).
  3. Đăng nhập bằng tài khoản **Test Account** ở trên.
  4. Quan sát thanh Sidebar menu bên trái dưới mục **HRM MANAGEMENT**.
* **Expected Result:**
  * Có mục liên kết **Positions** xuất hiện ngay dưới **Employees**.
  * Click vào **Positions** chuyển hướng thành công đến URL `/position`.

---

### Test Case 2: Position List View (Trang danh sách chức vụ)
* **Steps:**
  1. Truy cập URL `/position`.
  2. Kiểm tra danh sách hiển thị trên bảng.
* **Expected Result:**
  * Danh sách chứa sẵn 3 chức vụ được seed từ database migration:
    1. **EMPLOYEE** (Level 1)
    2. **DEPT_MANAGER** (Level 2)
    3. **CEO** (Level 3)
  * Các cột hiển thị đầy đủ: NO, Code, Name, Level, và cột Actions (Edit, Remove).

---

### Test Case 3: Create Position (Thêm chức vụ mới)
* **Steps:**
  1. Click vào nút **Add Position** ở góc phải phía trên bảng.
  2. Điền thông tin vào form Modal:
     - **Position Code:** `HR_MANAGER`
     - **Position Name:** `HR Manager`
     - **Level:** `2`
  3. Click **Save**.
* **Expected Result:**
  * Xuất hiện thông báo toast "Position created successfully".
  * Modal tự đóng và trang tự động reload lại.
  * Chức vụ mới `HR_MANAGER` hiển thị trong danh sách.

---

### Test Case 4: Update Position (Cập nhật chức vụ)
* **Steps:**
  1. Click vào nút **Edit** bên cạnh dòng chức vụ `HR_MANAGER` vừa tạo.
  2. Thay đổi:
     - **Position Name:** `Human Resources Manager`
     - **Level:** `3`
  3. Click **Save**.
* **Expected Result:**
  * Xuất hiện thông báo toast "Position updated successfully".
  * Trang tự động reload lại và thông tin đã cập nhật được hiển thị chính xác.

---

### Test Case 5: Link Position with Employee
* **Steps:**
  1. Click vào mục **Employees** trên Sidebar (`/employee`).
  2. Chọn một nhân viên và click **Edit** (hoặc click **Add Employee**).
  3. Tại trường **Position (Optional)**, click mở select dropdown.
* **Expected Result:**
  * Select dropdown hiển thị đầy đủ danh sách chức vụ động từ database: `Employee`, `Department Manager`, `CEO`, `Human Resources Manager`.
  * Chọn một chức vụ và lưu lại thành công, không gặp lỗi hệ thống.

---

### Test Case 6: Remove/Delete Position
* **Steps:**
  1. Quay lại trang `/position`.
  2. Click **Remove** bên cạnh dòng chức vụ `HR_MANAGER` (`Human Resources Manager`).
  3. Trên modal xác nhận xóa, click **Delete**.
* **Expected Result:**
  * Xuất hiện thông báo toast xóa thành công.
  * Chức vụ biến mất khỏi danh sách.

---

## 4. Troubleshooting & Error Reporting (Cách xử lý & báo lỗi nếu UAT thất bại)
* **Nếu gặp lỗi 403 Forbidden:**
  * Kiểm tra xem tài khoản đã đăng nhập có role `Admin` chưa.
  * Kiểm tra bảng `permission` và `role_to_permission` xem đã seed permission `VIEW_POSITION` và `UPDATE_POSITION` cho role `Admin` chưa.
* **Nếu UAT thất bại ở bước nào:**
  * Vui lòng chụp ảnh màn hình giao diện (hoặc copy chi tiết thông báo lỗi trong tab Console F12 của trình duyệt).
  * Copy log lỗi từ terminal chạy ứng dụng dotnet backend gửi lại cho tôi để tôi tiến hành sửa đổi ngay lập tức.
