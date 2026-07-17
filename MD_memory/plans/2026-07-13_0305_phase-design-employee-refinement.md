# Kế hoạch Tinh chỉnh Giao diện Employee Directory & Global Shell (Swiss International - DP Style 06)

## 1. Xác định Giai đoạn (Phase Restatement)
- **Giai đoạn**: Tinh chỉnh visual fidelity giao diện Employee Directory, Global Shell và các modals đi kèm theo đúng Prototype Stitch đã duyệt (DP Style 06 - Swiss International).
- **Ranh giới Kiến trúc bảo toàn**:
  - `Web.Backend -> Application -> Domain`
  - `Infrastructure -> Application/Domain`
  - Tuyệt đối không thay đổi backend C#, DB, Auth, Keycloak. Chỉ thay đổi tầng hiển thị Razor View (`Index.cshtml`, `_Layout.cshtml`, các modal partials).

## 2. Minh chứng Hiện tại & Ranh giới Kỹ thuật (Current Evidence & Discovery)
- **Trạng thái Git**: Working tree hiện đang dirty (có các thay đổi chưa được commit từ phiên làm việc trước). Nhánh hoạt động: `main`.
- **Nguồn thiết kế đáng tin cậy (Stitch Screen IDs)**:
  - Desktop Screen: `81667db3ec1649018cd1133168e058e7` (HTML đã được tải xuống `MD_memory/debug/desktop.html`)
  - Mobile Screen: `54cd65c41e3745edbe9836795f466155` (HTML đã được tải xuống `MD_memory/debug/mobile.html`)
- **Nguyên tắc DP Style 06 (Swiss International)**:
  - Canvas: Màu nền trắng/off-white (`#FAF9F9`).
  - Đường viền hairline mỏng màu xám (`#D1D1D1`).
  - Không có bo góc (0px radius) cho tất cả container, button, input.
  - Sử dụng Font: `Geist` cho body và headline, `JetBrains Mono` cho các mã code (EMP-XXXX), thông số kỹ thuật và trạng thái.
  - Điểm nhấn đỏ Thụy Sĩ (`#E62429` hoặc `#bb0015`).

## 3. Danh sách các thay đổi cần thực hiện (Refactoring Scope)

### A. Global Shell (`_Layout.cshtml`)
1. **Brand Block (Sidebar)**:
   - Thêm đường viền đỏ Thụy Sĩ bên dưới tiêu đề `HRM PORTAL` (`swiss-underline` / `border-bottom: 2px solid #E62429`).
   - Tiêu đề `HRM PORTAL` phải có thanh đỏ bên trái (`border-left: 4px solid #bb0015` với padding và font Geist/Inter Bold).
   - Subtitle "Institutional Authority" hiển thị rõ nét với font `Geist` viết hoa, cỡ chữ nhỏ, tracking rộng.
2. **Topbar**:
   - Bên trái: Hiển thị thanh breadcrumb `SYS / DIRECTORY / EMPLOYEES` bằng font Monospace (`JetBrains Mono`), viết hoa, ngăn cách bởi `/`.
   - Bên phải: Hiển thị thông tin Realm và User dạng: `REALM: HRM | USER: admin@hrm.local` rõ ràng bằng font Monospace, cách biệt bởi viền mỏng bên trái nút Logout.
   - Nút Logout: 0px border-radius, phẳng, màu nền đỏ `#bb0015` đổi sang màu đen khi hover, chứa biểu tượng và chữ "LOGOUT" viết hoa rõ ràng.
3. **Sidebar Navigation**:
   - Canh chỉnh các menu item hoạt động (Active): tô đen toàn bộ nền (`bg-primary`), chữ trắng (`text-on-primary`), thêm vạch đen dọc bên trái (`active-indicator`).
   - Các menu item bình thường: màu xám, hover đổi màu nhẹ nhàng, icon hiển thị thống nhất dạng outline.
4. **Footer**:
   - Canh chỉnh thanh footer hệ thống hiển thị thông tin Session và mã hóa của Keycloak, font chữ Monospace 9px.

### B. Employee List Desktop & Mobile (`Index.cshtml`)
1. **Desktop View (Bảng nhân viên)**:
   - Thanh bộ lọc (Filter bar): Gồm Search Directory (với kính lúp), Dept select, Position select, Status select và nút "ADD EMPLOYEE" nằm sát lề phải.
   - Bảng nhân viên (Table): Đường viền hairline `#D1D1D1` bao quanh các cột, header viết hoa, nền xám nhẹ cho header. Mã code nhân viên `EMP-XXXX` dùng font Monospace và in đậm.
   - Badge trạng thái: Viền đen chữ đen in hoa cho `ACTIVE`, viền đứt nét chữ xám cho `INACTIVE`. Tất cả đều là góc vuông (0px radius).
   - Phân trang (Pagination): Nút "PREV", "NEXT" góc vuông, hairline border, chữ in hoa.
2. **Mobile View (Card nhân viên)**:
   - Chuyển đổi bảng thành danh sách card trên mobile đúng theo cấu trúc `mobile.html` từ Stitch.
   - Mỗi card có Header nền xám nhẹ `#FBFBFB`, hiển thị mã code và badge trạng thái góc vuông.
   - Body hiển thị Tên nhân viên (chữ in hoa đậm) và lưới thông tin: Dept, Role, Email, Report To.
   - Footer card gồm 3 nút in hoa: `[VIEW]`, `[EDIT]`, `[DELETE]` cách nhau bởi viền hairline mỏng.

### C. Create/Edit/Provision/Delete Modals
1. Cập nhật các file partial:
   - `_CreateEmployeePartial.cshtml`
   - `_UpdateEmployeePartial.cshtml`
   - `_ProvisionAccountPartial.cshtml`
   - `_ConfirmDeletePartial.cshtml`
2. Đổi toàn bộ các input field sang góc vuông (0px border-radius), viền hairline `#D1D1D1`, focus hiện viền đen.
3. Nút hành động chính (Lưu, Xác nhận): Màu đen hoặc đỏ đậm, góc vuông hoàn toàn.

## 4. Kế hoạch Kiểm thử & Xác minh (Verification Plan)
1. **Kiểm tra biên dịch**: Chạy `dotnet build HRM_Leave_Management/LUC.sln` để đảm bảo không làm gãy code backend hay Razor syntax.
2. **Xác minh hiển thị**: Tạo báo cáo UAT chi tiết để tự kiểm tra bằng mắt các điểm visual fidelity trên desktop/mobile (màu nền, bo góc, căn chỉnh, cỡ chữ).
