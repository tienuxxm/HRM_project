# Báo cáo Cải tiến Quy trình Thôi việc Nhân viên (Employee Offboarding Refactor) - Phase 3D

## 1. Tổng quan
Báo cáo này tài liệu hóa các chỉnh sửa kỹ thuật cho luồng thôi việc nhân viên (Employee deletion/offboarding) từ chiến lược "Keycloak-first" sang chiến lược "DB-first, Keycloak-after", đảm bảo tính toàn vẹn dữ liệu ứng dụng trước khi thực hiện các cuộc gọi API bên ngoài. Báo cáo cũng làm rõ cách xử lý tình huống thất bại một phần (Partial Failure) bằng cách ghi log lỗi chi tiết Keycloak, kết hợp cổng chặn đăng nhập ở tầng ứng dụng và yêu cầu đối soát thủ công (hiện tại hệ thống chưa cấu hình cơ chế tự động chạy lại/Retry hoặc Outbox pattern).

## 2. Chi tiết các thay đổi
### A. Áp dụng Chiến lược DB-First, Keycloak-After (`DeleteEmployeeCommandHandler.cs`)
*   **Thứ tự thực hiện**:
    1.  Kiểm tra xem nhân viên có cấp dưới đang hoạt động hay không. Nếu có, chặn ngay lập tức.
    2.  Kiểm tra dữ liệu lịch sử HRM (số dư phép, đơn phép, lịch sử duyệt, audit trail).
    3.  Tải thông tin tài khoản `User` liên kết và trích xuất `IdentityId` (Keycloak ID) trước khi thay đổi DB.
    4.  Cập nhật trạng thái trong DB:
        *   Nếu có lịch sử HRM: Đặt nhân viên thành không hoạt động (`IsActive = false`).
        *   Nếu không có lịch sử HRM: Xóa cứng nhân viên khỏi DB.
        *   Nếu có tài khoản liên kết, đánh dấu xóa mềm tài khoản (`User.Delete()`).
    5.  Lưu các thay đổi vào cơ sở dữ liệu (`SaveChangesAsync`).
    6.  Chỉ khi DB lưu thành công, tiến hành gọi API Keycloak `IAuthenticationService.DeleteUser` để thu hồi tài khoản đăng nhập.
*   **Capture dữ liệu cục bộ trước khi thay đổi DB**:
    *   Các biến cục bộ (`employeeId`, `userId`, `identityId`, `username`, `email`) được capture ngay từ đầu Handler trước khi diễn ra bất kỳ thay đổi thực thể (mutation) nào. Điều này đảm bảo thông tin nguyên bản luôn sẵn sàng phục vụ việc ghi log ngay cả khi thực thể bị xóa cứng.
*   **Thông báo kết quả chính xác**: Success message được tính toán linh hoạt dựa trên thực tế cuộc gọi Keycloak:
    *   Có thu hồi Keycloak (`wasKeycloakRevoked == true`):
        *   Có lịch sử HRM: `"Employee deactivated and login access revoked."`
        *   Không có lịch sử HRM: `"Employee deleted and login access revoked."`
    *   Không có liên kết tài khoản / không gọi Keycloak (`wasKeycloakRevoked == false`):
        *   Có lịch sử HRM: `"Employee deactivated."`
        *   Không có lịch sử HRM: `"Employee deleted successfully."`
*   **Ghi log lỗi thất bại một phần (Partial Failure)**:
    *   Sử dụng `ILogger<DeleteEmployeeCommandHandler>` được inject vào Handler.
    *   Khi cuộc gọi xóa Keycloak thất bại, ghi nhận lỗi chi tiết qua `LogError` sử dụng các biến cục bộ đã capture: `EmployeeId`, `UserId`, `IdentityId`, `Username`, `Email`, `ErrorCode` và `ErrorMessage` (lấy từ thuộc tính `Error.Name`).
    *   Trả về lỗi nghiệp vụ rõ ràng: `EmployeeErrors.KeycloakRevokeFailed`.
*   **Chuẩn hóa Code**: Chuyển toàn bộ comment tiếng Việt và các ký tự không chuẩn (mojibake) trong mã nguồn C# sang comment tiếng Anh rõ nghĩa.

### B. Chặn Đăng Nhập Nhân Viên Ngừng Hoạt Động (`AdminLoginCommandHandler.cs`)
*   Inject `IEmployeeRepository` để kiểm tra trực tiếp trạng thái hoạt động của nhân viên được liên kết với tài khoản đăng nhập.
*   Nếu nhân viên liên kết có trạng thái `IsActive == false`, từ chối cấp token đăng nhập ngay tại cổng vào với mã lỗi `UserErrors.InvalidCredentials`.
*   **Ghi nhận Technical Debt**: Giữ nguyên logic `ToListAsync()` và lọc in-memory để đảm bảo so sánh chuỗi case-insensitive hoạt động giống như trước mà không có nguy cơ gây lỗi collation/casing ở mức DB; bổ sung chú thích rõ ràng bằng tiếng Anh.

