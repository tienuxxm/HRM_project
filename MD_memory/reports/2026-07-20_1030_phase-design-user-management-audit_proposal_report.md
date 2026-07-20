# Báo Cáo Audit & Đề Xuất Thiết Kế: User Management Refactor (Swiss Style 06)

*   **Mã Phase:** `phase-design-user-management-audit`
*   **Ngày tạo:** 2026-07-20
*   **Tác giả:** Senior .NET Fullstack Engineer & Database Architect

---

## 1. Tuyên Bố Ranh Giới Kiến Trúc (Architecture Boundary)
Để bảo toàn tính nhất quán và cấu trúc Clean Architecture của hệ thống, chúng tôi cam kết tuân thủ ranh giới phụ thuộc sau trong suốt quá trình chuẩn bị và thực thi phase:
*   `Web.Backend -> Application -> Domain`
*   `Infrastructure -> Application/Domain`
*   **Cam kết:** Tuyệt đối không thay đổi mã nguồn C# backend, không sửa đổi database/migration, và không can thiệp vào cơ chế xác thực Keycloak hoặc phân quyền hệ thống.

---

## 2. Báo Cáo Trạng Thái Git Hiện Tại (Git Baseline Status)
Trước khi thực hiện phân tích, hệ thống Git hiện tại ghi nhận các thay đổi chưa được commit (dirty/untracked baseline) như sau:
*   **Branch hiện tại:** `main`
*   **File đã thay đổi (Modified):**
    *   `HRM_Leave_Management/Web.Backend/Views/LeaveApproverAssignment/Index.cshtml`
*   **Các file chưa được theo dõi (Untracked):**
    *   `.codex_tmp/`
    *   `HRM_Leave_Management/Web.Backend/tailwind.debug.config.js`
    *   `HRM_Leave_Management/Web.Backend/test.html`
    *   `MD_memory/reports/2026-07-20_0846_phase-design-approver-assignments_proposal_report.md`

*Cam kết bảo vệ lịch sử Git: Không tự ý checkout, restore, reset, clean hoặc xóa các tệp tin thuộc baseline trên.*

---

## 3. Kết Quả Audit Hệ Thống User Management Hiện Tại

### 3.1. Cấu Trúc Views & CSS Gaps
Module quản lý người dùng hiện tại bao gồm 4 tệp tin view Razor tại thư mục `Views/User`:
1.  `Index.cshtml` (Danh sách người dùng):
    *   Sử dụng thư viện DataTables.net cũ để hiển thị bảng dữ liệu.
    *   Nút "Add User" và thanh tìm kiếm có style cũ (`bg-indigo-600`, `hover:scale-110`, vv.) không đồng bộ với Swiss Style 06.
    *   Sử dụng pagination mặc định 10 dòng/trang, điều khiển thủ công qua DataTables API.
    *   Thiếu hoàn toàn khả năng hiển thị responsive (stacked cards) cho thiết bị di động.
2.  `CreateUserView.cshtml` (Màn hình tạo mới):
    *   Layout form 12 cột thô sơ, khoảng cách và lề chưa được tối ưu hóa.
    *   Các input sử dụng style cũ của Tailwind (`bg-gray-50`, `border-gray-300`).
    *   Nút Action sử dụng màu vàng cũ (`bg-wnz-yellow`) và icon kiểu cũ.
    *   Sử dụng SumoSelect cho dropdown phân quyền chưa được tối ưu hóa giao diện.
3.  `Detail.cshtml` (Màn hình cập nhật):
    *   Có cấu trúc tương tự màn hình Create nhưng Username được set `readonly`.
    *   Thiếu nút quay lại (Back to list) nổi bật và đồng bộ.
4.  `_EditUserModal.cshtml`:
    *   Hiện tại chỉ chứa khai báo `@model Web.Backend.Models.ModalUserModel` và chưa có nội dung triển khai thực tế (dữ liệu đang được chỉnh sửa qua trang Detail độc lập).

