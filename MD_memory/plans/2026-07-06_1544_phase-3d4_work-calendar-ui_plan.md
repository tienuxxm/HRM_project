# HRM Phase 3D.4 Plan — Work Calendar UI

## 1. Scope
Phase này tập trung vào việc phát triển giao diện Razor MVC cho các trang quản trị Lịch làm việc, bao gồm trang quản lý chính, trang Preview lô nhập Excel, trang báo cáo kết quả Recalculation và tích hợp hiển thị cảnh báo lên chi tiết đơn nghỉ phép.

---

## 2. Views Design & Integration

### A. Route `/work-calendar` (Index.cshtml)
*   **Bảng dữ liệu**: Hiển thị danh sách các ngày làm việc/nghỉ đặc thù được cấu hình trong DB.
*   **Bộ lọc**: Lọc nhanh theo tháng/năm.
*   **Hành động**:
    *   Nút *Add Configuration* mở Modal cho phép thêm nhanh một ngày cấu hình đơn lẻ.
    *   Nút *Download Template* để tải file mẫu Excel.
    *   Nút *Import Excel* để mở Modal tải tệp tin cấu hình.

### B. Route `/work-calendar/preview/{batchId}` (Preview.cshtml)
*   **Bảng xem trước**: Hiển thị toàn bộ dữ liệu dòng của lô import tạm thời.
*   **Dòng hợp lệ**: Hiển thị thông thường (nền trắng/neutral).
*   **Dòng lỗi**: Hiển thị viền/nền màu đỏ kèm cột ghi nhận lý do lỗi cụ thể.
*   **Nút Apply**: Chỉ cho phép nhấn nếu không tồn tại bất kỳ dòng lỗi nghiêm trọng nào. Khi nhấn, gửi Request `ConfirmCalendarImportBatchCommand`.

### C. Route `/work-calendar/summary/{batchId}` (Summary.cshtml)
*   **Thống kê tổng quan**: Số lượng ngày được cập nhật thành công.
*   **Danh sách đơn bị ảnh hưởng**: Hiển thị bảng chi tiết các đơn phép bị thay đổi thời lượng hoặc chuyển trạng thái Approved $\rightarrow$ Pending:
    *   Mã đơn, Tên nhân viên, Ngày bắt đầu/kết thúc.
    *   Trạng thái cũ $\rightarrow$ Trạng thái mới.
    *   Thời lượng cũ $\rightarrow$ Thời lượng mới (và chênh lệch Delta).
    *   Nút bấm đi thẳng đến chi tiết đơn nghỉ phép tương ứng.

### D. Tích hợp cảnh báo chi tiết Đơn nghỉ phép (LeaveRequest/Details.cshtml)
*   Nếu đơn nghỉ phép có bản ghi lịch sử recalculation trong bảng `leave_request_recalculation_audit`:
    *   Hiển thị một banner cảnh báo màu vàng phía trên thông tin đơn nghỉ: *"This request was reopened because calendar changes affected your dates. Old approved duration was X days. Please review and resubmit."*.
    *   Hệ thống lấy thông tin này trực tiếp từ thực thể Audit log qua MediatR Query, tuyệt đối không đọc từ trường `Comment` của đơn nghỉ.

---

## 3. Sidebar Integration & Access Control
*   **Sidebar Link**: Thêm liên kết `Work Calendar` vào Sidebar trong layout chung của HRM (`Views/Shared/_Layout.cshtml` hoặc tệp Sidebar tương ứng).
*   **Chốt chặn quyền**:
    ```html
    @if (UserHasPermission("VIEW_WORK_CALENDAR"))
    {
        <li><a href="/work-calendar">Work Calendar</a></li>
    }
    ```

---

## 4. UI/UX Rules
*   **Toast Notifications**: Mọi thông báo thành công, thất bại hoặc xác nhận hành động phải sử dụng Toast/Modal có sẵn của dự án, tuyệt đối không dùng `window.alert()` hoặc `window.confirm()`.
*   **Ngôn ngữ**: Toàn bộ chữ hiển thị trên giao diện quản trị sử dụng tiếng Anh chuẩn (English UI only).

---

## 5. UAT Checklist (Manual View Verification)
*   [ ] Sidebar chỉ hiển thị mục `Work Calendar` đối với User có quyền `VIEW_WORK_CALENDAR`.
*   [ ] Giao diện Preview hiển thị đúng màu đỏ cảnh báo cho các dòng dữ liệu Excel lỗi.
*   [ ] Giao diện Summary liệt kê chính xác sự thay đổi trạng thái và thời lượng của các đơn phép bị ảnh hưởng.
*   [ ] Giao diện Chi tiết đơn nghỉ hiển thị đúng banner cảnh báo lấy từ Audit Log đối với các đơn bị Reopen.
