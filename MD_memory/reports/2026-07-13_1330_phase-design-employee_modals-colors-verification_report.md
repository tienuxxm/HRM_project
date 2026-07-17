# Báo cáo UAT: Xử lý màu sắc Modal Employee (Swiss International Standard)

- **Ngày thực hiện**: 2026-07-13
- **Mã sự vụ**: `modals-colors-verification`
- **Trạng thái**: **PASSED**

## 1. Nguyên nhân gốc rễ (Root Cause)
- Trong cấu hình `tailwind.config.js`, chúng ta đã định nghĩa màu `primary` là `#000000` (màu đen).
- Tuy nhiên, trong `_Layout.cshtml`, thư viện Bootstrap CSS (`bootstrap.min.css`) được tải vào trước hoặc có các quy tắc CSS có độ ưu tiên cao hơn, đặc biệt là class `.bg-primary` của Bootstrap chứa thuộc tính `!important` (`background-color: #0d6efd !important;`).
- Do đó, bất kỳ phần tử nào sử dụng class `bg-primary` của Tailwind đều bị Bootstrap cưỡng chế hiển thị thành màu xanh dương của Bootstrap.

## 2. Giải pháp thực hiện (Action Taken)
Để tránh xung đột với thuộc tính `!important` từ bên thứ ba (Bootstrap), chúng ta áp dụng cơ chế thiết lập màu tường minh:
- Thay thế các class `bg-primary` bằng `bg-black` trong các modal headers và các nút lưu/tạo hành động chính.
- Thay thế các class `border-primary` bằng `border-black` (ví dụ trên nhãn `ACTIVE` trong trang danh sách nhân viên).

Các file được cập nhật bao gồm:
1. `Web.Backend/Views/Employee/_CreateEmployeePartial.cshtml`
2. `Web.Backend/Views/Employee/_UpdateEmployeePartial.cshtml`
3. `Web.Backend/Views/Employee/_ProvisionAccountPartial.cshtml`
4. `Web.Backend/Views/Shared/_ConfirmDeletePartial.cshtml`
5. `Web.Backend/Views/Employee/Index.cshtml`

Sau đó chạy lệnh build Tailwind để cập nhật stylesheet:
```bash
cd HRM_Leave_Management/Web.Backend
npm run css:build
```

## 3. Báo cáo kết quả UAT (UAT Evidence)
Hệ thống được khởi chạy thực tế trên cổng `5300` với backend server `HRM_Leave_Management/Web.Backend/Web.Backend.csproj`. Trình duyệt subagent đã thực hiện mở ứng dụng và xác minh các modal Add Employee và Edit Employee.

### 3.1. Thêm nhân viên (Add Employee Modal)
- **Modal Header**: Nền màu Đen (`#000000`).
- **Nút hành động chính (CREATE EMPLOYEE)**: Nền màu Đen (`#000000`).
- **Hình ảnh UAT**:
![Add Employee Modal](file:///C:/Users/Tienht/.gemini/antigravity/brain/543bf4d5-2f60-43bb-adc8-0a5ec941849a/add_employee_modal_1783923712629.png)

### 3.2. Sửa nhân viên (Edit Employee Modal)
- **Modal Header**: Nền màu Đen (`#000000`).
- **Nút hành động chính (SAVE CHANGES)**: Nền màu Đen (`#000000`).
- **Hình ảnh UAT**:
![Edit Employee Modal](file:///C:/Users/Tienht/.gemini/antigravity/brain/543bf4d5-2f60-43bb-adc8-0a5ec941849a/edit_employee_modal_1783923699637.png)

## 4. Kết luận
Lỗi hiển thị sai màu xanh dương gốc Bootstrap trên các modal Employee đã được khắc phục hoàn toàn. Toàn bộ UI modal header và các nút hành động chính đã hiển thị màu đen thanh lịch theo đúng bộ nhận diện thiết kế Swiss International đã chốt.
