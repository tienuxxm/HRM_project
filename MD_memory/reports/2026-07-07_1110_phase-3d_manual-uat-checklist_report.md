# Hướng dẫn Kiểm thử UAT Thủ công — Phase 3D Work Calendar & Leave Recalculation

Tài liệu này là Hướng dẫn Kiểm thử Thủ công (Manual UAT Guide) từng bước dành cho các tính năng của Phase 3D: Work Calendar (Lịch làm việc) và Leave Recalculation (Tính toán lại ngày phép). Vui lòng sử dụng tài liệu này để thực hiện kiểm thử và ghi lại kết quả.

---

## 1. Cấu hình môi trường (Environment Configuration)

Đảm bảo môi trường kiểm thử local của bạn đã được cấu hình như sau:

*   **URL Trang chủ Ứng dụng**: `http://localhost:5300`
*   **Màn hình Đăng nhập (Login)**: `http://localhost:5300/auth/login-screen`
*   **Chế độ Xác thực (Authentication Mode)**: Keycloak thật (`UseMockAuth = false` trong file `appsettings.json`)
*   **URL Keycloak Server**: `http://localhost:8080`
*   **Keycloak Realm**: `hrm`
*   **Keycloak Client**: `hrm-web`
*   **Database Schema**: Đảm bảo tất cả migration đã được áp dụng. Chạy lệnh:
    ```powershell
    dotnet ef database update --project HRM_Leave_Management/Infrastructure --startup-project HRM_Leave_Management/Web.Backend
    ```
*   **Xác minh Build hệ thống**:
    > [!IMPORTANT]
    > Nếu ứng dụng đang chạy, hãy **dừng tiến trình dotnet** trước khi chạy lệnh build để tránh lỗi khóa file DLL/apphost:
    > ```powershell
    > dotnet build HRM_Leave_Management/LUC.sln --no-restore
    > ```

---

## 2. Tài khoản kiểm thử (Test Accounts)

| Vai trò tài khoản | Username / Email | Mật khẩu (Password) | Quyền yêu cầu (Required Permissions) |
| :--- | :--- | :--- | :--- |
| **Admin / HR** | `admin` hoặc `admin@hrm.local` | `Admin@123456` | `VIEW_WORK_CALENDAR`, `UPDATE_WORK_CALENDAR`, `VIEW_LEAVE_REQUEST`, `APPROVE_LEAVE_REQUEST`, `VIEW_LEAVE_BALANCE`, `UPDATE_LEAVE_BALANCE` |
| **Employee** | `uat.p3d.employee01@hrm.local` | `Admin@123456` | `VIEW_LEAVE_REQUEST`, `CREATE_LEAVE_REQUEST`, `VIEW_LEAVE_BALANCE` |

### Thiết lập tài khoản kiểm thử Nhân viên (Employee)

Nếu tài khoản nhân viên kiểm thử chưa có sẵn hoặc chưa được tạo trong database, kiểm thử viên phải tạo và liên kết tài khoản thông qua UI của ứng dụng:

1. Đăng nhập ứng dụng với tài khoản **Admin/HR** (`admin` / `Admin@123456`).
2. Truy cập trực tiếp vào đường dẫn cấp tài khoản nhân viên tại: `/employee`
3. Chọn **Add Employee** (hoặc nút tương đương để tạo mới nhân viên).
4. Điền các thông tin sau:
    *   **Username**: `uat.p3d.employee01`
    *   **Email**: `uat.p3d.employee01@hrm.local`
    *   **Password**: `Admin@123456`
    *   **Role**: Chọn role standard `employee` (role này sẽ được cấu hình ánh xạ với các quyền `VIEW_LEAVE_REQUEST`, `CREATE_LEAVE_REQUEST`, `VIEW_LEAVE_BALANCE`).
5. Lưu bản ghi nhân viên và ghi lại thông tin định danh:
    *   **Tên nhân viên được ánh xạ**: \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
    *   **ID nhân viên được ánh xạ**: \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

> [!CAUTION]
> Tuyệt đối không sử dụng các câu lệnh INSERT trực tiếp vào database để tạo hoặc liên kết tài khoản nhân viên này. Cơ sở dữ liệu Keycloak và cơ sở dữ liệu ứng dụng phải được đồng bộ thông qua UI quản lý nhân viên.

---

## 3. Các bước thiết lập quyền (Permission Setup Steps)

Thực hiện các bước sau để cấu hình vai trò và quyền hạn trước khi tiến hành chạy kiểm thử:

1. Đăng nhập ứng dụng với tài khoản **Admin/HR** (`admin` / `Admin@123456`).
2. Truy cập trực tiếp trang cấu hình vai trò và quyền tại: `/role`
3. Nhấp nút sửa (Edit) đối với vai trò cần thiết lập (ví dụ: vai trò **Admin/HR**).
4. Tích chọn các ô phân quyền sau:
    *   [x] `VIEW_WORK_CALENDAR`
    *   [x] `UPDATE_WORK_CALENDAR`
    *   [x] `VIEW_LEAVE_REQUEST`
    *   [x] `APPROVE_LEAVE_REQUEST`
    *   [x] `VIEW_LEAVE_BALANCE`
    *   [x] `UPDATE_LEAVE_BALANCE`
5. Nhấp nút **Save** để lưu lại cấu hình phân quyền.
6. Lặp lại các bước tương tự cho vai trò **Employee**, đảm bảo đã tích chọn:
    *   [x] `VIEW_LEAVE_REQUEST`
    *   [x] `CREATE_LEAVE_REQUEST`
    *   [x] `VIEW_LEAVE_BALANCE`
7. Đăng xuất khỏi hệ thống và đăng nhập lại để làm mới phiên làm việc.
8. **Điều kiện Vượt qua (Pass Condition)**: Menu "Work Calendar" hiển thị thành công trên thanh điều hướng bên (sidebar).

