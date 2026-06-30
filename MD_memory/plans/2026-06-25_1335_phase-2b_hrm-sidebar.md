# Kế hoạch Phase 2B: Thiết lập Sidebar HRM & Ẩn Menu Cũ

Kế hoạch này chi tiết hóa các bước thực hiện của Phase 2B nhằm cập nhật lại giao diện Sidebar của hệ thống quản trị, chuyển đổi từ nghiệp vụ của Project LUC cũ sang nghiệp vụ chuyên biệt của HRM / Leave Management.

---

## 1. Trạng thái và Điều kiện bắt đầu
* **Trạng thái hiện tại**: Phase 2A.2 (Employee CRUD) đã hoàn tất kiểm thử UAT local thành công với Keycloak thật theo Báo cáo UAT Phase 2A.2.
* **Điều kiện vào Phase 2B**:
  * [x] Trạng thái hoàn thành của Phase 2A.2 đã được cập nhật chính xác trong tài liệu `MD_memory/hrm_refactor_mapping.md`.
  * [ ] Thực hiện biên dịch thử nghiệm dự án (build) thành công ngay tại bước đầu của Phase 2B để đảm bảo mã nguồn hiện tại không có lỗi biên dịch.
  * [ ] Xác nhận hệ thống có thể khởi chạy và đăng nhập bình thường.

---

## 2. Mục tiêu Phase 2B
* **Đổi sidebar sang HRM**: Thiết kế lại giao diện Sidebar của Web.Backend để hiển thị các menu phục vụ HRM:
  * **Tổng quan**: Dashboard.
  * **Quản lý HRM**: Phòng ban (`/department`), Nhân viên (`/employee`).
  * **Quản lý Nghỉ phép (Leave Management)**: Các menu placeholder hoặc route tương lai cho loại nghỉ phép, yêu cầu nghỉ phép.
  * **Quản trị hệ thống**: Users, Roles, Permissions (từ Project LUC gốc).
* **Ẩn menu cũ của LUC**: Ẩn/loại bỏ các liên kết không thuộc phạm vi HRM bao gồm: Vouchers, Promotions, Products, Restaurants, Orders, Bookings, Membership, v.v.

---

## 3. Các bước thực hiện dự kiến
1. **Xác định vị trí Sidebar Layout**:
   * Tìm file Razor View định nghĩa Sidebar (ví dụ: `_Sidebar.cshtml`, `_Layout.cshtml` hoặc tương đương).
2. **Thiết kế lại Sidebar**:
   * Sửa cấu trúc mã HTML/Razor của Sidebar để trỏ đến các controller HRM: `EmployeeController`, `DepartmentController`.
   * Thêm kiểm tra quyền truy cập động cho các mục menu HRM tương ứng bằng cách dùng `_roleService` hoặc quyền kiểm tra trong View.
3. **Ẩn các menu nghiệp vụ LUC**:
   * Loại bỏ hoặc ẩn các khối HTML liên quan đến menu nhà hàng/loyalty cũ.
4. **Kiểm tra và xác thực (Verification)**:
   * Chạy build và run ứng dụng Web.Backend.
   * Thực hiện UAT trực quan trên trình duyệt để kiểm tra:
     * Cấu trúc sidebar hiển thị gọn gàng, đẹp mắt và trực quan.
     * Các liên kết dẫn đến đúng trang `/employee` và `/department`.
     * Menu cũ đã biến mất hoàn toàn.
     * Quyền hiển thị của từng mục menu hoạt động đúng.

---

## 5. Kế hoạch UAT và Báo cáo
* **Auth mode**: Keycloak thật (`UseMockAuth: false`).
* **Tài khoản dùng để test**: `admin` hoặc `admin@hrm.local` / `Admin@123456`.
* **Minh chứng yêu cầu**: Chụp ảnh màn hình giao diện Sidebar mới trên trình duyệt.
* **File báo cáo kết quả**: Sẽ được tạo tại `MD_memory/reports/2026-06-25_1335_phase-2b_hrm-sidebar_report.md` sau khi hoàn thành.