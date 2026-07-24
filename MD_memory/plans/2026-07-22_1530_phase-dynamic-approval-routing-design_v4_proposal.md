# Proposal & Architecture Design V4 — Final Single-Step Dynamic Superior Approval Routing



> **File Location**: `MD_memory/plans/2026-07-22_1530_phase-dynamic-approval-routing-design_v4_proposal.md`

> **Phase**: `phase-dynamic-approval-routing-design`

> **Date**: 2026-07-22

> **Status**: 📋 FINAL DESIGN PROPOSAL V4 (Single-Step Dynamic Routing + Level Assignment Audit + Child Table Candidates + Lifecycle Protection — KHÔNG CODE SOURCE / KHÔNG DB MIGRATION)

> **Architecture Boundary Preserved**:

> - `Web.Backend -> Application -> Domain`

> - `Infrastructure -> Application/Domain`



---



## 1. Tóm Tắt Tất Cả Quy Tắc Nghiệp Vụ Đã Chốt (Final Business Rules)



1. **Single-Step Dynamic Superior Routing**: Mỗi đơn `LeaveRequest` ở trạng thái `Pending` tại một thời điểm **CHỈ CÓ DUY NHẤT 1 ASSIGNED APPROVER HIỆN HÀNH**. Đơn KHÔNG trải qua quy trình duyệt qua nhiều cấp tuần tự.

2. **Phân Tách Chức Danh (`Position`) và Cấp Duyệt (`ApprovalRouteLevel`)**:

   - Không áp dụng quy tắc "Unique Active Employee" cho mọi Position HR (như *Software Engineer*, *Accountant*).

   - Tách riêng `ApprovalRouteLevel` và `ApprovalRouteLevelAssignment`. Quy tắc unique active holder chỉ áp dụng cho vai trò đảm nhận slot duyệt phép trong phòng ban.

3. **Lưu Vết Lịch Sử Đảm Nhận Cấp Duyệt (`ApprovalRouteLevelAssignment`)**:

   - Lưu vết theo khung thời gian (`EffectiveFrom`, `EffectiveTo`, `CreatedByUserId`, `Reason`) để Audit chính xác ai từng nắm giữ slot duyệt nào trong quá khứ.

4. **Cấu Trúc Candidate Bảng Con (`ApprovalRouteRuleCandidate`)**:

   - Dùng child table thay vì lưu danh sách Guid/JSON mơ hồ, đảm bảo toàn vẹn khóa ngoại (FK Integrity), hỗ trợ index và sắp xếp theo `PriorityOrder`.

5. **Specific Approver Precedence & Fallback Rules**:

   - Ưu tiên 1: Người duyệt chỉ định đích danh (`SpecificApproverEmployeeId`).

   - Nếu Specific Approver inactive/mất quyền: KHÔNG tự ý duyệt thay. Bắt buộc Admin/HR chọn: Gán người thay thế, hoặc cho phép hệ thống auto-route lên `ApprovalRouteLevel` cao hơn trong policy (nếu có).

6. **Bảo Vệ Lifecycle Khi Approver Inactive/Delete**:

   - Application layer kiểm tra tác động (Impact Analysis) trước khi Inactive/Delete/Unassign Approver.

   - Bắt buộc Admin/HR xử lý toàn bộ đơn `Pending` bị ảnh hưởng trước khi hoàn tất thao tác.

7. **Chặn Tạo Đơn Khi Thiếu Approver (Validation)**:

   - Nếu không phân giải được Approver hợp lệ từ Policy: **CHẶN TẠO ĐƠN** và trả lỗi rõ:

     `"Approval route is not configured for this department. Please assign an approver before submitting leave request."`

8. **Bảo Toàn Trạng Thái Domain (Clean Domain State)**:

   - Giữ nguyên `LeaveRequest.Status = LeaveRequestStatus.Pending`. Quản lý trạng thái luân chuyển người duyệt tại `LeaveRequestApprovalAssignment.AssignmentStatus` (`Assigned`, `NeedsAdminAttention`, `Reassigned`).

9. **Phân Quyền & Dashboard W4/W5**:

   - Admin/HR **KHÔNG ĐƯỢC DUYỆT THAY** người duyệt ngoại trừ việc cấu hình lại routing.

   - W4/W5 Admin View tách rõ 2 queue: Đơn pending bình thường và Đơn pending cần Admin/HR chú ý (`NeedsAdminAttention`).



---



## 2. Thiết Kế Domain Model V4 & Database Schema Text-Based



### 2.1. Chi Tiết Các Domain Entities V4



1. **`ApprovalRoutePolicy` (Aggregate Root)**: Cấu hình policy duyệt phòng ban.

   - `Id` (Guid)

   - `DepartmentId` (Guid?, null = Global Default Policy)

   - `Name` (string)

   - `IsActive` (bool)



2. **`ApprovalRouteLevel` (Entity thuộc Policy)**: Cấp duyệt trong phòng ban.

   - `Id` (Guid)

   - `PolicyId` (Guid)

   - `LevelName` (string, e.g. "Direct Leader", "Department Head", "Director")

   - `LevelRank` (int: 1, 2, 3...)

   - `CanApproveLeave` (bool, default `true`)

   - `IsActive` (bool)



