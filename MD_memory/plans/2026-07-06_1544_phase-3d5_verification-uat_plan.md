# HRM Phase 3D.5 Plan — Verification & UAT Plan

## 1. Scope
Tài liệu này định nghĩa phương pháp kiểm thử, các bước thực hiện UAT thủ công và quy tắc xác minh nghiệp vụ cho phân hệ Lịch làm việc & Quy tắc nghỉ phép (Phase 3D).

---

## 2. UAT Execution Rules & Environment

### A. Quy tắc UAT thủ công
*   Theo quy định của dự án, **không tự động sử dụng browser/subagent để UAT** trừ khi có chỉ thị trực tiếp từ người dùng.
*   Cung cấp kịch bản kiểm thử từng bước (step-by-step) chi tiết để người dùng tự thực hiện kiểm tra và phản hồi.

### B. Cấu hình môi trường UAT
*   **Chế độ xác thực**: Sử dụng Keycloak thật (`UseMockAuth = false`).
*   **Địa chỉ Keycloak**: `http://localhost:8080`
*   **Realm**: `hrm`
*   **Client**: `hrm-web`
*   **Tài khoản UAT**:
    *   Username: `admin` hoặc `admin@hrm.local`
    *   Password: `Admin@123456`

### C. Cơ chế phân quyền (Permissions Setup)
Trước khi chạy thử nghiệm, các quyền sau cần được đảm bảo khả dụng:
*   Các quyền cần thiết bao gồm:
    *   Quyền quản lý lịch làm việc: `VIEW_WORK_CALENDAR`, `UPDATE_WORK_CALENDAR`.
    *   Quyền thao tác đơn nghỉ phép: `VIEW_LEAVE_REQUEST`, `CREATE_LEAVE_REQUEST`, `APPROVE_LEAVE_REQUEST`.
*   **Quy tắc an toàn phân quyền**: Quyền hạn phải được thiết lập sẵn sàng thông qua cơ chế khởi tạo quyền lũy đẳng được phê duyệt (idempotent permission provisioning path), luồng quản lý phân quyền tích hợp của EF Core Migration, hoặc qua giao diện quản trị phân quyền của ứng dụng. Tuyệt đối không thực hiện chèn dữ liệu trực tiếp thủ công bằng lệnh SQL vào cơ sở dữ liệu runtime trừ khi có sự phê duyệt rõ ràng từ người dùng.

---

## 3. Detailed UAT Test Cases

### TC-01: Đơn nghỉ phép đi qua ngày Chủ nhật
1.  Đăng nhập bằng tài khoản Employee.
2.  Tạo đơn nghỉ phép mới từ Thứ sáu đến Thứ hai tuần sau.
3.  **Xác minh**: Thời lượng đơn phép hiển thị trên màn hình tạo đơn phải bằng **2.0 ngày** (loại trừ Thứ bảy và Chủ nhật là ngày nghỉ mặc định).

### TC-02: Đơn nghỉ phép chỉ chứa ngày nghỉ
1.  Đăng nhập bằng tài khoản Employee.
2.  Tạo đơn nghỉ phép chỉ gồm 2 ngày Thứ bảy và Chủ nhật.
3.  Nhấn nút gửi đơn.
4.  **Xác minh**: Hệ thống chặn hành động và hiển thị cảnh báo lỗi: `LeaveRequest.OnlyNonWorkingDays`.

### TC-03: Đơn nghỉ phép đi qua ngày nghỉ lễ công ty
1.  Đăng nhập bằng tài khoản Admin. Thêm cấu hình ngày Thứ hai tuần sau là ngày nghỉ lễ (`PublicHoliday`).
2.  Đăng nhập bằng tài khoản Employee. Tạo đơn nghỉ phép từ Thứ sáu đến Thứ ba tuần sau.
3.  **Xác minh**: Thời lượng đơn phép tính toán được phải là **2.0 ngày** (Trừ Thứ bảy, Chủ nhật và ngày Thứ hai nghỉ lễ).

### TC-04: Đăng ký nghỉ nửa ngày trên ngày làm việc nửa ngày
1.  Đăng nhập bằng tài khoản Admin. Cấu hình ngày Thứ ba tuần sau là ngày làm việc nửa ngày sáng (`MorningOnly`).
2.  Đăng nhập bằng tài khoản Employee. Tạo đơn nghỉ phép chọn ngày nghỉ là Thứ ba tuần sau, ca nghỉ là buổi sáng (`Morning`).
3.  **Xác minh**: Đơn phép gửi thành công với thời lượng ghi nhận là **0.5 ngày**.

### TC-05: Đăng ký sai ca trên ngày làm việc nửa ngày
1.  Cấu hình ngày Thứ tư tuần sau là ngày làm việc nửa ngày sáng (`MorningOnly`).
2.  Nhân viên thực hiện tạo đơn nghỉ phép chọn ngày nghỉ là Thứ tư tuần sau nhưng chọn ca nghỉ là buổi chiều (`Afternoon`).
3.  **Xác minh**: Hệ thống chặn hành động gửi đơn và trả về thông báo lỗi: `LeaveRequest.InvalidShiftRegistration`.

### TC-06: Chặn sửa lịch trong quá khứ khi không đủ quyền
1.  Đăng nhập bằng tài khoản Admin không được gán quyền `UPDATE_PAST_WORK_CALENDAR`.
2.  Cố gắng chỉnh sửa cấu hình lịch làm việc của một ngày trong quá khứ (ví dụ: ngày hôm qua).
3.  **Xác minh**: Hệ thống chặn lưu cấu hình và hiển thị lỗi `WorkCalendar.PastEditingNotAllowed`.

### TC-07: Preview Import Excel
1.  Đăng nhập bằng tài khoản Admin. Truy cập trang `/work-calendar` và chọn tải lên file Excel cấu hình lịch.
2.  File Excel tải lên chứa một dòng dữ liệu có định dạng ngày sai.
3.  **Xác minh**: Trang chuyển hướng sang `/work-calendar/preview/{batchId}`. Dòng lỗi được tô màu đỏ nổi bật kèm cột báo lỗi chi tiết. Nút *Apply* bị vô hiệu hóa (disabled).

### TC-08: Cập nhật lịch làm việc ảnh hưởng đến đơn phép Approved (Reopen)
1.  Nhân viên tạo đơn xin nghỉ phép thời lượng 3 ngày (Thứ hai, Thứ ba, Thứ tư). Đơn phép đã được Admin phê duyệt (`Approved`).
2.  Admin thực hiện cập nhật lịch làm việc khiến ngày Thứ ba trở thành ngày làm việc bắt buộc.
3.  Hệ thống chạy recalculation.
4.  **Xác minh**:
    *   Trạng thái đơn nghỉ phép chuyển về `Pending`.
    *   Số dư `UsedDays` trong `LeaveBalance` được giảm đi 3 ngày (hoàn trả phép cũ).
    *   Các trường phê duyệt cũ trên bản ghi đơn nghỉ phép (`ProcessedBy`, `ProcessedAt`) được đặt về `null`.
    *   Chi tiết đơn nghỉ hiển thị một banner cảnh báo màu vàng lấy thông tin trực tiếp từ thực thể recalculation audit.
    *   Trường bình luận ban đầu của đơn nghỉ phép (`LeaveRequest.Comment`) không bị thay đổi.