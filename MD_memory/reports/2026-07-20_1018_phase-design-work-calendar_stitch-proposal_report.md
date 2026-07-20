# Báo cáo Đề xuất Thiết kế Giao diện Mô-đun Lịch Làm Việc (Work Calendar)

## 1. Bối cảnh & Mục tiêu
Báo cáo này trình bày đề xuất thiết kế giao diện cho mô-đun **Lịch Làm Việc (Work Calendar)** thuộc hệ thống quản trị nhân sự HRM. Thiết kế được thực hiện trong môi trường Stitch (Stitch Project ID: `17479353588209716186`) tuân thủ nghiêm ngặt hệ thống thiết kế **Swiss International HR** (phong cách tối giản, bố cục bất đối xứng, mật độ thông tin cao, bo góc 0px, không đổ bóng, font chữ Geist cho nhãn và JetBrains Mono cho dữ liệu số/ngày/mã).

Mục tiêu chính là chuyển đổi giao diện hiện tại của mô-đun Work Calendar (gồm các view Index, Preview, Summary) sang giao diện chuẩn Swiss International, tối ưu hóa hiển thị trên cả máy tính (Desktop) và thiết bị di động (Mobile) trước khi chuyển sang giai đoạn phát triển và triển khai thực tế.

## 2. Bảo toàn Biên Kiến trúc & Ràng buộc Kỹ thuật
Trong suốt quá trình thiết kế và chuẩn bị báo cáo này:
*   **Biên kiến trúc được bảo toàn tuyệt đối:**
    *   `Web.Backend -> Application -> Domain`
    *   `Infrastructure -> Application/Domain`
*   **Không thay đổi code runtime:** Không có bất kỳ dòng code C#, Razor (.cshtml), cấu hình cơ sở dữ liệu hoặc cấu hình Keycloak/Auth nào bị chỉnh sửa.
*   **Không tự ý commit/push:** Tuân thủ hướng dẫn Refactor Guard, không thực hiện commit hay push lên remote khi chưa có sự xác nhận của người dùng.

---

## 3. Kết quả Audit Giao diện Work Calendar Hiện tại

Hệ thống hiện tại có 3 View chính phụ trách luồng nghiệp vụ lịch làm việc:

### A. Lịch làm việc chính (`Views/WorkCalendar/Index.cshtml`)
*   **Chức năng:** Hiển thị danh sách lịch làm việc của công ty theo dạng bảng lưới hoặc danh sách. Cho phép lọc theo năm, tháng, phòng ban.
*   **Giao diện cũ:** Sử dụng các thành phần Tailwind CSS không đồng bộ, khoảng cách padding lỏng lẻo, nút nhấn bo góc mềm mại không đúng tinh thần tối giản của hệ thống mới.
*   **Hành vi UI:** Có nút mở modal để tải lên tệp Excel import lịch làm việc và modal ghi đè thủ công (manual override) ngày làm việc cho một cá nhân hoặc phòng ban.

### B. Xem trước kết quả tải lên (`Views/WorkCalendar/Preview.cshtml`)
*   **Chức năng:** Hiển thị dữ liệu tạm thời của lô (batch) import từ tệp Excel trước khi lưu chính thức vào cơ sở dữ liệu.
*   **Giao diện cũ:** Bảng hiển thị thông tin thiếu cấu trúc phân loại lỗi rõ ràng. Các cảnh báo quá khứ (past-date) hoặc trùng lặp ngày nghỉ không nổi bật, dẫn đến rủi ro HR bấm xác nhận nhầm.
*   **Hành vi UI:** Chứa hàm JavaScript `confirmImport` thực hiện cuộc gọi AJAX gửi yêu cầu đến `ConfirmCalendarImportBatchCommand` ở tầng Application và chuyển hướng người dùng sang trang Summary.

### C. Tổng kết kết quả import (`Views/WorkCalendar/Summary.cshtml`)
*   **Chức năng:** Hiển thị báo cáo kết quả sau khi lô import được áp dụng thành công.
*   **Giao diện cũ:** Hiển thị danh sách đơn giản các ngày đã thay đổi. Chưa chỉ rõ tác động (impact) trực tiếp lên các đơn xin nghỉ phép (`LeaveRequest`) hiện có của nhân viên (ví dụ: ngày nghỉ phép trùng với ngày làm việc mới được cập nhật).
*   **Hành vi UI:** Hiển thị thông tin nhật ký thay đổi và số ngày phép được hoàn trả/cập nhật.

---

## 4. Đặc tả 6 Thiết kế Giao diện trên Stitch (Swiss International HR)

Chúng tôi đã thiết kế thành công 6 màn hình giao diện (3 Desktop, 3 Mobile) trên Stitch để chuẩn hóa toàn bộ luồng nghiệp vụ lịch làm việc:

### 1. Work Calendar List & Overrides (Danh sách Lịch & Ghi đè)
Màn hình trung tâm quản lý lịch làm việc thường niên và các ngày đặc biệt.
*   **Desktop Screen ID:** `620ee6b4d320478a87b8d81ce4e4d6a8` (Rộng: 2560px, Cao: 2048px)
*   **Mobile Screen ID:** `47a25a50787e45448378c89b7d8ec88b` (Rộng: 780px, Cao: 1778px)
*   **Đặc điểm UI/UX:**
    *   **Bảng dữ liệu mật độ cao:** Lưới hiển thị ngày làm việc rõ ràng với các cột: Ngày, Thứ, Loại ngày (Ngày làm việc thường, Ngày nghỉ cuối tuần, Ngày nghỉ lễ công ty, Ngày ghi đè), Trạng thái, Lý do.
    *   **Bộ lọc tối giản:** Các trường lọc Năm/Tháng và Phòng ban dạng hộp vuông viền 1px, không có hiệu ứng đổ bóng.
    *   **Modal ghi đè thủ công (Manual Override):** Khi nhấp vào một ngày, modal hiện lên dạng card phẳng viền đen 2px. Các ô nhập liệu có nhãn phía trên sử dụng font Geist, dữ liệu ngày nhập sử dụng font JetBrains Mono để dễ đối chiếu. Tích hợp sẵn trường nhập lý do bắt buộc và bộ lọc Past-date Guard (cảnh báo hoặc chặn nếu sửa ngày trong quá khứ).

