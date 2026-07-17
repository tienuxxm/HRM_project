# Báo cáo Phân tích và Xử lý Lỗi Work Calendar Edit Modal

## 1. Tóm tắt sự cố (Incident Summary)
- **Triệu chứng**: Trên giao diện quản lý Lịch làm việc (`/work-calendar`), khi người dùng bấm nút **Edit** ở một số dòng (đặc biệt là dòng số 1 và số 6), modal chỉnh sửa thủ công không mở ra. Các dòng khác vẫn hoạt động bình thường.
- **Lỗi trên Developer Console của trình duyệt**:
  - `Uncaught SyntaxError: Invalid or unexpected token` tại rendered page `/work-calendar` (dòng 209 và 304).
- **Ghi chú về lỗi phụ (Separate Technical Debt)**:
  - `GET /images/logo.svg 404 (Not Found)`: Đây là lỗi thiếu tài nguyên hình ảnh logo, không liên quan đến lỗi cú pháp JavaScript và không phải nguyên nhân gây lỗi không mở được modal. Lỗi này được phân loại là một khoản nợ kỹ thuật riêng biệt cần xử lý sau.

---

## 2. Phân tích Nguyên nhân Gốc rễ (Root Cause Analysis)
- **Tầng lỗi**: Tầng **UI / Razor View / JavaScript**.
- **Cơ chế lỗi**:
  - Đoạn code ban đầu sử dụng thuộc tính `onclick` inline để truyền trực tiếp các giá trị từ mô hình Razor vào hàm JavaScript:
    ```html
    onclick="openManualModal('@day.Date.ToString("yyyy-MM-dd")', '@day.DayType', '@day.WorkShift', '@(day.Description ?? "")', @(day.IsActive ? "true" : "false"))"
    ```
  - Khi trường `Description` chứa ký tự xuống dòng vật lý (`\n`), Razor render trường này ra HTML sinh ra thực thể HTML `&#xA;`.
  - Khi trình duyệt phân tích cú pháp HTML và biên dịch đoạn mã JavaScript inline trong thuộc tính `onclick`, thực thể `&#xA;` được giải mã ngược lại thành ký tự xuống dòng vật lý. Điều này tạo ra một dòng xuống hàng thực tế bên trong chuỗi ký tự được bọc bởi dấu nháy đơn `'...'` của JavaScript.
  - Theo chuẩn cú pháp JavaScript (ngoại trừ Template Literals sử dụng dấu backtick `` ` ``), các chuỗi ký tự dạng `'...'` hoặc `"..."` không được phép chứa ký tự xuống dòng trực tiếp. Trình duyệt sẽ quăng lỗi `Uncaught SyntaxError: Invalid or unexpected token` và chặn không cho hàm hoạt động.
  - Ngoài ra, nếu `Description` chứa các ký tự nháy đơn `'` hoặc nháy kép `"`, nó cũng sẽ làm vỡ cấu trúc thuộc tính `onclick` hoặc chuỗi đối số JavaScript, tạo ra nguy cơ quote-break rất cao.

---

## 3. Giải pháp Khắc phục Triệt để (Robust Fix Applied)
Để loại bỏ hoàn toàn nguy cơ quote-break và lỗi cú pháp do ký tự xuống dòng/ký tự đặc biệt, chúng ta không sử dụng gọi hàm JavaScript inline qua thuộc tính `onclick`. Thay vào đó, áp dụng giải pháp lưu trữ dữ liệu thông qua các thuộc tính **`data-*` HTML5** kết hợp với cơ chế **Event Delegation (Ủy quyền sự kiện)** trong jQuery.

### A. Thay đổi ở phần markup nút Edit
- **File chỉnh sửa**: [Index.cshtml](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Web.Backend/Views/WorkCalendar/Index.cshtml)
- Nút **Edit** được refactor loại bỏ hoàn toàn `onclick` inline, bổ sung class định danh `js-edit-calendar` và lưu trữ các tham số vào `data-*` attributes:
  ```html
  <button type="button" 
          class="js-edit-calendar text-blue-600 hover:text-blue-900 font-medium"
          data-date="@day.Date.ToString("yyyy-MM-dd")"
          data-day-type="@day.DayType"
          data-work-shift="@day.WorkShift"
          data-description="@(day.Description ?? "")"
          data-is-active="@(day.IsActive ? "true" : "false")">
      Edit
  </button>
  ```
- *Ưu điểm*: Trình duyệt tự động parse và xử lý an toàn các ký tự xuống dòng và nháy kép khi đưa vào thuộc tính HTML mà không làm vỡ mã JavaScript.

### B. Thay đổi ở phần mã script xử lý sự kiện
- Đăng ký một sự kiện click ủy quyền (delegated event listener) trên đối tượng `document` nhắm vào class `.js-edit-calendar` ở khối `<script>` phía cuối trang:
  ```javascript
  $(document).ready(function() {
      // ... các event handler khác ...

      $(document).on('click', '.js-edit-calendar', function() {
          const dateStr = $(this).data('date') || '';
          const dayType = $(this).data('day-type') || 'PublicHoliday';
          const workShift = $(this).data('work-shift') || 'None';
          const description = $(this).data('description') || '';
          const isActive = $(this).data('is-active') === true || $(this).data('is-active') === 'true';
          
          openManualModal(dateStr, dayType, workShift, description, isActive);
      });
  });
  ```

---

## 4. Đánh giá Tác động & Blast Radius (Impact Analysis)
- **GitNexus Blast Radius**: `UNKNOWN / Limited`
  - *Lý do*: Thay đổi nằm hoàn toàn trong tập tin UI Razor View (`Index.cshtml`), không chứa các C# symbol của hệ thống backend được index bởi GitNexus.
- **Đánh giá thủ công**:
  - **Mức độ rủi ro**: **Thấp (LOW)**.
  - **Phạm vi tác động**: Giới hạn hoàn toàn tại giao diện hiển thị danh sách Lịch làm việc phía client.
  - **Tính toàn vẹn**:
    - Không làm thay đổi Controller, Application, Domain hay Infrastructure.
    - Không thay đổi cấu trúc cơ sở dữ liệu hay phân quyền Keycloak.
    - Không sửa bất kỳ mã nguồn legacy của Customer Management System (CMS).

---

## 5. Kết quả Xác minh & Build Logs (Verification & Build Evidence)

### A. Kết quả biên dịch (Build Results)
- **Trạng thái**: **Not verified / pending clean build**
- **Lý do**: Lệnh biên dịch qua thiết bị đầu cuối (`dotnet build`) không thể thực thi trực tiếp do cơ chế bảo mật môi trường sandbox hạn chế quyền (`unexpected user interaction type: not permission`). Cần chạy build sạch độc lập trên môi trường của nhà phát triển để nghiệm thu chính thức.

### B. Kết quả kiểm tra UAT trình duyệt (Browser UAT Result)
Kiểm thử tự động bằng Browser Subagent đã ghi nhận kết quả thành công và chụp lại các bằng chứng xác minh cục bộ. 

> [!NOTE]
> Các liên kết ảnh dưới đây là **bằng chứng cục bộ (local-only evidence)** đặc thù trên môi trường chạy thử nghiệm của agent, không phải là tài nguyên tĩnh thuộc mã nguồn dự án được lưu trữ trong Git repository.

1. **Xác minh lỗi cú pháp**:
   - Khi tải trang `/work-calendar`, Developer Console của trình duyệt hoàn toàn sạch sẽ, không còn xuất hiện lỗi `Uncaught SyntaxError`.
2. **Xác minh mở modal Edit**:
   - Click nút **Edit** ở dòng số 1 -> Modal chỉnh sửa mở lên bình thường.
     ![Edit Row 1 Click (Local-Only)](file:///C:/Users/Tienht/.gemini/antigravity/brain/bca94314-42fc-4f49-b186-377a4f003dc5/.system_generated/click_feedback/click_feedback_1783491248914.png)
     ![Edit Row 1 Modal (Local-Only)](file:///C:/Users/Tienht/.gemini/antigravity/brain/bca94314-42fc-4f49-b186-377a4f003dc5/.system_generated/click_feedback/click_feedback_1783491254267.png)
   - Click nút **Edit** ở dòng số 6 -> Modal mở rộng và hiển thị đầy đủ chuỗi mô tả tiếng Việt xuống dòng.
     ![Edit Row 6 Click (Local-Only)](file:///C:/Users/Tienht/.gemini/antigravity/brain/bca94314-42fc-4f49-b186-377a4f003dc5/.system_generated/click_feedback/click_feedback_1783491267960.png)
     ![Edit Row 6 Modal (Local-Only)](file:///C:/Users/Tienht/.gemini/antigravity/brain/bca94314-42fc-4f49-b186-377a4f003dc5/.system_generated/click_feedback/click_feedback_1783491275190.png)
3. **Xác minh các nút chức năng khác (Không lỗi hồi quy - No Regression)**:
   - Click nút **Add Calendar Day** -> Mở modal với các trường dữ liệu trống bình thường.
     ![Add Calendar Day Click (Local-Only)](file:///C:/Users/Tienht/.gemini/antigravity/brain/bca94314-42fc-4f49-b186-377a4f003dc5/.system_generated/click_feedback/click_feedback_1783491283099.png)
     ![Add Calendar Day Modal (Local-Only)](file:///C:/Users/Tienht/.gemini/antigravity/brain/bca94314-42fc-4f49-b186-377a4f003dc5/.system_generated/click_feedback/click_feedback_1783491290623.png)
   - Click nút **Import Excel** -> Mở modal tải tệp Excel bình thường.
     ![Import Excel Click (Local-Only)](file:///C:/Users/Tienht/.gemini/antigravity/brain/bca94314-42fc-4f49-b186-377a4f003dc5/.system_generated/click_feedback/click_feedback_1783491300177.png)
     ![Import Excel Modal (Local-Only)](file:///C:/Users/Tienht/.gemini/antigravity/brain/bca94314-42fc-4f49-b186-377a4f003dc5/.system_generated/click_feedback/click_feedback_1783491304626.png)

---

## 6. Trạng thái Mã hóa & Git Hygiene (Encoding & Git Status)

### A. Kết quả quét mã hóa (Encoding Scan Raw Output)
Dưới đây là kết quả quét mã hóa chính thức được xác nhận bởi Codex trên toàn bộ tài liệu:
- **Files scanned**: 39
- **BOM failures**: 0
- **Mojibake hits**: 0
- **Exit code**: 0

### B. Trạng thái Git (Git Status Report)
Các tập tin liên quan hiện tại ở trạng thái **chưa được stage** (unstaged) và **chưa theo dõi** (untracked):
- Đường dẫn thư mục chứa view WorkCalendar mới: `HRM_Leave_Management/Web.Backend/Views/WorkCalendar/` (Untracked - `??`)
- Báo cáo lỗi này: `MD_memory/reports/2026-07-08_1150_phase-3d_work-calendar-modal-bug_report.md` (Untracked - `??`)

> [!WARNING]
> **Cảnh báo an toàn**: Nhà phát triển tuyệt đối **KHÔNG** sử dụng lệnh `git add .` hoặc `git add -A`. Bắt buộc chỉ thực hiện stage các tập tin cụ thể và được phê duyệt rõ ràng bằng lệnh `git add -- <file>` để tránh đưa các tập tin local-only hoặc cấu hình môi trường tạm thời vào Git lịch sử.
