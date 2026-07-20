# Báo cáo Phân tích Nguyên nhân & Xác minh UAT Lỗi SumoSelect trên Mobile

*   **Mã Phase:** `phase-2b-hotfix`
*   **Ngày tạo:** 2026-07-20 10:38
*   **Tác giả:** Senior .NET Fullstack Engineer & Technical Reviewer

---

## 1. Báo Cáo Phân Tích Nguyên Nhân Gốc (Root Cause Analysis - RCA)

### 1.1. Khởi tạo SumoSelect ban đầu
Trong cả hai tệp `CreateUserView.cshtml` và `Detail.cshtml`, SumoSelect được khởi tạo cơ bản:
```javascript
$('#RoleIds').SumoSelect({
    placeholder: 'select permission' // hoặc 'Selet role'
});
```
Không có cấu hình bổ sung nào cho các tùy chọn như `floatWidth`, `nativeOnDevice`, `okCancelInMulti`.

### 1.2. Nguyên nhân gốc gây lỗi trên Mobile (viewport 400x845)
1.  **Cơ chế Floating Mobile của SumoSelect:**
    *   Mặc định, SumoSelect cấu hình `floatWidth: 400`. Khi độ rộng màn hình (viewport width) nhỏ hơn hoặc bằng 400px, plugin tự động kích hoạt chế độ di động bằng cách đặt cờ `is_floating = true`.
    *   Trong chế độ `is_floating`, SumoSelect hiển thị danh sách dạng popup toàn màn hình (floating modal) và tự tạo thêm thanh điều khiển `.MultiControls` chứa hai nút **OK** và **Cancel**.
    *   Khi ở chế độ này, mọi tương tác (check/uncheck các checkbox) chỉ thay đổi trạng thái tạm thời trên giao diện dropdown. Plugin cố ý **chặn việc cập nhật caption ngay lập tức**. Việc đồng bộ caption và đẩy dữ liệu được chọn sang thẻ `<select>` gốc chỉ diễn ra sau khi người dùng bấm nút **OK** để xác nhận.
2.  **Lỗi khi thay đổi kích thước viewport (Resize Bug):**
    *   Khi tải trang lần đầu trên Desktop, SumoSelect được khởi tạo ở chế độ thông thường (`is_floating = false`).
    *   Khi co nhỏ cửa sổ trình duyệt xuống kích thước Mobile (400x845), sự kiện resize kích hoạt làm đổi cờ `is_floating = true`. Tuy nhiên, plugin **chỉ tạo cụm nút OK/Cancel khi khởi tạo (init) lần đầu**. Khi chuyển trạng thái qua resize, DOM của `.MultiControls` không hề được sinh ra.
    *   Hậu quả là người dùng trên mobile viewport thay đổi checkbox nhưng không có nút OK để bấm xác nhận, khiến caption bị kẹt vĩnh viễn ở trạng thái cũ (ví dụ: `"3 ALL SELECTED!"`).
3.  **Xung đột CSS Swiss Design:**
    *   CSS override Swiss Design áp dụng quy tắc cứng `position: absolute !important` và `top: 40px !important` cho `.optWrapper`.
    *   Điều này bẻ gãy giao diện floating modal nguyên bản của SumoSelect trên màn hình nhỏ, làm các thành phần điều khiển (nếu được sinh ra) bị che khuất hoặc hiển thị lệch lạc, dẫn tới trải nghiệm không đồng nhất.

### 1.3. Lỗi jQuery Selector
*   Ở các đoạn validate và lấy giá trị submit, tồn tại một số selector bị thừa khoảng trắng ở cuối: `$('#password ')`, `$("#password ")`, và `$('#RoleIds ')`.
*   Khoảng trắng thừa này có thể gây lỗi tham chiếu phần tử trên một số trình duyệt hoặc phiên bản thư viện.

---

## 2. Giải Pháp Khắc Phục Tối Thiểu & An Toàn

