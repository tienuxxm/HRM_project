# Kế Hoạch Refactor Form User Management (Swiss Style 06)

*   **Mã Phase:** `phase-2_user-form-refactor`
*   **Ngày tạo:** 2026-07-20
*   **Tác giả:** Senior .NET Fullstack Engineer & Technical Reviewer

---

## 1. Tuyên Bố Ranh Giới Kiến Trúc (Architecture Boundary)
Để bảo toàn tính toàn vẹn hệ thống và kiến trúc Clean Architecture, chúng tôi tuân thủ ranh giới phụ thuộc sau:
*   `Web.Backend -> Application -> Domain`
*   `Infrastructure -> Application/Domain`
*   **Cam kết:** Không sửa đổi bất kỳ mã C# backend nào (Controllers, Handlers, Repositories), không can thiệp vào database/migration, và không thay đổi cấu hình bảo mật Keycloak/Authorization.

---

## 2. Báo Cáo Trạng Thái Git (Git Baseline Status)
Trước khi bắt đầu phase mới, trạng thái Git được ghi nhận như sau:
*   **Branch hiện tại:** `main`
*   **File đã sửa đổi (Modified - chưa commit từ Phase 1):**
    *   `HRM_Leave_Management/Web.Backend/Views/LeaveApproverAssignment/Index.cshtml`
    *   `HRM_Leave_Management/Web.Backend/Views/User/Index.cshtml`
*   **Các file chưa được theo dõi (Untracked):**
    *   `.codex_tmp/`
    *   `HRM_Leave_Management/Web.Backend/tailwind.debug.config.js`
    *   `HRM_Leave_Management/Web.Backend/test.html`
    *   `MD_memory/reports/2026-07-20_0846_phase-design-approver-assignments_proposal_report.md`
    *   `MD_memory/reports/2026-07-20_0914_phase-design-user-management-audit_proposal_report.md`
    *   `MD_memory/reports/2026-07-20_1030_phase-design-user-management-audit_proposal_report.md`

*Cam kết bảo vệ lịch sử Git: Không tự ý checkout, restore, reset, clean hoặc xóa các tệp tin thuộc baseline trên.*

---

## 3. Phạm Vi Thay Đổi (Scope of Changes)
*   **Các file được phép sửa đổi:**
    1.  `HRM_Leave_Management/Web.Backend/Views/User/CreateUserView.cshtml`
    2.  `HRM_Leave_Management/Web.Backend/Views/User/Detail.cshtml`
*   **Các file tuyệt đối không được chạm vào:**
    *   `_Layout.cshtml`, sidebar, header, footer, bottom nav.
    *   `UserController.cs` và toàn bộ các file `.cs` khác.
    *   Tệp cấu hình `appsettings.json`, config Keycloak.

---

## 4. Thiết Kế Chi Tiết Theo Swiss Style 06

### 4.1. Cấu trúc Giao Diện & Layout Form
*   **Màu sắc:** Phối màu đơn sắc (Monochrome) kết hợp đen, trắng và xám nhạt (`#D1D1D1`).
*   **Bố cục (Layout):** Sử dụng lưới grid phẳng tối giản, khoảng cách thông thoáng, loại bỏ bóng đổ (box-shadow) và bo góc mềm (`rounded-lg` chuyển thành `rounded-none`).
*   **Đường kẻ (Hairline Borders):** Sử dụng các đường hairline màu `#D1D1D1` chia cắt các khối thông tin một cách dứt khoát.
*   **Typography:**
    *   Tiêu đề trang viết hoa lớn: `THE USER CREATION` / `THE USER UPDATE` font Inter/Outfit đậm nét.
    *   Label inputs viết hoa nhỏ, đậm nét (`font-mono text-[10px] tracking-wider text-[#4C4546] uppercase`).

### 4.2. Refactor Inputs & Dropdowns (SumoSelect)
*   **Text Inputs:**
    *   Chuyển từ style Tailwind mặc định sang style Swiss: `bg-white border border-[#D1D1D1] text-black text-sm rounded-none focus:ring-0 focus:border-black block w-full p-3 font-sans transition-all`.
    *   Đối với Username ở màn hình `Detail.cshtml` (Readonly): Thiết lập style phẳng `bg-[#F5F5F5] border border-[#D1D1D1] text-neutral-500 cursor-not-allowed rounded-none p-3 font-sans`.
