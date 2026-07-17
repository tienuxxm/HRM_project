# Plan - Employee Directory UI Refactor

## 1. Trạng Tá Hì Hiện Tại & Gap Analysis

Đối chiếu giao diện runtime `/employee` hiện tại với Stitch Approved Screen (`81667db3ec1649018cd1133168e058e7`):

| Thành phần | Stitch Approved Design | Giao diện Runtime hiện tại | Khoảng trống & Hướng xử lý |
| :--- | :--- | :--- | :--- |
| **Breakpoint Hệ Thống** | Sử dụng breakpoint `lg:` cho cả Sidebar và Workspace. | Sử dụng `md:` cho ẩn/hiện table và filters trong `Employee/Index.cshtml`. | **Lệch nghiêm trọng:** Ở kích thước màn hình từ 768px (md) đến 1024px (lg), table desktop hiện nhưng sidebar lại ẩn. Cần đổi toàn bộ `md:` sang `lg:` trong view Employee. |
| **Chiều rộng Sidebar** | `260px` | `296px` (được định nghĩa cứng trong `#sidebarMenu`) | **Lệch:** Làm hẹp workspace và sai tỷ lệ layout. Cần sửa chiều rộng `#sidebarMenu` về `260px` trong `_Layout.cshtml`. |
| **Brand Block** | Padding `px-gutter pt-stack-md pb-stack-sm` (px-6 pt-8 pb-4). Viền trái đỏ thẫm `border-secondary`. Dưới có text "Institutional Authority" mờ. | Padding `px-6 pt-6 pb-4`. Viền trái `border-swiss-red`. | **Lệch nhẹ:** Cần chuẩn hóa padding, font-family và màu sắc viền trái cho khối này trong `_Layout.cshtml`. |
| **Cột Bảng Dữ Liệu** | Thứ tự: `CODE | NAME | EMAIL | DEPT | POSITION | MANAGER | STATUS | ACTIONS`. | Có đủ cột CODE, tuy nhiên cấu hình padding và font size chưa tối ưu cho độ phân giải cao. | **Tối ưu:** Cần căn chỉnh padding header/td về `12px 16px`, cỡ chữ `12px` Geist cho text thường và font mono JetBrains Mono cho Employee Code. |
| **Trạng Thái (Status)** | ACTIVE: viền đen nét liền.<br>INACTIVE: viền nhạt nét đứt `#cfc4c5`, chữ `#4c4546`. | ACTIVE: viền đen nền trắng.<br>INACTIVE: viền xám nhạt nền nhạt. | **Lệch nhẹ:** Đổi màu viền và màu chữ của nhãn INACTIVE để khớp chính xác mã màu `#cfc4c5` và `#4c4546`. |
| **Nút Thao Tác (Actions)** | Nút dạng chữ thường, in hoa, không dùng font mono: `VIEW | EDIT | DELETE`. | Đang dùng font-mono và khoảng cách tracking rộng cho các nút: `PROVISION | EDIT | DELETE`. | **Căn chỉnh:** Bỏ `font-mono` và `tracking-wider`, đổi font actions thành chữ in hoa trơn để đồng bộ visual với design. Giữ nguyên logic Provision tài khoản của backend. |

---

## 2. Kế Hoạch Triển Khai Chi Tiết

### Bước 1: Điều chỉnh chiều rộng Sidebar và Brand Block trong `Shared/_Layout.cshtml`
- Thay đổi chiều rộng `#sidebarMenu` từ `296px` thành `260px` trong cả style tag và các class tailwind (chuyển `w-296` nếu có thành `w-[260px]`).
- Điều chỉnh padding khối Brand Block từ `pt-6 pb-4` thành `pt-8 pb-4` (tương đương `pt-stack-md pb-stack-sm`).
- Sửa lại viền trái của thẻ h1 tiêu đề từ màu `border-swiss-red` sang màu `border-secondary` (đỏ thẫm/Swiss red).

### Bước 2: Chuẩn hóa breakpoint trong `Employee/Index.cshtml`
- Tìm và thay thế tất cả các class responsive:
  - `hidden md:flex` -> `hidden lg:flex` (Filter bar desktop)
  - `block md:hidden` -> `block lg:hidden` (Filter bar mobile)
  - `hidden md:block` -> `hidden lg:block` (Desktop table view)
  - `block md:hidden` -> `block lg:hidden` (Mobile cards view)
  - `hidden md:flex` -> `hidden lg:flex` (Desktop pagination)
  - `block md:hidden` -> `block lg:hidden` (Mobile pagination)

### Bước 3: Tinh chỉnh font chữ, padding bảng và Actions
- Đảm bảo thẻ `th` dùng font chữ `12px` Geist (`font-label-md text-label-md`) và chữ màu nhạt `text-on-surface-variant` (`#4c4546`).
- Đảm bảo thẻ `td` của Employee Code dùng font `JetBrains Mono`, kích thước `11px`, `font-bold`.
- Điều chỉnh nhãn `INACTIVE` để có đường viền nét đứt màu `#cfc4c5` (`border-[#cfc4c5]`) và chữ màu `#4c4546` (`text-[#4c4546]`).
- Loại bỏ các lớp `font-mono` và `tracking-wider` ra khỏi cột Actions, đưa về dạng chữ thường in hoa đậm chuẩn (`font-bold uppercase text-[11px]`).

---

## 3. Quy Trình Xác Minh & UAT
Sau khi chỉnh sửa, tôi sẽ cung cấp tài liệu UAT thủ công chi tiết để người dùng tự kiểm tra theo đúng quy trình:
1. Kiểm tra trên màn hình desktop độ rộng lớn (>1024px) để xem bảng, cột `CODE` hiển thị đầy đủ, sidebar rộng `260px`, brand block cân đối.
2. Co giãn trình duyệt về dưới 1024px để kiểm tra xem breakpoint hoạt động mượt mà, tự động chuyển sang chế độ mobile (ẩn sidebar, hiện mobile header, hiện stacked cards và bottom nav).
3. Đảm bảo toàn bộ các tương tác tìm kiếm, lọc theo Department, Position, Status và các modal Provision, Edit, Delete hoạt động bình thường, không xảy ra lỗi Javascript hay lỗi hiển thị CSS.
