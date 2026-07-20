# Báo cáo Audit & Đề xuất Thiết kế UI/UX Nhóm Quyền (Role / Permission Group)

**Thời gian tạo:** 2026-07-20 11:41
**Người thực hiện:** Senior .NET Fullstack Engineer & Technical Reviewer
**Trạng thái:** Chờ User phê duyệt

---

## 1. Trạng thái Git & Xác nhận Ranh giới Kiến trúc

### Trạng thái Working Tree
Kết quả lệnh `git status --short` và `git branch --show-current`:
* **Nhánh hiện tại:** `main`
* **Danh sách các file đang modified (dirty baseline từ phase trước):**
  * `HRM_Leave_Management/Web.Backend/Views/LeaveApproverAssignment/Index.cshtml`
  * `HRM_Leave_Management/Web.Backend/Views/User/CreateUserView.cshtml`
  * `HRM_Leave_Management/Web.Backend/Views/User/Detail.cshtml`
  * `HRM_Leave_Management/Web.Backend/Views/User/Index.cshtml`

> [!IMPORTANT]
> Toàn bộ các thay đổi trên working tree được giữ nguyên không dùng lệnh reset/clean phá hủy. Không có thay đổi code mới nào được thực hiện trong lượt audit này.

### Ranh giới Kiến trúc bảo toàn
* **Web.Backend -> Application -> Domain**
* **Infrastructure -> Application/Domain**
* Độc lập tầng giao diện, không can thiệp logic nghiệp vụ C#, database, hay cấu hình Keycloak/Auth.

---

## 2. Kết quả Audit kỹ thuật (Role / Permission Group)

Màn hình này chịu trách nhiệm quản lý **Role (Nhóm quyền)** trong hệ thống HRM, kiểm soát quyền truy cập của các tài khoản (ví dụ: `VIEW_USER`, `UPDATE_ROLE`, v.v.). Đây hoàn toàn không phải là màn hình quản lý Employee hay User trực tiếp.

### 2.1. Kiểm tra Contract & Endpoint Hiện tại
* **Định tuyến (Routes) & Controller actions:**
  * Trang danh sách: GET `/role` -> Gọi `Index` trong `RoleController.cs` -> Trả về `GetAllRolePagedResponse`.
  * Trang tạo mới: GET `Role/Create` -> Gọi `CreateRoleView` -> Trả về `ManageRoleViewModel` chứa danh sách tất cả các permission để người dùng gán.
  * Trang chi tiết/chỉnh sửa: GET `role/{id}` -> Gọi `Detail` -> Trả về `ManageRoleViewModel` chứa thông tin nhóm quyền và danh sách permission kèm trạng thái đã được gán.
  * API submit tạo mới: POST `/Role/create` nhận payload `ManageRoleModel`.
  * API submit cập nhật: POST `/Role/Update` nhận payload `ManageRoleModel`.
* **HTML / Model Property Mapping:**
  * Input tên nhóm quyền: `ManageRoleModel.DisplayName`.
  * Checkbox chọn từng quyền riêng lẻ: class `.permissionId`, sử dụng thuộc tính `data-id` để lưu ID của Permission.
  * Checkbox chọn tất cả: `#all`.
  * Button gửi form: `#submit`.

