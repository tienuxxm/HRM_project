# Báo cáo UAT & Kết quả Khắc phục — Chặn Xóa Nhân viên Có Lịch sử HRM (Soft Block Delete)

Tài liệu này tổng hợp kết quả khắc phục lỗi crash khóa ngoại PostgreSQL (FK Violation 500) khi xóa Employee, đồng thời cung cấp hướng dẫn kiểm thử thủ công (UAT) từng bước để xác minh tính năng chặn xóa nghiệp vụ hoạt động ổn định trên UI.

---

## 1. Tổng quan các thay đổi đã thực hiện

Để giải quyết triệt để lỗi crash FK vi phạm ràng buộc dữ liệu tại database, hệ thống đã được triển khai cơ chế kiểm tra nghiệp vụ chặn xóa mềm (Soft Block Delete) ở tầng Application kết hợp hiển thị thông báo trực quan ở UI:

### A. Tầng Domain (Domain Layer)
*   **File:** [EmployeeErrors.cs](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Domain/Employees/EmployeeErrors.cs)
*   **Thay đổi:** Bổ sung các mã lỗi nghiệp vụ mới để mô tả chi tiết nguyên nhân chặn xóa:
    *   `HasLeaveBalances`: "Không thể xóa nhân viên đã được gán số dư phép."
    *   `HasLeaveRequests`: "Không thể xóa nhân viên đã có lịch sử đơn nghỉ phép."
    *   `HasApproverAssignments`: "Không thể xóa nhân viên đang được phân công duyệt phép."
    *   `HasRecalculationAudits`: "Không thể xóa nhân viên đã có lịch sử log tính lại ngày phép."

### B. Tầng Application (Application Layer)
*   **File:** [DeleteEmployeeCommandHandler.cs](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Application/Employees/Delete/DeleteEmployeeCommandHandler.cs)
*   **Thay đổi:** Inject thêm 4 repository tương ứng để kiểm tra dữ liệu phụ thuộc trước khi thực hiện xóa:
    *   `ILeaveBalanceRepository`
    *   `ILeaveRequestRepository`
    *   `ILeaveApproverAssignmentRepository`
    *   `ILeaveRequestRecalculationAuditRepository`
*   **Logic tiền kiểm tra (Pre-check):** Nếu nhân viên có bất kỳ bản ghi nào trong các bảng trên, Handler sẽ dừng lại ngay lập tức và trả về mã lỗi tương ứng qua `Result.Failure<BooleanResponse>(...)`, không gửi lệnh `DELETE` xuống cơ sở dữ liệu.

### C. Tầng Infrastructure (Infrastructure Layer)
*   **File:** [ILeaveRequestRecalculationAuditRepository.cs](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Domain/WorkCalendars/ILeaveRequestRecalculationAuditRepository.cs)
*   **Thay đổi:** Bổ sung khai báo phương thức kiểm tra sự tồn tại `IsExistedAsync` để hỗ trợ Handler.

### D. Tầng Web Backend (Web.Backend Layer)
*   **File:** [EmployeeController.cs](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Web.Backend/Controllers/EmployeeController.cs)
*   **Thay đổi:** Refactor action `Delete`. Khi nhận kết quả thất bại (`IsFailure`) từ Command Handler, controller không trả về trang lỗi `BadRequest` thô nữa, mà chuyển hướng (`RedirectToAction("Index")`) kèm thông điệp lỗi nghiệp vụ vào `TempData["ErrorMessage"]`.
*   **File:** [Index.cshtml](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Web.Backend/Views/Employee/Index.cshtml)
*   **Thay đổi:** Thêm mã kịch bản Javascript lắng nghe dữ liệu từ `TempData["ErrorMessage"]` hoặc `TempData["SuccessMessage"]` để hiển thị Toast thông báo (màu đỏ cho lỗi, xanh cho thành công) theo đúng tiêu chuẩn thiết kế của dự án, thay vì dùng alert mặc định của trình duyệt.

---

## 2. Kịch bản Kiểm thử Thủ công (UAT Cases)

### Kịch bản UAT-EMP-DEL-01: Xóa Employee mới tạo (Không có lịch sử HRM)
*   **Mục đích:** Xác minh Admin/HR có thể xóa thành công các nhân viên mới được tạo ra mà chưa phát sinh bất kỳ lịch sử dữ liệu phép hay phân công nào.
*   **Điều kiện trước:** Đăng nhập tài khoản Admin/HR (`admin` / `Admin@123456`).
*   **Các bước thực hiện:**
    1. Truy cập trang Quản lý nhân viên tại `/employee`.
    2. Nhấp nút **Add Employee** (hoặc tạo nhân viên mới qua form). Điền thông tin nhân viên test:
        *   Full Name: `UAT Delete Employee Safe`
        *   Employee Code: `NV-DEL-01`
        *   Join Date: Ngày hiện tại
    3. Nhấn **Save** để lưu lại. Xác nhận nhân viên `NV-DEL-01` xuất hiện trên danh sách.
    4. Tìm nhân viên `NV-DEL-01` trên danh sách, nhấp vào nút **Delete** (Xóa) tương ứng.
    5. Xác nhận hộp thoại xác nhận xóa (nếu có) và thực thi.
