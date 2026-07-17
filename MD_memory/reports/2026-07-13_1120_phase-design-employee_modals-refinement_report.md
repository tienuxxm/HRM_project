# BÁO CÁO UAT THỦ CÔNG: TINH CHỈNH MODALS EMPLOYEE (SWISS INTERNATIONAL STYLE)

> [!NOTE]
> Tài liệu này ghi nhận kết quả tinh chỉnh giao diện kỹ thuật (technical build) của 4 Modals Employee và cung cấp hướng dẫn kiểm thử thủ công (Visual UAT) trực tiếp trên trình duyệt cho người dùng.

---

## 1. THÔNG TIN PHIÊN BẢN & MÔI TRƯỜNG KỸ THUẬT
- **Trạng thái Visual UAT**: **PENDING** (Chờ người dùng kiểm tra thực tế và xác nhận).
- **Realm Keycloak**: `hrm` (Local: `http://localhost:8080`)
- **Tài khoản kiểm thử**: `admin` / `admin@hrm.local` (Password: `Admin@123456`)
- **Quy tắc thiết kế**: DP Style 06 (Swiss International HR - 0px border-radius, hairline borders, red accents `#bb0015`/`#E62429`, font chữ Geist & JetBrains Mono).
- **Trạng thái Build**: **PROCESS LOCK** (Mã nguồn C# không thay đổi. Khi chạy lệnh build, tệp `Web.Backend.exe` bị khóa bởi tiến trình ID `5500` đang chạy server. Các thay đổi về Razor `.cshtml` và CSS Tailwind sẽ có hiệu lực trực tiếp khi reload trình duyệt mà không cần build lại assembly).
- **Trạng thái CSS compiler**: **PASS** (Đã chạy thành công `npm run css:build` để rebuild `styles.css` sau khi cập nhật token màu gốc).

---

## 2. CẤU HÌNH PALETTE GỐC (TAILWIND.CONFIG.JS)
Để loại bỏ triệt để màu xanh dương (blue primary) của Flowbite/Tailwind mặc định, chúng tôi đã đưa cấu hình semantic token về hệ màu Swiss International trực tiếp trong `tailwind.config.js`:
- `primary`: `#000000` (Màu đen chủ đạo cho toàn bộ class `bg-primary`, `border-primary` của modal)
- `primary-foreground`: `#FFFFFF`
- `danger`: `#bb0015` (Màu đỏ sẫm Thụy Sĩ)
- `swiss-red`: `#bb0015`
- `swiss-accent-red`: `#E62429`
- `swiss-light`: `#FAF9F9`
- `swiss-border`: `#D1D1D1`

---

## 3. DANH SÁCH MODALS ĐÃ TINH CHỈNH (VISUAL REFACTORING SUMMARY)

| Tên Modal | Tệp Tin Partial | Màu sắc & Style | Khối Hướng dẫn / Cảnh báo (Guidance Notice) |
| :--- | :--- | :--- | :--- |
| **Add Employee** | `_CreateEmployeePartial.cshtml` | Nền trắng, viền mảnh. Header đen. Nút Action đen. | **Neutral Info Note** (Nền xám `#F4F3F3`, viền xám `#D1D1D1`, text xám đen `#4C4546`, icon `info`): `INFO: ACCOUNT ACCESS IS NOT CREATED UNTIL PROVISIONING IS COMPLETED.` |
| **Edit Employee** | `_UpdateEmployeePartial.cshtml` | Nền trắng, viền mảnh. Header đen. Nút Action đen. Input Code readonly (nền xám). | **Neutral Info Note** (Nền xám `#F4F3F3`, viền xám `#D1D1D1`, text xám đen `#4C4546`, icon `info`): `INFO: CHANGES TO EMPLOYEE DEPARTMENT OR POSITION WILL BE APPLIED IMMEDIATELY.` |
| **Provision Account** | `_ProvisionAccountPartial.cshtml` | Nền trắng, viền mảnh. Header đen. Nút Action đen. | **Neutral Info Note** (Nền xám `#F4F3F3`, viền xám `#D1D1D1`, text xám đen `#4C4546`, icon `info`): `INFO: PROVISIONING CREATES LOGIN ACCESS AND ROLE-BASED PERMISSIONS.` |
| **Confirm Deletion** | `_ConfirmDeletePartial.cshtml` | Nền trắng, viền mảnh. Header đen. Nút Action đỏ (`#E62429`). | **Caution Warning** (Nền đỏ nhạt `#FFF0F0`, viền đỏ `#E62429`, text đỏ, icon `warning`): `WARNING: DELETION MAY BE BLOCKED WHEN ACTIVE SUBORDINATES OR HRM HISTORY EXIST. EXISTING RECORDS MAY BE DEACTIVATED INSTEAD OF HARD-DELETED.` |

---

## 4. BÁO CÁO NỢ KỸ THUẬT (TECHNICAL DEBT REPORT)
Các class màu xanh dương (`blue`) còn tồn tại ngoài phạm vi module Employee & Layout/Shell (được xem là nợ kỹ thuật cần refactor ở các phase sau):
- **Work Calendar Module**:
  - `Views/WorkCalendar/Summary.cshtml` (các class `text-blue-600`, `bg-blue-50 px-2 py-1 text-xs font-bold text-blue-700 ring-1 ring-inset ring-blue-700/10`)
  - `Views/WorkCalendar/Preview.cshtml` (các class `bg-blue-50 px-2 py-0.5 text-xs font-semibold text-blue-700`, `focus:ring-blue-100`)
  - `Views/WorkCalendar/Index.cshtml` (các class `focus:ring-blue-500`, `focus:border-blue-500`, `bg-blue-600`, `hover:bg-blue-700`, `bg-blue-700`, `hover:bg-blue-800`, `text-blue-600`, `bg-blue-100`, `file:bg-blue-50`, `file:text-blue-700`, `animate-spin h-5 w-5 text-blue-600`)
- **Voucher Module (Legacy)**:
  - `Views/Voucher/ManageVoucherView.cshtml`
  - `Views/Voucher/Index.cshtml`
- **Các Module Khác (Restaurant, Membership, Loyalty)**: Vẫn còn giữ lại màu sắc nguyên bản của dự án cũ (LUC) và sẽ được dọn dẹp/refactor khi tách các dự án đó hoặc triển khai các phase tiếp theo.

---

## 5. CHECKLIST KIỂM THỬ THỦ CÔNG (MANUAL UAT STEPS)

### Điều kiện tiên quyết trước khi kiểm thử
1. Đảm bảo container Docker `keycloak-hrm` đang chạy.
2. Khởi chạy ứng dụng Web.Backend (ứng dụng hiện đang được host sẵn ở local).
3. Truy cập địa chỉ `/employee`, đăng nhập bằng tài khoản admin.
4. **Bắt buộc:** Thực hiện **Ctrl + F5** (Hard Reload) trên trình duyệt để xóa sạch cache CSS cũ.

### Kịch bản UAT 1: Modal "Add Employee" (Thêm nhân viên)
1. **Thao tác**: Nhấp vào nút **[ADD EMPLOYEE]** ở góc trên bên phải của bảng danh sách.
2. **Điểm cần quan sát (Visual Parity)**:
   - Header modal có màu nền đen (`bg-primary`), tiêu đề `ADD EMPLOYEE` viết hoa đậm nét, nút đóng [X] màu đỏ Thụy Sĩ (`#E62429`).
   - Không còn warning màu đỏ. Thay vào đó hiển thị **Info Box** nền xám trung tính với biểu tượng `info` xám và text màu đen/xám nhạt: `INFO: ACCOUNT ACCESS IS NOT CREATED UNTIL PROVISIONING IS COMPLETED.`
   - Nút **[CREATE EMPLOYEE]** màu đen (`bg-primary` đã chuyển thành màu đen #000 gốc). Không chứa màu xanh dương.

### Kịch bản UAT 2: Modal "Edit Employee" (Sửa nhân viên)
1. **Thao tác**: Nhấp vào nút **[EDIT]** trên một dòng nhân viên bất kỳ trong danh sách.
2. **Điểm cần quan sát (Visual Parity)**:
   - Tiêu đề modal hiển thị `EDIT EMPLOYEE RECORD: [MÃ_NV]`.
   - Hiển thị **Info Box** trung tính màu xám: `INFO: CHANGES TO EMPLOYEE DEPARTMENT OR POSITION WILL BE APPLIED IMMEDIATELY.` (Không khẳng định ảnh hưởng luồng phê duyệt).
   - Input **Employee Code** hiển thị ở trạng thái readonly với nền xám nhạt (`bg-[#F4F3F3]`) và con trỏ chuột dạng `cursor-not-allowed`.

### Kịch bản UAT 3: Modal "Provision Account" (Cấp tài khoản)
1. **Thao tác**: Nhấp vào nút **[PROVISION]** của một nhân viên chưa có tài khoản.
2. **Điểm cần quan sát (Visual Parity)**:
   - Tiêu đề `PROVISION ACCOUNT`.
   - Các ô nhập Username, Email, Password, và ô Select Role đều hiển thị theo phong cách phẳng, góc vuông 0px.
   - Hiển thị **Info Box** trung tính màu xám cố định: `INFO: PROVISIONING CREATES LOGIN ACCESS AND ROLE-BASED PERMISSIONS.`
   - Nút **[PROVISION ACCOUNT]** sử dụng màu đen (`bg-primary`).

### Kịch bản UAT 4: Modal "Confirm Deletion" (Xác nhận xóa)
1. **Thao tác**: Nhấp vào nút **[DELETE]** trên dòng nhân viên bất kỳ.
2. **Điểm cần quan sát (Visual Parity)**:
   - Modal hiển thị hộp cảnh báo hủy diệt góc vuông hoàn toàn.
   - Hộp cảnh báo màu hồng-đỏ (`bg-[#FFF0F0]`, viền `border-[#E62429]`, biểu tượng `warning` đỏ) với thông điệp nghiệp vụ cụ thể: `WARNING: DELETION MAY BE BLOCKED WHEN ACTIVE SUBORDINATES OR HRM HISTORY EXIST. EXISTING RECORDS MAY BE DEACTIVATED INSTEAD OF HARD-DELETED.`
   - Nút **[DELETE]** có nền màu đỏ đậm (`bg-[#E62429]`), khi hover chuyển sang màu trắng viền đỏ.
