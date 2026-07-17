# Báo cáo: Refactor App Shell (Swiss International Style)

- **Ngày thực hiện:** 2026-07-13 02:30
- **Phase:** Refactor Shell (Institutional Swiss International Style)
- **Mục tiêu:** Loại bỏ toàn bộ giao diện cũ của LUC (Warning Zone, logo vàng/cam, bo góc, bóng mờ) và thay bằng hệ thống giao diện tối giản chuẩn Swiss International.

---

## 1. Các thay đổi chi tiết

### 1.1. Cấu trúc Layout (`_Layout.cshtml`)
*   **Bảng màu (Palette):** Chuyển dịch toàn bộ sang tone màu monochrome (Off-white canvas `#F8F9FA`, viền hairline `#D1D1D1` màu xám mỏng, text đen `#000000` hoặc xám đậm `#7F7F7F`).
*   **Bo góc (Radius):** Enforce `rounded-none` (0px radius) cho toàn bộ container, button, avatar, và thanh điều hướng.
*   **Brand Block:** 
    *   Thay thế chữ `LOGO HERE` bằng block thương hiệu cấu trúc cao: `HRM PORTAL` (chữ hoa đậm nét, font-mono) được bọc trong đường viền đỏ đặc trưng phía dưới (`border-bottom: 2px solid #E62429;`) và thanh gạch đứng màu đỏ ở cạnh trái (`border-left: 4px solid #bb0015;`), kèm sub-label `INSTITUTIONAL AUTHORITY` màu xám nhỏ phía dưới.
*   **Hệ thống Topbar:**
    *   Thiết kế dạng System Utility Bar mỏng, có đường viền dưới mỏng (`border-b border-[#D1D1D1]`).
    *   Bên trái: Hiển thị phân vùng hệ thống dạng breadcrumb: `SYS / DIRECTORY / EMPLOYEES` viết hoa, font-mono chữ nhỏ.
    *   Bên phải: Hiển thị thông tin phiên kết nối trực tiếp dạng text: `REALM: HRM | USER: [USER]`, đi kèm nút `LOGOUT` in đậm, màu đỏ Swiss, góc vuông tuyệt đối, loại bỏ menu dropdown ẩn.
*   **Thẻ Title:** Cập nhật hậu tố `<title>` từ `Warning Zone` thành `HRM Portal`.

### 1.2. Javascript Side-Menu Rendering (`_Layout.cshtml`)
*   **Tab/Headers:** Chuyển sang chữ hoa nhỏ (`text-[9px]`), in đậm, khoảng cách chữ rộng (`tracking-widest`), font chữ monospace xám `#7F7F7F`.
*   **Active Item:** Nền đen hoàn toàn (`bg-black`), text trắng (`text-white`), icon fill trắng (`fill-white`).
*   **Hover Item:** Không còn hover màu vàng cũ, thay bằng hover nền xám nhạt (`bg-[#F5F5F5]`), text đen, icon fill đen (`fill-black`).
*   **Bo góc:** Loại bỏ `rounded-xl` cũ, sử dụng cạnh vuông tuyệt đối.

### 1.3. Hệ thống Tìm kiếm & Lọc phía Client (Employee Index)
*   **Lọc dữ liệu Real-time:** Bổ sung script jQuery lắng nghe sự kiện nhập liệu (`input`) của ô Search Directory và sự kiện thay đổi (`change`) của các thẻ dropdown lọc Phòng ban (Dept), Chức vụ (Position), Trạng thái (Status).
*   **Đồng bộ hóa giao diện:** Hỗ trợ lọc đồng thời cả hàng dữ liệu trong bảng máy tính (Desktop Table Rows) và các thẻ xếp chồng của thiết bị di động (Mobile Stacked Cards) dựa trên từ khóa tìm kiếm và các tiêu chí đã chọn.

---

## 2. Trạng thái Build & Git Hygiene
*   **Build Verification:**
    *   Đã chạy lệnh `dotnet build HRM_Leave_Management/LUC.sln`.
    *   *Lưu ý:* Build trả về warning/error copy file `.exe` do tiến trình ứng dụng `Web.Backend.exe` (PID 11504) đang chạy và khóa file thực thi. Điều này là bình thường vì các file Razor `.cshtml` và CSS/JS tĩnh sẽ được runtime biên dịch/tải động mà không cần rebuild nhị phân của Web.Backend.
*   **Git Status:** working tree hiện đang ở trạng thái sửa đổi (dirty). Không có file nào được stage hoặc commit/push tự động theo đúng rule bảo toàn lịch sử của dự án.

---

## 3. Kế hoạch kiểm thử UAT thủ công (Manual UAT Checklist)

Do rule cấm tự động chạy browser/subagent UAT khi không có yêu cầu rõ ràng, dưới đây là checklist chi tiết để User tự kiểm thử:

| STT | Tính năng / Màn hình | Tài khoản test | Điều kiện trước | Các bước thao tác | Kết quả mong đợi | Trạng thái (User điền) |
|---|---|---|---|---|---|---|
| 1 | **Login Flow** | `admin` / `Admin@123456` | Trực quan hóa qua Keycloak thật | Truy cập trang chủ `/`, đăng nhập. | Điều hướng mượt mà vào Dashboard, giao diện Login dạng Swiss International. | |
| 2 | **Sidebar & Topbar Shell** | `admin` | Đã đăng nhập | Quan sát thanh sidebar bên trái và thanh topbar phía trên. | - Sidebar màu off-white, có viền hairline dọc.<br>- Brand block hiển thị `HRM PORTAL / INSTITUTIONAL AUTHORITY`.<br>- Menu hiển thị in hoa, font-mono, item hoạt động có nền đen vuông vắn.<br>- Không còn vết tích màu vàng/cam LUC. | |
| 3 | **Employee Page** | `admin` | Đã đăng nhập | Nhấn vào menu **Employees** | - Bảng Employee danh sách hiển thị với cạnh phẳng, viền hairline.<br>- Modals Create/Edit/Provision hiển thị dạng Swiss International phẳng, nút bấm đen. | |
| 4 | **Responsive Mobile Layout** | `admin` | Sử dụng chế độ Responsive của Chrome (width < 768px) | Co nhỏ cửa sổ trình duyệt về giao diện mobile. | - Sidebar chuyển sang dạng thu gọn (chỉ hiển thị icon phẳng nền đen khi active).<br>- Bảng Employee chuyển sang dạng stackable cards dọc.<br>- Không bị overlap text/elements. | |
| 5 | **Logout Action** | `admin` | Đã đăng nhập | Nhấn vào tên User ở góc phải topbar, chọn **LOGOUT** | Điều hướng thành công về trang Login. | |
