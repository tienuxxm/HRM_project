﻿﻿# Báo cáo kết quả Phase 3D.2 — Duration Calculator & Leave Request Creation Integration

## 1. Trạng thái Git hiện tại (Git status)
Cây làm việc hiện tại đang dirty (chứa các thay đổi chưa commit từ các ngày trước). Chúng tôi cam kết giữ nguyên các file này, không đưa vào vùng commit (stage) hay sửa đổi bất kỳ file nào ngoài phạm vi Phase 3D.2.

### Các tệp tin sản phẩm (Production Files) thuộc phạm vi Phase 3D.2:
- **Thêm mới**:
  - `HRM_Leave_Management/Application/WorkCalendars/IWorkCalendarService.cs`
  - `HRM_Leave_Management/Application/WorkCalendars/WorkCalendarService.cs`
- **Sửa đổi**:
  - `HRM_Leave_Management/Domain/LeaveRequests/LeaveRequestErrors.cs`
  - `HRM_Leave_Management/Infrastructure/DependencyInjection.cs`
  - `HRM_Leave_Management/Application/LeaveRequests/Create/CreateLeaveRequestCommandHandler.cs`

> [!IMPORTANT]
> Toàn bộ các tệp tin trong thư mục `MD_memory/debug/` bao gồm:
> - `MD_memory/debug/TestDbApp/VerifyDuration.cs`
> - `MD_memory/debug/TestDbApp/Program.cs`
> - `MD_memory/debug/2026-07-07_0926_write-bom.py`
>
> Chỉ là các tệp tin kiểm thử và debug cục bộ (local-only debug files), tuyệt đối **KHÔNG ĐƯỢC** đưa vào vùng commit (stage) hay push lên kho lưu trữ chính thức.

---

## 2. Kết quả Phân tích Tác động (GitNexus Impact Analysis)
Chúng tôi thực hiện phân tích tác động ngược dòng (upstream) cho các ký hiệu chính thông qua GitNexus:

*   **CreateLeaveRequestCommandHandler**:
    *   *Tác động (Upstream Impact)*: **THẤP (LOW)**.
    *   *File bị ảnh hưởng trực tiếp (Direct Affected)*: 1 file duy nhất là `HRM_Leave_Management/Web.Backend/Controllers/LeaveRequestController.cs`.
    *   *Rủi ro*: Không ảnh hưởng đến các thành phần hoặc tiến trình nghiệp vụ cốt lõi khác.

*   **WorkCalendarService**:
    *   *Đặc điểm*: Đây là một ký hiệu hoàn toàn mới (new symbol) được tạo ra trong Phase 3D.2 này.
    *   *Lưu ý*: Do là ký hiệu mới, chỉ mục đồ thị của GitNexus có thể chưa cập nhật kịp thời (stale) cho đến khi lượt phân tích kế tiếp được chạy. Tuy nhiên, qua phân tích tĩnh, dịch vụ này chỉ được đăng ký dịch vụ trong `Infrastructure/DependencyInjection.cs` và được gọi trực tiếp bởi `CreateLeaveRequestCommandHandler`.

---

## 3. Quy tắc Nghiệp vụ và Hợp đồng Dịch vụ (Service Contract & Business Rules)

### Hợp đồng Dịch vụ (WorkCalendarService Contract)
Dịch vụ `WorkCalendarService.CalculateLeaveDurationAsync` đóng vai trò là một bộ tính toán trung lập (neutral calculator). Nếu khoảng thời gian đăng ký chỉ chứa toàn bộ ngày không làm việc (ví dụ nghỉ Chủ Nhật hoặc ngày nghỉ lễ không cấu hình ca làm việc), dịch vụ sẽ tính toán thành công và trả về kết quả `Success(0.0)`. Dịch vụ không chịu trách nhiệm chặn các yêu cầu có tổng ngày bằng `0`.

### Quy tắc Nghiệp vụ Tầng Ứng dụng (Application Layer Rule)
Lớp `CreateLeaveRequestCommandHandler` chịu trách nhiệm kiểm soát nghiệp vụ tạo mới đơn xin nghỉ phép. Nếu tổng số ngày nghỉ tính được từ dịch vụ trả về là `0.0` ngày, Handler sẽ ngăn chặn và trả về lỗi `LeaveRequestErrors.OnlyNonWorkingDays`. 

> [!NOTE]
> Việc chặn lỗi này tại Handler được xác thực tĩnh thông qua kiểm duyệt mã nguồn (code/static verified) và biên dịch dự án. Nó sẽ được kiểm chứng toàn diện ở mức API/UI trong các đợt UAT chức năng sau này.

### Chốt chặn tự thân tại Service (Self-contained guards):
1.  **Kiểm tra năm khác biệt (Cross Year)**:
    *   Nếu `startDate.Year != endDate.Year` -> Trả về lỗi `LeaveRequestErrors.CrossYearNotAllowed`.
2.  **Kiểm tra lệch buổi nghỉ trong ngày (Day Part Mismatch)**:
    *   Nếu `startDate == endDate && startDayPart != endDayPart` -> Trả về lỗi `LeaveRequestErrors.DayPartMismatch`.

---

