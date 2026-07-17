# Báo cáo UAT Thủ công (Manual UAT Report) - Kiểm thử Màu sắc Modals Employee

**Thời gian:** 2026-07-13 14:30  
**Phase:** `phase-design-employee`  
**Mục tiêu:** Xác minh các thay đổi thiết kế theo chuẩn Swiss International cho phân hệ Employee.

---

## 1. Thông tin Chung & Điều kiện Trước khi Test (Prerequisites)

- **Ứng dụng Web đang chạy tại:** `http://localhost:5300`
- **Mã xác thực (Auth Mode):** Keycloak thật (`UseMockAuth: false`)
- **Tài khoản kiểm thử:**
  - Username: `admin` hoặc `admin@hrm.local`
  - Password: `Admin@123456`
- **Quyền đã seed:** `VIEW_EMPLOYEE`, `UPDATE_EMPLOYEE`
- **File CSS đã build:** `HRM_Leave_Management/Web.Backend/wwwroot/css/styles.css` đã được biên dịch lại thông qua `npm run css:build`.

---

## 2. Kịch bản Kiểm thử & Các Bước Thực hiện (Step-by-Step Test Cases)

### Kịch bản TC-01: Kiểm tra Giao diện Modal "ADD EMPLOYEE"

1. **Bước 1:** Mở trình duyệt (khuyên dùng ẩn danh/clear cache) và truy cập `http://localhost:5300/Employee`.
2. **Bước 2:** Đăng nhập bằng tài khoản `admin` / `Admin@123456`.
3. **Bước 3:** Nhấp vào nút **"+ ADD EMPLOYEE"** ở thanh công cụ phía trên bên phải.
4. **Bước 4:** Quan sát Modal Header và Modal Content hiện lên:
   - **Kết quả mong đợi:**
     - Header của Modal phải có màu nền đen hoàn toàn (`bg-black` -> `#000000`) và màu chữ trắng (`text-white` -> `#FFFFFF`).
     - Viền bao quanh Modal phải có độ dày 1px và màu xám nhạt (`border-swiss-border` -> `#D1D1D1`).
     - Các góc của Modal và các ô nhập liệu phải vuông vức 100% (không bo tròn, `rounded-none`).
     - Các nút hành động:
       - Nút **"CANCEL"**: Nền trắng, chữ đen, viền xám nhạt (`bg-white border-swiss-border text-black`), đổi sang nền xám nhạt (`bg-swiss-light` -> `#FAF9F9`) khi hover.
       - Nút **"ADD EMPLOYEE"**: Nền đen, chữ trắng (`bg-black text-white`), đổi sang nền trắng chữ đen viền đen khi hover.
       - Nút **"Close (X)"** ở góc trên phải: Nền đỏ accent (`bg-swiss-accent-red` -> `#E62429`), đổi sang màu đỏ sẫm (`bg-swiss-red` -> `#bb0015`) khi hover.
     - Các thông tin bắt buộc có dấu hoa thị màu đỏ accent (`text-swiss-accent-red`).
     - Phần Info Box bên dưới sử dụng nền xám nhạt (`bg-swiss-light`) và viền xám nhạt (`border-swiss-border`).

### Kịch bản TC-02: Kiểm tra Giao diện Modal "UPDATE EMPLOYEE"

1. **Bước 1:** Trên trang danh sách nhân viên, nhấp vào biểu tượng chỉnh sửa (Edit) ở một dòng nhân viên bất kỳ.
2. **Bước 2:** Quan sát Modal Chỉnh sửa thông tin nhân viên:
   - **Kết quả mong đợi:**
     - Header Modal chỉnh sửa màu đen hoàn toàn (`bg-black`) chữ trắng.
     - Toàn bộ border đầu vào, Info Box, các góc `rounded-none`, nút Close (X) và nút Save tương tự như Modal Add Employee.
     - Nút **"SAVE CHANGES"**: Nền đen, chữ trắng (`bg-black text-white`), khi hover chuyển thành nền trắng chữ đen viền đen.

### Kịch bản TC-03: Kiểm tra Giao diện Modal "PROVISION ACCOUNT"

1. **Bước 1:** Trên trang danh sách nhân viên, nhấp vào nút hoặc biểu tượng **"PROVISION ACCOUNT"** cho nhân viên chưa có tài khoản.
2. **Bước 2:** Quan sát Modal Provision Account:
   - **Kết quả mong đợi:**
     - Header Modal màu đen (`bg-black`) chữ trắng.
     - Nút Close (X) màu đỏ accent (`bg-swiss-accent-red` -> `#E62429`), chuyển sang đỏ đậm khi hover.
     - Khối thông báo lỗi (Error block nếu có) hiển thị nền hồng nhạt, chữ và viền màu đỏ accent (`text-swiss-accent-red border-swiss-accent-red`).
     - Nút **"PROVISION ACCOUNT"**: Nền đen, chữ trắng (`bg-black text-white`), khi hover chuyển thành nền trắng chữ đen viền đen.
     - Phần Info Box bên dưới sử dụng nền xám nhạt (`bg-swiss-light`) và viền xám nhạt (`border-swiss-border`).

---

## 3. Cách ghi nhận lỗi nếu kiểm thử thất bại

Nếu bất kỳ phần tử nào không hiển thị đúng màu đen/đỏ/xám theo thiết kế hoặc vẫn còn màu xanh da trời (Bootstrap default):
1. Nhấp chuột phải vào phần tử đó, chọn **Inspect** (Kiểm tra).
2. Chụp ảnh màn hình hiển thị lỗi kèm theo tab **Computed Styles** chỉ ra màu sắc thực tế được áp dụng đến từ file CSS hoặc rule nào.
3. Ghi lại các bước cụ thể dẫn đến lỗi và gửi phản hồi.
