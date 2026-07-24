# Proposal & Architecture Design V3 — Single-Step Dynamic Superior Approval Routing



> **File Location**: `MD_memory/plans/2026-07-22_1520_phase-dynamic-approval-routing-design_v3_proposal.md`

> **Phase**: `phase-dynamic-approval-routing-design`

> **Date**: 2026-07-22

> **Status**: 📋 DESIGN PROPOSAL V3 (Single-Step Dynamic Routing + Approval Route Levels + Lifecycle Protection — KHÔNG CODE SOURCE / KHÔNG DB MIGRATION)

> **Architecture Boundary Preserved**:

> - `Web.Backend -> Application -> Domain`

> - `Infrastructure -> Application/Domain`



---



## 1. Tóm Tắt Định Hướng Mới (V3 Refined Scope)



### 1.1. Bản Chất Nghiệp Vụ V3

1. **Single-Step Dynamic Routing**: Mỗi đơn `Pending` tại một thời điểm **CHỈ CÓ 1 ASSIGNED APPROVER HIỆN HÀNH**. Đơn KHÔNG trải qua chuỗi duyệt nhiều bước tuần tự.

2. **Tách Biệt Position & Approval Route Level**:

   - KHÔNG ép toàn bộ `Position` (như Software Engineer, Accountant) theo quy tắc "1 active employee per position per department".

   - Tách riêng khái niệm **`ApprovalRouteLevel`** (Cấp duyệt trong phòng ban). Ràng buộc "Unique Active Employee per Level per Department" chỉ áp dụng cho các slot cấp duyệt có `CanApproveLeave = true`.

3. **Specific Approver Precedence**: Ưu tiên người duyệt chỉ định đích danh (`SpecificApproverEmployeeId`). Nếu người duyệt đích danh không hợp lệ (inactive, no permission), fallback sang cấp duyệt theo Level Rank kèm cảnh báo Audit.

4. **Lifecycle Safety (Bảo vệ khi Approver nghỉ việc/xóa/inactive)**:

   - Khi HR/Admin inactive hoặc xóa một Approver, Application layer **bắt buộc kiểm tra tác động (Impact Analysis)**.

   - Nếu có đơn `Pending` bị ảnh hưởng, UI bắt buộc yêu cầu HR/Admin chọn phương án xử lý (Assign người thay thế, Override specific approver, hoặc Auto-route sang cấp cao hơn) trước khi cho phép hoàn tất thao tác.

5. **Trạng Thái Đơn Bảng Sạch (Clean Domain State)**:

   - Giữ nguyên `LeaveRequest.Status = LeaveRequestStatus.Pending`.

   - Quản lý chi tiết trạng thái routing trên `LeaveRequestApprovalAssignment.AssignmentStatus` (`Assigned`, `Unassigned`, `NeedsAdminAttention`, `Reassigned`).



---



## 2. Phân Tích & Phản Biện Chi Tiết (Critical Evaluation)



### 2.1. Đánh giá Đề Xuất "Approval Route Level" (Codex Proposal)

- **Đánh giá của Anti**: **HOÀN TOÀN HỢP LÝ VÀ CHÍNH XÁC**.

- **Lý do**:

  - `Position` trong thực tế là chức danh công việc chuyên môn (ví dụ: Senior Software Engineer, Accountant). Một phòng ban có thể có 50 nhân viên cùng mang `Position = Senior Software Engineer`. Nếu khóa "1 active employee per position", module Quản lý Nhân sự (Employee) sẽ bị phá hỏng.

  - Việc tách riêng lớp cấu hình **`ApprovalRouteLevel`** giúp phân định rõ:

    - Nhân viên chuyên môn: Tùy ý tạo nhiều nhân viên cùng `Position`.

    - Slot Duyệt phép (`ApprovalRouteLevel`): Ví dụ Level 1 (Trưởng nhóm), Level 2 (Trưởng phòng), Level 3 (Giám đốc khối). Mỗi slot duyệt phép này trong 1 phòng ban **chỉ có tối đa 1 Nhân viên Active đảm nhận tại một thời điểm**.