*   **Dropdown SumoSelect:** Ghi đè CSS ngay trong view để biến SumoSelect thành thiết kế phẳng:
    *   Khung hiển thị chính (`.CaptionCont`): Nền trắng, viền `#D1D1D1`, bo góc 0px, chiều cao cân đối với input, icon mũi tên tối giản.
    *   Khung danh sách lựa chọn (`.optWrapper`): Bỏ shadow, viền màu `#D1D1D1`, bo góc 0px. Các dòng lựa chọn font mono nhỏ gọn, checkbox phẳng.

### 4.3. Các Nút Hành Động (Action Buttons)
*   **Nút Lưu (Save/Submit):** Thiết kế phẳng đen tuyền `bg-black text-white hover:bg-neutral-800 font-bold text-[11px] uppercase tracking-wider rounded-none px-6 py-3 transition-all`.
*   **Nút Hủy/Quay Lại (Cancel/Back):** Thiết kế viền mỏng xám `bg-white border border-[#D1D1D1] text-black hover:bg-neutral-50 font-bold text-[11px] uppercase tracking-wider rounded-none px-6 py-3 transition-all`.
*   **Nút BACK TO LIST:** Nằm cùng hàng ngang với tiêu đề chính, chống wrap dòng (`white-space: nowrap`) trên màn hình di động, định dạng `[← BACK TO LIST]`.

### 4.4. Validation & Errors
*   Thông báo lỗi validation hiển thị ngay dưới input bằng font chữ mono đỏ nhỏ gọn (`text-[10px] text-[#E62429] font-mono mt-1 uppercase`).

---

## 5. Quy Trình Kiểm Thử Thủ Công (Manual UAT Checklist)

Do chính sách bảo mật UAT không tự ý chạy browser subagent khi chưa được yêu cầu, dưới đây là kịch bản kiểm thử thủ công chi tiết:

### Kịch bản UAT:
*   **Tài khoản đăng nhập:** `admin` / `Admin@123456` (hoặc Keycloak thật `UseMockAuth: false`).
*   **TC-01: Kiểm tra Giao diện Tạo Mới User (`/User/Create`)**
    *   *Bước 1:* Truy cập `/User` rồi nhấn nút `ADD USER` hoặc truy cập trực tiếp `/User/Create`.
    *   *Kết quả mong đợi:* Form tạo mới hiển thị phẳng hoàn chỉnh, không còn bo góc mềm, SumoSelect có viền xám phẳng gọn gàng, các nút bấm đồng bộ thiết kế Swiss Style 06.
*   **TC-02: Kiểm tra Giao diện Cập Nhật User (`/User/Detail/{id}`)**
    *   *Bước 1:* Từ danh sách User, nhấn nút `EDIT` của một user bất kỳ.
    *   *Kết quả mong đợi:* Trang chỉnh sửa hiển thị, Username ở trạng thái readonly có màu nền xám nhạt phẳng, con trỏ không cho phép sửa. Nút `[← BACK TO LIST]` thẳng hàng ngang với tiêu đề chính.
*   **TC-03: Kiểm tra Validation & Thông báo lỗi**
    *   *Bước 1:* Tại form Create, để trống tất cả các trường rồi nhấn `SAVE`.
    *   *Kết quả mong đợi:* Các nhãn lỗi hiển thị màu đỏ viết hoa bên dưới mỗi trường tương ứng, không bị vỡ layout form.
*   **TC-04: Thử nghiệm luồng Submit Tạo mới & Cập nhật**
    *   *Bước 1:* Điền đầy đủ thông tin hợp lệ (tạo user test mới hoặc sửa fullname của user hiện tại) rồi nhấn `SAVE`.
    *   *Kết quả mong đợi:* Gọi AJAX thành công, hiển thị Toast thông báo và chuyển hướng an toàn về trang `/User` mà không có lỗi JS trong Console.

---

## 6. Kế Hoạch Thực Hiện & Checklist
- [x] 1. Tạo file plan `MD_memory/plans/2026-07-20_1100_phase-2_user-form-refactor.md`.
- [ ] 2. Trình bày đề xuất và xin phê duyệt từ User.
- [ ] 3. Refactor `CreateUserView.cshtml` theo Swiss Style 06 (giữ nguyên name inputs, form submit AJAX).
- [ ] 4. Refactor `Detail.cshtml` theo Swiss Style 06 (giữ nguyên logic data binding và readonly Username).
- [ ] 5. Kiểm tra build dự án (`dotnet build`) đảm bảo không phát sinh lỗi biên dịch.
- [ ] 6. Tiến hành kiểm tra Git status, tạo báo cáo UAT thủ công hoặc chạy browser UAT nếu được phê duyệt.
