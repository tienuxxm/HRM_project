# Báo cáo Audit Kỹ thuật & Kế hoạch Refactor: Approver Assignments

**Mã tài liệu:** `MD_memory/reports/2026-07-20_0846_phase-design-approver-assignments_proposal_report.md`  
**Phase:** `phase-design-approver-assignments`  
**Module:** Cấu hình phê duyệt nghỉ phép (Leave Approver Assignments)  
**Trạng thái:** Báo cáo đề xuất UI ONLY & Technical Debt (Đang chờ duyệt)  

---

## 1. Tóm tắt kết quả Audit & Đánh giá rủi ro

Trong lượt kiểm tra kỹ thuật (technical audit) và giao diện (UI audit), chúng tôi đã đánh giá toàn bộ module cấu hình người phê duyệt phép (`LeaveApproverAssignment`). Mục tiêu là chuyển đổi giao diện sang chuẩn **Swiss International Style 06** đồng thời cô lập các phát hiện toàn vẹn dữ liệu thành Technical Debt để xử lý sau.

---

## 2. Technical Debt & Khảo sát Backend (Để dành cho các pha sau)

Các phát hiện về tính toàn vẹn dữ liệu ở backend dưới đây được ghi nhận lại dưới dạng **Technical Debt / Future Phase** và **tuyệt đối không can thiệp code backend** trong pha giao diện này:

1. **Lỗi FK Restrict Constraint khi xóa Department/Position:**
   * *Hiện trạng:* Cấu hình database thiết lập quan hệ khóa ngoại giữa bảng `leave_approver_assignment` và `department` / `position` là `DeleteBehavior.Restrict`.
   * *Rủi ro:* Khi xóa Department hoặc Position, Command Handler tương ứng chỉ xóa thực thể mà không kiểm tra xem nó có đang được liên kết làm mục tiêu cấu hình duyệt phép hay không. Điều này dẫn đến crash cơ sở dữ liệu (lỗi SQL 500) ở runtime.
2. **Dữ liệu cấu hình mồ côi (Orphaned Assignment) khi Employee bị soft-delete:**
   * *Hiện trạng:* Khi nhân viên (Approver) nghỉ việc/vô hiệu hóa (`IsActive = false`), bản ghi cấu hình duyệt phép liên quan vẫn hoạt động (`IsActive = true`) và hiển thị trên bảng quản trị.
3. **Thiếu validation xác thực thực thể ở Application layer:**
   * *Hiện trạng:* Các command tạo/cập nhật cấu hình không kiểm tra xem Employee/Department/Position có tồn tại hoặc đang active hay không.

> [!WARNING]  
> Các lỗi trên thuộc phạm vi Backend Integrity và được lưu lại trong danh sách Technical Debt. Trong Phase UI ONLY này, chúng ta không chỉnh sửa bất kỳ lớp C#, Database, Migration hay Authentication/Keycloak nào.

---

## 3. Đề xuất Refactor Giao diện (Phase UI ONLY)

### 3.1. Phạm vi chỉnh sửa cho phép
* Chỉ sửa duy nhất: `HRM_Leave_Management/Web.Backend/Views/LeaveApproverAssignment/Index.cshtml` (bao gồm các cấu trúc HTML, modal nội bộ và các đoạn script bổ trợ bên trong).
* Không chạm vào bất kỳ file runtime hay layout dùng chung nào (`_Layout.cshtml`, sidebar, header, footer, bottom nav).

### 3.2. Ngôn ngữ Thiết kế: Swiss International HR Style 06
Giao diện sẽ được đồng bộ trực tiếp từ các module đã hoàn thiện (`Position`, `Department`, `LeaveType`):
* **Desktop Layout:**
  * Loại bỏ breadcrumb nội bộ không cần thiết (tránh trùng lặp với global header).
  * Tiêu đề chính viết hoa kích thước lớn `32px` (`text-[32px] font-bold text-black uppercase tracking-tight`), đi kèm phụ đề hướng dẫn tinh gọn `14px`.
  * Thanh tìm kiếm độc lập trên Desktop (`SEARCH CONFIGURATIONS...`) dùng đường viền xám hairline `border-[#D1D1D1]` và font chữ `font-mono text-[11px]`.
  * Nút bấm tạo mới đen nguyên khối, viết hoa, không bo tròn, chữ mảnh có khoảng giãn lớn: `+ CREATE CONFIGURATION` (`bg-black text-white rounded-none uppercase text-[11px] font-bold tracking-widest`).
  * Bảng dữ liệu thiết kế hairline (`border border-[#D1D1D1] bg-white rounded-none shadow-none`), sử dụng hiệu ứng trượt ngang nhẹ 4px khi di chuột (`hover:translate-x-1 transition-transform`). Header bảng màu xám nhạt (`bg-[#F5F5F5]`) với chữ hoa `font-mono text-[10px] font-bold text-[#4c4546]`.