---

## 4. Thiết lập dữ liệu kiểm thử chung (Common Test Data Setup)

Trước khi thực hiện các kịch bản kiểm thử chính, hãy đảm bảo các dữ liệu chung sau đã được chuẩn bị trên giao diện:

1.  **Loại nghỉ phép (Leave Type)**: Chọn một loại phép đang hoạt động (ví dụ: *Annual Leave*), hoặc truy cập `/leave-type` để tạo mới nếu cần.
2.  **Số dư ngày phép (Leave Balance)**: Đảm bảo nhân viên kiểm thử có số ngày phép khả dụng lớn hơn 0 cho năm hiện tại. Đăng nhập Admin, mở trang `/leave-balance` để kiểm tra và gán/cập nhật số dư phép nếu đang bằng 0.
3.  **Ánh xạ tài khoản**: Đảm bảo tài khoản nhân viên kiểm thử đã được liên kết chính xác với một bản ghi Employee có thông tin phòng ban hợp lệ.
4.  **Quy ước đặt tên khi kiểm thử**:
    *   Mọi mô tả ngày lịch làm việc được nhập thủ công phải có tiền tố: `UAT-P3D-` (ví dụ: `UAT-P3D-HolidayDescription`).
    *   Mọi ghi chú trong đơn xin nghỉ phép phải có tiền tố: `UAT-P3D-` (ví dụ: `UAT-P3D-LeaveRequestComment`).
    *   Tên file Excel import phải có định dạng: `phase3d_uat_<mô-tả>.xlsx`.
5.  **Ghi nhận dữ liệu thực tế**: Trong quá trình kiểm thử, kiểm thử viên phải điền các thông tin sau:
    *   Tên nhân viên / Employee ID kiểm thử: \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
    *   Loại phép sử dụng (Leave Type): \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
    *   ID Đơn phép được tạo (Leave Request ID): \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
    *   Các ngày lịch được chỉnh sửa (Modified Dates): \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
    *   Import Batch ID: \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

---

## 5. Các ngày kiểm thử UAT dành riêng (Reserved UAT Dates)

Để tránh hiện tượng nhiễm chéo dữ liệu giữa các kịch bản kiểm thử, các khoảng thời gian tương lai riêng biệt đã được phân bổ cho từng ca kiểm thử. **Không được tái sử dụng các ngày trùng nhau cho các kịch bản khác nhau.**

| Ca kiểm thử (Test Case) | Ngày bắt đầu | Ngày kết thúc / Ngày cụ thể | Cấu hình & Mô tả |
| :--- | :--- | :--- | :--- |
| **TC-01** | `2026-07-17` (Thứ Sáu) | `2026-07-20` (Thứ Hai) | Kiểm tra loại trừ ngày nghỉ cuối tuần thông thường |
| **TC-02** | `2026-07-25` (Thứ Bảy) | `2026-07-26` (Chủ Nhật) | Kiểm tra chặn đơn phép chỉ chứa ngày nghỉ |
| **TC-03** | `2026-08-07` (Thứ Sáu) | `2026-08-11` (Thứ Ba) | Ngày `2026-08-10` (Thứ Hai) cấu hình là `PublicHoliday` |
| **TC-04 / 05** | `2026-08-18` (Thứ Ba) | `2026-08-18` (Thứ Ba) | Ngày `2026-08-18` cấu hình là `StandardWorkingDayOverride`, `MorningOnly` |
| **TC-06** | `2026-07-06` (Thứ Hai) | `2026-07-06` (Thứ Hai) | Kiểm tra ngày trong quá khứ (trước ngày hiện tại `2026-07-07`) |
| **TC-07** | `2026-08-01` (Thứ Bảy) | `2026-08-05` (Thứ Tư) | Các dòng dữ liệu trong mẫu file Excel import |
| **TC-08A** | `2026-08-24` (Thứ Hai) | `2026-08-26` (Thứ Tư) | Ngày `2026-08-26` (Thứ Tư) là `PublicHoliday`, sau đó sửa thủ công |
| **TC-08B** | `2026-08-31` (Thứ Hai) | `2026-09-02` (Thứ Tư) | Ngày `2026-09-02` (Thứ Tư) là `PublicHoliday`, sửa qua Excel import |

---

## 6. Các kịch bản kiểm thử nghiệp vụ chính (TC-01 đến TC-08B)

### TC-01: Nghỉ từ Thứ Sáu tới Thứ Hai loại trừ Thứ Bảy / Chủ Nhật
*   **Mục đích**: Xác minh hệ thống tự động loại trừ các ngày nghỉ cuối tuần mặc định (Thứ Bảy và Chủ Nhật) khỏi thời gian nghỉ phép của đơn.
*   **Tài khoản sử dụng**: Tài khoản Nhân viên (`uat.p3d.employee01@hrm.local`).
*   **Quyền yêu cầu**: `CREATE_LEAVE_REQUEST`, `VIEW_LEAVE_BALANCE`.
*   **Điều kiện trước**: Tài khoản nhân viên có đủ số dư ngày phép năm.
*   **Màn hình / Đường dẫn**: `/leave-request`
*   **Các bước thao tác**:
    1. Đăng nhập ứng dụng với tài khoản Nhân viên kiểm thử.
    2. Điều hướng tới menu **Leave Requests** $\rightarrow$ bấm nút **Create New Leave Request**.
    3. Chọn loại phép đang hoạt động (ví dụ: Annual Leave).
    4. Chọn **Start Date** là Thứ Sáu `2026-07-17`.
    5. Chọn **End Date** là Thứ Hai `2026-07-20`.
    6. Đảm bảo phần **Work Shift** được chọn là `FullDay`.
    7. Nhập ghi chú `UAT-P3D-TC01` và bấm nút gửi đơn.