### 3.2. Đánh Giá Rủi Ro Toàn Vẹn Dữ Liệu (User ↔ Employee)
*   **Hiện trạng quan hệ:** Bảng `Employee` liên kết với bảng `User` thông qua trường `UserId` (Nullable Guid). Một User có thể liên kết với tối đa một Employee.
*   **Rủi ro khi xóa User:**
    *   Khi xóa Employee (`DeleteEmployeeCommandHandler`), hệ thống sẽ tự động soft-delete User liên kết (`linkedUser.Delete()` set `IsDeleted = true`) và xóa tài khoản trên Keycloak để thu hồi quyền truy cập.
    *   **Tuy nhiên**, khi xóa User trực tiếp thông qua `DeleteUserCommandHandler`, hệ thống **chỉ** đánh dấu `IsDeleted = true` cho User đó và gọi Keycloak API để xóa tài khoản. Hệ thống **không** cập nhật hoặc gỡ bỏ liên kết `UserId` trong bảng `Employee`.
    *   **Hậu quả:** Employee liên quan sẽ bị mồ côi (chỉ tới một `UserId` đã bị soft-delete). Điều này có thể dẫn tới lỗi null reference hoặc không nhất quán dữ liệu khi tải danh sách Employee.
*   **Đề xuất khắc phục (Dành cho Phase Backend tương lai):** Cần bổ sung logic gỡ bỏ liên kết (`Employee.UserId = null` hoặc deactive Employee) trong `DeleteUserCommandHandler`. Trong phạm vi UI-only hiện tại, chúng tôi sẽ hiển thị cảnh báo rõ ràng trên giao diện hoặc ghi nhận lỗi này vào Technical Debt.

---

## 4. Đề Xuất Refactor Giao Diện Theo Phong Cách Swiss International HR Style 06

Chúng tôi đề xuất kế hoạch refactor UI thuần túy (không sửa backend) chia làm 3 Phase cụ thể:

### Phase 1: Tối Ưu Hóa Trang Danh Sách (Index.cshtml)
*   **Giao diện Desktop:**
    *   Sử dụng tiêu đề lớn `THE USERS` viết hoa (Uppercase), font chữ không chân tinh tế (Inter/Outfit).
    *   Bọc bảng dữ liệu bằng cấu hình ẩn của DataTables, chỉ dùng DataTables để fetch dữ liệu từ `/User/LoadData` và quản lý trạng thái.
    *   Render lại giao diện bảng thuần túy với các đường viền mỏng hairline màu xám nhạt (`#D1D1D1`), hàng tiêu đề nền xám nhạt tinh tế, loại bỏ hoàn toàn các double-border.
    *   Chuẩn hóa kích thước phân trang mặc định về **5 items/page** bằng cách sửa tham số cấu hình JS `pageLength: 5` (không chạm tới backend).
*   **Giao diện Mobile:**
    *   Ẩn bảng dữ liệu khi ở màn hình nhỏ (`hidden lg:block`).
    *   Tự động render danh sách dưới dạng các thẻ thông tin xếp chồng (Stacked Cards) tối giản, hiển thị đầy đủ thông tin: Full Name, Username, Email, Quyền hạn, kèm nút Edit/Delete dễ thao tác.
*   **Bộ lọc và Tìm kiếm:**
    *   Refactor input tìm kiếm về dạng viền hairline xám, nhúng icon tìm kiếm tinh tế góc trái.

### Phase 2: Refactor Form Tạo Mới & Cập Nhật (CreateUserView.cshtml & Detail.cshtml)
*   **Cấu trúc Form:**
    *   Thay thế toàn bộ style input cũ bằng thiết kế phẳng (Flat Design): Viền mỏng `#D1D1D1`, góc bo nhẹ, màu nền trắng tinh khiết, trạng thái active có viền xám đậm hoặc đen.
    *   Đối với màn hình cập nhật (`Detail.cshtml`), thiết lập input Username dạng khóa (`readonly`) với nền xám nhạt phẳng và con trỏ không cho phép chọn.
*   **Nút Hành Động (Actions):**
    *   Chuyển đổi các nút màu vàng (`bg-wnz-yellow`) thành thiết kế Swiss: Nút chính màu đen tối giản chữ trắng (`bg-black text-white hover:bg-neutral-800`), nút phụ (Cancel/Back) màu trắng viền xám chữ đen (`border-[#D1D1D1] text-black hover:bg-neutral-50`).
    *   Đặt nút `BACK TO LIST` nằm ngang hàng với tiêu đề cập nhật, giữ nguyên trên một dòng (`white-space: nowrap`) tránh bị wrap text trên mobile.