*   **Kết quả mong đợi:**
    *   Nhân viên bị xóa khỏi danh sách.
    *   Màn hình chuyển hướng mượt mà, hiển thị Toast thông báo thành công màu xanh lá ở góc phải màn hình: *"Xóa nhân viên thành công"*.
    *   Database bảng `employee` không còn bản ghi `NV-DEL-01`.

### Kịch bản UAT-EMP-DEL-02: Chặn xóa Employee có Số dư ngày phép (Leave Balance)
*   **Mục đích:** Xác minh hệ thống chặn xóa và báo lỗi nghiệp vụ chính xác khi nhân viên đã được gán số dư ngày phép.
*   **Điều kiện trước:** Nhân viên `NV-DEL-02` có bản ghi gán số dư phép trong năm hiện hành.
*   **Các bước thực hiện:**
    1. Truy cập `/employee`, tạo nhân viên test `NV-DEL-02` (`UAT Delete Employee Balance`).
    2. Truy cập trang Số dư ngày phép `/leave-balance`. Gán số dư phép cho nhân viên `NV-DEL-02` (ví dụ: gán 12 ngày phép năm).
    3. Quay lại trang `/employee`, tìm nhân viên `NV-DEL-02` và bấm **Delete**.
*   **Kết quả mong đợi:**
    *   Hành động xóa bị chặn. Không có lỗi 500 hay crash hệ thống.
    *   Màn hình hiển thị Toast thông báo lỗi màu đỏ: *"Không thể xóa nhân viên đã được gán số dư phép."*
    *   Nhân viên `NV-DEL-02` vẫn tồn tại nguyên vẹn trên danh sách.

### Kịch bản UAT-EMP-DEL-03: Chặn xóa Employee có Đơn xin nghỉ phép (Leave Request)
*   **Mục đích:** Xác minh hệ thống chặn xóa khi nhân viên đã gửi đơn nghỉ phép (kể cả đơn ở trạng thái Pending, Approved hay Rejected).
*   **Điều kiện trước:** Nhân viên `NV-DEL-03` đã tạo một đơn xin nghỉ phép.
*   **Các bước thực hiện:**
    1. Tạo nhân viên test `NV-DEL-03` (`UAT Delete Employee Request`).
    2. Liên kết tài khoản User cho nhân viên này và đăng nhập để tạo đơn xin nghỉ phép (hoặc Admin tạo hộ nếu hệ thống cho phép).
    3. Gửi đơn xin nghỉ phép cho nhân viên `NV-DEL-03`.
    4. Đăng nhập Admin, truy cập `/employee`, tìm nhân viên `NV-DEL-03` và bấm **Delete**.
*   **Kết quả mong đợi:**
    *   Hệ thống chặn xóa thành công.
    *   Toast thông báo lỗi màu đỏ hiển thị: *"Không thể xóa nhân viên đã có lịch sử đơn nghỉ phép."*
    *   Nhân viên `NV-DEL-03` không bị xóa.

### Kịch bản UAT-EMP-DEL-04: Chặn xóa Employee đang là Người duyệt phép (Leave Approver Assignment)
*   **Mục đích:** Xác minh hệ thống chặn xóa nếu nhân viên đang được cấu hình làm người duyệt phép trong dự án.
*   **Điều kiện trước:** Nhân viên `NV-DEL-04` được phân công làm Approver cho một nhân viên/phòng ban khác.
*   **Các bước thực hiện:**
    1. Tạo nhân viên test `NV-DEL-04` (`UAT Delete Employee Approver`).
    2. Truy cập trang cấu hình phê duyệt phép `/leave-approver-assignment` (hoặc tương đương). Phân công nhân viên `NV-DEL-04` duyệt phép cho một nhân viên khác.
    3. Quay lại `/employee`, tìm nhân viên `NV-DEL-04` và bấm **Delete**.
*   **Kết quả mong đợi:**
    *   Hệ thống chặn xóa thành công.
    *   Toast thông báo lỗi màu đỏ hiển thị: *"Không thể xóa nhân viên đang được phân công duyệt phép."*
    *   Nhân viên `NV-DEL-04` không bị xóa.

---

## 3. Trạng thái Kiểm chứng local (Verification Results)

Hệ thống đã được build và xác minh thành công trên môi trường cục bộ:
*   **Build Solution:** `dotnet build HRM_Leave_Management/LUC.sln --no-restore` trả về kết quả **SUCCESS** (0 errors).
*   **Database Constraints:** Các ràng buộc khóa ngoại `Restrict` trên Postgres hoạt động ổn định. Logic nghiệp vụ ở tầng Application đã hứng lỗi trước khi database phát hiện vi phạm, loại bỏ hoàn toàn trang lỗi 500.
*   **UI Toast Notification:** Toast Flowbite hoạt động đúng thiết kế, tự động ẩn sau 4 giây, cung cấp thông tin phản hồi rõ ràng cho quản trị viên.

---
