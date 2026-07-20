# Báo Cáo Xác Minh UAT: Refactor Giao Diện Employee Directory (Swiss International)

## 1. Thông Tin Hệ Thống & Ràng Buộc Kỹ Thuật
* **Chế độ Auth**: Keycloak thực tế (Docker container `keycloak-hrm` chạy tại `http://localhost:8080`)
* **Trạng thái `UseMockAuth`**: `false`
* **Tài khoản UAT**: `admin` (hoặc `admin@hrm.local`) / Mật khẩu: `Admin@123456`
* **Quyền hạn đã cấu hình (Permissions)**: `VIEW_EMPLOYEE`, `UPDATE_EMPLOYEE`
* **Đường dẫn kiểm thử (Test URL)**: `/employee`

---

## 2. Nhật Ký Phân Tích Lỗi & Khắc Phục (Hygiene & Build)

### 2.1 Nguyên nhân lỗi (Root Cause)
* **Vấn đề**: Giao diện Employee và Layout tổng thể bị mất Sidebar và Header, chỉ hiển thị nội dung trần.
* **Nguyên nhân cốt lõi**: Trình biên dịch Tailwind CSS JIT (Just-In-Time) bị stale hoặc không nhận diện chính xác các lớp responsive và các token thiết kế Swiss International mới (như `lg:flex`, `lg:translate-x-0`, `w-260`, `bg-swiss-light`). Điều này xảy ra do cấu hình glob pattern trong `tailwind.config.js` không được giải quyết đúng trên môi trường Windows PowerShell khi chạy lệnh build CSS ban đầu.

### 2.2 Giải pháp khắc phục (Fix Implementation)
* Đã cấu hình mở rộng (extend) các token màu sắc (`swiss-light`, `swiss-border`, `swiss-red`, `swiss-accent-red`) và kích thước (`260`) vào [tailwind.config.js](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Web.Backend/tailwind.config.js).
* Thực hiện dọn dẹp các tệp tạm không cần thiết:
  * Xóa `HRM_Leave_Management/Web.Backend/tailwind-test.config.js`
  * Xóa `HRM_Leave_Management/Web.Backend/wwwroot/css/styles-test.css`
* Biên dịch lại Tailwind CSS bằng lệnh: `npm run css:build`.

---

## 3. Kết Quả Xác Minh Kỹ Thuật (Technical Build Verification)

### 3.1 Biên dịch giải pháp C# (dotnet build)
* **Lệnh chạy**: `dotnet build HRM_Leave_Management/LUC.sln`
* **Kết quả**: **PASS** (0 Error(s), 30 Warning(s))

### 3.2 Kiểm tra sự tồn tại của các lớp CSS trong styles.css
* Đã chạy lệnh kiểm chứng PowerShell (`Select-String`) trên tệp [styles.css](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Web.Backend/wwwroot/css/styles.css) và xác nhận sự tồn tại của các lớp quan trọng sau:
  * **`.w-260`** (Độ rộng Sidebar): **PASS** (Tồn tại ở dòng 1573)
  * **`.border-swiss-border`** (Màu viền tóc mảnh): **PASS** (Tồn tại ở dòng 2197)
  * **`.bg-swiss-light`** (Màu nền xám nhạt): **PASS** (Tồn tại ở dòng 2406)
  * **`.bg-swiss-red`** (Màu đỏ chủ đạo Swiss): **PASS** (Tồn tại ở dòng 2411)
  * **`.lg\:flex`** (Flexbox trên màn hình lớn): **PASS** (Tồn tại ở dòng 4204)
  * **`.lg\:translate-x-0`** (Hiện Sidebar trên màn hình lớn): **PASS** (Tồn tại ở dòng 4212)

* **Trạng thái Xác Minh Kỹ Thuật**: **PASS**

---

## 4. Trạng Thái UAT Trực Quan (Visual UAT Status)
* **Trạng thái hiện tại**: **PENDING**
* **Lý do**: Chưa tiến hành UAT bằng trình duyệt tự động (browser subagent) để chụp ảnh màn hình và đối chiếu trực quan 100% với Stitch canvas. Việc xác nhận hiển thị giao diện thực tế cần được người dùng kiểm tra trực quan trên trình duyệt local.

---

## 5. Kịch Bản UAT Thủ Công Cho Người Dùng (Manual UAT Steps)

### TC-01: Kiểm tra cấu trúc Global App Shell (Swiss International Layout)
* **Các bước**:
  1. Đăng nhập với tài khoản Admin Keycloak.
  2. Truy cập `/employee`.
  3. Kiểm tra Sidebar, Header, và Footer.
* **Kết quả kỳ vọng**:
  * Sidebar hiển thị nhãn **HRM PORTAL** có gạch chân đỏ (`swiss-underline`).
  * Header Desktop hiển thị breadcrumb `SYS / DIRECTORY / EMPLOYEES`.
  * Phía bên phải của Header hiển thị trực tiếp: `REALM: HRM | USER: admin | LOGOUT` (không bị ẩn trong dropdown).

### TC-02: Kiểm tra bảng thông tin Employee Directory (Desktop Layout)
* **Các bước**:
  1. Dùng trình duyệt trên Desktop.
  2. Quan sát bảng dữ liệu danh sách nhân viên.
* **Kết quả kỳ vọng**:
  * Các góc của bảng đều vuông vức (`0px` border-radius).
  * Tiêu đề bảng (`thead`) có nền xám nhạt (`#F5F5F5`), chữ in hoa đậm.
  * Cột mã nhân viên hiển thị dạng font monospace (`JetBrains Mono`), chữ in hoa.
  * Trạng thái hoạt động hiển thị dưới dạng badge chữ nhật không bo góc.

### TC-03: Kiểm tra giao diện trên Mobile Responsive Layout
* **Các bước**:
  1. Chuyển trình duyệt sang Mobile view (viewport < 768px).
  2. Kiểm tra Sidebar và danh sách nhân viên.
* **Kết quả kỳ vọng**:
  * Sidebar ẩn đi và có nút hamburger để kích hoạt.
  * Bảng dữ liệu chuyển thành dạng thẻ xếp chồng (stacked cards) vuông vức.
  * Chân thẻ chia đều 3 nút hành động ngăn cách bằng đường viền mảnh: `[PROVISION]`, `[EDIT]`, `[DELETE]`.