* **Mobile Layout:**
  * Ẩn bảng dữ liệu trên thiết bị di động để tránh hiện tượng tràn viền (horizontal overflow).
  * Chuyển danh sách sang dạng **Stacked Cards** sắc cạnh. Mỗi thẻ hiển thị rõ ràng thông tin: *Tên người duyệt (Approver)*, *Phòng ban áp dụng (Target Department)*, *Chức vụ áp dụng (Target Position)*.
  * Thiết kế khoảng đệm phía dưới (bottom spacing) hợp lý để tránh bị che khuất bởi thanh điều hướng dưới cùng (bottom nav) trên điện thoại di động.
* **Modal CRUD phẳng (Create / Update / Delete Confirm):**
  * Tiêu đề modal sử dụng thanh header đen đặc (`bg-black text-white font-bold uppercase py-3 px-4 text-[12px] tracking-wider rounded-none`).
  * Nút đóng modal dạng chữ X hoặc chữ CLOSE màu đỏ (`text-swiss-red` hoặc `#E62429`), không dùng hiệu ứng bo tròn.
  * Các trường nhập liệu phẳng, góc vuông (`rounded-none border-[#D1D1D1] bg-white text-[12px]`).
  * Nút bấm submit đen phẳng hoặc đỏ phẳng (đối với hành động xóa) viết hoa, không bo góc.
  * Giữ nguyên toàn bộ các thẻ ID, trường `name`, `data-modal-target`, `data-modal-hide`, các endpoint gửi yêu cầu Ajax (`/leave-approver-assignment/create`, `/update`, `/delete`), các biến dữ liệu từ ViewBag và kiểm tra quyền `canUpdate`.

### 3.3. Giải pháp Lọc & Phân trang phía Client (Client-side Search & Pagination)
* **Lý do lựa chọn:**
  1. **Số lượng bản ghi nhỏ:** Trong thực tế, các doanh nghiệp vừa và nhỏ chỉ có vài chục cấu hình duyệt phép (thường dưới 100 dòng), việc tải toàn bộ danh sách một lần và lọc/phân trang trên trình duyệt mang lại trải nghiệm nhanh, mượt mà (tức thời) không độ trễ.
  2. **Không thay đổi Backend Contract:** Không cần bổ sung tham số truy vấn phân trang hay sửa đổi kiểu trả về trong controller, giúp cô lập mã nguồn C#.
  3. **Không ảnh hưởng phân quyền/dữ liệu:** Dữ liệu tải về đã được lọc và kiểm soát quyền ở Controller thông qua quyền truy cập của người dùng đăng nhập hiện tại, đảm bảo an toàn bảo mật.
* **Quy chuẩn phân trang:**
  * Kích thước trang mặc định (Page Size) là **5 dòng/trang** (để tối ưu hóa hiển thị và kiểm thử).
  * Sử dụng Javascript thuần và JQuery hiện có để ẩn/hiện động các hàng trong bảng dữ liệu trên Desktop và các thẻ Card trên Mobile.
  * Phân trang tự động tính toán lại tổng số trang khi có thay đổi trong ô tìm kiếm.

---

## 4. Kế hoạch Verification & Kịch bản UAT (Manual)

### 4.1. Pre-flight Checks (Kiểm tra mã nguồn trước/sau khi code)
1. **Kiểm tra trạng thái Git:**
   * Chạy `git status --short` để xác nhận Working Tree không bị ô nhiễm bởi các file ngoài phạm vi.
2. **Kiểm tra cú pháp và sửa đổi:**
   * Chạy `git diff --check` để phát hiện các khoảng trắng thừa ở cuối dòng (trailing whitespace) hoặc xung đột dòng kẻ.
3. **Kiểm tra biên dịch:**
   * Chạy lệnh `dotnet build Web.Backend --no-restore` từ thư mục gốc để đảm bảo thay đổi UI không gây lỗi biên dịch nào ở Web project.

### 4.2. Kịch bản Kiểm thử UAT thủ công (Manual UAT Checklist)

#### TC-01: Giao diện Desktop & Hiệu ứng Swiss Grid
* **Điều kiện trước:** Màn hình máy tính độ phân giải rộng (>= 1024px).
* **Các bước thực hiện:**
  1. Điều hướng đến `/leave-approver-assignment`.
  2. Quan sát tiêu đề chính `Positions` dạng chữ hoa phẳng, không có breadcrumb trùng lặp.
  3. Di chuột qua các dòng trong bảng.
