# Báo cáo UAT Thủ công & Thiết kế Work Calendar "Enterprise Calm"
> **Thời gian:** 2026-07-20 18:00
> **Phase:** phase-design-work-calendar-runtime
> **Trạng thái:** SẴN SÀNG UAT (Build Success)

---

## 1. Thông tin Môi trường & Cấu hình UAT
*   **Chế độ Auth (Auth Mode):** Keycloak thật (Real Keycloak).
*   **Cấu hình `UseMockAuth`:** `false`.
*   **Tài khoản UAT đề xuất:** `admin` (hoặc `admin@hrm.local` / `Admin@123456`).
*   **Permissions yêu cầu:** Đã seed đầy đủ các permission liên quan:
    *   `VIEW_LEAVE`
    *   `CREATE_LEAVE`
    *   `APPROVE_LEAVE`
*   **Các Route kiểm thử:**
    *   `/work-calendar` (Trang danh sách & Lịch)
    *   `/work-calendar/preview` (Trang xem trước Batch Import)
    *   `/work-calendar/summary` (Trang tóm tắt kết quả Import)

---

## 2. Kịch bản Kiểm thử Thủ công (Manual UAT Steps)

### Kịch bản 1: Kiểm thử Giao diện Danh sách & Bộ lọc (Index.cshtml)
*   **URL:** `http://localhost:5000/work-calendar` (hoặc cổng HTTP của Web.Backend local)
*   **Tài khoản:** `admin`
*   **Điều kiện trước khi test:** Đảm bảo Keycloak Docker đang chạy ở `http://localhost:8080` và đã đăng nhập thành công.
*   **Các bước thực hiện:**
    1.  Mở trình duyệt truy cập vào URL `/work-calendar`.
    2.  Kiểm tra Layout tiêu đề trang: Đã chuyển sang font **Geist/JetBrains Mono**, không bo góc (`border-radius: 0px`).
    3.  Kiểm tra "Whisper Borders" (đường viền nhạt `border-[#D1D1D1]` hoặc `border-black`).
    4.  Nhấp vào ô Bộ lọc Năm (Year Select) và Bộ lọc Tháng (Month Select), kiểm tra trạng thái tương tác hover/focus có màu sắc tương phản cao (Black/White).
    5.  Thu nhỏ cửa sổ trình duyệt (hoặc bật chế độ Responsive của Chrome DevTools - Mobile view) để kiểm tra giao diện di động:
        *   Các khối thông tin sắp xếp theo chiều dọc (stack) mượt mà.
        *   Không bị tràn viền (horizontal overflow) hoặc chồng chéo văn bản.

### Kịch bản 2: Kiểm thử Hộp thoại Nhập thủ công & Import Excel (Modals)
*   **Các bước thực hiện:**
    1.  Tại trang danh sách, nhấp vào nút **"Import Excel"**.
    2.  Quan sát Modal hiện lên:
        *   Kiểm tra viền ngoài (border-black, border-2), góc nhọn (`rounded-none`).
        *   Nút chọn file và nút hủy/xác nhận phải hiển thị rõ ràng, không dùng các lớp CSS Tailwind bo góc tròn.
    3.  Đóng modal, nhấp tiếp vào nút **"Add Event"** (Manual Change).
    4.  Kiểm tra Form nhập thủ công:
        *   Ô chọn ngày (`input[type="date"]`), ô chọn Day Type (`select`) và ô Mô tả (`textarea`) đều có thiết kế phẳng, viền xám Whisper (`#D1D1D1`), không bo góc.
        *   Nút bấm lưu và đóng có hiệu ứng hover đổi màu trắng-đen tối giản.

### Kịch bản 3: Kiểm thử Giao diện Preview & Validation (Preview.cshtml)
*   **URL tương ứng:** `/work-calendar/preview` (kích hoạt sau khi tải file Excel thành công)
*   **Các bước thực hiện:**
    1.  Chuẩn bị một file Excel mẫu để import.
    2.  Thực hiện tải file qua nút **"Import Excel"**.
    3.  Hệ thống chuyển hướng qua trang **Preview**.
    4.  Quan sát bảng kiểm tra lỗi (Validation Table):
        *   Tiêu đề bảng có màu nền xám phẳng `#eeeeee`, viền ngăn cách dọc 1px tinh tế.
        *   Các hàng dữ liệu hiển thị font chữ Mono (`font-mono`) cho cột Ngày (Date) và Số thứ tự hàng (Row Index).
        *   Các nhãn trạng thái lỗi (Validation Status / Error Messages) có nhãn viền mỏng, không bo góc (đỏ cho Invalid, xanh cho Valid).
    5.  Thử nghiệm với file Excel có lỗi (ví dụ: ngày quá khứ hoặc sai cấu trúc) để xác nhận các dòng lỗi hiển thị nền hồng nhạt `#FEF2F2` (hoặc `bg-red-50`).

### Kịch bản 4: Kiểm thử Tóm tắt kết quả (Summary.cshtml)
*   **URL tương ứng:** `/work-calendar/summary` (sau khi nhấn Apply Import thành công)
*   **Các bước thực hiện:**
    1.  Tại trang Preview (khi toàn bộ dữ liệu hợp lệ), nhấp vào nút **"Apply Import"**.
    2.  Hệ thống xử lý và chuyển hướng qua trang **Summary**.
    3.  Kiểm tra banner thông báo thành công (Success Banner):
        *   Nền phẳng, viền đen nổi bật, nội dung căn chỉnh sạch sẽ.
    4.  Kiểm tra Grid tóm tắt chỉ số (Processed Rows, Changes, Skipped Rows):
        *   Các ô số liệu lớn dạng Grid tối giản.
    5.  Kiểm tra bảng lịch sử thay đổi (Audit Log Table) bên dưới:
        *   Đồng bộ phong cách thiết kế phẳng 0px border-radius, Whisper Borders.

---

## 3. Cách chụp/ghi nhận lỗi khi UAT thất bại
*   Nếu phát hiện lỗi hiển thị hoặc lỗi javascript:
    1.  Nhấn `F12` to open Chrome Developer Tools, chuyển sang tab **Console** xem có lỗi JS đỏ không.
    2.  Chụp ảnh màn hình toàn cảnh giao diện bị lỗi.
    3.  Lưu lại file log backend nếu có lỗi `500 Internal Server Error`.
    4.  Báo cáo trực tiếp cho nhóm kỹ thuật kèm theo các thông tin trên.

---

## 4. Tóm tắt thay đổi mã nguồn (Git Diff Stat)
```diff
 .stitch/SITE.md                                    |   6 +-
 .stitch/metadata.json                              |  55 +++
 .../Web.Backend/Views/WorkCalendar/Index.cshtml    | 453 +++++++++------------
 .../Web.Backend/Views/WorkCalendar/Preview.cshtml  | 250 +++++-------
 .../Web.Backend/Views/WorkCalendar/Summary.cshtml  | 120 +++---
 5 files changed, 418 insertions(+), 466 deletions(-)
```

---
*Tài liệu UAT được biên soạn theo đúng nguyên tắc Clean Architecture và quy chuẩn thiết kế "Enterprise Calm".*