### 2.2. Thứ Tự Phân Giải Approver & Cơ Chế Fallback (Specific Approver Precedence)

- **Quy tắc Ưu tiên**:

  1. **Bước 1**: Kiểm tra xem Rule có `SpecificApproverEmployeeId` không.

     - Nếu CÓ và Nhân viên đó HỢP LỆ (IsActive = true, Linked User active, có quyền `APPROVE_LEAVE_REQUEST`, không phải chính người tạo đơn): **Specific Approver CHIẾN THẮNG** (`AssignmentReason = SpecificEmployeeOverride`).

  2. **Bước 2**: Nếu Specific Approver bị INACTIVE / KHÔNG CÓ QUYỀN / NGHỈ VIỆC:

     - **Lựa chọn thiết kế của Anti (Option 2B - Best Practice)**: Tự động Fallback sang cấp duyệt theo `LevelRank` tiếp theo được cấu hình trong Policy phòng ban, đồng thời ghi log warning vào `ApprovalRouteAuditLog` ("Specific approver X inactive, fell back to Route Level Y").

     - Nếu không có cấp Level nào hợp lệ -> Chuyển trạng thái đơn sang `AssignmentStatus = NeedsAdminAttention` và phát thông báo lỗi cho người nộp đơn.



---



## 3. Quy Trình Bảo Vệ Lifecycle (Approver Inactive / Delete / Reassignment Workflow)



Khi HR/Admin thực hiện thao tác: **Inactive Employee**, **Xóa Employee**, hoặc **Gỡ Employee khỏi Approval Route Level**:



```

[HR/Admin ấn Inactive/Delete/Unassign Approver B]

                     │

                     ▼

[Application Layer: Check Impact Analysis]

  - Tìm tất cả LeaveRequest (Pending) có AssignedApprover = Approver B

  - Tìm tất cả ApprovalRouteLevel do Approver B đảm nhận

                     │

         ┌───────────┴───────────┐

         │ Impact Count == 0     │ Impact Count > 0

         ▼                       ▼

  [Cho phép Inactive     [TẠM DỪNG THAO TÁC + Trả về Impact Summary DTO]

   bình thường]           - Hiển thị danh sách X đơn Pending bị ảnh hưởng

                          - Hiển thị danh sách Y Route Level bị trống

                                 │

                                 ▼

                          [UI Modal: Bắt buộc Admin/HR chọn 1 trong 4 Phương án]

                          ├── A. Gán Nhân viên thay thế vào Approval Level đó.

                          ├── B. Gán Specific Approver Override cho các đơn bị ảnh hưởng.

                          ├── C. Auto-route các đơn sang cấp cao hơn (nếu policy có).

                          └── D. Hủy thao tác (Cancel).

                                 │

                                 ▼

                          [Thực thi Reassignment + Hoàn tất Inactive Approver B]

```



**An Toàn Tuyệt Đối**: Hệ thống **KHÔNG CHO PHÉP** Inactive/Xóa Approver nếu Admin/HR chưa chọn phương án xử lý đơn Pending bị ảnh hưởng.



---



## 4. Thiết Kế Domain Model V3 & Database Schema Text-Based



### 4.1. Các Domain Entities V3



1. **`ApprovalRoutePolicy` (Aggregate Root)**: Cấu hình quy trình duyệt phòng ban.

   - `Id` (Guid)

   - `DepartmentId` (Guid?, null = Global Policy)

   - `Name` (string)

   - `IsActive` (bool)



