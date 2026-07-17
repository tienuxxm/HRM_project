# Báo cáo Khắc phục Lỗi Cập nhật Nhân viên làm Mất Trạng thái Liên kết Tài khoản

## 1. Triệu chứng & Phát hiện Lỗi (Symptom & Discovery)
*   **Triệu chứng**: Khi cập nhật thông tin một nhân viên đã được liên kết tài khoản (trạng thái hiển thị trên giao diện danh sách là "Provisioned"), sau khi bấm lưu, dòng thông tin của nhân viên đó bị chuyển lại thành nút "Provision".
*   **Nguyên nhân**: Quá trình cập nhật thông tin nhân viên thông qua UI gửi yêu cầu cập nhật hồ sơ (họ tên, mã nhân viên, phòng ban, chức vụ, ngày tham gia, quản lý trực tiếp) nhưng form chỉnh sửa không chứa trường `UserId`. Khi Binding dữ liệu trong controller, thuộc tính `UserId` trong `UpdateEmployeeCommand` mang giá trị `null`. Do đó, phương thức `Update` trên Domain Model và `UpdateEmployeeCommandHandler` đã ghi đè giá trị `null` này lên thuộc tính `UserId` hiện tại của Entity `Employee`, làm mất liên kết tài khoản đang hoạt động.

## 2. Giải pháp Thực hiện (Proposed & Implemented Solution)
Để bảo toàn ranh giới kiến trúc (Clean Architecture Boundary: `Web.Backend -> Application -> Domain` và `Infrastructure -> Application/Domain`) và nguyên tắc cô lập nghiệp vụ:
*   Quy trình liên kết tài khoản (Provisioning) và hủy liên kết tài khoản (Unlinking) là các nghiệp vụ độc lập, được xử lý riêng biệt (ví dụ: thông qua `ProvisionEmployeeAccountCommandHandler` sử dụng `LinkUser()`).
*   Quy trình chỉnh sửa hồ sơ nhân viên (`UpdateEmployeeCommand`) không được tự ý sửa đổi hoặc xóa liên kết `UserId` hiện tại.
*   **Thực hiện**:
    1.  Loại bỏ tham số `Guid? UserId` khỏi bản ghi `UpdateEmployeeCommand`.
    2.  Cập nhật signature của phương thức `Update` trong lớp Domain Entity `Employee.cs` để không nhận và không gán đè trường `UserId`.
    3.  Cập nhật logic xử lý trong `UpdateEmployeeCommandHandler.cs` tương ứng để gọi phương thức `employee.Update` mà không cần truyền `userId`.

## 3. Chi tiết Mã Nguồn Thay đổi (Code Changes)

### A. Domain Layer

Lớp `HRM_Leave_Management/Domain/Employees/Employee.cs`:
```diff
-    public void Update(
-        string fullName,
-        string employeeCode,
-        DepartmentId? departmentId,
-        UserId? userId,
-        PositionId? positionId,
-        DateTime joinDate,
-        EmployeeId? managerId)
-    {
-        FullName = fullName;
-        EmployeeCode = employeeCode;
-        DepartmentId = departmentId;
-        UserId = userId;
-        PositionId = positionId;
-        JoinDate = joinDate;
-        ManagerId = managerId;
-    }
+    public void Update(
+        string fullName,
+        string employeeCode,
+        DepartmentId? departmentId,
+        PositionId? positionId,
+        DateTime joinDate,
+        EmployeeId? managerId)
+    {
+        FullName = fullName;
+        EmployeeCode = employeeCode;
+        DepartmentId = departmentId;
+        PositionId = positionId;
+        JoinDate = joinDate;
+        ManagerId = managerId;
+    }
```

### B. Application Layer

Lớp `HRM_Leave_Management/Application/Employees/Update/UpdateEmployeeCommand.cs`:
```diff
 public record UpdateEmployeeCommand(
     Guid Id,
     string FullName,
     string EmployeeCode,
     Guid? DepartmentId,
-    Guid? UserId,
     Guid? PositionId,
     DateTime JoinDate,
     Guid? ManagerId) : ICommand<Employee>;
```

Lớp `HRM_Leave_Management/Application/Employees/Update/UpdateEmployeeCommandHandler.cs`:
```diff
         DepartmentId? departmentId = request.DepartmentId.HasValue
             ? new DepartmentId(request.DepartmentId.Value)
             : null;
 
-        UserId? userId = request.UserId.HasValue
-            ? new UserId(request.UserId.Value)
-            : null;
-
         EmployeeId? managerId = request.ManagerId.HasValue
             ? new EmployeeId(request.ManagerId.Value)
             : null;
 
         PositionId? positionId = request.PositionId.HasValue
             ? new PositionId(request.PositionId.Value)
             : null;
 
-        employee.Update(request.FullName, request.EmployeeCode, departmentId, userId,
+        employee.Update(request.FullName, request.EmployeeCode, departmentId,
             positionId, DateTime.SpecifyKind(request.JoinDate, DateTimeKind.Utc), managerId);
```

