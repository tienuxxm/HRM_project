# Proposal & Architecture Design V2 — Phase: Single-Step Dynamic Superior Approval Routing



> **File Location**: `MD_memory/plans/2026-07-22_1450_phase-dynamic-approval-routing-design_v2_proposal.md`

> **Phase**: `phase-dynamic-approval-routing-design`

> **Date**: 2026-07-22

> **Status**: 📋 DESIGN PROPOSAL V2 (Single-Step Dynamic Routing — KHÔNG CODE SOURCE / KHÔNG DB MIGRATION)

> **Architecture Boundary Preserved**:

> - `Web.Backend -> Application -> Domain`

> - `Infrastructure -> Application/Domain`



---



## 1. Phân Tích Định Hướng Mới (V2 Refined Scope)



### 1.1. Bản Chất Nghiệp Vụ Chốt

- **KHÔNG PHẢI Multi-step Workflow**: Đơn nghỉ phép KHÔNG phải trải qua chuỗi duyệt nhiều bước tuần tự (Leader duyệt xong -> Manager duyệt tiếp -> Header duyệt tiếp).

- **LÀ Single-Step Dynamic Superior Routing**: Mỗi đơn `Pending` tại một thời điểm **CHỈ CÓ DUY NHẤT 1 ASSIGNED APPROVER HỆN HÀNH**.

- **Cơ chế Superior Escalation**:

  - Khi Employee nộp đơn, hệ thống tra cứu cây cấp bậc/chính sách của phòng ban.

  - Hệ thống tìm cấp trên trực tiếp hợp lệ đầu tiên (Active, Linked User active, có quyền `APPROVE_LEAVE_REQUEST`, không phải chính người tạo đơn).

  - Nếu cấp trên trực tiếp không hợp lệ (nghỉ việc, inactive, chưa tuyển), hệ thống tự động leo cấp (escalate) lên cấp trên tiếp theo **đã được định nghĩa trong policy phòng ban đó**.

  - Nếu không còn cấp nào trong policy hợp lệ, hệ thống **CHẶN TẠO ĐƠN** và thông báo lỗi rõ ràng.



---



## 2. Phản Biện & Đánh Giá Rủi Ro Nghiệp Vụ (Critical Thinking)



### 2.1. Rule "Mỗi Position chỉ có 1 Active Employee trong từng Department"

- **Đánh giá nghiệp vụ**:

  - Với các vị trí quản lý/duyệt (ví dụ: `Team Leader`, `Department Manager`, `Department Head`), quy tắc "1 vị trí - 1 nhân viên active per department" là chuẩn xác và giúp triệt tiêu sự mơ hồ khi auto-pick người duyệt.

  - Với các vị trí chuyên viên/nhân viên (ví dụ: `Software Engineer`, `Accountant`), thực tế có thể có nhiều nhân viên cùng giữ position này trong 1 phòng.

- **Giải pháp thiết kế**:

  - Ràng buộc "Unique Active Employee per Position per Department" sẽ được áp dụng cho **Position được dùng làm Target Approver in Route Rules** (các vị trí quản lý).

  - Hệ thống bổ sung Validation tại CRUD Employee / Position Assignment: Khi gán vị trí quản lý cho nhân viên, nếu phòng ban đó đã có 1 nhân viên Active giữ vị trí đó, hệ thống sẽ cảnh báo/chặn.



### 2.2. Xử Lý Đơn Pending Khi Người Duyệt Thay Đổi (Reassignment Strategy)

- **Đơn đã kết thúc (`Approved`, `Rejected`, `Canceled`)**: **GIỮ NGUYÊN 100%**, không reopen, không reroute, bảo toàn Audit Trail.

- **Đơn đang `Pending`**:

  - Khi người duyệt nghỉ việc/inactive hoặc policy thay đổi, chạy lệnh `ReassignPendingApprovalsCommand`.

  - Hệ thống phân giải lại approver mới theo policy hiện hành.

  - **Nếu KHÔNG tìm thấy approver hợp lệ mới**:

    1. Đơn `Pending` được giữ nguyên nhưng chuyển sang sub-status `Pending_NeedsAdminAttention` kèm Warning flag.

    2. Chặn các đơn nghỉ phép mới từ phòng ban đó cho tới khi Admin/HR gán người duyệt mới.

    3. Đơn `Pending_NeedsAdminAttention` xuất hiện trên Dashboard W4/W5 của Admin/HR dưới dạng "Unassigned Approval Queue" để Admin/HR gán duyệt thủ công hoặc duyệt thay.



