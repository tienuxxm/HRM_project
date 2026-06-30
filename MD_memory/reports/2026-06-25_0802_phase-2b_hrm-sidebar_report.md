# Báo cáo UAT Phase 2B: Thay đổi Sidebar sang nghiệp vụ HRM & Ẩn các menu cũ

- **Ngày thực hiện:** 2026-06-25
- **Người thực hiện:** Antigravity (Senior .NET Fullstack Engineer)
- **Phase:** Phase 2B
- **Mục tiêu:** Cấu hình menu/sidebar trong file layout dùng chung để chỉ hiển thị các menu HRM mới và ẩn toàn bộ các menu cũ của Project LUC.

---

## 1. Trạng thái Môi trường UAT
- **Thư mục chạy Build/Run:** Lệnh build và run hệ thống được thực hiện trực tiếp từ thư mục dự án `HRM_Leave_Management`.
  - Build: `dotnet build`
  - Run: `dotnet run --project Web.Backend`
- **Auth Mode:** Keycloak thật (`UseMockAuth: false` cấu hình tại `Web.Backend/appsettings.json`)
- **Keycloak Server:** Đang hoạt động tại `http://localhost:8080` (Realm: `hrm`, Client: `hrm-web`)
- **Tài khoản UAT sử dụng:** `admin@hrm.local` / `Admin@123456`
- **Port ứng dụng Web.Backend:** `http://localhost:5300`

---

## 2. Chi tiết các thay đổi đã thực hiện
Chúng ta đã chỉnh sửa file layout dùng chung:
- **File:** `HRM_Leave_Management/Web.Backend/Views/Shared/_Layout.cshtml`
- **Nội dung thay đổi:** Cập nhật lại mảng `menuItems` Javascript ở dòng 111-209.
  - Loại bỏ hoàn toàn các menu cũ của Project LUC (Table Booking, Delivery, Customers, Vouchers, Promotions, Partners, v.v.).
  - Khai báo các menu HRM mới phân chia theo các nhóm chức năng rõ ràng:
    - **GENERAL:** Dashboard (`/dashboard`)
    - **HRM MANAGEMENT:** Departments (`/department`), Employees (`/employee`)
    - **LEAVE MANAGEMENT:** Leave Types (`/leavetype`), Leave Requests (`/leaverequest`), Leave Balances (`/leavebalance`)
    - **SYSTEM:** Users (`/user`), Roles & Permissions (`/role`)

---

## 3. Kết quả Kiểm thử & Xác minh (UAT)

### 3.1. Phân tích thay đổi qua GitNexus detect_changes
- **Trạng thái:** Công cụ GitNexus `detect_changes` báo cáo không phát hiện thay đổi (`No changes detected`).
- **Nguyên nhân:** Do kho lưu trữ Git mới được khởi tạo và chưa có commit đầu tiên (chưa có HEAD/unstaged tracked changes), các thư mục và file trong dự án hiện ở trạng thái `Untracked`.
- **Lưu ý:** Chúng tôi không claim scope clean tự động dựa trên detect_changes do công cụ chưa khả dụng hoàn toàn cho các file untracked. Tuy nhiên, việc rà soát thủ công xác nhận chỉ có file `_Layout.cshtml` được chỉnh sửa ngoài các file debug/report tạm thời.

### 3.2. Biên dịch hệ thống
Hệ thống được biên dịch thành công sau khi tắt tiến trình cũ đang chạy nền và chạy lệnh build tại thư mục `HRM_Leave_Management`:
```bash
dotnet build
```
- **Kết quả:** Build thành công, 0 lỗi (0 Errors), 30 cảnh báo (30 Warnings).

### 3.3. Đăng nhập & Hiển thị Sidebar HRM
- Thực hiện truy cập `http://localhost:5300/dashboard`, hệ thống chuyển hướng tự động qua trang đăng nhập Keycloak thật.
- Đăng nhập thành công với tài khoản `admin@hrm.local`, hệ thống chuyển hướng lại về `/dashboard`.
- **Kết quả Sidebar:** Sidebar hiển thị đúng cấu trúc HRM mới gồm các nhóm GENERAL, HRM MANAGEMENT, LEAVE MANAGEMENT, SYSTEM. Hoàn toàn không còn xuất hiện bất kỳ menu cũ nào của Project LUC.

### 3.4. Kiểm tra các trang UAT cụ thể
* **`/dashboard` (Dashboard):** Truy cập thành công, sidebar hiển thị đúng.
* **`/department` (Quản lý Phòng ban):** Truy cập thành công. Trang load danh sách phòng ban bình thường và kế thừa sidebar HRM mới.
* **`/employee` (Quản lý Nhân viên):** Truy cập thành công. Trang load danh sách nhân viên bình thường và kế thừa sidebar HRM mới.
* **`/user` (Quản lý tài khoản):** Đã click test thông qua Sidebar. Route `/user` load thành công, hiển thị đúng danh sách user hiện tại và highlight menu hoạt động trên sidebar.
* **`/role` (Quản lý vai trò & quyền):** Đã click test thông qua Sidebar. Route `/role` load thành công, hiển thị nhóm quyền ADMIN cùng các quyền tương ứng và highlight menu hoạt động trên sidebar.

### 3.5. Trạng thái các route Placeholder (chưa UAT thành công)
* Các liên kết thuộc nhóm **LEAVE MANAGEMENT** gồm:
  * `/leavetype` (Leave Types)
  * `/leaverequest` (Leave Requests)
  * `/leavebalance` (Leave Balances)
* **Tình trạng:** Hiện tại các liên kết này chỉ là **placeholder/chưa có module** trong dự án mới. Các controller và view tương ứng chưa được định nghĩa và tạo ra, do đó các route này chưa được UAT thành công và sẽ được xử lý ở các phase sau.

---

## 4. Hình ảnh minh chứng UAT

### Ảnh 1: Trang Dashboard với Sidebar HRM mới
![Trang Dashboard với Sidebar HRM mới](C:/Users/Tienht/.gemini/antigravity/brain/43809bbe-caf5-4dab-83d7-a422aa7deb85/dashboard_view_1782373800595.png)

### Ảnh 2: Trang Department với Sidebar HRM mới
![Trang Department với Sidebar HRM mới](C:/Users/Tienht/.gemini/antigravity/brain/43809bbe-caf5-4dab-83d7-a422aa7deb85/hrm_department_page_1782374445979.png)

### Ảnh 3: Trang Employee với Sidebar HRM mới
![Trang Employee với Sidebar HRM mới](C:/Users/Tienht/.gemini/antigravity/brain/43809bbe-caf5-4dab-83d7-a422aa7deb85/hrm_employee_page_1782374453239.png)

### Ảnh 4: Trang User List hoạt động
![Trang User List hoạt động](C:/Users/Tienht/.gemini/antigravity/brain/43809bbe-caf5-4dab-83d7-a422aa7deb85/uat_user_page_1782374965101.png)

### Ảnh 5: Trang Roles & Permissions hoạt động
![Trang Roles & Permissions hoạt động](C:/Users/Tienht/.gemini/antigravity/brain/43809bbe-caf5-4dab-83d7-a422aa7deb85/uat_role_page_1782374986156.png)

---

## 5. Kết luận
Phase 2B sidebar UI pass cho các route đã UAT: `/dashboard`, `/department`, `/employee`, `/user`, `/role`. Các route placeholder cần xử lý ở phase sau.