3. **`ApprovalRouteLevelAssignment` (Entity lưu vết người giữ Level)**:

   - `Id` (Guid)

   - `ApprovalRouteLevelId` (Guid, FK)

   - `AssignedEmployeeId` (Guid, FK)

   - `EffectiveFrom` (DateOnly)

   - `EffectiveTo` (DateOnly?)

   - `IsActive` (bool)

   - `Reason` (string)

   - `CreatedByUserId` (Guid)

   - `CreatedAt` (DateTime)



4. **`ApprovalRouteRule` (Entity thuộc Policy)**: Rule phân giải theo loại vị trí nhân viên nộp đơn.

   - `Id` (Guid)

   - `PolicyId` (Guid)

   - `RequesterPositionId` (Guid?, null = Mọi vị trí trong phòng)

   - `SpecificApproverEmployeeId` (Guid?, Override duyệt đích danh)

   - `IsActive` (bool)



5. **`ApprovalRouteRuleCandidate` (Entity Bảng Con đại diện danh sách Cấp duyệt ưu tiên)**:

   - `Id` (Guid)

   - `ApprovalRouteRuleId` (Guid, FK)

   - `ApprovalRouteLevelId` (Guid, FK)

   - `PriorityOrder` (int: 1, 2, 3...)

   - `IsActive` (bool)



6. **`LeaveRequestApprovalAssignment` (Entity 1:1 với LeaveRequest)**:

   - `Id` (Guid)

   - `LeaveRequestId` (Guid, Unique Index)

   - `AssignedApproverEmployeeId` (Guid?, Người duyệt hiện tại)

   - `AssignmentStatus` (Enum: `Assigned`, `NeedsAdminAttention`, `Reassigned`)

   - `AssignmentReason` (Enum: `DirectLevelMatch`, `SuperiorLevelEscalated`, `SpecificEmployeeOverride`, `AdminManualReassigned`)

   - `AssignedAt` (DateTime)



7. **`ApprovalRouteAuditLog` (Entity Audit)**:

   - `Id` (Guid)

   - `LeaveRequestId` (Guid)

   - `PreviousApproverEmployeeId` (Guid?)

   - `NewApproverEmployeeId` (Guid?)

   - `TriggerReason` (string)

   - `CreatedDate` (DateTime)



### 2.2. Text-Based ERD Proposal



```

+---------------------+       1:N       +-------------------------+       1:N       +-------------------------------+

| ApprovalRoutePolicy | --------------> |   ApprovalRouteLevel    | --------------> | ApprovalRouteLevelAssignment  |

+---------------------+                 +-------------------------+                 +-------------------------------+

| Id (PK)             |                 | Id (PK)                 |                 | Id (PK)                       |

| DepartmentId (FK)   |                 | PolicyId (FK)           |                 | ApprovalRouteLevelId (FK)     |

| Name                |                 | LevelName               |                 | AssignedEmployeeId (FK)       |

| IsActive            |                 | LevelRank               |                 | EffectiveFrom / EffectiveTo   |

+---------------------+                 | CanApproveLeave         |                 +-------------------------------+

          │                             +-------------------------+

          │ 1:N

          ▼

+---------------------+       1:N       +----------------------------------+

|  ApprovalRouteRule  | --------------> |   ApprovalRouteRuleCandidate     |

+---------------------+                 +----------------------------------+

| Id (PK)             |                 | Id (PK)                          |

| PolicyId (FK)       |                 | ApprovalRouteRuleId (FK)         |

| RequesterPosId (FK) |                 | ApprovalRouteLevelId (FK)        |

| SpecificApprover(FK)|                 | PriorityOrder                    |

+---------------------+                 +----------------------------------+



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



## 3. Thuật Toán Phân Giải Approver (Approval Resolve Algorithm)



Khi Nhân viên A nộp đơn nghỉ phép (`CreateLeaveRequestCommand`):



```

1. Tìm ApprovalRoutePolicy active của Department của Nhân viên A (Fallback: Global Policy).

2. Tìm ApprovalRouteRule khớp với RequesterPositionId của Nhân viên A.

3. [SPECIFIC APPROVER OVERRIDE CHECK]:

   - Nếu Rule có SpecificApproverEmployeeId:

     - Check: IsActive == true AND Linked User active AND Has APPROVE_LEAVE_REQUEST permission AND SpecificApprover != Requester.

     - IF PASS -> Assign Specific Approver. REASON = SpecificEmployeeOverride. RETURN SUCCESS.

     - IF FAIL -> Log Warning Audit ("Specific approver invalid, proceeding to Rule Candidates").

