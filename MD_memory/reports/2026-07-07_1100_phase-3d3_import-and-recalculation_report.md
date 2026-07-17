# Báo cáo Kiểm thử Phase 3D.3 — Excel Import & Leave Request Recalculation Backend

Báo cáo này xác nhận kết quả kiểm thử và đánh giá kỹ thuật đối với phân hệ import lịch làm việc từ Excel và tự động tính toán lại thời gian đơn xin nghỉ phép (`Leave Request Recalculation Engine`) của HRM.

> [!NOTE]
> Phân hệ backend lõi của Phase 3D.3 đã đạt trạng thái **technical verification PASS, pending UI/UAT**. Tất cả các kiểm thử tích hợp nội bộ và kiểm tra hồi quy thời lượng đã được Codex xác minh thành công.

---

## 1. Bối cảnh & Mục tiêu Nghiệp vụ

Mục tiêu chính của Phase 3D.3 là hoàn thiện phần backend lõi cho luồng import Excel lịch làm việc (Entity `WorkCalendarDay`) và kích hoạt bộ máy tính toán lại thời gian nghỉ phép tự động cho các đơn xin nghỉ phép bị ảnh hưởng bởi thay đổi lịch làm việc.

Các quy tắc nghiệp vụ đặc thù được bảo đảm:
- **Tính toán lại & Trạng thái Đơn nghỉ phép**: Khi lịch làm việc thay đổi (ví dụ: ngày thường thành ngày nghỉ hoặc ngược lại), các đơn xin nghỉ phép bị chồng lấn sẽ được tính toán lại thời gian thực tế.
  - Các đơn xin nghỉ phép (`LeaveRequest`) bị ảnh hưởng bởi sự thay đổi thời lượng (hoặc cần phê duyệt lại) sẽ chuyển trạng thái hoặc giữ nguyên trạng thái là **`Pending`** để chờ cấp trên duyệt lại. Trạng thái thực tế của đơn nghỉ phép không tự ý bị chuyển sang các trạng thái không được phê duyệt khác.
  - Trạng thái `NeedsEmployeeRevision` chỉ là một trạng thái của bản ghi kiểm toán tính toán lại (`RecalculationAuditStatus`) nhằm gắn cờ cảnh báo trong log kiểm toán cho UI/HR theo dõi, không phải trạng thái `LeaveRequestStatus` của chính đơn xin nghỉ phép đó.
- **Bảo toàn dữ liệu lịch sử phê duyệt**:
  - Khi đơn xin nghỉ phép đã phê duyệt (`Approved`) bị mở lại thành `Pending`, hệ thống sẽ lưu vết toàn bộ dữ liệu phê duyệt cũ (trạng thái, người duyệt, thời gian duyệt, thời lượng cũ) vào Entity `LeaveRequestRecalculationAudit` để lưu giữ lịch sử.
  - **`Comment`** gốc (ý kiến phản hồi của nhân viên hoặc người phê duyệt) trên đơn xin nghỉ phép tuyệt đối **không bị ghi đè** bởi các thông báo tự động của hệ thống.
- **Hoàn trả số ngày phép đã sử dụng**:
  - Đối với các đơn nghỉ phép đã phê duyệt (`Approved`) khi bị mở lại thành `Pending` do recalculation, hệ thống thực hiện hoàn lại số ngày phép đã sử dụng (`UsedDays`) của nhân viên thông qua phương thức domain `ReturnUsedDays` của Entity `LeaveBalance`.
- **Hỗ trợ đồng bộ hóa & Re-import**:
  - Việc re-import các ngày làm việc đã bị vô hiệu hóa (`WorkCalendarDay` ở trạng thái inactive) được xử lý đồng bộ hóa bằng cách cập nhật và kích hoạt lại (reactivate) bản ghi cũ thay vì chèn dòng mới gây lỗi trùng lặp khóa chính. Kịch bản này đã được kiểm chứng thông qua ca kiểm thử **TC-10**.
- **Tính nguyên tử của Giao dịch**: Rollback toàn bộ các thay đổi lịch và cập nhật đơn nghỉ phép nếu xảy ra lỗi ghi DB ở bất kỳ bước nào trong quá trình áp dụng lô import (được chứng minh qua **TC-08**).

