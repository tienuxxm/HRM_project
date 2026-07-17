# HRM Phase 3D Plan — Work Calendar & Leave Rules

## 1. Scope
Tài liệu này xác định các đặc tả nghiệp vụ, kiến trúc phần mềm, cấu trúc dữ liệu và kế hoạch kiểm thử chấp nhận người dùng (UAT) cho phân hệ **Work Calendar & Leave Rules (Phase 3D)** thuộc hệ thống quản lý nghỉ phép HRM (`HRM_Leave_Management`). Mục tiêu chính là tích hợp lịch làm việc tùy biến (Work Calendar) vào quy trình tính thời lượng đơn nghỉ phép và tự động cập nhật/tính toán lại số dư khi lịch làm việc thay đổi.

---

## 2. Business Decisions (BD-A to BD-E)

### BD-A: Saturday & Sunday Default Rules
*   **Chủ nhật (Sundays)**: Mặc định luôn là ngày nghỉ không làm việc (Non-working day).
*   **Thứ bảy (Saturdays)**: Mặc định là ngày nghỉ không làm việc. Thứ bảy chỉ được tính là ngày làm việc nếu có cấu hình ghi đè cụ thể trong cơ sở dữ liệu thông qua loại ngày `WorkingSaturdayOverride`.
*   **Chặn đơn trống**: Đơn xin nghỉ phép chỉ chứa các ngày nghỉ không làm việc sẽ bị hệ thống chặn ngay từ bước tạo đơn và báo lỗi `LeaveRequest.OnlyNonWorkingDays`.

### BD-B: Excel Import Mode
*   **Cơ chế nhập liệu**: Việc nhập lịch làm việc từ Excel sử dụng cơ chế **Incremental Update** (cập nhật tăng dần). Lô nhập chỉ thêm mới hoặc cập nhật các ngày được liệt kê rõ ràng trong tệp Excel.
*   **Xử lý ngày không có trong Excel**: Các ngày đã có cấu hình lịch làm việc trong DB nhưng không xuất hiện trong file Excel mới nhập sẽ được **giữ nguyên trạng thái hoạt động**, không bị xóa bỏ hay vô hiệu hóa.
*   **Cột IsActive**: Để vô hiệu hóa một ngày cấu hình cũ, tệp Excel phải chứa ngày đó và đặt giá trị cột `IsActive` thành `FALSE` (hoặc thực hiện vô hiệu hóa thủ công trên giao diện quản trị).

### BD-C: Calendar Change Affecting Leave Requests
Khi có sự thay đổi lịch làm việc (ví dụ: ngày nghỉ lễ được cập nhật thành ngày làm việc bình thường hoặc ngược lại) ảnh hưởng đến các đơn nghỉ phép hiện tại:
1.  **Đối với đơn nghỉ phép ở trạng thái Pending**:
    *   Hệ thống tự động tính toán lại thời lượng đơn phép theo lịch mới.
    *   Trạng thái đơn giữ nguyên là `Pending`.
    *   Nếu thời lượng mới vượt quá số dư khả dụng của nhân viên, hệ thống sẽ chặn và tạo cảnh báo lỗi `NeedsEmployeeRevision` lưu trong bảng Audit để hiển thị trên giao diện của nhân viên, yêu cầu họ cập nhật lại đơn.
2.  **Đối với đơn nghỉ phép ở trạng thái Approved**:
    *   **Áp dụng lịch làm việc mới**: Hệ thống áp dụng thành công cập nhật lịch.
    *   **Hoàn lại số dư phép cũ**: `LeaveBalance.UsedDays` của nhân viên được hoàn lại (giảm đi) một lượng đúng bằng thời lượng đã duyệt cũ (`OldDuration`).
    *   **Tính toán thời lượng mới**: Tính lại thời lượng mới của đơn phép dựa trên lịch vừa cập nhật.
    *   **Chuyển trạng thái đơn**: Đơn nghỉ phép bị ảnh hưởng được đưa trực tiếp từ trạng thái `Approved` trở về **`Pending`** để nhân viên có thể sửa đổi và gửi duyệt lại.
    *   **Xóa thông tin phê duyệt hiện tại**: Các trường xử lý phê duyệt trên bản ghi đơn nghỉ phép hiện tại bị xóa bỏ: `ProcessedBy = null`, `ProcessedAt = null`.
    *   **Ghi nhận nhật ký Recalculation**: Toàn bộ thông tin phê duyệt cũ và các chỉ số thời lượng cũ/mới được lưu trữ đầy đủ vào thực thể nhật ký tính toán lại (`LeaveRequestRecalculationAudit`).
    *   **Chặn số dư âm khả dụng**: Quá trình tính toán lại không được phép làm âm số dư phép khả dụng thực tế của nhân viên.
