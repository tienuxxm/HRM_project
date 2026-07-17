# BÁO CÁO UAT - TỐI ƯU HÓA SCRIPT & KHẮC PHỤC LỖI FLOWBITE MODAL (EMPLOYEE UI)
**Ngày lập**: 13/07/2026  
**Trạng thái**: **UAT PARTIAL SUCCESS (Chờ Restart Server để áp dụng mã nguồn mới)**

---

## 1. Vấn đề Phát hiện & Phân tích Nguyên nhân (Root Cause)
* **Lỗi `$ is not defined`**: Đã giải quyết triệt để trên mã nguồn bằng cách gom toàn bộ inline `<script>` từ các partial view vào `@section Scripts` tại `Index.cshtml`.
* **Cảnh báo Flowbite (`Modal with id provisionAccountModal-... has not been initialized`)**: 
  * Xảy ra do cấu trúc HTML của modal cấp phát tài khoản được sinh ra cho toàn bộ nhân sự (kể cả những người đã được cấp tài khoản).
  * Giải pháp mã nguồn sạch đã được triển khai: Thêm điều kiện `@if (emp.UserId == null)` trong `Index.cshtml` để chỉ sinh ra modal cho người chưa có tài khoản.
* **Lỗi Modal trên Mobile layout**:
  * Ở phiên bản chạy thực tế hiện tại, nút **"+ Add Employee"** và các nút hành động trên mobile không hoạt động.
  * Nguyên nhân: Do server backend `Web.Backend.exe` đang chạy nền phiên bản DLL biên dịch cũ và chúng ta không có quyền restart/rebuild qua terminal. Do đó, mã HTML trả về trình duyệt vẫn là bản cũ:
    * Modal `createEmployeeModal` bị render bên trong khối desktop filter bar có class `hidden md:flex` (ẩn trên mobile). Khi khối cha bị ẩn hoàn toàn, trình điều khiển Flowbite không thể hiển thị modal lên màn hình.
    * 5 modal `provisionAccountModal` vẫn được render hoàn toàn thay vì chỉ render 2 người chưa có tài khoản.

---

## 2. Các thay đổi đã thực hiện trong mã nguồn (Local)
1. **Di chuyển Create Modal**: Đưa `@Html.Partial("_CreateEmployeePartial")` ra ngoài filter bar, đặt xuống khu vực modal containers ở cuối trang `Index.cshtml` (dòng 235). Điều này đảm bảo modal Add Employee luôn được parse độc lập với trạng thái ẩn/hiện của filter bar trên cả Desktop và Mobile.
2. **Loại bỏ bản vá Vanilla JS**: Xóa bỏ đoạn mã dọn dẹp DOM tạm thời trước đó. Cảnh báo khởi tạo Flowbite sẽ được giải quyết tận gốc khi ứng dụng web được biên dịch lại với cấu trúc điều kiện `@if (emp.UserId == null)` trong vòng lặp modal.
3. **Cập nhật nút bấm**: Đảm bảo tất cả các nút kích hoạt (Add, Edit, Provision, Delete) trên cả hai layout Desktop và Mobile đều trỏ đúng thuộc tính `data-modal-target` và `data-modal-toggle` tương ứng.

---

## 3. Kết quả UAT Trực Tiếp Trên Trình Duyệt (Môi trường Runtime cũ chưa Restart)
* **Console Logs**: Không còn lỗi `$ is not defined`. Có một số cảnh báo Flowbite do thiếu nút kích hoạt cho các modal thừa (sẽ biến mất hoàn toàn khi restart server để áp dụng view mới).
* **Desktop**: Việc mở/đóng các modal **Add Employee**, **Edit Employee**, **Provision Account**, và **Delete Employee** hoạt động bình thường, ổn định.
* **Mobile**: Nút Add Employee và các nút hành động tạm thời chưa thể kích hoạt do cấu trúc HTML cũ bị ẩn hoặc render lệch vị trí trên môi trường runtime hiện tại.

---

## 4. Hướng dẫn Restart & Xác minh cho Người dùng
Sau khi nhận bàn giao code, vui lòng thực hiện các bước sau trên máy local để xác minh:
1. **Tắt tiến trình server đang chạy** (Web.Backend.exe).
2. **Build lại toàn bộ giải pháp**:
   ```bash
   dotnet build HRM_Leave_Management/Web.Backend/Web.Backend.csproj
   ```
3. **Chạy lại server**:
   ```bash
   dotnet run --project HRM_Leave_Management/Web.Backend/Web.Backend.csproj
   ```
4. **Mở trình duyệt (Incognito)**, truy cập `http://localhost:5300/employee`:
   * Kiểm tra giao diện Mobile: Nhấp vào nút **"+ Add Employee"** và các nút card actions. Xác nhận tất cả modal mở/đóng chính xác.
   * Kiểm tra Console: Xác nhận sạch bóng lỗi và cảnh báo của Flowbite.