4. [RULE CANDIDATES LEVEL CHECK]:

   - Query danh sách ApprovalRouteRuleCandidate thuộc Rule, ORDER BY PriorityOrder ASC:

     - Với mỗi Candidate Level:

       - Tìm Active Record trong ApprovalRouteLevelAssignment (EffectiveFrom <= Today <= EffectiveTo).

       - Check Employee hợp lệ (IsActive == true, Linked User active, Has APPROVE_LEAVE_REQUEST, Approver != Requester).

       - IF PASS -> Assign Approver. REASON = DirectLevelMatch (nếu Candidate 1) hoặc SuperiorLevelEscalated (nếu Candidate > 1). RETURN SUCCESS.

5. [NO VALID APPROVER RESOLVED]:

   - System CHẶN TẠO ĐƠN. Throw Domain Exception / Business Error:

     "Approval route is not configured for this department. Please assign an approver before submitting leave request."

```



---



## 4. Quy Trình Bảo Vệ Lifecycle (Employee Inactive/Delete/Unassign Workflow)



```

[Admin/HR thực hiện thao tác Inactive / Delete / Unassign Approver B]

                             │

                             ▼

[Application Layer: Running Impact Analysis Query (Dry-run)]

  - Đếm số đơn LeaveRequest (Pending) có AssignedApprover == Approver B

  - Kiểm tra các ApprovalRouteLevelAssignment active của Approver B

                             │

            ┌────────────────┴────────────────┐

            │ Impact Count == 0               │ Impact Count > 0

            ▼                                 ▼

   [Cho phép thao tác             [HALT THAO TÁC + Trả về Impact Summary DTO]

    Inactive bình thường]          - Đơn Pending bị ảnh hưởng: N đơn

                                   - Route Level bị trống: M level

                                   - Danh sách Candidate thay thế đề xuất

                                          │

                                          ▼

                                   [UI Modal: Bắt buộc Admin/HR chọn 1 trong 3 Phương án]

                                   ├── A. Gán Nhân viên thay thế vào Level / Specific Override.

                                   ├── B. Continue auto-routing sang Level cao hơn (nếu policy có).

                                   └── C. Hủy thao tác (Cancel).

                                          │

                                          ▼

                                   [Thực thi Reassignment + Hoàn tất Inactive Approver B]

```



---



## 5. Phân Quyền & Tác Động Tới Dashboard W4/W5



1. **Nguyên Tắc Phân Quyền (Permission Rules)**:

   - Admin/HR **KHÔNG ĐƯỢC DUYỆT THAY** đơn nghỉ phép của nhân viên nếu không phải là assigned approver.

   - Thao tác gán lại người duyệt yêu cầu permission: `UPDATE_APPROVAL_POLICY` hoặc `UPDATE_EMPLOYEE`.



2. **Scoped Approver View (W4/W5)**:

   - Query: `LeaveRequestApprovalAssignment` JOIN `LeaveRequest`

   - Filter: `AssignedApproverEmployeeId == currentEmployee.Id` AND `AssignmentStatus == Assigned` AND `LeaveRequest.Status == Pending`.



3. **Admin / HR Global Dashboard View**:

   - Chia làm 2 Queue rõ ràng:

     - **Operational Queue**: Tất cả đơn Pending có Assigned Approver (chỉ xem thông tin).

     - **Needs Attention Queue**: Danh sách đơn `AssignmentStatus == NeedsAdminAttention` (đơn mất approver giữa chừng) kèm nút "Reassign Approver".



---



## 6. Chiến Lược Migration An Toàn Từ `LeaveApproverAssignment`



- **KHÔNG dùng script tự động convert ngây thơ** (vì `LeaveApproverAssignment` cũ là Scope Filter tĩnh, không có khái niệm cấp bậc).

- **Chiến lược 3 Giai đoạn**:

  1. *Giai đoạn 1*: Giữ `LeaveApproverAssignment` cũ dạng read-only fallback.

  2. *Giai đoạn 2*: Cung cấp UI Mapping Tool & Dry-Run Report Tool để HR/Admin cấu hình `ApprovalRoutePolicy` cho từng phòng ban và preview kết quả.

  3. *Giai đoạn 3*: Deprecate `LeaveApproverAssignment` sau khi UAT phase mới pass 100%.



---



## 7. Tự Kiểm Tra & Phản Biện Cuối Cùng (Critical Self-Review)



- ✅ **FK Integrity**: Đã dùng child table `ApprovalRouteRuleCandidate` thay cho List Guid/JSON.

- ✅ **Audit Trail**: Đã lưu vết lịch sử giữ level qua `ApprovalRouteLevelAssignment`.

- ✅ **Domain Cleanliness**: Giữ nguyên `LeaveRequest.Status = Pending`.

- ✅ **No Hardcoded Roles**: Dùng permission capability, không hardcode role name hay magic GUID.

- ✅ **No Orphan Requests**: Bảo vệ lifecycle 100% bằng Impact Analysis + Resolution Modal.



---



## 8. Lệnh Kiểm Tra UTF-8 BOM & Mojibake



- **File**: `MD_memory/plans/2026-07-22_1530_phase-dynamic-approval-routing-design_v4_proposal.md`

- **Yêu cầu**: File UTF-8 BOM, kiểm tra sạch 100% bằng `scan-mojibake.py --require-bom`.