## 4. Tài liệu và Bằng chứng Kiểm chứng (Verification Evidence)
Chúng tôi triển khai dự án Console Test độc lập tại `MD_memory/debug/TestDbApp/VerifyDuration.cs` sử dụng mock repository `FakeWorkCalendarDayRepository` để xác minh chính xác các kịch bản tính toán thời gian nghỉ phép ở cấp độ Service.

### Danh sách các ca kiểm thử (Test Cases) — **Codex Verified PASS**:
*   **TC-01: Ca làm việc tiêu chuẩn mặc định (Default Full Workdays - 1)**
    *   *Kịch bản*: Nghỉ từ Thứ Hai `Morning` đến Thứ Ba `Afternoon` (Cấu hình `FullDay` cả 2 ngày).
    *   *Kết quả*: **PASS** (`Mon Morning -> Tue Afternoon = 2.0`).
*   **TC-02: Ca làm việc tiêu chuẩn mặc định (Default Full Workdays - 2)**
    *   *Kịch bản*: Nghỉ từ Thứ Hai `Afternoon` đến Thứ Ba `Morning` (Cấu hình `FullDay` cả 2 ngày).
    *   *Kết quả*: **PASS** (`Mon Afternoon -> Tue Morning = 1.0`).
*   **TC-03: Nghỉ qua cuối tuần (Skip Weekends)**
    *   *Kịch bản*: Nghỉ từ Thứ Sáu `FullDay` đến Thứ Hai `FullDay` (Thứ Bảy/Chủ Nhật có ca làm việc `None`).
    *   *Kết quả*: **PASS** (`Fri FullDay -> Mon FullDay skips Sat/Sun = 2.0`).
*   **TC-04: Nghỉ vào ngày nghỉ hoàn toàn (Sunday-only)**
    *   *Kịch bản*: Đăng ký nghỉ duy nhất ngày Chủ Nhật (Cấu hình ca làm việc `None`).
    *   *Kết quả*: **PASS** (`Sunday-only returns Success(0.0) at service level`).
*   **TC-05: Kiểm tra lỗi chéo năm (Cross Year Guard)**
    *   *Kịch bản*: Đăng ký nghỉ từ 31/12/2025 đến 01/01/2026.
    *   *Kết quả*: **PASS** (`CrossYearNotAllowed guard triggers correctly`).
*   **TC-06: Kiểm tra lệch buổi cùng ngày (Day Part Mismatch Guard)**
    *   *Kịch bản*: Đăng ký nghỉ cùng ngày Thứ Hai nhưng có `StartDayPart = Morning` và `EndDayPart = Afternoon`.
    *   *Kết quả*: **PASS** (`DayPartMismatch triggers correctly on same-day request with different day parts`).
*   **TC-07: Bỏ qua ngày cuối tuần chưa cấu hình mặc định (Unconfigured Sat/Sun skipped)**
    *   *Kịch bản*: Nghỉ từ Thứ Sáu `FullDay` đến Thứ Hai `FullDay` nhưng không cấu hình ngày Thứ Bảy và Chủ Nhật trong kho lưu trữ (trả về null).
    *   *Kết quả*: **PASS** (`Unconfigured Sat/Sun skipped by default = 2.0`).
*   **TC-08: Kiểm tra ngày Thứ Bảy override đi làm (Working Saturday)**
    *   *Kịch bản*: Nghỉ Thứ Bảy, ngày này được cấu hình loại `WorkingSaturdayOverride` với ca làm việc `FullDay`.
    *   *Kết quả*: **PASS** (`WorkingSaturdayOverride with FullDay = 1.0`).

---

## 5. Kết quả biên dịch (Build Results) — **Codex Verified PASS**
*   **Build Infrastructure**:
    *   *Lệnh*: `dotnet build HRM_Leave_Management/Infrastructure/Infrastructure.csproj --no-restore`
    *   *Kết quả*: **PASS** (0 Lỗi / Errors).
*   **Build Web.Backend**:
    *   *Lệnh*: `dotnet build HRM_Leave_Management/Web.Backend/Web.Backend.csproj --no-restore`
    *   *Kết quả*: **PASS** (0 Lỗi / Errors).

### Lưu ý về Encoding của Source Code C# và MD:
*   Các tệp tin C# nguồn trong hệ thống được định dạng mặc định theo chuẩn UTF-8 không có BOM.
*   Chỉ các tệp tin báo cáo/kế hoạch Markdown (`.md`) trong thư mục `MD_memory/` mới yêu cầu nghiêm ngặt UTF-8 BOM.

---

## 6. Kết quả kiểm tra Encoding (Encoding Scan Results) — **Codex Verified PASS**
Chạy script kiểm tra mã hóa:
*   *Lệnh*: `python MD_memory/debug/2026-06-26_1430_scan-mojibake.py MD_memory/plans/*.md MD_memory/reports/*.md --require-bom`
*   *Kết quả*: **PASS**
    *   **BOM failures**: `0`
    *   **Mojibake hits**: `0`
    *   **Exit code**: `0`

---

## 7. Cam kết không đột biến (No Mutation & No Stage/Commit)
*   Không thực hiện cập nhật cơ sở dữ liệu (`dotnet ef database update`).
*   Không tác động, đột biến Keycloak, quyền hay tài khoản người dùng.
*   Không chạy kiểm thử tự động UAT qua trình duyệt.
*   Không thực hiện `git add`, `git commit` hay `git push`.
