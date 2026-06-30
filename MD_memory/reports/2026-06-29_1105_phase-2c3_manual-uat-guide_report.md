﻿# Hướng dẫn kiểm thử thủ công (Manual UAT Guide) - Phase 2C.3

Tài liệu này cung cấp hướng dẫn từng bước (step-by-step) để USER tự thực hiện kiểm thử chấp nhận người dùng (UAT) cho các tính năng thuộc **Phase 2C.3 (Leave Request Management)** của hệ thống **HRM Leave Management**.

---

## 1. Trạng thái kỹ thuật hiện tại
* **Trạng thái Build**: Ứng dụng `Web.Backend` đã build thành công với 0 lỗi (`dotnet build` PASS).
* **URL Server đang hoạt động**:
  * HTTP: `http://localhost:5300`
  * HTTPS: `https://localhost:7068`
* **Cơ sở dữ liệu & Phân quyền**:
  * Kết nối tới cơ sở dữ liệu Postgres `hrm_baseline_db`.
  * Đã seed đầy đủ Role và Permission tương ứng cho các tài khoản UAT.
* **Chế độ Xác thực (Authentication Mode)**:
  * Sử dụng Keycloak thật (`UseMockAuth = false` trong `appsettings.json`).
  * Endpoint Keycloak local: `http://localhost:8080` (Realm: `hrm`).

---

## 2. Thông tin tài khoản UAT
Sử dụng các tài khoản sau để đăng nhập trên giao diện:
1. **Tài khoản Admin (Quản trị nhân sự/Người phê duyệt)**:
   * **Username / Email**: `admin` hoặc `admin@hrm.local`
   * **Password**: `Admin@123456`
2. **Tài khoản Employee (Nhân viên)**:
   * **Username / Email**: `employee` hoặc `employee@hrm.local`
   * **Password**: `Admin@123456`

> [!IMPORTANT]
> **Lưu ý an toàn**: Vui lòng không thay đổi mật khẩu của các tài khoản này và không truy cập vào trang quản trị Keycloak để chỉnh sửa cấu hình user/client trừ khi được yêu cầu đặc biệt.

---

## 3. Các bước UAT vai trò Admin
**Mục tiêu**: Kiểm tra giao diện quản lý dành cho Admin, đảm bảo quyền xem toàn bộ đơn và lọc theo nhân viên. (Approve/Reject chưa triển khai, sẽ làm ở Phase 3.)

1. **Bước 1**: Mở trình duyệt và truy cập: `http://localhost:5300/leave-request`.
2. **Bước 2**: Hệ thống sẽ tự động chuyển hướng đến màn hình đăng nhập của Keycloak. Nhập tài khoản **Admin** (`admin` / `Admin@123456`).
3. **Bước 3**: Sau khi đăng nhập thành công, bạn sẽ được đưa về lại trang danh sách **Leave Requests** (`http://localhost:5300/leave-request`).
4. **Bước 4**: Kiểm tra các thành phần giao diện:
   * **Bộ lọc nhân viên (Employee Filter)**: Phải hiển thị ở phía trên bảng dữ liệu (mặc định chọn "All Employees"). Click vào bộ lọc để đảm bảo danh sách nhân viên load đầy đủ.
   * **Danh sách đơn**: Phải hiển thị tất cả các đơn nghỉ phép của mọi nhân viên gửi lên hệ thống.
   * **Cột thao tác (Actions)**:
     * Đối với các đơn do nhân viên khác gửi: Hiển thị ký tự `-` (Admin không được phép hủy đơn của nhân viên khác thông qua nút Cancel của Employee).
     * Đối với đơn của chính Admin (nếu có): Hiển thị nút "Cancel" nếu đơn ở trạng thái `Pending`.
   * **Quyền phê duyệt/từ chối (Approve/Reject)**: Chưa được triển khai giao diện và logic xử lý ở Phase 2C.3 (các chức năng phê duyệt, từ chối đơn nghỉ phép sẽ được triển khai trong Phase 3). Admin hiện tại chỉ có quyền xem danh sách đơn nghỉ và lọc theo nhân viên.

---

## 4. Các bước UAT vai trò Employee
**Mục tiêu**: Kiểm tra giao diện cá nhân của nhân viên, đảm bảo tính bảo mật (chỉ xem được đơn của mình) và các tính năng tạo/hủy đơn.

1. **Bước 1**: Đăng xuất tài khoản Admin (nếu đang đăng nhập) và truy cập `http://localhost:5300/leave-request`.
2. **Bước 2**: Đăng nhập bằng tài khoản **Employee** (`employee` / `Admin@123456`).
3. **Bước 3**: Kiểm tra giao diện danh sách đơn:
   * **Bộ lọc nhân viên (Employee Filter)**: **Không được xuất hiện** trên giao diện của Employee.
   * **Danh sách đơn**: Chỉ hiển thị các đơn xin nghỉ phép của chính tài khoản `employee` (Nguyen Van Employee), không được hiển thị đơn của người khác.
4. **Bước 4**: Kiểm tra nút tạo đơn:
   * Có nút "**Request Leave**" ở góc trên bên phải để mở popup tạo đơn nghỉ phép.
5. **Bước 5**: Kiểm tra tính năng hủy đơn:
   * Ở cột **Actions**, nút "**Cancel**" chỉ xuất hiện ở những dòng đơn có trạng thái là `Pending`.
   * Đối với các đơn có trạng thái `Approved`, `Rejected` hoặc `Canceled`, cột Actions hiển thị ký tự `-`.

---

## 5. Các bước UAT kiểm thử các Validation nghiệp vụ
Khi đăng nhập bằng tài khoản **Employee**, thực hiện click nút **Request Leave** và nhập các thông số kiểm thử sau đây để xác minh các quy tắc nghiệp vụ:

