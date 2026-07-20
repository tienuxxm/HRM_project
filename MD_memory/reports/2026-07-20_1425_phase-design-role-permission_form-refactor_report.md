# Báo cáo Kết quả UAT & Xác minh Role / Permission Group Form Refactor (Swiss Style 06)

**Thời gian thực hiện:** 2026-07-20 14:25  
**Người thực hiện:** Senior .NET Fullstack Engineer & Technical Reviewer  
**Môi trường thử nghiệm:** Local Development  
**Trạng thái Xác minh:** **PENDING MANUAL UAT** (Sẵn sàng để người dùng kiểm thử thủ công)

---

## 1. Boundary & Quy tắc Kiểm soát (Architectural Boundaries)
* **Web.Backend -> Application -> Domain** (Giữ nguyên, chỉ sửa giao diện Razor View, không sửa logic nghiệp vụ).
* **Infrastructure -> Application/Domain** (Giữ nguyên, không thay đổi Database, EF Core hay Keycloak Client).
* **Git Status:** Các chỉnh sửa chỉ giới hạn trong phạm vi hai tệp tin giao diện:
  1. `HRM_Leave_Management/Web.Backend/Views/Role/CreateRoleView.cshtml`
  2. `HRM_Leave_Management/Web.Backend/Views/Role/Detail.cshtml`

---

## 2. Chi tiết Cải tiến Giao diện (Swiss Style 06 Form Refactor)
Đã tái cấu trúc toàn diện giao diện biểu mẫu tạo mới và cập nhật Nhóm Quyền dựa trên mẫu thiết kế tối giản của User Profile đã nghiệm thu trước đó:
* **Giao diện tổng thể:** Giao diện phẳng hoàn toàn, sử dụng tông màu chủ đạo xám/đen (`border-[#D1D1D1]`, nền `bg-white`), loại bỏ hoàn toàn bo góc (`rounded-none`) và đổ bóng (`shadow-none`).
* **Header & Điều hướng:** 
  * Tiêu đề in hoa đậm nét `CREATE GROUP PERMISSION` / `UPDATE GROUP PERMISSION` sử dụng kích thước `text-[32px]`.
  * Nút quay lại danh sách `[← BACK TO LIST]` được thiết kế nhỏ gọn (`text-[11px] font-bold uppercase`), đảm bảo không tự động xuống dòng (`whitespace-nowrap`) kể cả trên màn hình di động hẹp.
* **Nhãn & Trường nhập liệu:** 
  * Nhãn (Label) viết hoa hoàn toàn bằng font đơn cách chuyên nghiệp (`font-mono text-[10px] uppercase`).
  * Ô nhập liệu `Group Name` được làm dẹt phẳng, đổi màu đường viền mảnh `#D1D1D1`, khi click chuột vào (focus) sẽ tự đổi sang viền đen phẳng sắc sảo.
* **Danh sách Quyền (Permission Table):**
  * Loại bỏ plugin SumoSelect không cần thiết cho bảng quyền. Thay thế bằng bảng lưới phẳng, tinh gọn, viền đầy đủ ngang dọc mảnh màu xám.
  * Hàng tiêu đề bảng được cố định (`sticky top-0`) khi cuộn và phân biệt rõ ràng.
  * Số thứ tự dạng monospace có tiền tố số 0 (`01`, `02`,...). Các ô checkbox chọn quyền được định dạng thành ô vuông phẳng góc cạnh, đồng bộ với thiết kế Swiss.
* **Hành động (Form Actions):** 
  * Nút "Cancel" viền mảnh đen, nút "Save" nền đen chữ trắng, không có bo góc, chuyển màu nhẹ khi hover chuột.

---

## 3. Kịch bản Hướng dẫn UAT Thủ công (Manual UAT Steps)

Người dùng thực hiện kiểm thử thực tế trên browser theo các bước kịch bản chi tiết dưới đây:

### Kịch bản 1: Kiểm thử màn hình Thêm mới Nhóm Quyền (`/role/create`)
1. **Truy cập:** Mở trình duyệt, đăng nhập với tài khoản Admin và truy cập đường dẫn `http://localhost:5300/role/create` (hoặc nhấn nút "+ ADD GROUP" từ trang danh sách).
2. **Xác minh giao diện (Desktop & Mobile):**
   * Đảm bảo phần tiêu đề và nút quay lại `[← BACK TO LIST]` nằm trên một dòng thẳng thớm, không bị tràn hay xuống dòng.
   * Chuyển chế độ xem di động (ví dụ: iPhone 12 Pro 390x844), kiểm tra giao diện bảng quyền tự động hiển thị thanh cuộn ngang/dọc trong khung tối đa `max-h-[400px]`, không làm đẩy vỡ khung trang web chính.
3. **Kiểm thử Chọn Quyền:**
   * Click chọn checkbox `SELECT ALL` ở tiêu đề bảng, kiểm tra toàn bộ checkbox phía dưới có được tích chọn đồng loạt hay không.
   * Bỏ tích `SELECT ALL`, kiểm tra toàn bộ checkbox con tự động bỏ tích.
   * Tích chọn thủ công vài checkbox con, kiểm tra việc lưu trạng thái hoạt động chính xác.
4. **Kiểm thử Validate dữ liệu:**
   * Để trống ô `Group Name` và nhấn **Save**. Kiểm tra hệ thống hiển thị thông báo yêu cầu nhập tên nhóm quyền (thông qua trình duyệt hoặc logic validate cũ).
5. **Kiểm thử Gửi Thành công:**
   * Điền tên nhóm quyền thử nghiệm (ví dụ: `TEST_ROLE_SWISS`) và chọn một số quyền.
   * Nhấn **Save**. Kiểm tra màn hình tự động điều hướng quay lại trang danh sách `/role` và hiển thị Nhóm quyền mới thêm trong bảng.

### Kịch bản 2: Kiểm thử màn hình Cập nhật Nhóm Quyền (`/role/detail/{id}`)
1. **Truy cập:** Từ trang danh sách `/role`, nhấn nút **EDIT** tại một dòng vai trò bất kỳ (ví dụ: vai trò `TEST_ROLE_SWISS` vừa tạo).
2. **Kiểm tra Dữ liệu cũ:**
   * Tên nhóm quyền cũ phải hiển thị đúng trong ô `Group Name`.
   * Các quyền đã chọn từ trước phải được tự động tích chọn sẵn trong bảng.
3. **Kiểm thử Chỉnh sửa & Lưu:**
   * Thay đổi tên nhóm quyền và tích chọn thêm/bớt một số quyền.
   * Nhấn **Save**. Đảm bảo thông tin cập nhật thành công và quay lại trang `/role` với dữ liệu mới hiển thị chính xác.
4. **Kiểm thử Cancel:**
   * Vào lại trang sửa, thực hiện thay đổi bất kỳ rồi nhấn nút **Cancel** hoặc nút `[← BACK TO LIST]`.
   * Hệ thống phải quay về `/role` mà không lưu lại các thay đổi vừa nhập.

---

## 4. Kết quả Tự động Kiểm tra Mã hóa (Encoding Scan Results)
* **BOM & Mojibake Check:** Báo cáo này đã được định dạng UTF-8 có BOM (`\ufeff`) và quét kiểm tra sạch lỗi hiển thị tiếng Việt, tuân thủ 100% quy tắc kiểm tra nghiêm ngặt của dự án.