### Phase 3: Đồng Bộ Hóa Hộp Thoại Xác Nhận Xóa (Delete Modal)
*   Sử dụng template modal phẳng hiện có trong hệ thống, loại bỏ các hiệu ứng bóng đổ phức tạp và màu sắc cảnh báo rực rỡ (đỏ tươi). Thay thế bằng tone xám/đen sang trọng, nút bấm dứt khoát.

---

## 5. Giới Hạn Phạm Vi UI & UX (UI/UX Boundaries)
*   **Dữ liệu động:** Việc phân trang và tìm kiếm sẽ tiếp tục được xử lý qua DataTables server-side để duy trì tính toàn vẹn với API `/User/LoadData` hiện có. Chúng ta chỉ bọc và tùy biến giao diện render để che giấu các phần tử mặc định của DataTables.
*   **Không thêm thư viện bên ngoài:** Tiếp tục sử dụng jQuery và SumoSelect có sẵn trong hệ thống để quản lý trạng thái form và dropdown phân quyền.
*   **Tránh Mojibake:** Đảm bảo tất cả các file view sau khi sửa đổi được lưu dưới định dạng **UTF-8 với BOM** để hiển thị đúng tiếng Việt có dấu.

---

## 6. Kế Hoạch Kiểm Thử Thủ Công (Manual UAT Checklist)

Do cơ chế UAT yêu cầu tính chính xác cao và tránh tự động chạy browser subagent khi chưa được phê duyệt, dưới đây là checklist các bước kiểm thử thủ công dành cho người dùng:

### Điều Kiện Tiên Quyết (Prerequisites):
1.  Đảm bảo container Docker `keycloak-hrm` đang hoạt động ổn định.
2.  Kiểm tra thuộc tính `UseMockAuth` trong cấu hình `appsettings.json` đang là `false`.
3.  Tài khoản kiểm thử: `admin` / mật khẩu `Admin@123456`.
4.  Quyền hạn yêu cầu đã được seed trong DB: `VIEW_USER`, `UPDATE_USER`.

### Kịch Bản Kiểm Thử (Test Cases):

| Mã TC | Tên Bước Kiểm Thử | Thao Tác Thực Hiện | Kết Quả Mong Đợi | Trạng Thái |
| :--- | :--- | :--- | :--- | :--- |
| **TC-01** | Truy cập danh sách người dùng | Đăng nhập tài khoản `admin` -> Truy cập đường dẫn `/user` | Giao diện hiển thị danh sách người dùng theo Swiss Style 06, phân trang mặc định hiển thị 5 dòng. Không có lỗi giao diện hoặc double-border. | Chờ duyệt |
| **TC-02** | Tìm kiếm người dùng | Nhập từ khóa tìm kiếm vào ô tìm kiếm ở trang `/user` | Danh sách tự động lọc theo từ khóa thông qua API server-side. | Chờ duyệt |
| **TC-03** | Kiểm tra độ phản hồi di động | Thu nhỏ trình duyệt về kích thước Mobile (dưới 1024px) | Bảng dữ liệu ẩn đi, danh sách chuyển sang dạng các thẻ thông tin xếp chồng (Stacked Cards) gọn gàng. | Chờ duyệt |
| **TC-04** | Giao diện tạo mới người dùng | Click nút "Add User" | Trang chuyển hướng tới `/user/create` hiển thị form phẳng tối giản, các input có viền xám nhạt mỏng `#D1D1D1`. | Chờ duyệt |
| **TC-05** | Giao diện cập nhật người dùng | Click nút "Edit" trên một dòng người dùng | Trang chuyển hướng tới `/user/{id}`. Trường Username hiển thị ở trạng thái readonly phẳng, nút `BACK TO LIST` hiển thị cùng dòng với tiêu đề trang. | Chờ duyệt |
| **TC-06** | Hộp thoại xác nhận xóa | Click nút "Delete" trên một dòng người dùng | Modal xác nhận hiển thị dạng phẳng tối giản, nút xác nhận hoạt động bình thường mà không gây lỗi JS. | Chờ duyệt |