### C. Cải tiến Dịch vụ Xác thực (`AuthenticationService.cs`)
*   Cập nhật `DeleteUser` để chấp nhận cả mã phản hồi HTTP `204 No Content` và `404 Not Found` là kết quả thành công (để tránh lỗi khi tài khoản đã bị xóa thủ công hoặc không tồn tại trên Keycloak).

### D. Vệ sinh Tài liệu (Documentation Hygiene)
*   **Cập nhật Checklist UAT (`2026-07-07_1110_phase-3d_manual-uat-checklist_report.md`)**:
    *   Cập nhật trạng thái của **TC-06** thành `PASS`.
    *   Điều chỉnh mô tả của **TD-04** để làm rõ cơ chế chặn ngày quá khứ (Past-Date Guard) đã hoàn thành và hoạt động đúng nghiệp vụ.
    *   *Lưu ý*: Việc cập nhật này chỉ nhằm mục đích vệ sinh tài liệu (documentation hygiene), phản ánh đúng hiện trạng kiểm thử lịch làm việc quá khứ và hoàn toàn độc lập với logic nghiệp vụ offboarding nhân viên.

## 3. Phân tích Rủi ro & Tình huống Thất bại một phần (Partial Failure)
*   **Khái niệm**: Khi cơ sở dữ liệu ứng dụng đã cập nhật trạng thái thôi việc của nhân viên thành công, nhưng kết nối API đến Keycloak bị lỗi, Keycloak sẽ vẫn giữ tài khoản của nhân viên ở trạng thái hoạt động.
*   **Cách giảm thiểu**: Hệ thống không có cơ chế tự động đồng bộ lại/Retry hoặc Outbox pattern, do đó không thể coi là "an toàn tuyệt đối". Tuy nhiên, rủi ro bảo mật đã được hạn chế tối đa ở mức ứng dụng nhờ cổng đăng nhập `AdminLoginCommandHandler` chặn mọi tài khoản đăng nhập liên kết với nhân viên có `IsActive == false`.
*   **Xử lý thủ công**: Admin cần tiến hành kiểm tra log lỗi hệ thống và thực hiện xóa tài khoản nhân viên này thủ công trên giao diện quản trị Keycloak để đồng bộ hóa hoàn toàn.

## 4. Phạm vi thay đổi (Diff Scope) và Trạng thái Git
*   **Runtime code scope của task**:
    1.  `HRM_Leave_Management/Application/Employees/Delete/DeleteEmployeeCommandHandler.cs` (Logic xóa DB-first + Capture local variables + Log Error + Đổi comment sang tiếng Anh + Message động)
    2.  `HRM_Leave_Management/Application/Users/Login/AdminLoginCommandHandler.cs` (Chặn đăng nhập nhân viên IsActive = false + Comment Technical Debt)
    3.  `HRM_Leave_Management/Domain/Employees/EmployeeErrors.cs` (Mã lỗi KeycloakRevokeFailed)
    4.  `HRM_Leave_Management/Infrastructure/Authentication/AuthenticationService.cs` (Xử lý HTTP 404 là thành công khi xóa User Keycloak)
*   **Documentation scope của task**:
    1.  `MD_memory/reports/2026-07-09_1154_phase-3d_employee-offboarding-refactor_report.md` (Báo cáo cải tiến - file hiện tại)
    2.  `MD_memory/reports/2026-07-07_1110_phase-3d_manual-uat-checklist_report.md` (Checklist UAT cập nhật TC-06 & mô tả TD-04)
*   **Existing dirty/untracked files outside this task**:
    *   Workspace has many pre-existing dirty/untracked files from previous phases; they are outside this task scope and must not be staged unless explicitly approved.

## 5. Kết luận & Trạng thái xác minh (Conclusion & Verification Status)
*   **Trạng thái kiểm tra kỹ thuật (Technical Verification)**: **PASS** (Biên dịch thành công 100%, tích hợp logic DB-first và ghi log chặt chẽ).
*   **Trạng thái UAT thực tế (UAT with real Keycloak/UI)**: **PENDING** (Chờ kiểm thử hành vi thực tế trên UI và Keycloak server).
*   **Technical Debt tại AdminLogin**: Logic `ToListAsync()` lọc in-memory được giữ lại có chủ đích (**intentional technical debt**) nhằm bảo toàn hành vi đăng nhập không phân biệt hoa thường (case-insensitive) của hệ thống hiện tại.
*   **Phương án xử lý lỗi Partial Failure**: Rủi ro thất bại một phần khi xóa tài khoản Keycloak được giảm thiểu thông qua cổng chặn đăng nhập ở tầng ứng dụng (`AdminLoginCommandHandler`) kết hợp ghi log sự kiện lỗi nghiệp vụ chi tiết và thực hiện đối soát thủ công bởi quản trị viên (**app login gate + logs + manual reconcile**), thay vì dùng cơ chế tự đồng bộ tự động như retry hay outbox pattern.
