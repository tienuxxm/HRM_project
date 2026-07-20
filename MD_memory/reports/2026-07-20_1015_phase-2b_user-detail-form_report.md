# UAT REPORT: PHASE 2B - USER DETAIL/EDIT FORM UI REFACTOR

- **Ngày thực hiện:** 2026-07-20
- **Trạng thái:** SẴN SÀNG UAT (READY FOR MANUAL UAT)
- **Tập tin thay đổi:** `HRM_Leave_Management/Web.Backend/Views/User/Detail.cshtml`
- **Phong cách thiết kế:** Swiss International HR Style 06 (Monochrome, Hairline Borders, 0px Radius, Typography Mono/Sans)

---

## 1. Điều Kiện Trước Khi Test (Prerequisites)
1. Đảm bảo ứng dụng backend đang chạy bình thường (`dotnet run` hoặc IIS/Kestrel).
2. Đã đăng nhập vào hệ thống quản trị với tài khoản có quyền quản lý User (hoặc tài khoản UAT mặc định: `admin` / `Admin@123456`).

---

## 2. Các Bước Thực Hiện UAT (Step-by-Step Test Cases)

### TC-01: Kiểm tra Giao diện Tổng quan & Khả năng Responsive
- **Đường dẫn:** Từ trang danh sách `/user`, click vào nút hành động **EDIT** của một User bất kỳ để chuyển tới màn `/user/detail/{id}`.
- **Thao tác:** Quan sát cấu trúc giao diện ở cả Desktop và thiết bị di động (Chrome DevTools Mobile Mode 390x844).
- **Kết quả mong đợi:**
  - Header in hoa nổi bật: **USER PROFILE** cùng nút quay lại dạng `[← BACK TO LIST]` cùng hàng, sử dụng `white-space: nowrap` chống wrap.
  - Giao diện bọc trong container viền phẳng hairline `#D1D1D1`, 0px radius, no shadow.
  - Phù hợp trên thiết bị di động 390x844: không bị overflow ngang, có thể cuộn xuống cuối trang dễ dàng, không bị che khuất bởi thanh bottom nav.

### TC-02: Kiểm tra Input Fields, Labels & Readonly Username
- **Thao tác:** Kiểm tra hiển thị của các trường nhập liệu: Full Name, Username, Email.
- **Kết quả mong đợi:**
  - Labels in hoa hoàn toàn, font mono nhỏ (`font-mono text-[10px]`), màu xám đậm `#4c4546`.
  - Các input fields được xếp trên grid 2 cột ở desktop, 1 cột ở mobile.
  - Trường **Username** hiển thị ở chế độ Readonly: Nền xám nhạt `#f5f5f5`, chữ xám `#8e8485`, con trỏ chuột dạng `cursor-not-allowed` và không thể chỉnh sửa giá trị.
  - Các trường input thông thường có viền hairline phẳng, focus sẽ chuyển viền đen (`focus:border-black`).

### TC-03: Kiểm tra Dropdown SumoSelect (Permission Group)
- **Thao tác:** Click chọn dropdown Permission Group để xem danh sách quyền được chọn.
- **Kết quả mong đợi:**
  - Dropdown có viền phẳng, 0px radius đồng bộ hoàn toàn với các input khác.
  - Tải đúng danh sách các role hiện có của user và cho phép multi-select bình thường.
  - Khi mở dropdown, danh sách option hiển thị không bo góc, có viền hairline, hiệu ứng hover nhẹ.

### TC-04: Kiểm tra Validation Client-side
- **Thao tác:** Xóa hết nội dung trường Full Name và Email, sau đó nhấn **SAVE**.
- **Kết quả mong đợi:**
  - Form ngăn không cho submit.
  - Hiển thị thông báo lỗi in hoa bằng font mono màu đỏ ngay dưới mỗi trường bị thiếu (ví dụ: `PLEASE ENTER FULL NAME`).
  - Giao diện lỗi không làm vỡ layout của form.

### TC-05: Kiểm tra Submit Form (AJAX Contract Validation)
- **Thao tác:** Điền đầy đủ thông tin hợp lệ và bấm **SAVE**.
- **Kết quả mong đợi:**
  - Loading overlay hiện lên trong quá trình gửi yêu cầu POST tới `/User/Update`.
  - Không có lỗi JavaScript nào trong Console.
  - Form cập nhật thành công, hiển thị Toast thông báo và tự động điều hướng quay lại trang danh sách `/user`.
  - Thông tin user vừa cập nhật được lưu đúng vào cơ sở dữ liệu.

---

## 3. Ghi Nhận Kết Quả UAT (Kết quả thực tế từ User)
*(Phần này dành cho User hoặc Codex cập nhật sau khi hoàn tất các bước trên)*
- **TC-01:** [ ] Pass / [ ] Fail. Chi tiết: _________________
- **TC-02:** [ ] Pass / [ ] Fail. Chi tiết: _________________
- **TC-03:** [ ] Pass / [ ] Fail. Chi tiết: _________________
- **TC-04:** [ ] Pass / [ ] Fail. Chi tiết: _________________
- **TC-05:** [ ] Pass / [ ] Fail. Chi tiết: _________________