2. **`ApprovalRouteLevel` (Entity thuộc Policy)**: Cấp duyệt trong phòng ban.

   - `Id` (Guid)

   - `PolicyId` (Guid)

   - `LevelName` (string, e.g. "Direct Leader", "Department Manager", "Director")

   - `LevelRank` (int: 1, 2, 3...)

   - `CanApproveLeave` (bool, mặc định `true`)

   - `AssignedEmployeeId` (Guid?, Nhân viên Active DUY NHẤT nắm giữ slot duyệt này per department)

   - `IsActive` (bool)



3. **`ApprovalRouteRule` (Entity thuộc Policy)**: Định nghĩa rule phân giải cho từng loại nhân viên/vị trí.

   - `Id` (Guid)

   - `PolicyId` (Guid)

   - `RequesterPositionId` (Guid?, null = Mọi vị trí)

   - `SuperiorHierarchyLevelIds` (List<Guid>: Danh sách Level ID theo thứ tự ưu tiên leo cấp)

   - `SpecificApproverEmployeeId` (Guid?, Override duyệt đích danh)

   - `IsActive` (bool)



4. **`LeaveRequestApprovalAssignment` (Entity 1:1 với `LeaveRequest`)**: Quản lý trạng thái người duyệt hiện hành.

   - `Id` (Guid)

   - `LeaveRequestId` (Guid, Unique Index)

   - `AssignedApproverEmployeeId` (Guid?, Người duyệt hiện tại)

   - `AssignmentStatus` (Enum: `Assigned`, `Unassigned`, `NeedsAdminAttention`, `Reassigned`)

   - `AssignmentReason` (Enum: `DirectLevelMatch`, `SuperiorLevelEscalated`, `SpecificEmployeeOverride`, `AdminManualReassigned`)

   - `AssignedAt` (DateTime)



5. **`ApprovalRouteAuditLog` (Entity Audit)**: Lịch sử luân chuyển người duyệt.

   - `Id` (Guid)

   - `LeaveRequestId` (Guid)

   - `PreviousApproverEmployeeId` (Guid?)

   - `NewApproverEmployeeId` (Guid?)

   - `TriggerReason` (string)

   - `CreatedDate` (DateTime)



### 4.2. Database Schema ERD Text-Based



```

+---------------------+       1:N       +-------------------------+

| ApprovalRoutePolicy | --------------> |   ApprovalRouteLevel    |

+---------------------+                 +-------------------------+

| Id (PK)             |                 | Id (PK)                 |

| DepartmentId (FK)   |                 | PolicyId (FK)           |

| Name                |                 | LevelName               |

| IsActive            |                 | LevelRank               |

+---------------------+                 | AssignedEmployeeId (FK) |

          │                             | CanApproveLeave         |

          │ 1:N                         +-------------------------+

          ▼

+---------------------+

|  ApprovalRouteRule  |

+---------------------+

| Id (PK)             |

| PolicyId (FK)       |

| RequesterPosId (FK) |

| HierarchyLevelIds   |

| SpecificApprover(FK)|

+---------------------+



+---------------------+       1:1       +-----------------------------------+

|    LeaveRequest     | --------------> | LeaveRequestApprovalAssignment    |

+---------------------+                 +-----------------------------------+

| Id (PK)             |                 | Id (PK)                           |

| EmployeeId (FK)     |                 | LeaveRequestId (FK, Unique)       |

| Status (Pending...) |                 | AssignedApproverEmployeeId (FK)   |

+---------------------+                 | AssignmentStatus (Assigned...)    |

                                        | AssignmentReason                  |

                                        +-----------------------------------+

```



---



## 5. Thuật Toán Phân Giải Approver (Approval Resolve Algorithm)



Khi Nhân viên A nộp đơn nghỉ phép (`CreateLeaveRequestCommand`):