*   **Kết quả mong đợi**:
    *   Hệ thống tính toán thời gian nghỉ của đơn phép chính xác là **2.0 ngày** (Thứ Bảy và Chủ Nhật được loại trừ thành công).
*   **Tiêu chí Vượt qua**: Đơn phép được tạo thành công và hiển thị trên danh sách với số ngày nghỉ là `2.0`.
*   **Minh chứng cần chụp**: Ảnh chụp chi tiết đơn phép hiển thị khoảng ngày Thứ Sáu tới Thứ Hai và số ngày tính toán tương ứng.
*   **Dọn dẹp**: Bấm hủy đơn phép sau khi hoàn tất kiểm tra để giải phóng số dư phép.

---

### TC-02: Chặn đơn phép chỉ chứa ngày nghỉ
*   **Mục đích**: Xác minh đơn phép không cho phép gửi nếu khoảng thời gian đăng ký chỉ nằm trọn trong các ngày nghỉ cuối tuần thông thường.
*   **Tài khoản sử dụng**: Tài khoản Nhân viên (`uat.p3d.employee01@hrm.local`).
*   **Quyền yêu cầu**: `CREATE_LEAVE_REQUEST`.
*   **Điều kiện trước**: Lịch cuối tuần là ngày nghỉ mặc định.
*   **Màn hình / Đường dẫn**: `/leave-request`
*   **Các bước thao tác**:
    1. Đăng nhập tài khoản Nhân viên.
    2. Bấm nút **Create New Leave Request**.
    3. Chọn **Start Date** là Thứ Bảy `2026-07-25`.
    4. Chọn **End Date** là Chủ Nhật `2026-07-26`.
    5. Bấm nút gửi đơn.
*   **Kết quả mong đợi**:
    *   Hệ thống chặn không cho gửi đơn. Một thông báo lỗi hoặc thông tin xác thực hiển thị mã lỗi `LeaveRequest.OnlyNonWorkingDays` (hoặc thông báo dịch tương đương thân thiện với người dùng).
*   **Tiêu chí Vượt qua**: Đơn phép không được lưu vào hệ thống, form đăng ký giữ nguyên và hiển thị lỗi trực quan.
*   **Minh chứng cần chụp**: Ảnh chụp thông điệp lỗi xuất hiện trên màn hình đăng ký đơn phép.

---

### TC-03: Nghỉ phép giao cắt qua Ngày lễ (Public Holiday)
*   **Mục đích**: Xác minh ngày nghỉ lễ được thiết lập trên lịch làm việc sẽ được loại trừ chính xác khỏi thời gian nghỉ phép của đơn.
*   **Tài khoản sử dụng**: Admin (để thiết lập), Nhân viên (để kiểm thử đơn phép).
*   **Quyền yêu cầu**: Admin: `UPDATE_WORK_CALENDAR`. Nhân viên: `CREATE_LEAVE_REQUEST`.
*   **Điều kiện trước**: Cấu hình Thứ Hai `2026-08-10` là ngày nghỉ lễ quốc gia trên lịch.
*   **Màn hình / Đường dẫn**: `/work-calendar` (đối với Admin), sau đó `/leave-request` (đối với Nhân viên).
*   **Các bước thao tác**:
    1. Đăng nhập tài khoản **Admin**. Truy cập `/work-calendar`.
    2. Bấm nút **Add Calendar Day**.
    3. Nhập **Date** là Thứ Hai `2026-08-10`, chọn **Day Type** là `PublicHoliday`, chọn **Shift** là `None`, nhập mô tả **Description** là `UAT-P3D-TC03-Holiday`, và bấm nút **Save**.
    4. Đăng xuất và đăng nhập lại bằng tài khoản **Nhân viên** (`uat.p3d.employee01@hrm.local`).
    5. Truy cập `/leave-request` và bấm nút **Create New Leave Request**.
    6. Chọn **Start Date** là Thứ Sáu `2026-08-07` và **End Date** là Thứ Ba `2026-08-11`.
    7. Chọn chế độ `FullDay` và bấm gửi đơn.
*   **Kết quả mong đợi**:
    *   Hệ thống tính toán thời gian nghỉ phép chính xác là **2.0 ngày** (loại trừ Thứ Bảy, Chủ Nhật và ngày lễ Thứ Hai `2026-08-10`. Chỉ tính ngày phép cho Thứ Sáu và Thứ Ba).
*   **Tiêu chí Vượt qua**: Đơn phép được tạo thành công với thời gian nghỉ hiển thị chính xác là `2.0`.
*   **Minh chứng cần chụp**: Ảnh màn hình lịch làm việc của Admin hiển thị Thứ Hai `2026-08-10` là ngày lễ, và chi tiết đơn phép đã tạo của nhân viên hiển thị số ngày nghỉ là 2.0.

---

### TC-04: Đơn phép nửa ngày hợp lệ trong ngày làm việc nửa buổi (MorningOnly)
*   **Mục đích**: Xác minh đăng ký nghỉ phép ca sáng trên một ngày được cấu hình chỉ làm buổi sáng sẽ tính là 0.5 ngày phép nghỉ thành công.
*   **Tài khoản sử dụng**: Admin (để thiết lập), Nhân viên (để kiểm thử).
*   **Quyền yêu cầu**: Admin: `UPDATE_WORK_CALENDAR`. Nhân viên: `CREATE_LEAVE_REQUEST`.
*   **Điều kiện trước**: Thiết lập Thứ Ba `2026-08-18` là ngày làm việc ca sáng.
*   **Màn hình / Đường dẫn**: `/work-calendar` (Admin), `/leave-request` (Nhân viên).
*   **Các bước thao tác**:
    1. Đăng nhập tài khoản **Admin**. Truy cập `/work-calendar`.
    2. Bấm nút **Add Calendar Day**. Thiết lập ngày **Date** là Thứ Ba `2026-08-18`, chọn **Day Type** là `StandardWorkingDayOverride`, chọn **Shift** là `MorningOnly`, nhập mô tả **Description** là `UAT-P3D-TC04`, và bấm nút **Save**.
    3. Đăng xuất và đăng nhập lại bằng tài khoản **Nhân viên** (`uat.p3d.employee01@hrm.local`).
    4. Bấm nút **Create New Leave Request** cho ngày đăng ký nghỉ phép `2026-08-18`.
    5. Tại mục **Work Shift**, chọn ca nghỉ là `Morning`.
    6. Bấm gửi đơn phép.
