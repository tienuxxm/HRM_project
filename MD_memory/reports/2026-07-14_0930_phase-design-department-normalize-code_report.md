# Báo cáo UAT: Chuẩn hóa định dạng mã phòng ban (Department Code Normalization)

*   **Ngày báo cáo:** 2026-07-14
*   **Trạng thái:** HOÀN THÀNH (AWAITING_USER_VERIFICATION)
*   **Mã Dự án Stitch:** `17479353588209716186`
*   **Mã thiết kế hệ thống:** Swiss International HR (Style 06)

---

## 1. Chi tiết các thay đổi đã thực hiện

Để đảm bảo Department Code luôn được chuẩn hóa thành dạng viết hoa (`UPPERCASE`), không chứa khoảng trắng thừa, và tránh trùng lặp do sai lệch chữ hoa/thường, chúng tôi đã triển khai các thay đổi sau trên các lớp kiến trúc (Web.Backend -> Application -> Domain):

### A. Lớp Domain (Domain Layer)
*   **Tệp tin:** `Domain/Departments/Department.cs`
*   **Nội dung:**
    *   Trong phương thức khởi tạo factory `Create`, thực hiện: `code.Trim().ToUpperInvariant()`.
    *   Trong phương thức cập nhật `Update`, thực hiện: `code.Trim().ToUpperInvariant()`.
    *   Đảm bảo dữ liệu trước khi gán vào thuộc tính `Code` của Entity luôn được chuẩn hóa ở mức thấp nhất.

### B. Lớp Application (Application Layer)
*   **Tệp tin 1:** `Application/Departments/Create/CreateDepartmentCommandHandler.cs`
    *   Chuẩn hóa mã phòng ban đầu vào: `var normalizedCode = request.Code.Trim().ToUpperInvariant();`.
    *   Cập nhật câu lệnh kiểm tra trùng lặp sử dụng so sánh không phân biệt chữ hoa/thường (case-insensitive):
        `_departmentRepository.IsExistedAsync(x => x.Code.ToUpper() == normalizedCode)`
    *   Truyền mã đã được chuẩn hóa (`normalizedCode`) vào hàm tạo `Department.Create`.
*   **Tệp tin 2:** `Application/Departments/Update/UpdateDepartmentCommandHandler.cs`
    *   Chuẩn hóa mã phòng ban đầu vào: `var normalizedCode = request.Code.Trim().ToUpperInvariant();`.
    *   Cập nhật câu lệnh kiểm tra trùng lặp loại trừ chính nó sử dụng so sánh không phân biệt chữ hoa/thường:
        `_departmentRepository.IsExistedAsync(x => x.Code.ToUpper() == normalizedCode && x.Id != new DepartmentId(request.Id))`
    *   Truyền mã đã được chuẩn hóa (`normalizedCode`) vào hàm cập nhật `department.Update`.

### C. Lớp Giao diện (Web.Backend View)
*   **Tệp tin:** `Web.Backend/Views/Department/Index.cshtml`
*   **Nội dung:**
    *   Cập nhật mã nguồn Javascript khi submit form thêm mới (`#saveDeptButton` click event):
        Sử dụng `const deptCode = ($('#deptCode').val() || '').trim().toUpperCase();` trước khi append vào `FormData` để đồng bộ UI hiển thị và trải nghiệm người dùng.
    *   Cập nhật mã nguồn Javascript khi submit form cập nhật (`[id^="saveDeptButton-"]` click event):
        Sử dụng `const deptCode = ($('#deptCode-' + id).val() || '').trim().toUpperCase();` trước khi append vào `FormData`.

---

## 2. Kịch bản kiểm thử thủ công (Manual UAT Guide)

Do môi trường thực thi lệnh terminal (shell) bị giới hạn quyền truy cập trực tiếp, vui lòng thực hiện kiểm thử thủ công theo quy trình dưới đây để xác minh kết quả.

### Chuẩn bị trước khi test (Prerequisites)
1. Đảm bảo dịch vụ Keycloak Docker đang hoạt động ở cổng `8080`.
2. Chạy ứng dụng Web.Backend (`dotnet run` hoặc qua Visual Studio) ở cổng `5300`.
3. Đăng nhập bằng tài khoản UAT:
   * **Username:** `admin` (hoặc `admin@hrm.local`)
   * **Password:** `Admin@123456`
4. Truy cập màn hình danh sách phòng ban: `/department`.

### Kịch bản 1: Kiểm thử chuẩn hóa tự động khi Tạo mới
1. Bấm nút **"+ Create Department"** để mở Modal thêm phòng ban.
2. Nhập thông tin:
   * **Department Code:** `  tech-ops  ` (nhập chữ thường, có khoảng trắng ở đầu và cuối).
   * **Department Name:** `Technology Operations`.
   * **Description:** `Test normalization`.
3. Bấm nút **"CREATE"** để gửi yêu cầu.
4. **Kết quả mong đợi:**
   * Hệ thống hiển thị thông báo thành công và tải lại trang.
   * Trên danh sách phòng ban, dòng mới hiển thị mã là `TECH-OPS` (đã được cắt khoảng trắng và tự động chuyển chữ hoa).
   * Trong DB, trường `Code` của phòng ban này được lưu chính xác là `TECH-OPS`.

### Kịch bản 2: Kiểm thử chặn trùng lặp mã phòng ban (Case-Insensitive Duplicate Check)
1. Bấm nút **"+ Create Department"** để mở Modal.
2. Nhập thông tin:
   * **Department Code:** `tech-ops` (hoặc bất kỳ biến thể chữ hoa/thường nào như `Tech-Ops`, `TECH-OPS`).
   * **Department Name:** `Duplicate Test Department`.
   * **Description:** `Should be blocked`.
3. Bấm nút **"CREATE"**.
4. **Kết quả mong đợi:**
   * Hệ thống chặn không cho tạo mới.
   * Hiển thị thông báo lỗi báo mã phòng ban đã tồn tại (hoặc Toast thất bại `"Department creation failed"`).

### Kịch bản 3: Kiểm thử chuẩn hóa tự động khi Chỉnh sửa (Edit)
1. Chọn phòng ban vừa tạo, bấm **"Edit"** để mở Modal chỉnh sửa.
2. Sửa thông tin **Department Code** từ `TECH-OPS` thành `  tech-dev  ` (chữ thường, có khoảng trắng).
3. Bấm nút **"SAVE"**.
4. **Kết quả mong đợi:**
   * Hệ thống cập nhật thành công.
   * Trên danh sách phòng ban, mã phòng ban được cập nhật hiển thị là `TECH-DEV`.
   * DB lưu giá trị trường `Code` là `TECH-DEV`.
