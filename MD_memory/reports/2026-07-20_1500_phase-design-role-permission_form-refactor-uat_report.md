# Báo cáo UAT & RCA - Phase 2: Role & Permission Forms Refactoring

**Mã báo cáo:** `2026-07-20_1500_phase-design-role-permission_form-refactor-uat_report`
**Ngày thực hiện:** 2026-07-20
**Tác giả:** Antigravity (Senior .NET Fullstack Engineer)

---

## 1. Phân tích nguyên nhân gốc rễ (RCA) - Trùng lặp quyền Position

### Triệu chứng
Trên giao diện tạo/chỉnh sửa Group Permission (Role), danh sách quyền hiển thị trùng lặp hai quyền là **"View Position"** (`VIEW_POSITION`) và **"Update Position"** (`UPDATE_POSITION`).

### Phân tích Code và Database Schema
1. **Tầng Application**:
   - `GetAllPermissionCommandHandler.cs` gọi phương thức `_permissionRepository.GetAll()` để lấy toàn bộ danh sách permission từ database mà không có bất kỳ logic lọc hay group trùng lặp nào.
   - `PermissionRepository.cs` kế thừa từ `Repository<Permission, PermissionId>` và sử dụng `DbContext.Set<Permission>().ToListAsync()` để lấy trực tiếp dữ liệu từ bảng `permission`.

2. **Database Schema & Constraints**:
   - Bảng `permission` được tạo ở migration ban đầu chỉ định nghĩa Primary Key là cột `id`.
   - **Không có bất kỳ Unique Constraint hay Unique Index nào được định nghĩa cho cột `resource_name`**. Điều này có nghĩa là database cho phép chèn nhiều bản ghi có cùng `resource_name` nhưng khác `id`.

3. **Lỗi trong Migration chèn dữ liệu (`20260630063720_AddPositionMasterData.cs`)**:
   - Migration này thực hiện chèn hai quyền `VIEW_POSITION` và `UPDATE_POSITION` thông qua câu lệnh SQL:
     ```sql
     INSERT INTO permission (id, resource_name, display_name, is_default, created_date)
     VALUES 
     ('{viewPositionPermissionId}', 'VIEW_POSITION', 'View Position', true, NOW()),
     ('{updatePositionPermissionId}', 'UPDATE_POSITION', 'Update Position', true, NOW())
     ON CONFLICT (id) DO NOTHING;
     ```
   - Câu lệnh này sử dụng `ON CONFLICT (id) DO NOTHING;` để chống trùng lặp. Tuy nhiên, điều này **chỉ có tác dụng khi trùng lặp cột `id`**.
   - Nếu trước đó (ví dụ do Keycloak sync hoặc quá trình seeding ban đầu của dự án gốc LUC) bảng `permission` đã tồn tại bản ghi với `resource_name = 'VIEW_POSITION'` hoặc `'UPDATE_POSITION'` nhưng mang một `id` khác, câu lệnh insert trên vẫn sẽ thực hiện thành công chèn thêm bản ghi mới mang `id` mới.
   - Kết quả là bảng `permission` chứa đồng thời 2 bản ghi cho mỗi quyền này, dẫn đến việc UI hiển thị trùng lặp.

### Giải pháp kiến nghị lâu dài (Khi được User duyệt)
1. Thêm một migration để dọn dẹp các dòng trùng lặp trong bảng `permission`.
2. Tạo unique index trên cột `resource_name` của bảng `permission` để ngăn ngừa việc duplicate dữ liệu xảy ra ở tầng database.

---

## 2. Kết quả nâng cấp UX Forms (Create/Edit)

Chúng tôi đã tiến hành thay thế các hộp thoại thông báo mặc định của trình duyệt (`alert()`) bằng cơ chế hiển thị Toast (`showToast()`) đẹp mắt và chuyên nghiệp hơn, đồng thời bổ sung độ trễ 1 giây (`setTimeout`) trước khi thực hiện chuyển hướng trang để người dùng kịp quan sát trạng thái thành công.

### Các tệp tin đã chỉnh sửa
1. **Create View**: `HRM_Leave_Management/Web.Backend/Views/Role/CreateRoleView.cshtml`
   - Thay thế `alert("Please enter a group name.")` thành `showToast("Please enter a group name.", false)`
   - Thay thế alert thành công và chuyển trang trực tiếp:
     ```javascript
     success: function (res){
         showToast("Permission created successfully", true);
         setTimeout(function () {
             location.href = '/Role';
         }, 1000);
     }
     ```
   - Thay thế alert lỗi thành `showToast("An error occurred while saving", false)`.

2. **Update/Detail View**: `HRM_Leave_Management/Web.Backend/Views/Role/Detail.cshtml`
   - Thay thế `alert("Please enter a group name.")` thành `showToast("Please enter a group name.", false)`
   - Thay thế alert thành công và chuyển hướng:
     ```javascript
     success: function (res){
         showToast("Permission updated successfully", true);
         setTimeout(function () {
             location.href = '/Role';
         }, 1000);
     }
     ```
   - Thay thế alert lỗi thành `showToast("An error occurred during the editing process", false)`.

---

## 3. Hướng dẫn UAT thủ công (Manual UAT)

Tuân thủ nghiêm ngặt **Keycloak/Auth/UAT Rules**, chúng tôi cung cấp hướng dẫn kiểm thử thủ công dưới đây để Người dùng tự thực hiện kiểm chứng trên môi trường runtime:

### Điều kiện chuẩn bị (Prerequisites)
- Docker container `keycloak-hrm` đang chạy.
- Ứng dụng chạy ở chế độ Keycloak thật (`UseMockAuth` trong cấu hình ứng dụng là `false`).
- Tài khoản đăng nhập: `admin` / mật khẩu `Admin@123456`.

### Kịch bản UAT 1: Tạo mới Group Permission (Create Flow)
1. **Đường dẫn truy cập:** Mở trình duyệt và truy cập vào `/Role/Create` (hoặc nhấn nút "Add Group Permission" trên trang danh sách).
2. **Kiểm tra Validation:**
   - Để trống trường **Group Name**.
   - Nhấn nút **Save**.
   - **Kết quả mong đợi:** Xuất hiện thông báo toast màu đỏ "Please enter a group name." ở góc phải. Trang không bị reload.
3. **Kiểm tra Tạo thành công:**
   - Nhập tên group bất kỳ (ví dụ: `TEST_ROLE_01`).
   - Chọn một vài quyền trong bảng danh sách permissions bên dưới.
   - Nhấn nút **Save**.
   - **Kết quả mong đợi:** Xuất hiện thông báo toast màu xanh lá cây "Permission created successfully" ở góc trên bên phải. Sau 1 giây, trang tự động chuyển hướng về `/Role` và danh sách hiển thị Group Permission mới.

### Kịch bản UAT 2: Cập nhật Group Permission (Update Flow)
1. **Đường dẫn truy cập:** Từ trang danh sách `/Role`, click vào Group Permission vừa tạo để truy cập vào trang chi tiết `/Role/Detail/{id}`.
2. **Kiểm tra Chỉnh sửa:**
   - Thay đổi tên Group Name hoặc chọn thêm/bỏ bớt các quyền.
   - Nhấn nút **Save**.
   - **Kết quả mong đợi:** Xuất hiện thông báo toast màu xanh lá cây "Permission updated successfully". Sau 1 giây, trang tự động chuyển hướng về danh sách `/Role`.
