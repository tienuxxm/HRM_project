# Báo cáo Thiết kế: Tinh chỉnh Màn hình Login - Hệ thống Quản lý Nghỉ phép (HRM Leave Management)

- **Thời gian:** 2026-07-10 14:00
- **Phase:** Thiết kế Giao diện (Design-Only Phase) - Tinh chỉnh Giao diện Đăng nhập (HRM Portal Login Screen)
- **Design System:** Swiss International HR (`assets/f4fbeeb3791c4c52991dd52c4fb92635`)
- **Stitch Project ID:** `17479353588209716186`

---

## 1. Kết quả thực hiện

Chúng tôi đã hoàn thành việc tạo mới và ánh xạ các màn hình đăng nhập (Login Screen) theo phong cách **Swiss International HR** (không bo góc, không đổ bóng, không dải màu, sử dụng font chữ Geist và màu đỏ Swiss Red làm điểm nhấn duy nhất).

Các màn hình mới được tạo trên Stitch canvas bao gồm:
1. **Desktop Secure Login Portal (Institutional Login):**
   - **Screen ID:** `497e239c2ac34153a0b01eacd61811fb`
   - **Aesthetic:** Layout căn giữa trên desktop, cấu trúc dạng lưới đối xứng nghiêm ngặt, card đăng nhập phẳng hoàn toàn với viền mảnh 1px `#D1D1D1` và dải màu đỏ Swiss Red (`#E62429`) dày 2px ở cạnh trên cùng để làm điểm nhấn định vị thương hiệu.
2. **Mobile Secure Login Portal (Secure Login):**
   - **Screen ID:** `d8c34d394a984945a151d812e85da88b`
   - **Aesthetic:** Thiết kế tối giản, thu hẹp kích thước bọc (card) để hiển thị tối ưu trên thiết bị di động, vẫn bảo toàn toàn bộ quy tắc thiết kế của hệ thống.

### Cập nhật cấu hình Local:
- File `.stitch/next-prompt.md` đã được viết lại để cập nhật toàn bộ quy tắc thiết kế của phong cách Swiss International làm tài liệu tham khảo cho các lần sinh/sửa màn hình tiếp theo.
- File `.stitch/metadata.json` đã được cập nhật ánh xạ chuẩn:
  - `login_swiss_international` trỏ tới `497e239c2ac34153a0b01eacd61811fb` (Desktop).
  - `login_swiss_international_desktop` trỏ tới `497e239c2ac34153a0b01eacd61811fb`.
  - `login_swiss_international_mobile` trỏ tới `d8c34d394a984945a151d812e85da88b`.

---

## 2. Manual UAT Report (Hướng dẫn tự kiểm tra cho Người dùng)

Vì đây là giai đoạn thiết kế thuần túy trên Stitch canvas và không can thiệp vào mã nguồn chạy thực tế (runtime ASP.NET Core), vui lòng thực hiện UAT trực quan trên môi trường Stitch theo các bước sau:

### Các bước kiểm tra:
1. **Mở dự án trên Stitch:**
   - Truy cập giao diện Stitch Project ID `17479353588209716186`.
2. **Kiểm tra màn hình Desktop Login (`497e239c2ac34153a0b01eacd61811fb`):**
   - Định dạng hiển thị: Căn giữa hoàn hảo, giao diện sáng (Light mode) với nền `#FAF9F9`.
   - Card chứa form đăng nhập: Màu nền trắng `#FFFFFF`, viền mảnh 1px màu xám `#D1D1D1`. Góc card vuông tuyệt đối (0px border-radius). Không có hiệu ứng đổ bóng (no drop shadow).
   - Điểm nhấn thương hiệu: Có một đường kẻ ngang màu đỏ Swiss Red (`#E62429`) dày 2px ở sát mép trên của card.
   - Tiêu đề: "HRM PORTAL" in hoa đậm (bold), cỡ chữ lớn (32px), font chữ Geist. Phía dưới có nhãn phụ "SECURE LOGON COCKPIT" in hoa nhỏ (10px), khoảng cách chữ rộng (letter-spacing 0.1em).
   - Form nhập liệu: Nhãn "USERNAME OR EMAIL" và "PASSWORD" hiển thị rõ ràng ngay phía trên ô nhập. Ô nhập vuông vức (0px radius).
   - Nút đăng nhập: Nút "SIGN IN" màu đen tuyền (#000000), chữ trắng (#FFFFFF), góc vuông 0px, viết hoa đậm.
   - Footer: Có liên kết gạch chân "Forgot password?" và "Contact system administrator". Có thông điệp bảo mật hệ thống: "CONFIDENTIAL SYSTEM: Unauthorized access is strictly prohibited." viết hoa màu xám.
3. **Kiểm tra màn hình Mobile Login (`d8c34d394a984945a151d812e85da88b`):**
   - Đảm bảo bố cục co giãn hợp lý trên giao diện di động mà không làm đè chữ hay tràn viền.

---

## 3. Kế hoạch tiếp theo
Sau khi người dùng phê duyệt giao diện Login này:
1. Chúng ta sẽ tiến hành xây dựng thư viện thành phần giao diện chuẩn (Buttons, Tables, Inputs, Badges) trực tiếp trên Stitch canvas theo bộ quy tắc Swiss International này.
2. Lập kế hoạch chi tiết (Refactoring Plan) để đưa các mã nguồn HTML/CSS tĩnh từ các màn hình Stitch này tích hợp vào Razor Views của hệ thống backend ASP.NET MVC mà không ảnh hưởng tới Keycloak hay phần xử lý logic nghiệp vụ có sẵn.
