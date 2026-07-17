# Report: Leave Request Modals Design & Manual UAT
**Date:** 2026-07-16

## 1. Kết Quả Design trên Stitch
Các screen đã được tạo thành công dựa trên guideline của **Swiss International HR**:
*   **Leave Request Create Modal (Desktop)**: `782896a972f04dfc9f0bcc47f3cb4f82`
*   **Leave Request Cancel Modal (Desktop)**: `4d87aaefa5e544f7bb752f6a640a781d`
*   **Leave Request Create Modal (Mobile)**: `f59bf6c80c6a474397a74d9d2c3b1d6e`
*   **Leave Request Cancel Modal (Mobile)**: `6ef407fb37ef4ee9b32abcef2add0149`

Các asset ID này đã được cập nhật vào `.stitch/metadata.json` và `.stitch/SITE.md` với status `GENERATED_FOR_REVIEW`.
Thư mục rác `HRM_Leave_Management/MD_memory/` cũng đã được xóa theo kế hoạch.

## 2. Kế Hoạch Manual UAT cho Leave Request List
Theo nguyên tắc dự án, Anti cung cấp báo cáo UAT thủ công để User kiểm tra trước khi chuyển sang bước viết code runtime cho Modals.

### Điều Kiện Trước Khi Test (Prerequisites)
*   **Môi trường:** Đã chạy Docker container `keycloak-hrm`.
*   **Chế độ Auth:** Keycloak thật (`UseMockAuth: false` trong appsettings).
*   **Tài khoản UAT:** `admin` hoặc `admin@hrm.local` (Password: `Admin@123456`).
*   **Phân quyền (Permissions):** User UAT phải có các quyền: `VIEW_LEAVE`, `CREATE_LEAVE`, `APPROVE_LEAVE`.

### Các Bước Test Thực Tế (Test Steps)

**TC-01: Truy cập danh sách Leave Request**
1. Đăng nhập hệ thống bằng tài khoản UAT.
2. Mở URL: `https://localhost:7xxx/LeaveRequest` (thay port phù hợp).
3. **Kết quả mong đợi:** Trang danh sách hiển thị dữ liệu thành công. Không bị lỗi `CS1061` như trước. Dữ liệu được iterate đúng (không dùng trực tiếp `Model` mà dùng `Model.Data`).

**TC-02: Kiểm tra UI Pagination (Swiss HR Style)**
1. Cuộn xuống cuối trang danh sách.
2. **Kết quả mong đợi:** Footer phân trang phải theo cấu trúc rõ ràng:
   - Các nút `PREV` và `NEXT` có khoảng cách hợp lý.
   - Text ở giữa hiển thị dạng `PAGE X OF Y`.
   - Nút disabled/enabled hoạt động đúng khi ở trang 1 hoặc trang cuối.

**TC-03: Kiểm tra Select Options Filter (RZ1034 Fix)**
1. Mở HTML source hoặc DevTools (F12) trên trang danh sách.
2. Kiểm tra bộ lọc trạng thái (Status Filter).
3. **Kết quả mong đợi:** Các tag `<option>` đóng mở đúng (đã sửa lỗi thẻ đóng `</!option>`), UI render bình thường không bị vỡ.

### 3. Yêu Cầu Chờ Duyệt (Next Steps)
1. **User duyệt thiết kế Stitch:** Review các Modal UI trên Stitch canvas.
2. **User hoàn thành UAT danh sách:** Test Leave Request List theo các bước thủ công trên.
3. Sau khi xác nhận Pass, Anti sẽ tiến hành apply code của `Create Modal` và `Cancel Modal` vào runtime.