---



## 3. Thiết Kế Domain Model V2 (Single-Step Dynamic Routing)



### 3.1. Các Domain Entities Đề Xuất



1. **`ApprovalRoutePolicy` (Aggregate Root)**: Policy cấu hình routing cho phòng ban.

   - `Id` (Guid)

   - `DepartmentId` (Guid?, null = Policy mặc định toàn công ty)

   - `Name` (string, e.g. "Chính sách duyệt phòng IT")

   - `IsActive` (bool)



2. **`ApprovalRouteRule` (Entity thuộc `ApprovalRoutePolicy`)**: Quy tắc phân giải người duyệt theo chức danh hoặc chỉ định.

   - `Id` (Guid)

   - `PolicyId` (Guid)

   - `RequesterPositionId` (Guid?, null = Áp dụng cho mọi vị trí trong phòng)

   - `SuperiorHierarchyPositionIds` (List<Guid>: Danh sách ID các vị trí cấp trên theo thứ tự ưu tiên leo cấp, e.g. `[LeaderPosId, ManagerPosId, HeaderPosId]`)

   - `SpecificApproverEmployeeId` (Guid?, Chỉ định đích danh 1 nhân viên duyệt cố định, e.g. case `Employee -> Employee` duyệt thay)

   - `IsActive` (bool)



3. **`LeaveRequestApprovalAssignment` (Entity 1:1 với `LeaveRequest`)**: Lưu duy nhất 1 người duyệt hiện hành cho đơn.

   - `Id` (Guid)

   - `LeaveRequestId` (Guid, Unique Index)

   - `AssignedApproverEmployeeId` (Guid, Nhân viên duy nhất đang nắm quyền duyệt đơn này)

   - `AssignedRouteRuleId` (Guid?)

   - `AssignedAt` (DateTime)

   - `AssignmentReason` (Enum: `DirectPositionMatch`, `SuperiorEscalated`, `SpecificEmployeeAssigned`, `AdminManualReassigned`)



4. **`ApprovalRouteAuditLog` (Entity lưu vết lịch sử chuyển người duyệt)**:

   - `Id` (Guid)

   - `LeaveRequestId` (Guid)

   - `PreviousApproverEmployeeId` (Guid?)

   - `NewApproverEmployeeId` (Guid)

   - `TriggerReason` (string, e.g. "Leave Created", "Approver Deactivated", "Admin Reassigned")

   - `CreatedDate` (DateTime)



---



## 4. Text-Based ERD Proposal



```

+---------------------+       1:N       +-------------------------+

| ApprovalRoutePolicy | --------------> |    ApprovalRouteRule    |

+---------------------+                 +-------------------------+

| Id (PK)             |                 | Id (PK)                 |

| DepartmentId (FK)   |                 | PolicyId (FK)           |

| Name                |                 | RequesterPositionId(FK) |

| IsActive            |                 | SuperiorHierarchyPositions (JSON/Child Table)|

+---------------------+                 | SpecificApproverEmpId   |

                                        +-------------------------+



+---------------------+       1:1       +-----------------------------------+

|    LeaveRequest     | --------------> | LeaveRequestApprovalAssignment    |

+---------------------+                 +-----------------------------------+

| Id (PK)             |                 | Id (PK)                           |

| EmployeeId (FK)     |                 | LeaveRequestId (FK, Unique)       |

| Status (Pending...) |                 | AssignedApproverEmployeeId (FK)   |

+---------------------+                 | AssignmentReason                  |

                                        +-----------------------------------+

```



---



## 5. Giải Quyết 8 Scenarios Nghiệp Vụ Yêu Cầu



1. **Phòng IT (Header -> Manager -> Leader -> Employee)**:

   - `SuperiorHierarchyPositionIds = [Leader, Manager, Header]`. Employee nộp đơn -> gán cho Leader.

