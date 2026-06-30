# Báo cáo Thiết lập Baseline & Đánh giá Trạng thái Dự án HRM

> **Ngày lập:** 2026-06-24  
> **Trạng thái:** Hoàn thành thiết lập Baseline (Đăng nhập & hiển thị Dashboard OK)  
> **Mục tiêu:** Kiểm tra và bàn giao hiện trạng hệ thống trước khi bước vào Phase refactor nghiệp vụ.

---

## 1. Trạng thái Git & Danh sách các File thay đổi (Git Status)

Do dự án đang trong giai đoạn chuyển đổi và nhân bản, toàn bộ thư mục `HRM_Leave_Management` và các tài liệu bổ trợ đang ở trạng thái **Untracked** (chưa thực hiện commit nào trên branch `master` cục bộ). 

Kết quả lệnh chạy `git status --short` tại root workspace:
```text
?? .agents/
?? .claude/
?? .understand-anything/
?? AGENTS.md
?? Application/
?? CLAUDE.md
?? Domain/
?? HRM_Leave_Management/
?? HRM_Leave_Management_old/
?? Infrastructure/
?? LUC.sln
?? MD_memory/
?? Persistence/
?? Readme.txt
?? Web.Backend/
```

### Các file thực tế đã được chỉnh sửa/tạo mới trong bước Baseline:

1. **Chỉnh sửa cấu hình chạy dự án mới (`HRM_Leave_Management`):**
   * **File:** `HRM_Leave_Management\Web.Backend\appsettings.json`
     * *Nội dung:* Trỏ Connection String sang database phát triển riêng `hrm_baseline_db`; Cấu hình các tham số xác thực OpenID Connect (Audience, Issuer, MetadataUrl) và Client Secret của Keycloak (`hrm` realm) để liên kết xác thực cục bộ.
   * **File:** `HRM_Leave_Management\Web.Backend\Properties\launchSettings.json`
     * *Nội dung:* Thay đổi cổng HTTP từ `5200` sang `5300`, HTTPS từ `7067` sang `7068` nhằm tránh xung đột cổng với dự án gốc khi chạy song song.

2. **Tạo mới công cụ Seed dữ liệu phân quyền:**
   * **File:** `MD_memory\debug\SeedDb\Program.cs`
     * *Nội dung:* Script console ứng dụng Npgsql để trực tiếp ghi các bản ghi phân quyền (Admin role, UserToRole, Permission và RoleToPermission) vào DB `hrm_baseline_db` giúp tài khoản Admin truy cập được các trang mà không bị lỗi 403 (No Permission).

3. **Cập nhật tài liệu thiết kế và phân tích:**
   * **File:** `MD_memory\hrm_leave_management_tach_du_an_plan.md`
     * *Nội dung:* Kế hoạch tách dự án HRM theo chiến lược "mini baseline trước, refactor từng lớp sau".
   * **File:** `MD_memory\gap_analysis.md`
     * *Nội dung:* Đánh giá sự khác biệt (gaps) về giao diện, route và luồng cookie giữa ứng dụng gốc và dự án mới.

---

## 2. Cảnh báo Quản lý Nguồn & Bảo mật (Source Code Management & Security Alerts)

> [!WARNING]
> Cần lưu ý các điểm sau để đảm bảo an toàn thông tin và quản lý mã nguồn sạch:
> * **Thư mục cũ `HRM_Leave_Management_old/`:** Đây là thư mục chứa bản copy cũ không còn sử dụng. Cần đưa thư mục này vào danh sách `.gitignore` (hoặc tiến hành xóa hẳn khi có sự đồng ý của Tech Lead) để tránh vô tình commit các tệp tin rác này vào kho lưu trữ Git.
> * **Mật khẩu viết cứng trong Script Seed (`MD_memory/debug/SeedDb/Program.cs`):** File này đang chứa chuỗi kết nối Postgres có ghi cứng mật khẩu (`Password=12345@abc`). Đây là script debug chạy một lần ở máy phát triển local và **tuyệt đối không được commit** lên nhánh production hoặc các môi trường dùng chung.
> * **Keycloak Client Secret trong `appsettings.json`:** Client secret được thiết lập trong cấu hình dự án HRM hiện tại chỉ dùng cho môi trường local development (môi trường Keycloak cục bộ chạy ở port 8080). Khi triển khai lên các môi trường staging/production, giá trị này bắt buộc phải lấy qua cấu hình biến môi trường hoặc user secrets, không được ghi trực tiếp vào tệp source kiểm soát phiên bản.

---

## 3. Đính chính thông tin sửa đổi mã nguồn (Mã nguồn & Cấu hình)