### 2.2. So sánh UI Hiện tại vs Tiêu chuẩn Swiss Style 06 (Màn hình mẫu đã UAT)
* **Trang danh sách (Role Index):**
  * **Hiện trạng:**
    * Sử dụng breadcrumbs kiểu cũ với màu sắc xanh lam, biểu tượng SVG rối rắm.
    * Nút "Create Group" sử dụng màu Indigo bo góc (`rounded`).
    * Bảng chứa trong khối có bóng mờ (`shadow`), bo góc (`rounded-lg`), header table màu xanh nhạt (`bg-[#F4F7FC]`).
    * Cột thao tác chỉnh sửa/xóa dùng icon SVG xanh lá/xanh lam lỗi thời và nút "Delete" gọi component modal có thiết kế bo tròn.
    * Sử dụng component `@Html.Partial("_Pagination")` dạng server-side truyền thống.
    * Chưa hỗ trợ hiển thị dạng single-column responsive trên mobile (scroll ngang).
  * **Đề xuất nâng cấp:**
    * Loại bỏ breadcrumb cũ, thay bằng tiêu đề lớn phong cách **Swiss Style 06**: `h2` kích thước `text-[32px] font-bold uppercase tracking-tight` kèm mô tả nhỏ phía dưới.
    * Đồng bộ bảng giống User Index: loại bỏ hoàn toàn card rounded/shadow, chuyển sang table phẳng viền hairline mảnh màu `#D1D1D1` cả dọc và ngang.
    * Nút "+ Add Group" chuyển sang màu đen phẳng (`bg-black text-white rounded-none uppercase text-[11px] font-bold`).
    * Cột Action loại bỏ SVG, sử dụng text link đơn giản: "Edit" màu đen gạch chân, "Remove" màu đỏ hủy diệt gạch chân.
    * Modal xác nhận xóa chuyển sang dạng Custom Swiss Destructive Modal (0px border radius, header warning màu đỏ hủy diệt `#E62429`, nút Cancel viền đen, nút Confirm nền đỏ phẳng).
    * Hỗ trợ mobile viewport (390x844): ẩn table, render danh sách dưới dạng khối stacked phẳng có viền mảnh `#D1D1D1`, không bị che bởi bottom nav.

* **Trang tạo mới & Chi tiết (Create / Detail):**
  * **Hiện trạng:**
    * Breadcrumb kiểu cũ không tương xứng. Tiêu đề H1 font chữ không nhất quán.
    * Input "Group Name" bo góc (`rounded-lg`), viền xám thô.
    * Bảng Permission sử dụng thanh cuộn cứng có chiều cao giới hạn (`max-height: 300px; overflow-y: scroll`), header màu xanh nhạt dính (`position: sticky`), checkbox gán margin cứng vô căn cứ (`margin-left: 84px`).
    * Các nút bấm "Cancel", "Save" bo góc (`rounded`), màu nền không nhất quán (`bg-wnz-yellow`, v.v.).
  * **Đề xuất nâng cấp:**
    * Chuyển toàn bộ khung bao ngoài thành phẳng (`rounded-none shadow-none border border-[#D1D1D1]`).
    * Đồng bộ thiết kế Input: viền mỏng `#D1D1D1`, phẳng hoàn toàn (`rounded-none`), focus border màu đen.
    * Cải tiến bảng Permission: Căn giữa tự nhiên cho các checkbox cột chọn, viền hairline `#D1D1D1`, màu nền header xám nhạt `#F5F5F5`.
    * Nút hành động: "Cancel" (nền trắng, viền đen, chữ đen, `rounded-none`), "Save / Update" (nền đen, chữ trắng, `rounded-none`).

### 2.3. Technical Debts & Rủi ro Hiện có
1. **Duplicate jQuery Import:** Cả hai file `CreateRoleView.cshtml` và `Detail.cshtml` đều chứa dòng `<script src="~/lib/jquery/dist/jquery.min.js"></script>` ở cuối. Lớp này bị trùng do layout chính `_Layout.cshtml` đã import jQuery. Cần loại bỏ.
2. **Native alert():** Trình duyệt gọi trực tiếp `alert("Permission created successfully")` và `alert("An error occurred...")`. Điều này phá vỡ thẩm mỹ và nguyên tắc UX. Đề xuất chuyển sang dùng hàm `showToast(message, isSuccess)` sử dụng Toastify có sẵn trong `site.js`.
3. **Mã SumoSelect thừa:** Trong file `CreateRoleView.cshtml` có đoạn code khởi tạo SumoSelect cho `#PermissionIds` nhưng thực tế DOM không chứa selector này. Đây là code dư thừa cần dọn dẹp.
4. **Margin checkbox cứng:** Thuộc tính `style="margin-left: 84px;"` ở thẻ checkbox trên dòng bảng permission sẽ gây lệch layout nghiêm trọng trên thiết bị màn hình nhỏ. Cần thay bằng flexbox căn giữa hoặc CSS class `text-center`.

