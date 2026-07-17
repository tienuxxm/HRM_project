# UAT Report: Leave Request Detail UI Audit

## Context
- **Phase:** Design Audit - Leave Request Detail Parity
- **Target File:** `HRM_Leave_Management/Web.Backend/Views/LeaveRequest/Detail.cshtml`
- **Objective:** Tái cấu trúc UI cho trang chi tiết đơn xin nghỉ phép (Leave Request Detail) nhằm đạt chuẩn thiết kế Stitch (2-column layout trên Desktop, stacked layout trên Mobile), áp dụng mật độ không gian "Enterprise Calm" (Swiss International), xử lý triệt để lỗi "double-header", đồng thời bảo toàn nguyên vẹn C# models và JavaScript contracts.

## Build Check & Impact Analysis
- **C# Compilation Check:** Đã chạy `dotnet build`. Kết quả xác nhận không có bất kỳ lỗi `error CS` nào phát sinh từ file `Detail.cshtml`. Model `@model Application.LeaveRequests.Get.LeaveRequestResponse` được giữ nguyên.
- **JavaScript Contract:** Flow duyệt đơn với hàm `submitApproval(action)`, Payload mapping (`FormData`), và API endpoints (`/leave-request/approve`, `/leave-request/reject`) được giữ nguyên bản.
- **Git Status:** 
  - File đã sửa: `M HRM_Leave_Management/Web.Backend/Views/LeaveRequest/Detail.cshtml`
  - Đã giữ sạch cây thư mục. Chưa stage, chưa commit, chưa push theo đúng quy định.
- **AppHost Lock Note:** Build hiện tại đang lock tiến trình do Ứng dụng Backend đang chạy (`The file is locked by: "Web.Backend (4544)"`), nhưng Razor code parsing thành công và không gây lỗi CS.

---

## Hướng Dẫn Manual UAT Dành Cho User

Mặc định subagent/browser không tự động UAT để đảm bảo an toàn. Xin vui lòng tự kiểm tra thông qua trình duyệt đang mở theo các bước sau:

**1. Điều Kiện Tiền Đề (Pre-requisites)**
- **Auth Mode:** Đang dùng Keycloak thật (`UseMockAuth: false`).
- **Account:** Sử dụng tài khoản HR/Admin đã cấu hình (VD: `admin` / `Admin@123456`).
- **Data:** Đảm bảo có ít nhất 1 Leave Request ở trạng thái **Pending** để test Approval Panel.

**2. Các Bước Kiểm Tra Layout**
- Truy cập vào danh sách đơn từ chối / xin nghỉ (`/leave-request`) và bấm xem **Chi Tiết** một đơn bất kỳ.
- **[Desktop View - Window > 1000px]**
  - Xác minh không còn lỗi "Double-header". Header compact nằm gọn trong khung chính với đường viền đen đậm.
  - Xác minh bố cục 2 cột (Left: Particulars, Right: Status & Panel) hiển thị chính xác.
  - Xác minh font chữ, viền (#D1D1D1) và background màu xám nhạt (`#faf9f9`) được áp dụng đúng chuẩn Enterprise.
- **[Mobile View - Inspect bằng DevTools hoặc Resize Window < 768px]**
  - Xác minh nội dung đổ xuống thành Stack dọc 1 cột.
  - Xác minh breadcrumb có nút "<" (Back) hiển thị thay vì dãy text dài.
  - Ngày tháng (`StartDate`, `EndDate`) hiển thị trên 1 dòng text liền mạch.

**3. Các Bước Kiểm Tra Nghiệp Vụ (Contracts)**
- Tìm đơn có trạng thái **Pending**.
- **Test Panel Duyệt Đơn:**
  - Nhập vào "Approver Comment".
  - Click **Approve Request** hoặc **Reject Request**.
  - Xác minh Request được gửi qua network (Check tab Network trong F12) là Payload `FormData` đúng gốc.
  - Xác minh trang reload lại thành công hoặc hiển thị Toast message sau khi duyệt.
- Tìm đơn đã **Approved** hoặc **Rejected**:
  - Xác minh "Processing History" ở cột trái đã hiển thị thời gian, người xử lý, và nội dung comment (nếu có).

## Đề Xuất Tiếp Theo
Nếu mọi bước UAT hiển thị đúng và logic không bị ảnh hưởng, bạn có thể thực hiện checkpoint commit local. Sau đó chúng ta sẽ chuyển sang Phase tiếp theo (List Position hoặc Refine UX khác).