## 4. Phân tích Tác động & Rủi ro (Impact Analysis)
*   **GitNexus Impact Analysis**: Chạy phân tích tác động (Impact Analysis) trả về mức độ rủi ro **Thấp (LOW)** do thay đổi chỉ khoanh vùng trong module Employee, cụ thể là API cập nhật thông tin nhân viên từ Controller `EmployeeController` gọi `UpdateEmployeeCommand`.
*   **Affected Processes**: 0 (Không làm ảnh hưởng đến luồng nghiệp vụ khác).
*   **Keycloak & Database**: Không làm thay đổi tài khoản người dùng trên Keycloak và dữ liệu người dùng (`User`) trong Database, duy trì tính ổn định của hệ thống xác thực.

## 5. Hướng dẫn Kiểm thử Thủ công (Manual UAT Guide)

Kiểm thử viên thực hiện theo các bước sau để xác nhận lỗi đã được khắc phục hoàn toàn:

### Chuẩn bị môi trường UAT
*   **Ứng dụng**: HRM Leave Management chạy tại `http://localhost:5300`
*   **Trạng thái cấu hình**: `UseMockAuth = false` (Keycloak chạy thực tế tại `http://localhost:8080`).

### Kịch bản kiểm thử: Cập nhật thông tin nhân viên đã liên kết tài khoản

1.  **Đăng nhập tài khoản Admin**:
    *   Tài khoản: `admin@hrm.local` (hoặc `admin`) / Mật khẩu: `Admin@123456`.
2.  **Truy cập Danh sách Nhân viên**:
    *   Mở trình duyệt truy cập đường dẫn `/employee`.
3.  **Xác định nhân viên mục tiêu**:
    *   Chọn một nhân viên hiện đang có trạng thái tài khoản là **"Provisioned"** (đã liên kết tài khoản Keycloak thành công).
    *   Ghi nhớ Tên hiện tại và Mã nhân viên của người này.
4.  **Thực hiện chỉnh sửa (Update)**:
    *   Nhấp vào nút **Edit** ở dòng tương ứng của nhân viên đó để mở Modal chỉnh sửa thông tin.
    *   Thay đổi **Full Name** (Ví dụ: Thêm hậu tố `_UAT` vào cuối tên).
    *   Nhấn **Save** để lưu thay đổi.
5.  **Xác nhận kết quả hiển thị trên bảng danh sách**:
    *   Hệ thống tải lại thông tin dòng nhân viên vừa sửa.
    *   Tên nhân viên hiển thị chính xác giá trị mới đã cập nhật.
    *   **Kết quả mong đợi**: Cột **Account** của nhân viên đó vẫn hiển thị nhãn màu xanh lá **"Provisioned"**, tuyệt đối không bị chuyển lại thành nút **"Provision"** màu xanh dương.
6.  **Xác nhận liên kết cơ sở dữ liệu**:
    *   Khi xem chi tiết nhân viên hoặc kiểm tra cơ sở dữ liệu, trường `UserId` của nhân viên đó vẫn giữ nguyên GUID cũ, không bị đưa về `NULL`.

## 6. Kết quả Quét Mã hóa & Cấu trúc (Encoding Scan Results)
*   **Công cụ kiểm tra**: `python MD_memory/debug/2026-06-26_1430_scan-mojibake.py --require-bom`
*   **Kết quả quét**:
    *   Số lượng files đã kiểm tra: 40
    *   Số lỗi thiếu BOM: 0 (Đã định dạng UTF-8 BOM đầy đủ)
    *   Số từ bị lỗi hiển thị (Mojibake): 0
    *   Exit code: 0

## 7. Trạng thái Git (Git Status)
Các tệp đã sửa đổi thuộc phạm vi giải quyết lỗi:
*   `HRM_Leave_Management/Domain/Employees/Employee.cs`
*   `HRM_Leave_Management/Application/Employees/Update/UpdateEmployeeCommand.cs`
*   `HRM_Leave_Management/Application/Employees/Update/UpdateEmployeeCommandHandler.cs`

> [!WARNING]
> Tuân thủ quy tắc quản lý mã nguồn Git: Chỉ `git add` đích danh các tệp trên và tệp báo cáo này khi được người dùng phê duyệt. Không dùng `git add .` hay `git add -A`.

## 8. Trạng thái Biên dịch (Build Status)
*   **Lệnh biên dịch**: `dotnet build HRM_Leave_Management/LUC.sln --no-restore`
*   **Kết quả**: **PASS** (Biên dịch thành công không có lỗi, 0 Errors, 15 Warnings).

## 9. Phục hồi dữ liệu cần thiết (Data Recovery Needed)
*   **Phòng ngừa**: Bản sửa lỗi trong mã nguồn sẽ ngăn chặn tình trạng mất liên kết tài khoản trong các lần cập nhật tiếp theo.
*   **Ảnh hưởng trước sửa lỗi**: Những nhân viên đã bị chỉnh sửa thông tin trước khi áp dụng bản vá này có thể vẫn đang có giá trị `Employee.UserId = NULL` trong cơ sở dữ liệu (tương đương hiển thị nút "Provision" trên giao diện).
*   **Khuyến nghị phục hồi**:
    *   **Không** bấm nút "Provision" lại một cách mù quáng để tránh phát sinh dữ liệu rác hoặc lỗi đồng bộ.
    *   Cần xác định chính xác các nhân viên bị ảnh hưởng và thực hiện liên kết lại (relink) với tài khoản `User` tương ứng trong hệ thống chỉ sau khi đã xác minh và được phê duyệt.