### Case 1: Thời gian nghỉ phép tính theo số ngày lịch liên tục (Calendar Days)
* **Mục đích**: Đảm bảo thời gian nghỉ tính đủ ngày, không loại trừ Thứ Bảy và Chủ Nhật.
* **Dữ liệu kiểm thử**:
  * **Start Date**: `2026-10-16` (Thứ Sáu)
  * **End Date**: `2026-10-19` (Thứ Hai tuần sau)
  * **Start Day Part**: `FullDay` (giá trị submit: 1)
  * **End Day Part**: `FullDay` (giá trị submit: 1)
* **Kết quả mong đợi**: Hệ thống chấp nhận tạo đơn thành công và tính toán số ngày nghỉ (**Duration**) hiển thị là **4.0 ngày**.
* **Lưu ý**: Chọn khoảng ngày chưa có đơn Pending/Approved nào overlap. Nếu gặp lỗi `"This request overlaps..."` thì đó là validation overlap hoạt động đúng, không phải lỗi calendar days — hãy chọn khoảng ngày khác chưa bị chiếm.

### Case 2: Nghỉ cùng ngày, chọn Morning + Afternoon
* **Mục đích**: Cấm chọn tổ hợp không hợp lệ trên cùng 1 ngày nghỉ.
* **Dữ liệu kiểm thử**:
  * **Start Date**: `2026-07-10`
  * **End Date**: `2026-07-10`
  * **Start Day Part**: `Morning` (giá trị submit: 2)
  * **End Day Part**: `Afternoon` (giá trị submit: 3)
* **Kết quả mong đợi**: Hệ thống từ chối tạo đơn và hiển thị thông báo lỗi `"Invalid session selection for a single-day request"`.

### Case 3: Đơn xin nghỉ phép bắc qua hai năm khác nhau
* **Mục đích**: Cấm tạo đơn nghỉ vượt qua ranh giới năm tài chính.
* **Dữ liệu kiểm thử**:
  * **Start Date**: `2026-12-30`
  * **End Date**: `2027-01-02`
  * **Start Day Part**: `FullDay` (giá trị submit: 1)
  * **End Day Part**: `FullDay` (giá trị submit: 1)
* **Kết quả mong đợi**: Hệ thống từ chối tạo đơn và trả về thông báo lỗi `"A leave request cannot cross multiple calendar years"`.

### Case 4: Xin nghỉ vượt quá số ngày phép khả dụng (Available Days)
* **Mục đích**: Đảm bảo nhân viên không nghỉ lố phép năm.
* **Dữ liệu kiểm thử**:
  * Chọn số ngày nghỉ (ví dụ: 10 ngày) lớn hơn số phép khả dụng còn lại của Employee hiển thị trên màn hình Leave Balances.
* **Kết quả mong đợi**: Hệ thống từ chối tạo đơn và báo lỗi thiếu số dư ngày phép `"Insufficient available leave days for this request"`.

### Case 5: Tạo đơn bị trùng thời gian (Overlap) với đơn Pending/Approved khác
* **Mục đích**: Cấm đăng ký trùng lặp lịch nghỉ.
* **Dữ liệu kiểm thử**:
  * Tạo đơn mới có khoảng thời gian giao thoa (ví dụ: trùng ngày) với một đơn nghỉ khác của chính mình đang ở trạng thái `Pending` hoặc `Approved`.
* **Kết quả mong đợi**: Hệ thống từ chối tạo đơn và trả về lỗi trùng lặp `"This request overlaps with an existing pending or approved leave request"`.

### Case 6: Hủy đơn Pending thành công
* **Mục đích**: Cho phép nhân viên tự rút đơn khi chưa được duyệt.
* **Thao tác**: Click nút **Cancel** tại dòng đơn đang ở trạng thái `Pending`.
* **Kết quả mong đợi**: Đơn chuyển sang trạng thái `Canceled` (giá trị lưu DB là 4). Số dư ngày phép khả dụng của nhân viên được hoàn trả chính xác.

### Case 7: Hủy đơn đã Canceled
* **Mục đích**: Đảm bảo tính toàn vẹn dữ liệu, không thể hủy đơn nhiều lần.
* **Thao tác**: (Không thể thực hiện trên UI do nút Cancel đã biến mất). Nếu cố tình gửi request API hủy đơn đã ở trạng thái `Canceled`.
* **Kết quả mong đợi**: Hệ thống trả về lỗi từ chối thao tác (đơn không ở trạng thái Pending (Pending = 1) thì không được phép hủy).

---

## 6. Hướng dẫn xử lý khi gặp lỗi kiểm thử (Test Fail)
Nếu trong quá trình kiểm thử, một bước nào đó không trả về kết quả mong đợi hoặc gặp lỗi hệ thống (ví dụ: màn hình xoay tròn, lỗi HTTP 500, lỗi 403 Forbidden):

1. **Chụp ảnh màn hình**: Chụp lại toàn bộ giao diện màn hình tại thời điểm xảy ra lỗi (bao gồm cả thanh địa chỉ URL).
2. **Thu thập Log Server**:
   * Kiểm tra cửa sổ dòng lệnh PowerShell đang chạy câu lệnh khởi động server (`dotnet run`).
   * Copy toàn bộ nội dung log lỗi/Exception stack trace xuất hiện ở terminal.
3. **Thu thập Log Web Console & Network**:
   * Nhấn `F12` trên trình duyệt để mở Developer Tools.
   * Chuyển qua tab **Console** để xem có log lỗi màu đỏ nào không.
   * Chuyển qua tab **Network**, tìm request API bị lỗi (thường có màu đỏ, status code là 400, 403, 500), click vào request đó và copy nội dung trong tab **Response** để cung cấp cho đội kỹ thuật.
