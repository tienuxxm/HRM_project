# Báo Cáo Audit & Đề Xuất Thiết Kế v2: User Management Refactor (Swiss Style 06)

*   **Mã Phase:** `phase-design-user-management-audit`
*   **Ngày tạo:** 2026-07-20
*   **Phiên bản:** v2 (Revise)
*   **Tác giả:** Senior .NET Fullstack Engineer & Database Architect

---

## 1. Tuyên Bố Ranh Giới Kiến Trúc (Architecture Boundary)
Để bảo toàn tính nhất quán và cấu trúc Clean Architecture của hệ thống, chúng tôi cam kết tuân thủ ranh giới phụ thuộc sau trong suốt quá trình chuẩn bị và thực thi phase:
*   `Web.Backend -> Application -> Domain`
*   `Infrastructure -> Application/Domain`
*   **Cam kết:** Tuyệt đối không thay đổi mã nguồn C# backend, không sửa đổi database/migration, và không can thiệp vào cơ chế xác thực Keycloak hoặc phân quyền hệ thống trong các phase UI-only.

---

## 2. Báo Cáo Trạng Thái Git Hiện Tại (Git Baseline Status)
Trước khi thực hiện phân tích, hệ thống Git hiện tại ghi nhận các thay đổi chưa được commit (dirty/untracked baseline) như sau:
*   **Branch hiện tại:** `main`
*   **File đã thay đổi (Modified):**
    *   `HRM_Leave_Management/Web.Backend/Views/LeaveApproverAssignment/Index.cshtml`
*   **Các file chưa được theo dõi (Untracked):**
    *   `.codex_tmp/`
    *   `HRM_Leave_Management/Web.Backend/tailwind.debug.config.js`
    *   `HRM_Leave_Management/Web.Backend/test.html`
    *   `MD_memory/reports/2026-07-20_0846_phase-design-approver-assignments_proposal_report.md`
    *   `MD_memory/reports/2026-07-20_1030_phase-design-user-management-audit_proposal_report.md` (Bản v1 cũ)

*Cam kết bảo vệ lịch sử Git: Không tự ý checkout, restore, reset, clean hoặc dùng lệnh xóa các tệp tin thuộc baseline trên.*

---

## 3. Kết Quả Audit Kỹ Thuật Hệ Thống User Management Hiện Tại

### 3.1. Phân Tích Cơ Chế Phân Trang (Pagination Analysis)
Qua rà soát mã nguồn C# và Razor View, chúng tôi phát hiện lỗi logic nghiêm trọng nếu cố tình chuyển đổi kích thước trang (pageSize) từ 10 về 5 chỉ bằng UI-only:
*   **Mã nguồn C# Backend (`UserController.LoadData`):**
    ```csharp
    var command = new GetAllUserPagedCommand
    {
        Page = (startValue + 10) / 10,
        PageSize = lengthValue > 0 ? lengthValue : 10
    };
    ```
    *   *Phân tích:* Công thức tính số trang (`Page`) đang bị **hardcode** chia cho 10. 
    *   *Hệ quả:* Nếu client gửi lên kích thước trang `lengthValue = 5`, khi người dùng bấm sang trang 2, DataTables sẽ gửi `startValue = 5`. Lúc này, C# Backend tính toán `Page = (5 + 10) / 10 = 1` (do phép chia số nguyên). Hệ thống sẽ trả về dữ liệu trang 1 thay vì trang 2, gây lỗi lặp dữ liệu và tê liệt phân trang.
*   **Mã nguồn Razor View (`Views/User/Index.cshtml`):**
    ```javascript
    const currentPage = Math.ceil(pageInfo.end / 10);
    const pages = Math.ceil(pageInfo.recordsDisplay / 10);
    ```
    *   *Phân tích:* Logic hiển thị số trang hiện tại và tổng số trang cũng bị hardcode chia cho 10.
*   **Kết luận:** Để đảm bảo tính chất **UI-only** không can thiệp C# Backend, chúng ta **bắt buộc phải giữ nguyên page size hiển thị mặc định là 10** trong các phase UI. Việc chuyển đổi sang page size bằng 5 sẽ được tách riêng thành một **Optional Phase 4** (yêu cầu sửa đổi C# Backend tối thiểu và đánh giá tác động qua GitNexus).

### 3.2. Cấu Trúc Views & Trạng Thái Edit Modal
*   **Trạng thái `_EditUserModal.cshtml`:** 
    *   Tệp tin này hiện tại trống, chỉ chứa khai báo Model và **không được gọi sử dụng ở bất kỳ đâu** trong runtime (cả ở Controller lẫn View Index). Giao dịch chỉnh sửa thông tin người dùng được chuyển hướng trực tiếp sang trang `Detail.cshtml`.
    *   *Quyết định:* Không tự ý dựng Edit Modal hay kích hoạt tệp tin này khi chưa có nhu cầu thực tế từ luồng nghiệp vụ. Giữ nguyên luồng chỉnh sửa tại trang `Detail.cshtml`.