---

## 3. Kế hoạch Triển khai (Micro-Phases)

* **Phase 1: Role Index UI Refactoring**
  * Tái cấu trúc giao diện danh sách Role trên Desktop và Mobile.
  * Thay thế SVG hành động bằng text links Swiss Style.
  * Tích hợp Swiss Style 06 Confirm Delete modal tùy chỉnh tại chỗ.
* **Phase 2: CreateRoleView Form Refactoring**
  * Tái cấu trúc form tạo nhóm quyền, loại bỏ border-radius, thiết kế lại bảng permission.
  * Xóa jQuery import thừa và nâng cấp thông báo từ `alert()` sang `showToast()`.
* **Phase 3: Detail Role Edit Form Refactoring**
  * Áp dụng giao diện Swiss Style cho form cập nhật quyền của nhóm quyền đã có.
  * Loại bỏ jQuery thừa, dọn dẹp hàm JS, chuyển alert sang toast.

---

## 4. Kế hoạch Xác minh & Kịch bản UAT (Verification Proposal)

Sau mỗi phase hoàn tất, bắt buộc thực hiện các bước kiểm tra sau để đảm bảo không lỗi biên dịch và hành vi UI đúng đắn:

### Lệnh kiểm tra kỹ thuật
1. Kiểm tra định dạng và khoảng trắng lạ:
   `git diff --check -- HRM_Leave_Management/Web.Backend/Views/Role/<File_Thay_Đổi>.cshtml`
2. Kiểm tra danh sách file bị tác động:
   `git diff --name-status -- HRM_Leave_Management/Web.Backend/Views/Role/<File_Thay_Đổi>.cshtml`
3. Biên dịch dự án:
   `dotnet build HRM_Leave_Management/Web.Backend/Web.Backend.csproj --no-restore`

### Kịch bản UAT Thủ công (UAT Checklist)
* **Kịch bản TC-01: Kiểm tra Danh sách Role (`/role`)**
  * **Bước thực hiện:** Đăng nhập admin -> Truy cập `/role`.
  * **Kết quả mong đợi:** Bảng phẳng không bo góc hiển thị đúng danh sách quyền, phân trang hoạt động ổn định, không có lỗi JS trong console. Giao diện mobile hiển thị dạng stacked list dọc phẳng.
* **Kịch bản TC-02: Kiểm tra Tạo mới Nhóm quyền (`/Role/Create`)**
  * **Bước thực hiện:** Nhấn "+ Add Group" -> Nhập tên nhóm quyền -> Tích chọn một số permission -> Nhấn "Save".
  * **Kết quả mong đợi:** Dữ liệu được lưu thành công, hệ thống hiển thị thông báo Toastify màu xanh lá cây góc phải, tự động điều hướng về `/role`.
* **Kịch bản TC-03: Kiểm tra Cập nhật Nhóm quyền (`/role/{id}`)**
  * **Bước thực hiện:** Tại danh sách, chọn nhóm quyền vừa tạo -> Nhấn "Edit" -> Thay đổi tên hoặc tích chọn thêm/bớt permission -> Nhấn "Save".
  * **Kết quả mong đợi:** Dữ liệu cập nhật thành công, toast thành công xuất hiện, điều hướng về `/role`.
* **Kịch bản TC-04: Thao tác Chọn tất cả (Select All)**
  * **Bước thực hiện:** Trong form Create/Detail -> Tích chọn checkbox "Select All".
  * **Kết quả mong đợi:** Toàn bộ checkbox bên dưới tự động tích chọn. Bỏ chọn "Select All" thì toàn bộ checkbox bên dưới tự động tắt.
* **Kịch bản TC-05: Kiểm tra Responsive di động (390x844)**
  * **Bước thực hiện:** Dùng chế độ giả lập thiết bị di động trong Chrome DevTools (390x844).
  * **Kết quả mong đợi:** Form và danh sách tự động co giãn vừa màn hình, không lỗi vỡ khung, nút bấm to rõ ràng, không bị thanh điều hướng dưới cùng (bottom navigation) che mất.