> [!NOTE]
> **Đính chính chính xác phạm vi thay đổi:**
> Hệ thống **không** chỉnh sửa bất kỳ business logic/mã nguồn nghiệp vụ nào trong dự án gốc (`Domain`, `Application`, `Infrastructure`, `Web.Backend` của Project LUC). 
> Tuy nhiên, đã thực hiện chỉnh sửa các tệp cấu hình chạy (`appsettings.json`, `launchSettings.json`) trong bản sao `HRM_Leave_Management` và viết thêm các script công cụ debug/seed dữ liệu ở ngoài (`MD_memory/debug/SeedDb`).

---

## 4. Xác nhận trạng thái "OK" của 10 trang quản trị

Trạng thái **"OK"** ở bước này được định nghĩa là:
* Yêu cầu HTTP trả về thành công (Status Code 200).
* Hệ thống vượt qua được bộ lọc phân quyền 403 (No Permission) nhờ tài khoản Admin được gán đầy đủ các quyền.
* Giao diện HTML/CSS render cấu trúc Dashboard và Sidebar hoàn chỉnh từ máy chủ.
* **Chưa thực hiện xác minh nghiệp vụ ghi/thao tác sâu** (như tạo mới/sửa/xóa hoặc các quy trình xử lý của dự án LUC cũ), vì mục tiêu duy nhất của Phase này là dựng thành công bản build chạy được (baseline) trước khi refactor.

---

## 5. Đánh giá nguyên nhân lỗi 500 của 7 Route: Mức độ chắc chắn

Qua phân tích mã nguồn tĩnh, chúng tôi xác định nguyên nhân chính gây lỗi 500 khi truy cập các route này là do logic gọi dịch vụ lưu trữ AWS S3 để lấy URL ảnh tạm thời, trong khi cấu hình AWS tại `appsettings.json` hiện đang bỏ trống.

> [!IMPORTANT]
> **Tuyên bố về mức độ chắc chắn:**
> * **Đã chứng minh code path tĩnh:** Cả 7 Query Handlers bên dưới đều có luồng logic trực tiếp gọi `_awsS3Service.GetUrlPresign(...)` khi map dữ liệu entity sang Response DTO. Do cơ sở dữ liệu seed có sẵn dữ liệu mock (chứa key ảnh mock), các hàm này bắt buộc phải thực thi.
> * **Chưa verify stack trace runtime:** Chúng tôi chưa thực hiện capture stack trace lỗi trực tiếp lúc runtime của từng route từ log chạy thực tế của ứng dụng.

### Chi tiết các điểm gọi dịch vụ AWS S3 trong code:

| Route lỗi | Handler xử lý dữ liệu | Dòng code gọi S3 | Chi tiết logic gọi |
| :--- | :--- | :--- | :--- |
| `/member` (Khách hàng) | `GetAllMemberPagedCommandHandler.cs` | [Dòng 55](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Application/Members/GetAllPaged/GetAllMemberPagedCommandHandler.cs#L55) | `AvatarUrl = member.Avatar != null ? _awsS3Service.GetUrlPresign(member.Avatar.Value) : ""` |
| `/products` (Thực đơn) | `GetProductsCommandHandler.cs` | [Dòng 68](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Application/Products/GetAllPaged/GetProductsCommandHandler.cs#L68) | `ImageUrl = _awsS3Service.GetUrlPresign(p.ImageUrl.Value) ?? ""` |
| `/news` (Tin tức) | `GetAllNewsPagedCommandHandler.cs` | [Dòng 42](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Application/News/GetAllPaged/GetAllNewsPagedCommandHandler.cs#L42) | `Thumbnail = _awsS3Service.GetUrlPresign(n.Thumbnail.Value)` |
| `/restaurant` (Chi nhánh) | `GetAllRestaurantPagedCommandHandler.cs` | [Dòng 47](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Application/Restaurants/GetAllPaged/GetAllRestaurantPagedCommandHandler.cs#L47) | `ImageUrl = r.ImageKey is null ? "" : _awsS3Service.GetUrlPresign(r.ImageKey.Value)` |
| `/voucher` (Vouchers) | `GetAllVoucherPagedCommandHandler.cs` | [Dòng 65 & 69](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Application/Vouchers/GetAllPaged/GetAllVoucherPagedCommandHandler.cs#L65) | `ImageUrl = _awsS3Service.GetUrlPresign(...)` và `QrCodeImageUrl = _awsS3Service.GetUrlPresign(...)` |
| `/promotion` (Khuyến mãi) | `GetPromotionsCommandHandler.cs` | [Dòng 51](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Application/Promotions/GetAllPaged/GetPromotionsCommandHandler.cs#L51) | `ImageUrl = _awsS3Service.GetUrlPresign(p.ImageUrl.Value)` |
| `/partner` (Đối tác) | `GetAllPartnerPagedCommandHandler.cs` | [Dòng 35](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Application/Partners/GetAllPaged/GetAllPartnerPagedCommandHandler.cs#L35) | `ImageUrl = x.QrCode != null ? _awsS3Service.GetUrlPresign(x.QrCode.Value, 60) : ""` |