*   **Kết quả mong đợi**:
    *   Đơn nghỉ phép được tạo thành công và thời gian nghỉ của đơn ghi nhận chính xác là **0.5 ngày**.
*   **Tiêu chí Vượt qua**: Đơn phép hoàn thành tạo mới với thông số thời gian là `0.5`.
*   **Minh chứng cần chụp**: Ảnh chụp chi tiết đơn phép vừa tạo hiển thị số ngày nghỉ là 0.5.

---

### TC-05: Chặn ca nghỉ phép không hợp lệ trên ngày làm việc nửa buổi (MorningOnly)
*   **Mục đích**: Xác minh hệ thống chặn không cho phép nhân viên đăng ký ca nghỉ chiều (Afternoon) trên một ngày lịch được thiết lập chỉ làm việc buổi sáng.
*   **Tài khoản sử dụng**: Nhân viên (`uat.p3d.employee01@hrm.local`).
*   **Quyền yêu cầu**: `CREATE_LEAVE_REQUEST`.
*   **Điều kiện trước**: Ngày `2026-08-18` đang có cấu hình lịch làm việc chỉ có ca sáng `MorningOnly` (đã tạo ở TC-04).
*   **Màn hình / Đường dẫn**: `/leave-request`
*   **Các bước thao tác**:
    1. Đăng nhập tài khoản **Nhân viên**.
    2. Bấm nút **Create New Leave Request** cho ngày nghỉ phép `2026-08-18`.
    3. Tại mục **Work Shift**, chọn ca nghỉ là `Afternoon`.
    4. Bấm nút gửi đơn.
*   **Kết quả mong đợi**:
    *   Hệ thống chặn việc gửi đơn. Hiển thị mã lỗi / thông điệp lỗi: `LeaveRequest.InvalidShiftRegistration`.
*   **Tiêu chí Vượt qua**: Đơn nghỉ phép bị từ chối và không được lưu vào cơ sở dữ liệu.
*   **Minh chứng cần chụp**: Ảnh màn hình hiển thị thông báo lỗi chặn đăng ký ca chiều.

---

### TC-06: Chặn chỉnh sửa lịch làm việc trong quá khứ (Past calendar edit guard)
*   **Mục đích**: Kiểm tra các cơ chế bảo mật và chặn nghiệp vụ đối với hành vi sửa lịch làm việc của các ngày đã qua.
*   **Tài khoản sử dụng**: Admin.
*   **Quyền yêu cầu**: `UPDATE_WORK_CALENDAR`.
*   **Điều kiện trước**: Chọn một ngày nằm trong quá khứ (trước ngày hiện tại `2026-07-07`, ví dụ: ngày `2026-07-06`).
*   **Màn hình / Đường dẫn**: `/work-calendar`
*   **Các bước thao tác**:
    1. Đăng nhập tài khoản **Admin**. Mở trang `/work-calendar`.
    2. Chọn một ngày trong quá khứ trên lưới lịch (ví dụ: ngày `2026-07-06`) hoặc nhấp nút **Add Calendar Day** và gõ một ngày trong quá khứ.
    3. Điền thông tin và bấm nút lưu lại các thay đổi của ngày này.
*   **Kết quả mong đợi**:
    *   Hành động lưu bị từ chối, ứng dụng báo lỗi `WorkCalendar.PastEditingNotAllowed`.
*   **Trạng thái kiểm thử (Status)**: **PASS**
*   **Bằng chứng kiểm thử**: Đã được xác minh và ghi lại trong báo cáo [Báo cáo UAT TC-06](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/reports/2026-07-08_1300_phase-3d_work-calendar-past-date_report.md).
*   **Chi tiết thực tế**:
    *   **Manual UI Flow**: Khi nhập ngày trong quá khứ, nút **Save** bị khóa và màn hình hiển thị lỗi `Configuring calendar days for past dates is not allowed`.
    *   **Excel Import Flow**: Các dòng có ngày trong quá khứ được gắn trạng thái **Invalid** kèm thông báo lỗi chi tiết, đồng thời nút **Apply Import** bị vô hiệu hóa để bảo vệ tính toàn vẹn dữ liệu.

---

### TC-07: Xem trước và cập nhật lũy tiến dữ liệu qua tệp Excel (Excel import)
*   **Mục đích**: Xác minh tính năng tải tệp Excel lên, xem trước dữ liệu khớp và hiển thị hàng lỗi trực quan trước khi thực hiện lưu dữ liệu lịch làm việc.
*   **Tài khoản sử dụng**: Admin.
*   **Quyền yêu cầu**: `UPDATE_WORK_CALENDAR`.
*   **Điều kiện trước**: Đã có sẵn một số cấu hình ngày lịch làm việc cụ thể trên hệ thống.
*   **Màn hình / Đường dẫn**: `/work-calendar`
*   **Các bước thao tác**:
    1. Đăng nhập tài khoản **Admin**. Truy cập trang `/work-calendar`.
    2. Bấm nút **Download Template** và mở file Excel mẫu vừa tải xuống.
    3. Điền dữ liệu vào file gồm 9 dòng thông tin như sau:
        *   5 dòng tạo mới ngày làm việc hợp lệ (ví dụ các ngày từ `2026-08-01` đến `2026-08-05` chọn loại `StandardWorkingDayOverride`)
        *   2 dòng cập nhật dữ liệu của ngày lịch làm việc đã tồn tại trên lưới
        *   1 dòng vô hiệu hóa ngày lịch cũ (đặt trường `IsActive = FALSE`)
        *   1 dòng chứa định dạng ngày không hợp lệ (ví dụ: `invalid-date-xyz`)
    4. Lưu tệp dưới tên `phase3d_uat_import.xlsx`.
    5. Tại màn hình `/work-calendar`, bấm nút **Import Excel**.
    6. Chọn tệp vừa tạo và bấm nút gửi tệp để tải lên (Upload).