3.  **Quy tắc Comment**: Hệ thống tuyệt đối không tự ý chèn hay nối thêm chuỗi cảnh báo/thông báo tính toán lại vào trường bình luận của đơn nghỉ phép (`LeaveRequest.Comment`). Giao diện người dùng sẽ lấy lý do tính toán lại trực tiếp từ thực thể Audit log hoặc DTO nghiệp vụ.

### BD-D: Past/Future Calendar Control
*   **Ngày trong tương lai**: Cho phép chỉnh sửa tự do bởi người quản trị có quyền `UPDATE_WORK_CALENDAR`.
*   **Ngày trong quá khứ**: Việc chỉnh sửa các ngày có ngày cấu hình nhỏ hơn ngày hiện tại (`Date < Today`) bị hạn chế nghiêm ngặt:
    *   Chỉ được phép thực hiện khi cấu hình hệ thống `HRM.Calendar.AllowPastEditing` được bật (`true`) và người dùng có quyền đặc trị `UPDATE_PAST_WORK_CALENDAR`.
    *   Nếu cố tình sửa đổi khi không đủ điều kiện, hệ thống trả về lỗi `WorkCalendar.PastEditingNotAllowed`.

### BD-E: Draft Preview Storage
*   Dữ liệu tải lên từ Excel trước khi được áp dụng chính thức phải được lưu trữ tạm thời tại bảng `calendar_import_batch_row` dưới trạng thái Draft của một Import Batch (`CalendarImportBatch`).
*   Cho phép người dùng xem trước danh sách dòng hợp lệ, dòng cập nhật, dòng vô hiệu hóa và các dòng lỗi (Error) trực tiếp trên giao diện trước khi bấm nút xác nhận áp dụng chính thức (Confirm/Apply).

---

## 3. Database Schema changes

### Bảng `work_calendar_day` (Lịch làm việc chi tiết)
*   `id`: GUID (Primary Key)
*   `date`: DateOnly (Unique Constraint, Index)
*   `day_type`: Integer (CalendarDayType: PublicHoliday = 1, CompanyCustomNonWorkingDay = 2, WorkingSaturdayOverride = 3, StandardWorkingDayOverride = 4)
*   `work_shift`: Integer (WorkShiftType: None = 0, FullDay = 1, MorningOnly = 2, AfternoonOnly = 3)
*   `description`: NVARCHAR(500)
*   `is_active`: Boolean
*   `created_by`: GUID
*   `created_at`: DateTime

### Bảng `leave_request_recalculation_audit` (Nhật ký tính toán lại đơn phép)
*   `id`: GUID (Primary Key)
*   `leave_request_id`: GUID (Foreign Key to `leave_request`)
*   `old_duration`: Decimal
*   `new_duration`: Decimal
*   `delta`: Decimal
*   `old_used_days`: Decimal
*   `new_used_days`: Decimal
*   `old_status`: NVARCHAR(50)
*   `new_status`: NVARCHAR(50)
*   `old_processed_by`: GUID (Nullable)
*   `old_processed_at`: DateTime (Nullable)
*   `calendar_change_source`: NVARCHAR(250) (e.g., Import Batch ID hoặc Tên file Excel)
*   `changed_by`: GUID
*   `changed_at`: DateTime

---

## 4. Test Cases (UAT Manual Scenarios)

### TC-01: Đơn nghỉ phép đi qua ngày Chủ nhật
*   **Điều kiện**: Nhân viên đăng ký nghỉ từ Thứ sáu đến Thứ hai tuần sau. Thứ bảy đã được cấu hình nghỉ mặc định.
*   **Hành động**: Nhân viên gửi đơn phép.
*   **Kết quả**: Thời lượng đơn phép tính toán được hiển thị là 2 ngày (không tính Thứ bảy và Chủ nhật).

