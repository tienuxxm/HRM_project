# Báo Cáo Kết Quả UAT - Phase 2C.3: Leave Request Management

- **Ngày UAT**: 2026-06-29
- **Người thực hiện**: Codex / User (UAT qua HTTP session thật)
- **Trạng thái**: **UAT PASS / CHỜ USER APPROVE** ✅
- **Auth mode**: Keycloak thật (`UseMockAuth = false`)
- **Routes test**: `/leave-request`, `/leave-balance`
- **Accounts UAT**: `admin` / `Admin@123456`, `employee` / `Admin@123456`
- **Server**: `http://localhost:5300` (Web.Backend chạy bằng `dotnet run --launch-profile http`)
- **Database**: Postgres `hrm_baseline_db`

---

## 1. Tóm Tắt Kết Quả

Toàn bộ **16 test case chính + 1 post-condition balance check** đã được kiểm tra UAT qua UI browser kết hợp HTTP/API cho các case không thể thao tác trực tiếp trên UI (ví dụ: cancel đơn đã Canceled). Không sửa code/config/Keycloak trong quá trình test. **Tất cả đều PASS.**

---

## 2. Chi Tiết Kết Quả Theo Từng Case

### 2.1. Phân quyền (Permission-driven)

| STT | Kịch Bản | Kết Quả | Trạng Thái |
| :--- | :--- | :--- | :---: |
| 1 | **Admin login** → thấy Employee filter | Admin đăng nhập thành công, bộ lọc nhân viên hiển thị đầy đủ, mặc định "All Employees" | ✅ Pass |
| 2 | **Admin xem toàn bộ đơn** | Danh sách hiển thị tất cả đơn nghỉ phép của mọi nhân viên | ✅ Pass |
| 3 | **Admin không có Approve/Reject** | Không hiển thị nút Approve/Reject (đúng thiết kế Phase 2C.3, chờ Phase 3) | ✅ Pass |
| 4 | **Employee login** → không thấy Employee filter | Employee đăng nhập thành công, không hiển thị bộ lọc nhân viên | ✅ Pass |
| 5 | **Employee self-view** | Chỉ thấy đơn của chính mình (EMP002 - Nguyen Van Employee) | ✅ Pass |
| 6 | **Employee có nút Request Leave** | Nút "Request Leave" hiển thị trên giao diện | ✅ Pass |

### 2.2. Tạo đơn nghỉ phép (Create Leave Request)

| STT | Kịch Bản | Dữ Liệu | Kết Quả Thực Tế | Trạng Thái |
| :--- | :--- | :--- | :--- | :---: |
| 7 | Tạo đơn hợp lệ (nguyên ngày) | FullDay→FullDay, 2 ngày | `total_days = 2.0`, status Pending | ✅ Pass |
| 8 | **Calendar days** (qua cuối tuần) | Fri 2026-10-02 → Mon 2026-10-05, FullDay→FullDay | `total_days = 4.0` (không loại trừ T7/CN) | ✅ Pass |

### 2.3. Validation nghiệp vụ (Reject cases)

| STT | Kịch Bản (Validation Rule) | Dữ Liệu | Error Message Thực Tế | Trạng Thái |
| :--- | :--- | :--- | :--- | :---: |
| 9 | Morning + Afternoon cùng ngày (V-5) | Cùng ngày, Morning→Afternoon | `"Invalid session selection for a single-day request"` | ✅ Pass |
| 10 | Cross-year (V-4) | 2026-12-30 → 2027-01-02 | `"A leave request cannot cross multiple calendar years"` | ✅ Pass |
| 11 | EndDate < StartDate (V-2) | Start > End | `"The start date must be before or equal to the end date"` | ✅ Pass |
| 12 | Past date (V-3) | Start = ngày quá khứ | `"Creating a leave request in the past is not allowed"` | ✅ Pass |
| 13 | Overlap Pending (V-6) | Trùng ngày với đơn Pending hiện có | `"This request overlaps with an existing pending or approved leave request"` | ✅ Pass |
| 14 | Insufficient balance (V-7) | Xin vượt quá AvailableDays | `"Insufficient available leave days for this request"` | ✅ Pass |

### 2.4. Hủy đơn (Cancel)

| STT | Kịch Bản | Thao Tác | Kết Quả Thực Tế | Trạng Thái |
| :--- | :--- | :--- | :--- | :---: |
| 15 | Cancel Pending thành công | Click Cancel trên đơn Pending | Đơn chuyển sang Canceled, PendingDays giải phóng | ✅ Pass |
| 16 | Cancel đơn đã Canceled | Gửi request API cancel đơn Canceled | Bị reject đúng (đơn không ở trạng thái Pending) | ✅ Pass |