*   **Delete Modal JS Helper/Contract:**
    *   `Index.cshtml` đang tích hợp cơ chế xóa thông qua JS helper động:
        ```javascript
        toggleDeleteModal('${row.id}')
        getDeleteModal(row.id, 'User', row.username, `/User/Delete`, function (result){ ... })
        ```
    *   *Quyết định:* Giữ nguyên hoàn toàn hợp đồng JS (JS helper/contract) này để tránh phá vỡ cơ chế kích hoạt modal và AJAX xóa của hệ thống. Chỉ can thiệp CSS để làm mịn giao diện của Modal được tạo ra (đảm bảo phẳng, viền xám `#D1D1D1`, nút bấm đồng bộ).

### 3.3. Đánh Giá Rủi Ro Toàn Vẹn Dữ Liệu (User ↔ Employee)
*   **Vấn đề:** Khi thực hiện xóa User thông qua `DeleteUserCommandHandler`, hệ thống thực hiện soft-delete User và gọi Keycloak API để xóa tài khoản, nhưng **không cập nhật khóa ngoại `UserId` trong bảng `Employee` về NULL**.
*   **Hậu quả:** Bản ghi Employee liên kết sẽ bị mồ côi (orphaned) trỏ đến một UserId không còn tồn tại trong bảng User hoạt động, dễ gây lỗi NullReferenceException ở runtime.
*   **Xử lý:** Ghi nhận vấn đề này vào danh mục **Technical Debt** để xử lý ở phase nâng cấp database/backend sau này. Tuyệt đối không can thiệp sửa đổi C# backend trong phase UI này.

---

## 4. Kế Hoạch Các Phase Thực Hiện Đề Xuất

Chúng tôi đề xuất kế hoạch thực hiện sửa đổi UI tuần tự như sau:

### Phase 1: Tối Ưu Hóa Trang Danh Sách (`Views/User/Index.cshtml`)
*   **Mục tiêu:** Áp dụng Swiss UI, giữ nguyên DataTables contract và paging behavior mặc định (10 items/page).
*   **Chi tiết giao diện Desktop:**
    *   Thay đổi tiêu đề thành `THE USERS` viết hoa (Uppercase), sử dụng font chữ không chân (Inter/Outfit).
    *   Bọc thẻ `<table>` bằng container cuộn ngang để chống tràn trên các màn hình trung bình.
    *   Render lại bảng với viền mỏng hairline màu xám nhạt (`#D1D1D1`), loại bỏ double-border.
    *   Cấu hình DataTables ẩn đi các thành phần tìm kiếm và phân trang mặc định của thư viện, dùng các phần tử điều khiển tự dựng bên ngoài để dễ dàng tùy biến style.
*   **Chi tiết giao diện Mobile:**
    *   Tự động ẩn bảng dữ liệu khi chiều rộng màn hình nhỏ hơn `1024px` (`hidden lg:block`).
    *   Hiển thị dữ liệu dưới dạng thẻ thông tin xếp chồng (Stacked Cards) gọn gàng, hiển thị đầy đủ thông tin kèm nút Edit/Delete dễ bấm.

### Phase 2: Refactor Form Tạo Mới & Chi Tiết (`CreateUserView.cshtml` & `Detail.cshtml`)
*   **Mục tiêu:** Đồng bộ giao diện nhập liệu phẳng (Flat Design) và nút bấm Swiss.
*   **Chi tiết chỉnh sửa:**
    *   Thay thế toàn bộ style input cũ bằng thiết kế phẳng: Viền mỏng màu `#D1D1D1`, góc bo nhẹ, không hiệu ứng nổi.
    *   Dropdown phân quyền sử dụng SumoSelect được tối ưu hóa hiển thị.
    *   Trang `Detail.cshtml` giữ trường Username ở trạng thái `readonly` với màu nền phẳng.
    *   Thay đổi các nút bấm thành tone màu Swiss: Nút chính màu đen (`bg-black text-white`), nút phụ màu trắng viền xám (`border-[#D1D1D1] text-black`).
    *   Chuẩn hóa nút `BACK TO LIST` nằm cùng hàng ngang với tiêu đề cập nhật, sử dụng `white-space: nowrap` để chống wrap chữ trên mọi kích thước màn hình.