2. **Phòng HRM (Header -> Employee)**:

   - `SuperiorHierarchyPositionIds = [Header]`. Employee nộp đơn -> gán trực tiếp cho Header.

3. **Phòng Kinh doanh (Leader -> Manager -> Employee)**:

   - `SuperiorHierarchyPositionIds = [Manager, Leader]`. Động theo danh sách position cấu hình trong rule, không hardcode tên chức danh.

4. **Leader nghỉ/inactive (Dynamic Escalation)**:

   - Employee phòng IT nộp đơn. Leader inactive -> Engine bỏ qua Leader, kiểm tra Manager. Manager active & hợp lệ -> Gán đơn cho Manager (`AssignmentReason = SuperiorEscalated`). Đơn vẫn chỉ có **1 Approver hiện hành là Manager**.

5. **Phòng 2 cấp chưa có Leader / Leader nghỉ (Validation & Chặn nộp đơn)**:

   - Policy phòng chỉ định `SuperiorHierarchyPositionIds = [Leader]`.

   - Khi Leader bị inactive hoặc chưa tuyển, Engine tra cứu danh sách cấp trên trong policy -> Không có cấp nào hợp lệ.

   - **Hành vi**: System chặn lệnh tạo đơn ở Application Layer (`CreateLeaveRequestCommandHandler`) và trả lỗi:

     `"Approval route is not configured for this department. Please assign an approver before submitting leave request."`

6. **Chỉ định đích danh người duyệt (Specific Approver)**:

   - Admin/HR gán `SpecificApproverEmployeeId = Employee_B` cho `RequesterPosition = Employee`. Nhân viên A nộp đơn -> Đơn gán cho Employee B duyệt (`Employee -> Employee`), không cần đổi Position của Employee B.

7. **Test Case Duyệt ngược (`Employee -> Header`)**:

   - Cấu hình rule cho Position Header có `SpecificApproverEmployeeId = Employee_C` hoặc `SuperiorHierarchyPositionIds = [EmployeePositionId]`. Header nộp đơn -> Đơn chuyển cho Employee C duyệt. Engine chứng minh tính policy-driven 100%.

8. **Nguồn dữ liệu Dashboard W4/W5**:

   - W4/W5 query trực tiếp `LeaveRequestApprovalAssignment` kết hợp `LeaveRequest.Status == Pending`.

   - Với Scoped Approver: Filter `AssignedApproverEmployeeId == currentEmployee.Id`.

   - Với Admin/HR Global View: Hiển thị tất cả các đơn `Pending` từ active employees, bao gồm cả các đơn thuộc `Pending_NeedsAdminAttention` để xử lý kịp thời.



---



## 6. So Sánh Với `LeaveApproverAssignment` Hiện Tại



| Tiêu chí | `LeaveApproverAssignment` Hiện Tại | Module Single-Step Routing Mới |

|---|---|---|

| **Khái niệm** | Scope filter duyệt tĩnh (Ai được duyệt phòng nào). | Routing Policy giải quyết Cấp trên hợp lệ (Superior Resolution Engine). |

| **Độ linh hoạt** | Không biết ai là cấp trên chính thức của ai. | Định nghĩa rõ chuỗi cấp bậc ưu tiên per department/position. |

| **Khuyến nghị** | Deprecate hoặc biến thành read-only fallback. | Tạo module mới `ApprovalRoutePolicy` để tránh phá hỏng semantics cũ. |



---



## 7. Các Câu Hỏi Cần User / Codex Chốt Trước Khi Sửa Code



1. **Xác nhận cấu trúc Entity V2**: User có chốt mô hình Single-Step (`LeaveRequestApprovalAssignment` lưu 1 `AssignedApproverEmployeeId` duy nhất) không?

2. **Thông báo lỗi khi không có Approver**: Thông báo lỗi `"Approval route is not configured for this department. Please assign an approver before submitting leave request."` khi chặn nộp đơn đã đúng yêu cầu UX chưa?

3. **Cơ chế cho đơn Pending bị mất Approver**: Đơn Pending cũ khi bị mất Approver sẽ chuyển sang `Pending_NeedsAdminAttention` để Admin/HR gán duyệt thủ công, User có đồng ý với cơ chế này không?