*   **Kết quả mong đợi**:
    *   Ứng dụng chuyển hướng sang trang xem trước tại `/work-calendar/preview/{batchId}`.
    *   Giao diện hiển thị danh sách gồm 8 dòng hợp lệ và 1 dòng bị lỗi được bôi đỏ kèm theo lý do lỗi chi tiết.
    *   Nút xác nhận thực thi (Apply) bị khóa/vô hiệu hóa (disabled).
    *   Không có dữ liệu thực tế nào được lưu xuống database (các dữ liệu lịch làm việc cũ của grid vẫn giữ nguyên trạng thái trước khi upload).
*   **Tiêu chí Vượt qua**: Trang preview tải đúng cấu trúc dữ liệu, tô đỏ dòng lỗi chính xác, và khóa nút bấm lưu khi còn lỗi format.
*   **Minh chứng cần chụp**: Ảnh màn hình trang xem trước hiển thị dòng lỗi định dạng và nút Apply bị disabled.

---

### TC-08A: Tự động cập nhật tính toán lại khi sửa lịch thủ công (TC-08A Manual)
*   **Mục đích**: Xác minh việc sửa ngày nghỉ thành ngày làm việc thủ công trên giao diện sẽ tự động cập nhật lại các đơn nghỉ phép đã phê duyệt bị ảnh hưởng và hoàn trả số dư phép cũ.
*   **Tài khoản sử dụng**: Nhân viên (để tạo đơn), Admin (để duyệt đơn phép và sửa lịch làm việc).
*   **Quyền yêu cầu**: Admin: `UPDATE_WORK_CALENDAR`, `APPROVE_LEAVE_REQUEST`. Nhân viên: `CREATE_LEAVE_REQUEST`.
*   **Điều kiện trước**:
    1. Đăng nhập Admin, cấu hình ngày Thứ Tư `2026-08-26` là ngày lễ `PublicHoliday`.
    2. Đăng nhập Nhân viên (`uat.p3d.employee01@hrm.local`), tạo một đơn xin nghỉ phép 3 ngày từ Thứ Hai `2026-08-24` đến Thứ Tư `2026-08-26`.
    3. **Thời gian nghỉ cũ tính toán là 2.0 ngày** (do Thứ Tư là ngày lễ không tính thời gian phép).
    4. Đăng nhập Admin và bấm duyệt (Approve) đơn phép này. Ghi nhận thời gian nghỉ cũ (`2.0` ngày) và giá trị số ngày phép đã sử dụng `UsedDays` trong số dư phép `LeaveBalance` của nhân viên.
*   **Màn hình / Đường dẫn**: `/work-calendar` (Admin), `/leave-request` (Nhân viên).
*   **Các bước thao tác**:
    1. Đăng nhập **Admin** và truy cập trang `/work-calendar`.
    2. Click vào ngày Thứ Tư `2026-08-26` và sửa thành ngày làm việc bình thường (`StandardWorkingDayOverride` hoặc chuyển `IsActive = FALSE` cho cấu hình lễ cũ). Bấm nút **Save** để cập nhật lịch.
    3. Điều hướng tới menu `/leave-request` tìm lại đơn nghỉ phép của nhân viên nói trên.
*   **Kết quả mong đợi**:
    *   Cập nhật thay đổi ngày lịch làm việc thành công.
    *   Trạng thái đơn nghỉ phép đã duyệt tự động chuyển ngược từ `Approved` về trạng thái chờ xử lý `Pending`.
    *   Các thông tin phê duyệt cũ (`ProcessedBy` và `ProcessedAt`) của bản ghi nghỉ phép được xóa về giá trị trống (`null`).
    *   **Số ngày đã nghỉ UsedDays trong LeaveBalance của nhân viên được hoàn trả chính xác** (giảm đi đúng bằng số ngày nghỉ cũ `2.0`). Giá trị thời gian nghỉ mới chưa được trừ vào UsedDays cho đến khi được duyệt lại.
    *   **Số ngày nghỉ của đơn được tính toán lại thành 3.0 ngày** (Thứ Tư nay đã là ngày làm việc nên tính thêm 1.0 ngày phép).
    *   Bản ghi log lịch sử tính lại ngày phép (Recalculation Audit Log) ghi nhận thành công thông tin người thay đổi, thời gian nghỉ cũ/mới và các thông tin phê duyệt đã bị gỡ bỏ.
    *   Nội dung lý do viết trong đơn của nhân viên được giữ nguyên vẹn.
    *   Màn hình chi tiết đơn phép hiển thị banner cảnh báo đơn này đã bị tính toán lại do thay đổi cấu hình ngày lịch.
*   **Tiêu chí Vượt qua**: Trạng thái đơn phép chuyển về `Pending`, số ngày nghỉ tăng lên `3.0`, UsedDays được hoàn trả tương đương, và xuất hiện banner cảnh báo recalculation trên UI đơn phép.
*   **Minh chứng cần chụp**: Ảnh chụp giao diện đơn nghỉ phép sau tính toán lại hiển thị trạng thái `Pending`, số ngày phép `3.0` và banner thông báo.

---