### Phase 3: Làm Mịn Giao Diện Delete Modal (Duy trì JS Contract)
*   **Mục tiêu:** Đồng bộ giao diện Delete Modal mà không làm thay đổi các hàm JS helper có sẵn.
*   **Chi tiết chỉnh sửa:**
    *   Áp dụng các định nghĩa CSS để tinh chỉnh cấu trúc modal được sinh ra từ hàm `getDeleteModal`.
    *   Đảm bảo modal hiển thị phẳng, viền xám `#D1D1D1`, loại bỏ bóng đổ mờ và căn chỉnh các nút xác nhận/hủy đồng bộ với phong cách Swiss.

### Optional Phase 4: Nâng Cấp Phân Trang Kích Thước 5 (Cần Phê Duyệt Backend)
*   **Mục tiêu:** Sửa đổi logic phân trang ở cả Client và Server để hỗ trợ page size = 5.
*   **Điều kiện thực hiện:** Chỉ bắt đầu sau khi User duyệt cho phép sửa đổi C# backend tối thiểu.
*   **Các bước thực hiện:**
    1.  Chạy GitNexus impact phân tích tác động đối với phương thức `UserController.LoadData` và command `GetAllUserPagedCommand`.
    2.  Sửa đổi công thức tính số trang trong `UserController.LoadData`:
        `Page = (startValue / lengthValue) + 1` hoặc `Page = (startValue + lengthValue) / lengthValue`.
    3.  Cập nhật cấu hình JavaScript ở View sang `pageLength: 5` và cập nhật các phép chia hardcode trong hàm `loadPage()`.

---

## 5. Quy Trình Xác Minh Bắt Buộc (Verification Checklist)

Trước khi báo cáo hoàn thành bất kỳ phase nào, các lệnh kiểm tra và UAT sau đây bắt buộc phải được thực hiện đầy đủ:

### 5.1. Lệnh Kiểm Tra Hệ Thống (CLI Verification)
1.  **Kiểm tra trạng thái Git:** `git status --short` để đảm bảo làm sạch vùng làm việc, không có thay đổi ngoài phạm vi cho phép.
2.  **Kiểm tra nhánh hiện tại:** `git branch --show-current` để xác nhận đang làm việc trên đúng nhánh được chỉ định.
3.  **Kiểm tra lỗi định dạng/conflict:** `git diff --check` để đảm bảo không để sót các ký hiệu conflict hoặc khoảng trắng thừa.
4.  **Kiểm tra biên dịch dự án:** `dotnet build HRM_Leave_Management/Web.Backend/Web.Backend.csproj --no-restore` để đảm bảo không gây lỗi biên dịch.

### 5.2. Kịch Bản Kiểm Thử Thủ Công (Manual UAT Checklist)
*Tài khoản UAT: `admin` / mật khẩu `Admin@123456` (Yêu cầu Keycloak local đang chạy và `UseMockAuth: false`).*

| Mã TC | Các Bước Thao Tác | Kết Quả Mong Đợi | Trạng Thái |
| :--- | :--- | :--- | :--- |
| **TC-01** | Truy cập `/user` trên Desktop | Danh sách tải thành công, giao diện bảng phẳng theo Swiss Style 06, phân trang hiển thị 10 dòng/trang. Không xuất hiện double-border. | Chờ duyệt |
| **TC-02** | Tìm kiếm dữ liệu | Nhập ký tự vào ô tìm kiếm -> Bảng lọc dữ liệu tự động mà không bị crash hoặc lỗi JS. | Chờ duyệt |
| **TC-03** | Kiểm tra hiển thị trên Mobile | Co trình duyệt nhỏ hơn 1024px -> Bảng ẩn đi, hiển thị dạng các thẻ thông tin xếp chồng (Stacked Cards) gọn gàng. | Chờ duyệt |
| **TC-04** | Kiểm tra phân trang | Nhấp nút Next/Prev trang -> Chuyển trang mượt mà, số trang hiển thị đúng dạng `X / Y`. | Chờ duyệt |
| **TC-05** | Truy cập màn hình tạo mới | Bấm "Add User" -> Chuyển hướng tới `/user/create`. Form phẳng, các input viền `#D1D1D1`. | Chờ duyệt |
| **TC-06** | Truy cập màn hình chi tiết | Bấm "Edit" trên một dòng -> Chuyển hướng tới `/user/{id}`. Username ở trạng thái readonly, nút `BACK TO LIST` hiển thị cùng dòng với tiêu đề và không bị rớt dòng. | Chờ duyệt |
| **TC-07** | Kích hoạt Delete Modal | Bấm "Delete" trên một dòng -> Modal hiển thị phẳng tối giản, nút xác nhận hoạt động bình thường, không xuất hiện lỗi JS trong Console. | Chờ duyệt |