```

1. Tìm ApprovalRoutePolicy active cho Department của Nhân viên A (nếu không có, lấy Global Policy).

2. Tìm ApprovalRouteRule khớp với RequesterPositionId của Nhân viên A.

3. [CHECK SPECIFIC OVERRIDE]:

   - Nếu Rule có SpecificApproverEmployeeId:

     - Check: IsActive == true AND Linked User active AND Has APPROVE_LEAVE_REQUEST permission AND SpecificApprover != Requester.

     - IF PASS -> Assign Specific Approver. REASON = SpecificEmployeeOverride. RETURN SUCCESS.

     - IF FAIL -> Log Warning Audit ("Specific approver invalid, falling back to Level Rank").

4. [CHECK LEVEL HIERARCHY RANK]:

   - Loop qua danh sách SuperiorHierarchyLevelIds trong Rule (từ Rank nhỏ -> lớn):

     - Lấy AssignedEmployeeId của ApprovalRouteLevel đó.

     - Check: AssignedEmployeeId != null AND Employee IsActive AND Linked User active AND Has APPROVE_LEAVE_REQUEST permission AND Approver != Requester.

     - IF PASS -> Assign Level Approver. REASON = DirectLevelMatch (nếu là level đầu) hoặc SuperiorLevelEscalated (nếu là level sau). RETURN SUCCESS.

5. [NO VALID APPROVER FOUND]:

   - System CHẶN tạo đơn và ném Exception / Error Message:

     "Approval route is not configured for this department. Please assign an approver before submitting leave request."

```



---



## 6. Ảnh Hưởng Tới Dashboard W4/W5



1. **Scoped Approver View (W4/W5)**:

   - Query: `LeaveRequestApprovalAssignment` JOIN `LeaveRequest`

   - Filter: `AssignedApproverEmployeeId == currentEmployee.Id` AND `AssignmentStatus == Assigned` AND `LeaveRequest.Status == Pending`.

   - **Tối ưu**: Query siêu nhanh qua Index `AssignedApproverEmployeeId`.



2. **Admin / HR Global View**:

   - Hiển thị danh sách tất cả đơn Pending.

   - Thêm tab / bộ lọc riêng: **`Needs Admin Attention`** (chứa các đơn có `AssignmentStatus == NeedsAdminAttention` hoặc `Unassigned`).

   - Admin / HR có thể trực tiếp duyệt thay hoặc gán người duyệt mới ngay từ Dashboard.



---



## 7. Migration Strategy từ `LeaveApproverAssignment`



- **Giai đoạn 1**: Tạo module `ApprovalRoutePolicy` / `ApprovalRouteLevel` mới.

- **Giai đoạn 2 (Migration Script)**: Tự động chuyển đổi các record `LeaveApproverAssignment` cũ thành các `ApprovalRouteLevel` mặc định cho từng phòng ban.

- **Giai đoạn 3**: Đánh dấu Deprecate `LeaveApproverAssignment` cũ.



---



## 8. Tự Kiểm Tra & Phản Biện (Critical Self-Review)



1. **Clean Domain State**: Đã giữ nguyên `LeaveRequest.Status = Pending`, không làm phình domain aggregate cũ. Trạng thái luân chuyển được quản lý độc lập tại `LeaveRequestApprovalAssignment.AssignmentStatus`.

2. **Loại bỏ ràng buộc cứng trên Position**: Vấn đề nhiều nhân viên cùng giữ position (như Software Engineer) được giải quyết triệt để bằng entity `ApprovalRouteLevel`.

3. **Bảo vệ Lifecycle Nhân sự**: Thao tác Inactive/Xóa Approver được bảo vệ bằng Impact Analysis + UI Resolution Modal, ngăn ngừa tuyệt đối tình trạng đơn Pending bị mồ côi.



---



## 9. Lệnh Kiểm Tra UTF-8 BOM & Mojibake



- **File**: `MD_memory/plans/2026-07-22_1520_phase-dynamic-approval-routing-design_v3_proposal.md`

- **Yêu cầu**: File UTF-8 BOM, kiểm tra sạch 100% bằng `scan-mojibake.py --require-bom`.
