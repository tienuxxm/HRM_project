# UAT REPORT: PHASE 2A - USER CREATE FORM UI REFACTOR

- **Ngày thực hiện:** 2026-07-20
- **Trạng thái:** SẴN SÀNG UAT (READY FOR MANUAL UAT)
- **Tập tin thay đổi:** `HRM_Leave_Management/Web.Backend/Views/User/CreateUserView.cshtml`
- **Phong cách thiết kế:** Swiss International HR Style 06 (Monochrome, Hairline Borders, 0px Radius, Typography Mono/Sans)

---

## 1. Điều Kiện Trước Khi Test (Prerequisites)
1. Đảm bảo ứng dụng backend đang chạy bình thường (`dotnet run` hoặc qua IIS/Kestrel).
2. Đã đăng nhập vào hệ thống quản trị với tài khoản có quyền quản lý User (hoặc sử dụng tài khoản UAT mặc định: `admin` / `Admin@123456`).

---

## 2. Các Bước Thực Hiện UAT (Step-by-Step Test Cases)

### TC-01: Kiểm tra Giao diện Tổng quan (Visual Parity Check)
- **Đường dẫn:** Truy cập `/user/create` hoặc nhấp vào nút tạo mới từ màn `/user`.
- **Thao tác:** Quan sát cấu trúc giao diện.
- **Kết quả mong đợi:**
  - Không còn breadcrumb rườm rà.
  - Header hiển thị dòng chữ uppercase lớn: **CREATE USER** cùng nút quay lại dạng `[← BACK TO LIST]` ở góc phải trên cùng một hàng ngang.
  - Form bọc trong container phẳng với viền hairline `#D1D1D1`, không bo góc (`rounded-none`), không shadow.
  - Các input fields được xếp đều trên lưới 2 cột ở desktop, tự động chuyển về 1 cột trên thiết bị di động.

### TC-02: Kiểm tra Input Fields & Labels
- **Thao tác:** Quan sát các trường nhập liệu: Full Name, Username, Email, Password, Confirm Password.
- **Kết quả mong đợi:**
  - Labels sử dụng font chữ Mono (`font-mono`), in hoa hoàn toàn, kích thước nhỏ gọn (`text-[10px]`) và có màu xám đậm chuyên nghiệp (`text-[#4c4546]`).
  - Các trường bắt buộc có dấu hoa thị màu đỏ `*`.
  - Các hộp nhập liệu (input) có viền phẳng, 0px radius, placeholder hiển thị chữ in hoa màu xám nhạt (`text-[#cfc4c5]`).
  - Khi hover/focus vào ô nhập liệu, viền chuyển mượt sang màu đen (`focus:border-black`).

### TC-03: Kiểm tra Dropdown SumoSelect (Group Permission)
- **Thao tác:** Click vào dropdown chọn **Group Permission**.
- **Kết quả mong đợi:**
  - Dropdown có viền phẳng 0px radius đồng bộ hoàn toàn với các input khác.
  - Placeholder mặc định: `SELECT PERMISSION` in hoa.
  - Menu lựa chọn (Option wrapper) hiển thị không bo góc, có viền hairline, danh sách lựa chọn có checkbox phẳng và khi di chuột qua có hiệu ứng hover xám nhẹ.

### TC-04: Kiểm tra Validation Client-side
- **Thao tác:** Nhấp thẳng vào nút **SAVE** mà không điền thông tin gì.
- **Kết quả mong đợi:**
  - Form ngăn chặn việc submit.
  - Dưới mỗi trường bắt buộc xuất hiện thông báo lỗi bằng font Mono màu đỏ (`text-red-600`), in hoa (ví dụ: `PLEASE ENTER FULL NAME`).
  - Điền thử thông tin không hợp lệ hoặc mật khẩu không khớp để kiểm tra thông báo lỗi tương ứng.

### TC-05: Kiểm tra Submit Form (AJAX Contract Validation)
- **Thao tác:** Điền đầy đủ thông tin hợp lệ và chọn ít nhất một quyền trong dropdown, sau đó nhấn **SAVE**.
- **Kết quả mong đợi:**
  - Loading overlay hiển thị trong quá trình gửi yêu cầu.
  - Khi thành công, hệ thống hiện Toast thông báo thành công và tự động điều hướng quay lại trang danh sách `/user`.
  - Kiểm tra xem user mới có xuất hiện trong bảng dữ liệu hay không.

---

## 3. Ghi Nhận Kết Quả UAT (Kết quả thực tế từ User)
*(Phần này dành cho User hoặc Codex cập nhật sau khi hoàn tất các bước trên)*
- **TC-01:** [ ] Pass / [ ] Fail. Chi tiết: _________________
- **TC-02:** [ ] Pass / [ ] Fail. Chi tiết: _________________
- **TC-03:** [ ] Pass / [ ] Fail. Chi tiết: _________________
- **TC-04:** [ ] Pass / [ ] Fail. Chi tiết: _________________
- **TC-05:** [ ] Pass / [ ] Fail. Chi tiết: _________________