### TC-08B: Tính toán lại ngày phép khi sửa lịch qua tệp Excel (TC-08B Import)
*   **Mục đích**: Xác minh tính năng sửa đổi lịch làm việc hàng loạt bằng file Excel import cũng kích hoạt đầy đủ tiến trình recalculation ngày phép và liệt kê đầy đủ đơn bị ảnh hưởng trên trang tổng hợp kết quả (summary page).
*   **Tài khoản sử dụng**: Nhân viên (để tạo đơn), Admin (để import tệp).
*   **Quyền yêu cầu**: Admin: `UPDATE_WORK_CALENDAR`, `APPROVE_LEAVE_REQUEST`. Nhân viên: `CREATE_LEAVE_REQUEST`.
*   **Điều kiện trước**:
    1. Đăng nhập Admin, cấu hình ngày Thứ Tư `2026-09-02` là ngày lễ `PublicHoliday`.
    2. Đăng nhập Nhân viên (`uat.p3d.employee01@hrm.local`), tạo đơn nghỉ phép 3 ngày từ Thứ Hai `2026-08-31` đến Thứ Tư `2026-09-02`.
    3. **Thời gian nghỉ phép tính toán ban đầu là 2.0 ngày** (Thứ Tư `2026-09-02` là ngày lễ được loại trừ).
    4. Đăng nhập Admin và phê duyệt đơn phép này. Ghi nhận thời gian nghỉ cũ (`2.0` ngày).
*   **Màn hình / Đường dẫn**: `/work-calendar` (Admin), `/work-calendar/preview/{batchId}` và `/work-calendar/summary/{batchId}`.
*   **Các bước thao tác**:
    1. Đăng nhập **Admin**. Mở trang `/work-calendar`.
    2. Chuẩn bị tệp Excel `phase3d_uat_recalc.xlsx` có dòng sửa Thứ Tư `2026-09-02` thành ngày làm việc (`StandardWorkingDayOverride` hoặc `IsActive = FALSE`).
    3. Chọn **Import Excel** để tải tệp lên.
    4. Trên màn hình xem trước `/work-calendar/preview/{batchId}`, nhấn nút **Apply**.
    5. Hệ thống xử lý import và tự động chuyển hướng người dùng đến trang tổng hợp kết quả import (hoặc trang xem kết quả recalculation tại `/work-calendar/summary/{batchId}`).
*   **Kết quả mong đợi**:
    *   Tệp Excel import được áp dụng thành công.
    *   Trang kết quả `/work-calendar/summary/{batchId}` (hoặc giao diện summary tương đương) hiển thị danh sách các đơn nghỉ phép bị ảnh hưởng bởi thay đổi này, bao gồm đơn xin phép của nhân viên đang kiểm thử với thông tin chuyển trạng thái từ `Approved` sang `Pending`.
    *   Số ngày nghỉ phép của đơn được cập nhật tính toán lại thành `3.0` ngày và số ngày phép đã sử dụng `UsedDays` được hoàn trả tương tự TC-08A.
*   **Tiêu chí Vượt qua**: Quá trình import hoàn tất và trang summary hiển thị đúng đơn phép bị ảnh hưởng kèm thông số trạng thái thay đổi.
*   **Minh chứng cần chụp**: Ảnh màn hình trang tổng hợp kết quả import (`/work-calendar/summary/{batchId}`) hiển thị dòng đơn phép bị tác động.

---

## 7. Các kịch bản kiểm thử giao diện UI và Bảo mật bổ sung

*   **TC-UI-SIDEBAR**: Đăng nhập tài khoản nhân viên thông thường không được gán quyền `VIEW_WORK_CALENDAR`. Xác minh menu "Work Calendar" hoàn toàn biến mất khỏi sidebar bên trái.
*   **TC-SEC-DIRECT**: Thử gõ trực tiếp URL `/work-calendar` vào thanh địa chỉ trình duyệt khi chưa đăng nhập hoặc khi đăng nhập bằng tài khoản không có quyền. Xác minh ứng dụng trả về trang lỗi truy cập `403 Forbidden` hoặc chuyển hướng sang trang báo lỗi từ chối truy cập (Access Denied).
*   **TC-UI-MODAL-MANUAL**: Đăng nhập Admin, bấm nút "Add Calendar Day". Xác minh modal được thiết kế căn giữa màn hình, cấu trúc Flowbite chuẩn, có nút đóng `X` ở góc trên bên phải, và các nút chân trang (footer) Cancel / Save được sắp xếp nằm ngang theo tông màu xanh chủ đạo `bg-blue-700`.
*   **TC-UI-MODAL-IMPORT**: Đăng nhập Admin, bấm nút "Import Excel". Xác minh giao diện modal import đồng bộ thiết kế với modal nhập tay, nút thực hiện tải lên sử dụng tông xanh chuẩn `bg-blue-700 hover:bg-blue-800`.
*   **TC-UI-NO-ALERT**: Thử để trống các trường bắt buộc và nhấn nút lưu ở các modal. Xác minh rằng hệ thống hiển thị cảnh báo lỗi bằng các thẻ div/span màu đỏ trên trang, tuyệt đối không kích hoạt hộp thoại popup mặc định của trình duyệt (`window.alert` / `window.confirm`).
*   **TC-VAL-IMPORT-FORMAT**: Chọn một file có định dạng không đúng (ví dụ: file hình ảnh `.png` hoặc file tài liệu `.txt`) để tải lên trong modal Import.
    *   **Kết quả mong đợi**: Ứng dụng từ chối tải tệp trực tiếp bằng mã script client-side hoặc trả về thông điệp báo lỗi định dạng tệp tải lên rõ ràng trên giao diện server-side.
*   **TC-FILE-TEMPLATE**: Bấm tải tệp mẫu bằng nút "Download Template". Mở tệp Excel đã tải về và xác nhận tệp chứa đúng các tiêu đề cột theo chuẩn: `Date`, `DayType`, `WorkShift`, `Description`, `IsActive`.

---