---

## 2. Các Thay Đổi Mã Nguồn Đã Thực Hiện

### Tầng Domain
- **File**: `HRM_Leave_Management/Domain/WorkCalendars/LeaveRequestRecalculationAudit.cs`
  - Cập nhật hàm khởi tạo tĩnh `Create` để nhận các tham số phê duyệt gốc (bao gồm cả Comment gốc) nhằm ghi nhận chính xác snapshot phê duyệt ban đầu vào audit trail.
- **File**: `HRM_Leave_Management/Domain/WorkCalendars/CalendarImportBatchRow.cs`
  - Chỉ đóng vai trò lưu trữ dòng dữ liệu đã được phân tích (parsed row), trạng thái dòng (status), các giá trị thô nhận được từ tệp Excel (raw values), và thông điệp lỗi (error message). Thực thể này không chứa logic kiểm tra nghiệp vụ.

### Tầng Application
- **File**: `HRM_Leave_Management/Application/WorkCalendars/CalendarImportService.cs`
  - **Kiểm tra tính nhất quán của Loại ngày & Ca làm việc**: Validation logic kiểm tra tính hợp lệ giữa loại ngày (`CalendarDayType`) và ca làm việc (`WorkShiftType`) được tích hợp trong phương thức `ParseAndSaveDraftAsync` khi phân tích dữ liệu nhập:
    - Nếu `DayType` là `PublicHoliday` hoặc `CompanyCustomNonWorkingDay`, ca làm việc bắt buộc phải là `WorkShiftType.None`.
    - Nếu `DayType` là `StandardWorkingDayOverride` hoặc `WorkingSaturdayOverride`, ca làm việc bắt buộc phải thuộc một trong các ca cụ thể: `WorkShiftType.FullDay`, `WorkShiftType.MorningOnly`, hoặc `WorkShiftType.AfternoonOnly`.
  - Thay đổi việc truy vấn từ `GetActiveByDateAsync` sang `GetByDateAsync` (lấy cả bản ghi inactive trong Domain repository) để cập nhật đè/reactivate ngày làm việc cũ thay vì chèn dòng mới.
  - Chụp ảnh dữ liệu phê duyệt của `LeaveRequest` trước khi thực hiện recalculate/reopen để lưu vào audit log mà không làm thay đổi hay ghi đè `Comment` gốc của đơn.
  - Bọc khối lệnh lưu trạng thái thất bại của lô (`MarkAsFailed`) trong catch block của `ApplyBatchAsync` bằng khối `try-catch` riêng biệt để cô lập lỗi cơ sở dữ liệu thứ cấp, đảm bảo transaction rollback an toàn và trả về ngoại lệ gốc.

---

## 3. Bằng Chứng Kiểm Chứng Kỹ Thuật (Exact Verification Evidence)

Kết quả thực thi kiểm thử tích hợp cục bộ trên môi trường phát triển độc lập (`TestDbApp`):

### 3.1. Chạy các ca kiểm thử tích hợp Recalculation
- **Command**:
  ```bash
  dotnet run --project MD_memory/debug/TestDbApp/TestDbApp.csproj -- --verify-recalculation
  ```
- **Kết quả**: **PASS TC-01..TC-10**
  ```text
  === STARTING EXCEL IMPORT & RECALCULATION TESTS ===
  TC-01 Passed: Valid Excel Parsing - 3/3 rows valid, status Draft
  TC-02 Passed: Excel Parsing with Invalid Rows correctly flags errors
  TC-03 Passed: Apply batch correctly blocked and marked as Failed when draft has invalid rows
  TC-04 Passed: Applied batch successfully updated WorkCalendarDays
  TC-05 Passed: Approved LeaveRequest correctly reopened to Pending with duration 1.0, balance restored, audit log created
  TC-06 Passed: Pending LeaveRequest correctly kept Pending, duration updated to 1.0, audit log created
  TC-07 Passed: LeaveRequest reduced to 0.0 duration correctly kept Pending, audit status marked NeedsEmployeeRevision with error message
  TC-08 Passed: Transaction successfully rolled back on simulated DB save failure.
  TC-09 Passed: DayType/WorkShift consistency validation correctly marks invalid rows.
  TC-10 Passed: Re-importing inactive day successfully updates and reactivates without duplicate records.
  === ALL IMPORT & RECALCULATION TESTS PASSED SUCCESSFULLY ===
  ```