* **Kết quả mong đợi:**
  * Bảng dữ liệu viền xám nhạt sắc cạnh, không bo góc, không đổ bóng.
  * Khi di chuột vào dòng, dòng đó dịch nhẹ 4px sang phải mượt mà.

#### TC-02: Giao diện Mobile Stacked Cards & Spacing
* **Điều kiện trước:** Sử dụng chế độ Responsive của trình duyệt (giả lập iPhone 12/Pro hoặc chiều rộng dưới 1024px).
* **Các bước thực hiện:**
  1. Truy cập vào trang cấu hình.
  2. Cuộn trang xuống dưới cùng.
* **Kết quả mong đợi:**
  * Bảng dữ liệu tự động ẩn đi; các thẻ Card xếp chồng hiển thị thông tin thay thế.
  * Bố cục vừa vặn chiều ngang, không bị tràn hay xuất hiện thanh cuộn ngang.
  * Khoảng đệm phía dưới cùng đủ rộng để không bị che khuất bởi thanh điều hướng dưới.

#### TC-03: Tìm kiếm và Phân trang Client-side (Page Size = 5)
* **Các bước thực hiện:**
  1. Kiểm tra nếu danh sách có nhiều hơn 5 cấu hình: bảng chỉ hiển thị tối đa 5 dòng ở trang đầu tiên.
  2. Nhấn nút `NEXT` và `PREV` ở cả Desktop và Mobile.
  3. Nhập từ khóa tìm kiếm (ví dụ tên nhân viên hoặc phòng ban) vào ô Search.
* **Kết quả mong đợi:**
  * Phân trang hoạt động đúng: chuyển tiếp giữa các nhóm 5 bản ghi.
  * Khi tìm kiếm, kết quả lọc tức thời (real-time) và tính toán lại phân trang chính xác dựa trên số lượng bản ghi khớp từ khóa. Nút NEXT/PREV tự động vô hiệu hóa nếu chỉ có 1 trang kết quả.

#### TC-04: Thao tác Modals CRUD phẳng
* **Các bước thực hiện:**
  1. Bấm nút `+ Create Configuration` để mở Modal thêm mới.
  2. Chọn các trường và bấm Lưu (hoặc đóng modal qua nút X màu đỏ).
  3. Bấm `Edit` ở một dòng dữ liệu để mở Modal chỉnh sửa.
  4. Bấm `Delete` để mở Modal xác nhận xóa.
* **Kết quả mong đợi:**
  * Các modal có thanh tiêu đề đen phẳng viết hoa, nút đóng màu đỏ sắc nét.
  * Không có hộp thoại mặc định của trình duyệt (`window.alert` hoặc `window.confirm`) được kích hoạt.
  * Các trường nhập liệu phẳng, góc vuông, không bo tròn.

#### TC-05: Kiểm tra Bảng điều khiển (Console Check)
* **Các bước thực hiện:** Mở Developer Tools (F12) -> tab Console.
* **Kết quả mong đợi:** Không có bất kỳ lỗi Javascript đỏ nào hiển thị khi tải trang, tìm kiếm, phân trang hoặc thao tác modal.

#### TC-06: Kiểm tra Giới hạn Phạm vi (Scope Check)
* **Các bước thực hiện:** Chạy `git status --short`.
* **Kết quả mong đợi:** Chỉ hiển thị duy nhất sự thay đổi tại tệp `HRM_Leave_Management/Web.Backend/Views/LeaveApproverAssignment/Index.cshtml`. Các tệp tin layout dùng chung, controller C#, database và cấu hình Keycloak tuyệt đối không bị sửa đổi.

---

## 5. Critical Thinking & Xử lý ngoại lệ

1. **Rủi ro lỗi SQL/Crash:** Nếu trong quá trình UAT, việc bấm nút Lưu hoặc Xóa gây ra crash hệ thống do lỗi FK Restrict từ phía cơ sở dữ liệu cũ, chúng tôi sẽ **dừng lại ngay lập tức** và báo cáo với USER để đưa việc sửa đổi backend vào lượt tiếp theo, không tự ý sửa C# hay migration DB để tránh phá vỡ phạm vi Phase UI ONLY.
2. **Thiếu dữ liệu để test Phân trang:** Nếu số lượng dữ liệu seed ban đầu dưới 5 bản ghi, chúng tôi đề xuất tạo thêm dữ liệu trực tiếp bằng giao diện UI thông qua modal Add New vừa được refactor, hoặc ghi nhận đây là bước chuẩn bị dữ liệu trong phase tiếp theo.