### 2.5. Leave Balance sau Cancel

| STT | Kịch Bản | Thao Tác | Kết Quả Thực Tế | Trạng Thái |
| :--- | :--- | :--- | :--- | :---: |
| 17 | Balance hoàn trả sau cancel | Cancel 1 đơn Pending → kiểm tra `/leave-balance` | AvailableDays tăng trở lại đúng số liệu | ✅ Pass |

---

## 3. Enum Values Đã Xác Nhận (Implementation Hiện Tại)

| Enum | Giá trị |
| :--- | :--- |
| `LeaveDayPart.FullDay` | 1 |
| `LeaveDayPart.Morning` | 2 |
| `LeaveDayPart.Afternoon` | 3 |
| `LeaveRequestStatus.Pending` | 1 |
| `LeaveRequestStatus.Approved` | 2 |
| `LeaveRequestStatus.Rejected` | 3 |
| `LeaveRequestStatus.Canceled` | 4 |

---

## 4. Vấn Đề UX Ghi Nhận (Technical Debt)

> [!NOTE]
> Các vấn đề sau **không ảnh hưởng đến tính đúng đắn logic/nghiệp vụ**, chỉ liên quan đến trải nghiệm người dùng (UX). Ghi nhận để cải thiện ở phase riêng.

### 4.1. `window.alert()` cho thông báo lỗi/thành công

- **Hiện trạng**: Leave Request đang dùng `window.alert(...)` của browser để hiển thị thông báo validation error và success.
- **Vấn đề**: Giao diện alert mặc định của browser xấu, không chuyên nghiệp, chặn luồng xử lý.
- **Đề xuất thay thế** (phase riêng "UX Notification Refactor"):
  - **Success/Error ngắn**: Dùng `Toastify` (đã load sẵn trong `_Layout.cshtml`) hoặc Flowbite Toast.
  - **Confirm nguy hiểm** (cancel đơn): Tiếp tục dùng Flowbite Modal như hiện tại.
  - **Validation trong form**: Hiển thị inline error box trong modal/form thay vì popup browser.
- **MVP đề xuất**: Tạo helper JS `showToast(type, message)` dùng Toastify, thay toàn bộ `window.alert()` trên các trang Leave Request, Leave Balance, Leave Type.
- **Không implement trong lượt này**. Chỉ ghi nhận vào technical debt.

---

## 5. Phase Tiếp Theo: Phase 3 — Approval Flow

### 5.1. Phạm vi dự kiến

Theo plan tổng Phase 2C (`plans/2026-06-25_1515_phase-2c_leave-management-plan.md`, mục BD-13 và mục 1), Phase 3 sẽ triển khai:

1. **Approve đơn Pending**: Admin/HR phê duyệt đơn → tăng `UsedDays` trong `leave_balance` theo `TotalDays` của đơn.
2. **Reject đơn Pending**: Admin/HR từ chối đơn → **không tăng, không hoàn `UsedDays`** (vì đơn Pending chưa bao giờ trừ `UsedDays`).
3. **ApproverComment**: Optional cho cả Approve và Reject. **Không validate bắt buộc comment khi reject.**
4. **Chống double-approve / double-deduct**: Transaction/concurrency control đảm bảo approve 1 đơn chỉ trừ balance 1 lần.
5. **Overlap check mở rộng** (BD-10): Phase 3 check cả đơn `Pending + Approved`, không chỉ `Pending`.
6. **UI nút Approve/Reject**: Hiển thị cho user có quyền `APPROVE_LEAVE_REQUEST`.
7. **Notification** (tùy plan): Thông báo cho nhân viên khi đơn được duyệt/từ chối.
8. **Reverse/Cancel Approved**: Không thuộc scope Phase 3 nếu user chưa chốt.

### 5.2. Điều kiện vào Phase 3

- Phase 2C.3 UAT PASS ✅ (đã xong).
- Phase 2C.3 được User APPROVE chính thức (đang chờ).
- Plan Phase 3 được tạo, review, và User duyệt trước khi code.

### 5.3. Trạng thái hiện tại

**Chưa code Phase 3. Chờ User duyệt Phase 2C.3 và duyệt plan Phase 3 trước.**

---

## 6. Lưu Ý An Toàn

- Không thao tác Keycloak trong quá trình UAT.
- Không reset password bất kỳ tài khoản nào.
- `UseMockAuth` luôn là `false` trong suốt quá trình test.
- Không có code/config/migration nào bị thay đổi trong quá trình UAT.