### 3.2. Chạy hồi quy bộ tính toán thời lượng (Duration Regression)
- **Command**:
  ```bash
  dotnet run --project MD_memory/debug/TestDbApp/TestDbApp.csproj -- --verify-duration
  ```
- **Kết quả**: **PASS 8/8**
  ```text
  === STARTING WORK CALENDAR DURATION TESTS ===
  Test 1 Passed: Mon Morning -> Tue Afternoon = 2.0
  Test 2 Passed: Mon Afternoon -> Tue Morning = 1.0
  Test 3 Passed: Fri FullDay -> Mon FullDay skips Sat/Sun = 2.0
  Test 4 Passed: Sunday-only returns Success(0.0) at service level
  Test 5 Passed: CrossYearNotAllowed guard triggers correctly
  Test 6 Passed: DayPartMismatch triggers correctly on same-day request with different day parts
  Test 7 Passed: Unconfigured Sat/Sun skipped by default = 2.0
  Test 8 Passed: WorkingSaturdayOverride with FullDay = 1.0
  === ALL TESTS PASSED SUCCESSFULLY ===
  ```

### 3.3. Biên dịch hệ thống (Solution Build)
- **Command**:
  ```bash
  dotnet build HRM_Leave_Management/LUC.sln --no-restore
  ```
- **Kết quả**: **PASS, 0 errors, 15 warnings**

### 3.4. Quét định dạng mã hóa tài liệu (Encoding Scan)
- **Command**:
  ```bash
  python MD_memory/debug/2026-06-26_1430_scan-mojibake.py MD_memory/plans/*.md MD_memory/reports/*.md --require-bom
  ```
- **Kết quả**: **PASS, 34 files, 0 BOM failures, 0 mojibake**

### 3.5. Phân tích ranh giới thay đổi (GitNexus detect_changes)
- **Kết quả**: **25 files, 24 symbols, risk LOW**

---

## 4. Nhật ký Cập nhật Cơ sở dữ liệu Cục bộ (EF Migration Update)

Trong quá trình phát triển Phase 3D.1 - 3D.3, các lệnh cập nhật database migration cục bộ đã được chạy nhằm đồng bộ hóa cấu trúc thực thể mới:
- **Migration đã áp dụng (Local)**:
  1. `20260707012425_AddWorkCalendarDay`: Tạo bảng vật lý `work_calendar_day` lưu thông tin lịch làm việc.
  2. `20260707025928_AddCalendarImportAndRecalculationAudit`: Tạo các bảng vật lý `calendar_import_batch`, `calendar_import_batch_row` phục vụ nạp file Excel, và bảng vật lý `leave_request_recalculation_audit` phục vụ lưu vết kiểm toán tính toán lại.
- **Lưu ý quan trọng**:
  - Đây là các hành động thay đổi cấu trúc cơ sở dữ liệu cục bộ (`DB mutation local`) trên máy phát triển để phục vụ viết code và kiểm thử tích hợp.
  - **Quy tắc bắt buộc**: Kể từ các phase tiếp theo, đại lý AI (agent) phải xin phê duyệt (`approval`) trực tiếp từ người dùng trước khi thực thi bất kỳ lệnh `dotnet ef database update` nào tác động lên cơ sở dữ liệu.

---

## 5. Quy trình Vệ sinh Git (Git Hygiene & Candidates)

- **Quy tắc an toàn**: 
  - Không tự ý thực hiện các thao tác `git add`, `git commit` hay `git push`.
  - Workspace hiện tại vẫn đang ở trạng thái dirty với nhiều thay đổi nằm ngoài scope phát triển. Việc staging sẽ chỉ được tiến hành thủ công và tường minh (`explicit staging`) cho các tệp tin nghiệp vụ chính xác sau khi người dùng phê duyệt.
  - Các tệp tin cấu hình hệ thống, tệp cấu hình của agent hoặc tài nguyên phát triển cục bộ tuyệt đối không được đưa vào stage bao gồm: thư mục `.agents/*`, tệp `AGENTS.md`, `CLAUDE.md`, `appsettings.json`, `Web.Backend/appsettings.json`, thư mục `MD_memory/debug/*` hoặc các tài liệu ngoài phạm vi khác.

