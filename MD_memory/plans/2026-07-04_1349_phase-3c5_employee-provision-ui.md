# Kế hoạch triển khai: Giao diện cấp tài khoản nhân viên (Employee Account Provisioning UI)

**Mã Phase:** Phase 3C.5 — Employee Account Provisioning UI (List Integration Only)  
**Ngày tạo:** 2026-07-04  
**Cập nhật:** 2026-07-04 13:49  
**Trạng thái:** DRAFT — Chờ phê duyệt  

---

## 1. Phạm vi công việc (Scope of Work)

### 1.1 Mục tiêu hiện tại (In-Scope)
Tập trung xây dựng giao diện tích hợp trực tiếp việc cấp tài khoản (account provisioning) cho nhân viên từ trang danh sách nhân viên (`Employee/Index.cshtml`):
- Thêm cột **Tài khoản** vào bảng danh sách nhân viên.
- Hiển thị trạng thái tài khoản:
  - Nếu đã liên kết tài khoản (`UserId != null`): hiển thị badge **Đã có** màu xanh dương.
  - Nếu chưa liên kết tài khoản (`UserId == null`): hiển thị nút **Cấp tài khoản** màu vàng/amber.
- Tích hợp Modal cấp tài khoản (`_ProvisionAccountPartial.cshtml`) cho mỗi dòng nhân viên chưa có tài khoản:
  - Các trường thông tin: Username (bắt buộc, max 50 ký tự), Email (bắt buộc, email hợp lệ, max 150 ký tự), Mật khẩu (bắt buộc, tối thiểu 6 ký tự - đồng bộ với backend), Danh sách vai trò (chọn nhiều - multiple select).
  - Validation client-side sử dụng tiếng Việt có dấu.
  - AJAX submit lên API `POST /employee/provision-account`.
  - Hiển thị thông báo thành công/thất bại bằng Toast hệ thống (`showToast`) và hộp thoại lỗi trong modal (không dùng `window.alert` / `window.confirm`).

### 1.2 Các mục chưa làm (Out-of-Scope cho sub-phase này)
Để đảm bảo chia nhỏ công việc và kiểm soát rủi ro, các mục sau thuộc Phase 3C.5 gốc nhưng **không thực hiện** trong đợt này:
- **Checkbox "Tạo tài khoản hệ thống"** trong modal tạo mới nhân viên (`_CreateEmployeePartial.cshtml`).
- **Nút/Dropdown "Liên kết User đã có"** (Link Existing User) cho nhân viên.
- **Trạng thái Active/Disabled** của tài khoản và nút kích hoạt/vô hiệu hóa tài khoản từ trang danh sách nhân viên.

---

## 2. Ràng buộc kiến trúc (Architectural Boundaries)

Tuân thủ nghiêm ngặt mô hình Clean Architecture của dự án:
- `Web.Backend -> Application -> Domain`
- `Infrastructure -> Application/Domain`

Đảm bảo:
- Controller `EmployeeController` chỉ đóng vai trò nhận request, gửi command qua MediatR (`ISender`) và trả về kết quả/View. Không tự xử lý logic nghiệp vụ hay truy cập trực tiếp DB Context.
- Không thay đổi Keycloak/auth configuration trong dự án.
- Không chỉnh sửa code của các layer Domain và Application đã hoàn thiện từ Phase 3C.2.

---

## 3. Thiết kế & Đồng bộ hóa UI/UX

### 3.1 Đồng bộ Validation Mật khẩu
- Backend validator (`ProvisionEmployeeAccountCommandValidator.cs`) quy định mật khẩu có độ dài tối thiểu là **6 ký tự** (`MinimumLength(6)`).
- Giao diện (HTML placeholder, helper text và Javascript validation) phải đồng bộ:
  - Placeholder mật khẩu: `Tối thiểu 6 ký tự`.
  - JS Check: `password.length < 6` -> báo lỗi: `Mật khẩu phải có ít nhất 6 ký tự.`

### 3.2 Giao diện tiếng Việt có dấu
- Toàn bộ nhãn (labels), text gợi ý, lỗi validation trên giao diện và nội dung Toast thông báo thành công/thất bại phải sử dụng **tiếng Việt có dấu** rõ ràng, chuyên nghiệp.
- File phải được lưu dưới dạng **UTF-8 với BOM** để tránh hiển thị sai ký tự tiếng Việt (mojibake).

### 3.3 Flowbite Modal Pattern
- Sử dụng đúng cấu trúc CSS và các thuộc tính dữ liệu (`data-modal-target`, `data-modal-toggle`, `data-modal-hide`) tương tự modal `Confirm Delete` và `Update Employee` hiện có để đảm bảo tính đồng bộ với thư viện Flowbite.
- Mỗi dòng nhân viên có một ID modal riêng biệt: `provisionAccountModal-@emp.val.Id.Value`.

### 3.4 Phản hồi & Ánh xạ Lỗi (Error Mapping)
- Phản hồi thành công: Gọi hàm `showToast('Cấp tài khoản nhân viên thành công!')` và tự động tải lại trang sau 1.5 giây.
- Phản hồi lỗi: Đọc chi tiết lỗi trả về từ backend:
  - Ưu tiên parse và đọc trường `description` hoặc `name` từ JSON response của API.
  - Hiển thị thông báo lỗi vào vùng `provisionError-@Model.Id.Value` phía đầu modal và hiển thị Toast màu đỏ qua `showToast(errorMsg, false)`.

---

## 4. Kế hoạch xác minh (Verification Plan)

Sau khi hoàn tất cập nhật code, thực hiện các bước xác minh bắt buộc sau:
1. **Kiểm tra Build:**
   ```powershell
   dotnet build HRM_Leave_Management/Web.Backend/Web.Backend.csproj --no-restore
   ```
2. **Kiểm tra BOM:**
   Quét và đảm bảo các file plan, report và views vừa sửa có UTF-8 BOM.
3. **Quét mã nguồn:**
   Đảm bảo không có bất kỳ lệnh `window.alert` hay `window.confirm` nào được sử dụng trong các views được chỉnh sửa.
4. **Không UAT tự động:**
   Không dùng browser subagent tự động, không thực hiện các hành động trực tiếp thay đổi tài khoản Keycloak hay dữ liệu DB nếu chưa có xác nhận từ người dùng.