## 8. Bảng ghi nhận kết quả kiểm thử (Result Recording Table)

| Mã kịch bản (TC) | Tên người test | Ngày giờ thực hiện | Kết quả (PASS/FAIL/BLOCKED) | Đường dẫn file ảnh minh chứng | Ghi chú / Chi tiết lỗi |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **TC-01** | | | | | |
| **TC-02** | | | | | |
| **TC-03** | | | | | |
| **TC-04** | | | | | |
| **TC-05** | | | | | |
| **TC-06** | User | 2026-07-08 | `PASS` | [2026-07-08_1300_phase-3d_work-calendar-past-date_report.md](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/reports/2026-07-08_1300_phase-3d_work-calendar-past-date_report.md) | Đã triển khai và xác minh guard chặn chỉnh sửa lịch làm việc quá khứ thành công ở cả UI và Excel import. |
| **TC-07** | User | 2026-07-08 | `PASS WITH ISSUE` | Browser screenshot local | Excel import xử lý thành công và cập nhật 7 ngày; ghi nhận thêm TD-14 vì màn hình Import Summary hiển thị sai format Duration Change và Status Change gây nhiễu. |
| **TC-08A**| | | | | |
| **TC-08B**| | | | | |
| **TC-UI-SIDEBAR** | | | | | |
| **TC-SEC-DIRECT** | | | | | |
| **TC-UI-MODAL-MANUAL**| | | | | |
| **TC-UI-MODAL-IMPORT**| | | | | |
| **TC-UI-NO-ALERT**| | | | | |
| **TC-VAL-IMPORT-FORMAT**|| | | | |
| **TC-FILE-TEMPLATE**| | | | | |

---

## 9. Nợ kỹ thuật và case xử lý sau (Deferred Technical Debt / Follow-up Cases)

Các mục dưới đây được rà lại từ plan Phase 3D, checklist UAT và code hiện tại. Không tự động xem toàn bộ các mục là blocker của Phase 3D hiện tại; cần phân biệt rõ giữa **nợ có thể xử lý sau**, **gap nên sửa trước UAT cuối**, và **ghi chú quy trình verify**.

| Mã nợ | Nội dung | Tầng ảnh hưởng | Trạng thái đề xuất | Ghi chú kiểm chứng |
| :--- | :--- | :--- | :--- | :--- |
| **TD-01** | Quan hệ cha-con giữa `DayType` và `WorkShift` đang được validate lặp lại ở nhiều nơi (`SaveManualCalendarDay`, `PreviewManualCalendarChange`, `CalendarImportService`, JavaScript UI). | Application / UI | Xử lý sau bằng một policy/helper dùng chung | Nên gom thành `CalendarDayTypePolicy.GetAllowedWorkShifts(...)` hoặc tương đương để tránh lệch rule giữa manual modal, preview và Excel import. |
| **TD-02** | Chưa có validation theo ngày trong tuần cho hai loại Thứ Bảy làm việc: `StandardWorkingDayOverride` và `WorkingSaturdayOverride`. | Application / Domain / UI | Xử lý sau nếu Phase 3D UAT chấp nhận phạm vi hiện tại | Business đã chốt hai loại này là **Thứ Bảy cố định** và **Thứ Bảy bổ sung**. Hiện code chỉ validate `WorkShift != None`, chưa chặn dùng chúng trên Chủ nhật hoặc ngày thường. |
| **TD-03** | Label UI hiện vẫn khá kỹ thuật: `Standard Working Day Override` và `Working Saturday Override`. | UI / UX | Xử lý sau | Nên đổi label thân thiện hơn, ví dụ `Fixed Working Saturday` và `Additional Working Saturday`, nhưng giữ nguyên enum/value backend để không phát sinh migration không cần thiết. |
| **TD-04** | Cấu hình tùy chọn trong tương lai cho phép bật/tắt tính năng chỉnh sửa lịch quá khứ hoặc phân quyền riêng biệt (ví dụ: `UPDATE_PAST_WORK_CALENDAR`). | Application / Domain / Permission | Cải tiến trong tương lai (Future enhancement) | Chức năng chặn chỉnh sửa lịch quá khứ (Past-Date Guard) đã được triển khai đầy đủ và hoạt động hoàn hảo (TC-06 PASS). Việc bổ sung cấu hình bật/tắt hoặc flag phân quyền động chỉ là cải tiến tuỳ chọn cho tương lai, hoàn toàn không phải blocker. |
| **TD-05** | Chưa có luồng Employee sửa/resubmit đơn sau khi đơn Approved bị reopen về Pending do calendar recalculation. | Web.Backend / Application / UI | Xử lý sau | Hiện có banner cảnh báo ở chi tiết đơn và đơn chuyển về Pending, nhưng chưa có màn hình/API edit/resubmit. Employee có thể cần cancel và tạo đơn mới nếu chưa có chức năng sửa. |
| **TD-06** | File upload validation mới ở mức tối thiểu: input HTML có `accept=".xlsx"` và server parse bằng EPPlus, nhưng chưa có validation rõ extension/content-type/size limit trước khi parse. | Controller / Application / Security | Xử lý sau hoặc sửa trước hardening | TC-VAL-IMPORT-FORMAT phải ghi nhận rõ server trả lỗi thân thiện hay chưa. |
| **TD-07** | Import template đang cần đối chiếu lại cột `IsActive`. Checklist yêu cầu template có `IsActive`, import parser có hỗ trợ `IsActive`, nhưng controller template cần được xác minh có xuất đủ cột này hay không. | Web.Backend / UX / Import | **Nên xử lý trước UAT import cuối**, không nên để backlog lâu | Nếu template thiếu `IsActive`, HR vẫn import được do parser mặc định active, nhưng template không còn đúng với checklist và khó test deactivate row. |
| **TD-08** | Build solution có thể fail ở bước copy apphost nếu `Web.Backend.exe` đang chạy. | DevOps / Verification | Không phải product bug; ghi chú quy trình | Dừng app trước khi chạy `dotnet build HRM_Leave_Management/LUC.sln --no-restore` để có exit code sạch. |
| **TD-09** | `PreviewManualCalendarChangeQueryHandler` còn tham chiếu chuỗi `StandardNonWorkingDayOverride`, trong khi enum `CalendarDayType` hiện không có giá trị này. | Application / Maintainability | Xử lý sau hoặc dọn trước final review | Đây là dấu hiệu stale design/dead branch. Hiện không chặn flow chính vì UI không gửi giá trị này, nhưng nên xóa hoặc thay bằng rule rõ ràng để tránh hiểu nhầm. |
| **TD-10** | Trang Preview import vẫn còn style `indigo` cho nút `Apply Import`. | UI / UX | Xử lý sau hoặc dọn cùng pass UI consistency | Index page đã chuyển về HRM blue; Preview page nên đồng bộ nếu muốn UI sạch hoàn toàn. |
| **TD-11** | Chưa có UAT browser thực tế ghi nhận kết quả cuối cho toàn bộ Phase 3D theo checklist mới. | UAT / Verification | **Bắt buộc trước khi claim UAT PASS** | Checklist đã có bước test, nhưng phải chạy và ghi kết quả PASS/FAIL/BLOCKED cùng evidence. |
| **TD-12** | Excel template import Work Calendar chưa có dropdown chọn sẵn cho `DayType`, `WorkShift`, `IsActive`. | Excel Template / UX / Import Safety | Xử lý sau, nên đưa vào template hardening | Nên tạo sheet ẩn `Options` và dùng Excel Data Validation List để HR chọn giá trị hợp lệ, tránh nhập sai enum hoặc sai định dạng text. |
| **TD-13** | Excel template và parser chưa có chiến lược chặt chẽ cho định dạng ngày nhập tay, dễ nhầm giữa `8/7/2026`, `8-7-2026`, kiểu ngày trước-tháng sau hoặc tháng trước-ngày sau. | Excel Template / Import Parser / Validation | Xử lý sau, nên ưu tiên trước khi dùng rộng rãi | Khuyến nghị template format cột `Date` là Excel Date thật với display `yyyy-mm-dd`, kèm validation/preview reject ngày text mơ hồ. Parser nên ưu tiên Excel serial date/DateTime và chỉ chấp nhận text ISO `yyyy-MM-dd` nếu bắt buộc nhập tay. |
| **TD-14** | Màn hình Import Summary / Recalculation Audit Logs sau TC-07 hiển thị `Duration Change` sai định dạng, ví dụ `0.50:0.# -> 0.00:0.#`, và `Status Change` hiển thị `Pending -> Pending` dù trạng thái không đổi. | Web.Backend / Razor UI / Formatting | Xử lý sau UAT hiện tại, nên sửa trước final polish | Core import đã xử lý thành công (`Calendar Days Modified = 7 days`, affected request = 1). Đây là lỗi hiển thị/audit presentation: duration nên hiển thị dạng `0.5 days -> 0.0 days` hoặc `0.50 -> 0.00`; nếu trạng thái không đổi nên hiển thị `No status change` hoặc để trống để tránh hiểu nhầm. |