1.  **Vô hiệu hóa chế độ Floating Mobile của SumoSelect:**
    *   Cấu hình tham số `floatWidth: 0` khi khởi tạo SumoSelect trong cả 2 tệp view.
    *   Khi `floatWidth: 0`, SumoSelect sẽ coi mọi độ rộng viewport luôn lớn hơn `floatWidth`, từ đó ép buộc sử dụng giao diện Custom UI phẳng đồng nhất trên cả Desktop và Mobile. Nhờ đó, caption sẽ cập nhật ngay lập tức khi người dùng click/tap chọn mà không cần thông qua nút OK/Cancel.
2.  **Sửa lỗi Selector jQuery:**
    *   Loại bỏ khoảng trắng thừa ở cuối các selector: `$('#password')`, `$("#password")`, `$('#RoleIds')`.

---

## 3. Bằng Chứng Xác Minh UAT Trình Duyệt (Browser UAT Evidence)

Kiểm thử được thực hiện tự động bằng subagent trình duyệt trên cổng `http://localhost:5300`:

### 3.1. UAT Trang Tạo Mới User (`/user/create`)
*   **Desktop Viewport:** 
    *   Mở dropdown "Group Permission".
    *   Bỏ chọn quyền `ADMIN` (giảm từ 3 xuống 2 quyền).
    *   **Kết quả:** Caption cập nhật ngay lập tức thành `"Leave Approver, EMPLOYEE_SELF_VIEW"`.
*   **Mobile Viewport (400x845):**
    *   Mở dropdown, đổi số lượng chọn từ 3 xuống 2 quyền.
    *   **Kết quả:** Caption cập nhật ngay lập tức thành `"Leave Approver, EMPLOYEE_SELF_VIEW"`.
*   **Submit Form:** Điền đầy đủ thông tin hợp lệ (tạo user `testuserc1`), nhấn **Save**. Dữ liệu gửi đi chính xác gồm 2 quyền đã chọn, hệ thống tạo tài khoản thành công và chuyển hướng về danh sách.

### 3.2. UAT Trang Cập Nhật User (`/user/detail/{id}`)
*   **Desktop Viewport:**
    *   Bấm **Edit** user `testuserc1` vừa tạo.
    *   Thay đổi quyền từ 2 lên 3 (chọn thêm `ADMIN`), caption hiển thị `"3 ALL SELECTED!"`. Bỏ chọn `ADMIN` để giảm về 2 quyền.
    *   **Kết quả:** Caption cập nhật lập tức về `"Leave Approver, EMPLOYEE_SELF_VIEW"`.
*   **Mobile Viewport (400x845):**
    *   Mở dropdown và thực hiện thao tác giảm từ 3 xuống 2 quyền tương tự.
    *   **Kết quả:** Caption cập nhật tức thì.
*   **Submit Form:** Thay đổi Họ tên thành `Test Create User Updated`, nhấn **Save**. Lưu dữ liệu thành công, chuyển hướng về trang danh sách và cập nhật chính xác.

### 3.3. Nhật ký Console
*   Hoàn toàn không xuất hiện lỗi JavaScript (`uncaught reference error`, `syntax error`, v.v.) trong suốt quá trình UAT trên cả hai trang.

---

## 4. Trạng Thế Code & Build

*   **Ranh giới kiến trúc:** Bảo toàn 100% ranh giới `Web.Backend -> Application -> Domain`. Không thay đổi bất kỳ file `.cs`, cấu hình Keycloak hay database.
*   **Xác minh Git:**
    *   `git status --short` chỉ có 4 file Razor View trong phạm vi cho phép bị chỉnh sửa.
    *   Không có trailing whitespace mới phát sinh trong file sửa đổi (`git diff --check` sạch).
    *   Build ứng dụng thành công, không gặp lỗi cú pháp hay cảnh báo Razor mới.