### Danh sách các tệp tin ứng viên (Explicit Candidate Files) cho Phase 3D.1 - 3D.3:

Khi có yêu cầu stage/commit chính thức từ người dùng, dưới đây là danh sách các tệp tin phát triển nghiệp vụ chính xác cần được cập nhật:

```text
# Domain Layer (Work Calendars & Business Logic changes)
HRM_Leave_Management/Domain/WorkCalendars/CalendarDayType.cs
HRM_Leave_Management/Domain/WorkCalendars/WorkShiftType.cs
HRM_Leave_Management/Domain/WorkCalendars/WorkCalendarDayId.cs
HRM_Leave_Management/Domain/WorkCalendars/WorkCalendarDay.cs
HRM_Leave_Management/Domain/WorkCalendars/CalendarImportBatch.cs
HRM_Leave_Management/Domain/WorkCalendars/CalendarImportBatchRow.cs
HRM_Leave_Management/Domain/WorkCalendars/LeaveRequestRecalculationAudit.cs
HRM_Leave_Management/Domain/WorkCalendars/IWorkCalendarDayRepository.cs
HRM_Leave_Management/Domain/WorkCalendars/ICalendarImportBatchRepository.cs
HRM_Leave_Management/Domain/WorkCalendars/ICalendarImportBatchRowRepository.cs
HRM_Leave_Management/Domain/WorkCalendars/ILeaveRequestRecalculationAuditRepository.cs
HRM_Leave_Management/Domain/LeaveBalances/LeaveBalance.cs
HRM_Leave_Management/Domain/LeaveRequests/LeaveRequest.cs
HRM_Leave_Management/Domain/LeaveRequests/LeaveRequestErrors.cs

# Application Layer (Services, Repositories abstractions & Recalculate logic)
HRM_Leave_Management/Application/WorkCalendars/IWorkCalendarService.cs
HRM_Leave_Management/Application/WorkCalendars/WorkCalendarService.cs
HRM_Leave_Management/Application/WorkCalendars/ICalendarImportService.cs
HRM_Leave_Management/Application/WorkCalendars/CalendarImportService.cs
HRM_Leave_Management/Application/LeaveRequests/Create/CreateLeaveRequestCommandHandler.cs

# Infrastructure Layer (EF Configurations, Repositories concrete, and Migrations)
HRM_Leave_Management/Infrastructure/Configurations/WorkCalendarDayConfiguration.cs
HRM_Leave_Management/Infrastructure/Configurations/CalendarImportBatchConfiguration.cs
HRM_Leave_Management/Infrastructure/Configurations/CalendarImportBatchRowConfiguration.cs
HRM_Leave_Management/Infrastructure/Configurations/LeaveRequestRecalculationAuditConfiguration.cs
HRM_Leave_Management/Infrastructure/Repositories/WorkCalendarDayRepository.cs
HRM_Leave_Management/Infrastructure/Repositories/CalendarImportBatchRepository.cs
HRM_Leave_Management/Infrastructure/Repositories/CalendarImportBatchRowRepository.cs
HRM_Leave_Management/Infrastructure/Repositories/LeaveRequestRecalculationAuditRepository.cs
HRM_Leave_Management/Infrastructure/Migrations/20260707012425_AddWorkCalendarDay.cs
HRM_Leave_Management/Infrastructure/Migrations/20260707012425_AddWorkCalendarDay.Designer.cs
HRM_Leave_Management/Infrastructure/Migrations/20260707025928_AddCalendarImportAndRecalculationAudit.cs
HRM_Leave_Management/Infrastructure/Migrations/20260707025928_AddCalendarImportAndRecalculationAudit.Designer.cs
HRM_Leave_Management/Infrastructure/ApplicationDbContext.cs
HRM_Leave_Management/Infrastructure/DependencyInjection.cs
HRM_Leave_Management/Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs
```