### TC-02: Đơn nghỉ phép chỉ chứa ngày nghỉ
*   **Điều kiện**: Đơn đăng ký chỉ gồm Thứ bảy và Chủ nhật (không có cấu hình làm việc bù).
*   **Hành động**: Nhân viên thực hiện nhấn gửi đơn nghỉ phép.
*   **Kết quả**: Hệ thống chặn và hiển thị thông báo lỗi `LeaveRequest.OnlyNonWorkingDays`.

### TC-03: Đơn nghỉ phép đi qua ngày nghỉ lễ (Public Holiday)
*   **Điều kiện**: Thứ hai là ngày nghỉ lễ Quốc khánh đã được cấu hình trong hệ thống. Nhân viên đăng ký nghỉ từ Thứ sáu đến Thứ ba tuần sau.
*   **Hành động**: Gửi đơn nghỉ phép.
*   **Kết quả**: Thời lượng tính toán được là 3 ngày (Thứ sáu, Thứ ba và Thứ bảy/Chủ nhật/Thứ hai bị loại trừ).

### TC-04: Đơn nghỉ nửa ngày trên ngày làm việc nửa ngày
*   **Điều kiện**: Ngày cấu hình có `WorkShift` là `MorningOnly` (Chỉ làm việc buổi sáng).
*   **Hành động**: Nhân viên đăng ký nghỉ buổi Sáng (`Morning` shift).
*   **Kết quả**: Đơn được gửi thành công, thời lượng tính toán là 0.5 ngày.

### TC-05: Đăng ký sai ca trên ngày làm việc nửa ngày
*   **Điều kiện**: Ngày cấu hình có `WorkShift` là `MorningOnly`.
*   **Hành động**: Nhân viên cố gắng đăng ký nghỉ buổi Chiều (`Afternoon` shift).
*   **Kết quả**: Hệ thống chặn hành động với lỗi validation: `LeaveRequest.InvalidShiftRegistration`.

### TC-06: Chặn chỉnh sửa ngày trong quá khứ khi không có quyền
*   **Điều kiện**: Tài khoản quản trị viên không có quyền `UPDATE_PAST_WORK_CALENDAR` hoặc cấu hình `HRM.Calendar.AllowPastEditing` đang tắt.
*   **Hành động**: Admin cố gắng thay đổi lịch làm việc của một ngày trong tuần trước.
*   **Kết quả**: Hành động bị chặn và hệ thống trả về lỗi `WorkCalendar.PastEditingNotAllowed`.

### TC-07: Preview Import Excel
*   **Điều kiện**: File Excel import chứa 5 ngày hợp lệ, 2 ngày cập nhật, 1 ngày vô hiệu hóa và 1 ngày có định dạng ngày sai (lỗi).
*   **Hành động**: Upload file lên hệ thống.
*   **Kết quả**: Giao diện hiển thị đúng 8 dòng hợp lệ ở trạng thái sẵn sàng và 1 dòng lỗi được bôi đỏ kèm thông báo lỗi cụ thể. Nút Apply bị vô hiệu hóa.

### TC-08: Cập nhật lịch làm việc ảnh hưởng đến đơn phép Approved (Trường hợp Reopen)
*   **Điều kiện**: Nhân viên A có đơn phép đã được phê duyệt (`Approved`) thời lượng 3 ngày. Sau đó, quản trị viên cập nhật lịch làm việc khiến một ngày trong đơn trở thành ngày làm việc bắt buộc.
*   **Hành động**: Hệ thống tự động thực hiện tính toán lại sau khi admin lưu lịch.
*   **Kết quả**:
    *   Trạng thái đơn phép của Nhân viên A chuyển từ `Approved` sang `Pending`.
    *   Số dư `UsedDays` trong `LeaveBalance` được hoàn trả lại đúng bằng thời lượng đã duyệt cũ (3 ngày).
    *   Thời lượng đơn phép được tính toán lại theo lịch mới.
    *   Trường `ProcessedBy` và `ProcessedAt` trên đơn nghỉ hiện tại được xóa về `null`.
    *   Thông tin phê duyệt cũ được ghi nhận đầy đủ vào bảng `leave_request_recalculation_audit`.
    *   Trường bình luận `LeaveRequest.Comment` ban đầu của đơn nghỉ phép không bị thay đổi hay chèn thêm thông báo hệ thống.