### Phân loại nhanh

*   **Có thể xử lý sau nếu user chấp nhận scope Phase 3D hiện tại**: TD-01, TD-02, TD-03, TD-04, TD-05, TD-06, TD-09, TD-10, TD-14.
*   **Nên sửa/verify trước khi chạy UAT import cuối hoặc trước khi dùng import rộng rãi**: TD-07, TD-12, TD-13.
*   **Bắt buộc trước khi claim UAT PASS**: TD-11.
*   **Không phải lỗi sản phẩm, chỉ là quy trình verify**: TD-08.

---

## 10. Xử lý lỗi và xác định phân tầng lỗi (Failure Handling)

Nếu xuất hiện kết quả **FAIL** ở bất kỳ ca kiểm thử nào:
1. Thực hiện lại các bước thao tác thêm một lần nữa.
2. Nếu lỗi vẫn tiếp tục xảy ra, ghi nhận trạng thái `FAIL` trên bảng kết quả và **không tự ý chỉnh sửa code nguồn của ứng dụng**.
3. Hãy xác định phân tầng gây lỗi dựa trên các tiêu chí sau:
    *   **UI / Razor / JS**: Lỗi giao diện, sai CSS, lệch nút bấm, lỗi Javascript xử lý sự kiện client, hoặc hiển thị sai chữ.
    *   **Controller**: Sai thông tin truyền tải API, lỗi định tuyến route, lỗi binding dữ liệu hoặc trả sai mã HTTP code.
    *   **Application Handler**: Các xử lý logic tính phép, MediatR command bị lỗi, hoặc lỗi ngoại lệ ràng buộc nghiệp vụ.
    *   **Domain**: Logic tính toán số ngày nghỉ phép bị sai công thức, vi phạm các quy tắc bất biến của thực thể (Entity Invariants).
    *   **Infrastructure / EF**: Lỗi truy vấn cơ sở dữ liệu, sai Entity Framework mapping, hoặc lỗi kết nối.
    *   **DB / Migration**: Thiếu cột, thiếu bảng trong database hoặc vi phạm các ràng buộc khóa chính/khóa ngoại.
    *   **Permission / Auth**: Lỗi liên quan đến Keycloak, bị chặn quyền `403 Forbidden` dù đã được gán quyền hợp lệ.
    *   **Runtime Data**: Số dư ngày phép không khớp dữ liệu, nhân viên chưa được liên kết, hoặc khoảng thời gian dữ liệu rỗng.
4. Ghi nhận chi tiết thông báo lỗi (Error log/Stack trace) và báo lại cho User/Codex để có phương án khắc phục.
