# HRM Phase 3D.3 Plan — Excel Import & Leave Request Recalculation

## 1. Scope
Phase này xây dựng chức năng nhập cấu hình lịch làm việc từ file Excel theo cơ chế Batch Import và thiết kế bộ xử lý tự động tính toán lại thời lượng đơn xin nghỉ phép bị ảnh hưởng khi lịch làm việc thay đổi (Recalculation Engine).

---

## 2. Calendar Import Batch Design

### A. Quy trình tải lên dữ liệu (Tệp tin Excel)
1.  **Khởi tạo Batch**: Khi người dùng tải lên file Excel, hệ thống sinh ra một mã lô nhập (`CalendarImportBatchId`) và tạo bản ghi lưu trữ thông tin lô với trạng thái `Draft`.
2.  **Đọc dòng chi tiết (`CalendarImportBatchRow`)**:
    *   Hệ thống đọc từng dòng dữ liệu từ file Excel và lưu vào bảng `calendar_import_batch_row` ở trạng thái Draft.
    *   Các cột dữ liệu gồm: `Date`, `DayType`, `WorkShift`, `Description`, `IsActive`.
3.  **Hợp lệ hóa dữ liệu (Validation)**:
    *   Kiểm tra định dạng ngày (phải là ngày hợp lệ YYYY-MM-DD).
    *   Kiểm tra tính logic: Ví dụ nếu `DayType` là ngày nghỉ lễ (`PublicHoliday`) thì `WorkShift` buộc phải là `None` (0).
    *   Nếu có lỗi: Ghi nhận trực tiếp mã lỗi và mô tả lỗi vào trường `ErrorMessage` của dòng dữ liệu tương ứng.

### B. Áp dụng lô nhập (Apply/Confirm)
*   Quản trị viên bấm nút xác nhận áp dụng lô cấu hình lịch làm việc.
*   **Chốt chặn an toàn**: Hệ thống chỉ cho phép áp dụng nếu lô nhập không chứa bất kỳ dòng lỗi nghiêm trọng nào.
*   Khi áp dụng, hệ thống thực hiện đồng bộ (Upsert) dữ liệu từ các dòng tạm thời vào bảng chính `work_calendar_day` theo quy tắc Incremental Update (chỉ ghi đè/thêm mới các ngày có trong Excel, giữ nguyên các ngày cấu hình cũ không xuất hiện trong Excel).

---

## 3. Auto Recalculation Logic

Sau khi dữ liệu lịch làm việc được lưu thành công, hệ thống kích hoạt Job tự động tính toán lại đơn nghỉ phép (`LeaveRequestRecalculationJob`):

### A. Quét tìm đơn phép bị ảnh hưởng
*   Tìm tất cả các đơn nghỉ phép có trạng thái là `Pending` hoặc `Approved` và có khoảng thời gian đăng ký (từ `StartDate` đến `EndDate`) giao thoa với các ngày cấu hình lịch làm việc vừa thay đổi.

### B. Xử lý đơn nghỉ phép trạng thái `Pending`
*   Gọi dịch vụ tính toán lại thời lượng mới của đơn phép (`NewDuration`).
*   Cập nhật trường `Duration` của đơn phép thành `NewDuration`.
*   Nếu `NewDuration` vượt quá số dư khả dụng thực tế của nhân viên:
    *   Hệ thống ghi nhận một bản ghi cảnh báo lỗi vào bảng `leave_request_recalculation_audit` với trạng thái `NeedsEmployeeRevision`.
    *   Không tự ý thay đổi hay chèn văn bản cảnh báo vào trường bình luận của đơn nghỉ phép (`LeaveRequest.Comment`).

### C. Xử lý đơn nghỉ phép trạng thái `Approved` (Quy trình Reopen)
Nếu đơn phép bị ảnh hưởng đang ở trạng thái đã duyệt (`Approved`):
1.  **Lấy thông tin cũ**: Lưu lại trạng thái phê duyệt hiện tại (`OldStatus = Approved`), thông tin người duyệt (`OldProcessedBy`), thời gian duyệt (`OldProcessedAt`) và thời lượng cũ (`OldDuration`).
2.  **Hoàn lại số dư**: Thực hiện giảm số ngày đã nghỉ (`UsedDays` trong `LeaveBalance`) đi một lượng bằng đúng `OldDuration`.
3.  **Tính toán thời lượng mới**: Tính thời lượng mới (`NewDuration`) dựa trên lịch làm việc vừa thay đổi.
4.  **Chuyển trạng thái đơn**: Đưa đơn nghỉ phép về trạng thái **`Pending`** để nhân viên rà soát và thực hiện gửi duyệt lại.
5.  **Xóa thông tin phê duyệt trên đơn**: Đặt lại giá trị `ProcessedBy = null` và `ProcessedAt = null` trên bản ghi đơn nghỉ phép hiện tại.
6.  **Ghi nhật ký Audit**: Thêm bản ghi mới vào bảng `leave_request_recalculation_audit` lưu vết toàn bộ dữ liệu lịch sử phê duyệt cũ và biến động thời lượng. Tuyệt đối không thay đổi trường `Comment` của đơn nghỉ phép.

---

## 4. Safety Guardrails & Access Control
*   Toàn bộ quy trình recalculation phải chạy bên trong một **Database Transaction** duy nhất. Nếu có bất kỳ lỗi nào xảy ra trong quá trình cập nhật trạng thái đơn phép hoặc số dư phép, transaction sẽ rollback toàn bộ để tránh sai lệch số liệu.
*   Việc kích hoạt recalculation chỉ thực hiện khi lưu cấu hình lịch làm việc thành công.

---

## 5. Verification Checklist
*   [ ] File Excel import mẫu tải xuống thành công và đúng định dạng.
*   [ ] Quy trình Import Batch lưu thành công dữ liệu Draft và bắt đúng các lỗi logic.
*   [ ] Đơn Approved sau khi recalculation chuyển về trạng thái `Pending` và xóa sạch `ProcessedBy`/`ProcessedAt`.
*   [ ] Số dư `UsedDays` được hoàn trả chính xác bằng thời lượng cũ đã duyệt.
*   [ ] Cảnh báo Recalculation hiển thị thông qua DTO/Audit, không ghi đè vào `LeaveRequest.Comment`.