### 2. Work Calendar Import Preview (Xem trước Lô nhập liệu Excel)
Màn hình xác thực dữ liệu trước khi lưu vào DB.
*   **Desktop Screen ID:** `f78119eb8ce845eaae5dae9c9c855a8f` (Rộng: 2560px, Cao: 2048px)
*   **Mobile Screen ID:** `f8bf01ec2ca34b6ea17e4bf648f5a5e3` (Rộng: 780px, Cao: 1778px)
*   **Đặc điểm UI/UX:**
    *   **Phân nhóm Trạng thái Dữ liệu:** Chia rõ thành 3 danh sách bằng các tab hoặc hộp viền mảnh:
        *   *Hợp lệ (Valid):* Ngày thường được thêm hoặc ghi đè đúng quy định.
        *   *Cảnh báo (Warning):* Ngày trùng với ngày nghỉ lễ quốc gia hiện có.
        *   *Không hợp lệ (Invalid):* Ngày nằm trong quá khứ (past-date violation), định dạng ngày sai hoặc trùng ngày nghỉ của công ty. Các ngày này được tô màu chữ đỏ Swiss (`#E62429`) kèm thông báo lỗi cụ thể để HR kịp thời sửa chữa.
    *   **Thông tin lô (Batch Meta):** Mã lô `BATCH-202607-001` và số lượng bản ghi hiển thị nổi bật ở góc trên bên phải.
    *   **Hành động quyết định:** Nút 'Confirm Import' (Màu đen đặc chữ trắng) chỉ khả dụng khi không còn lỗi nghiêm trọng (Invalid) hoặc hiển thị hộp thoại xác nhận ghi đè rủi ro. Nút 'Cancel Batch' (Nền trắng viền đen) cho phép hủy toàn bộ lô.

### 3. Work Calendar Import Summary (Tổng kết Lô nhập liệu)
Màn hình báo cáo tác động sau khi áp dụng lịch làm việc mới.
*   **Desktop Screen ID:** `b048dc088e8647dd808a61abe8c8f6d3` (Rộng: 2560px, Cao: 2048px)
*   **Mobile Screen ID:** `4298007a8f994d81ba6bca66c32e4c32` (Rộng: 780px, Cao: 1778px)
*   **Đặc điểm UI/UX:**
    *   **Hộp trạng thái thành công:** Banner lớn viền đen 2px thông báo import hoàn tất thành công.
    *   **Báo cáo tác động nghỉ phép (Leave Request Impact):** Điểm cải tiến quan trọng nhất. Hiển thị danh sách các đơn xin nghỉ phép của nhân viên bị ảnh hưởng trực tiếp (ví dụ: ngày nhân viên xin nghỉ phép trước đây là ngày nghỉ cuối tuần nay bị chuyển thành ngày đi làm bù, hoặc ngược lại).
    *   **Chi tiết thay đổi số dư:** Hiển thị rõ số ngày phép được hoàn lại vào quỹ của nhân viên (ví dụ: `Nguyen Van A - EMP-0293 - Hoàn lại 1.0 ngày phép do ngày nghỉ trùng ngày đi làm bù`). Dữ liệu này sử dụng font JetBrains Mono nhằm tăng tốc độ rà soát của bộ phận C&B.
    *   **Nút điều hướng nhanh:** Nút 'Back to Calendar' đưa HR quay lại màn hình quản lý chính.

---

## 5. Hướng dẫn Triển khai Giao diện (Implementation Roadmap)
Khi bước vào giai đoạn lập trình (Phase tiếp theo), lập trình viên frontend cần lưu ý:
1.  **CSS Tokens:** Định nghĩa các biến CSS trong `index.css` khớp với hệ thống thiết kế Swiss International HR:
    *   `--primary-color: #000000;`
    *   `--accent-color: #E62429;` (Đỏ Swiss)
    *   `--bg-color: #FAF9F9;`
    *   `--border-color: #D1D1D1;`
    *   `--border-width: 1px;`
    *   `--border-radius: 0px;`
2.  **Typography Split:**
    *   Sử dụng font `Geist` cho tất cả văn bản giao diện, nút bấm, nhãn trường nhập liệu.
    *   Sử dụng font `JetBrains Mono` cho ngày tháng (ví dụ: `2026-07-20`), mã số nhân viên (EMP-xxxx), mã lô import, và số lượng ngày phép (ví dụ: `1.0 ngày`).
3.  **Razor Views Update:** Thay thế các lớp CSS tiện ích Tailwind tự do trong `Index.cshtml`, `Preview.cshtml`, `Summary.cshtml` bằng các class ngữ nghĩa được định nghĩa trong CSS dùng chung của hệ thống để duy trì cấu trúc hộp phẳng, cứng cáp.

Báo cáo đề xuất này đã hoàn thành phần thiết kế và sẵn sàng cho sự phê duyệt của quản lý dự án trước khi tiến hành code thực tế.
