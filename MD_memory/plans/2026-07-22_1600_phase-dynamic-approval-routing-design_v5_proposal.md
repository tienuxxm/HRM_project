# Proposal & Architecture Design V5 — Single-Step Dynamic Superior Approval Routing Engine (Final Design)



> **File Location**: `MD_memory/plans/2026-07-22_1600_phase-dynamic-approval-routing-design_v5_proposal.md`

> **Phase**: `phase-dynamic-approval-routing-design`

> **Date**: 2026-07-22

> **Status**: 📋 FINAL DESIGN PROPOSAL V5 (Single-Step Dynamic Routing + Department-Only Policy + New UI Module + Strict Config-Driven Routing + Rich Audit + Impact Protection — KHÔNG CODE SOURCE / KHÔNG DB MIGRATION)

> **Architecture Boundary Preserved**:

> - `Web.Backend -> Application -> Domain`

> - `Infrastructure -> Application/Domain`



---



## 1. Tóm Tắt Các Quyết Định Nghiệp Vụ Chính Thức (Final Business Decisions)



1. **Single-Step Dynamic Superior Routing**:

   - Mỗi đơn `LeaveRequest` ở trạng thái `Pending` tại một thời điểm **CHỈ CÓ DUY NHẤT 1 ASSIGNED APPROVER HIỆN HÀNH**.

   - Đơn **KHÔNG** trải qua chuỗi duyệt nhiều bước tuần tự (không phải workflow Leader -> Manager -> Header).

2. **Chính Sách Duyệt Riêng Theo Phòng Ban (Department-Specific Policy Only)**:

   - Mỗi phòng ban (Department) **bắt buộc phải có `ApprovalRoutePolicy` riêng biệt**.

   - **KHÔNG sử dụng Global Default Policy làm runtime fallback**.

   - Nếu một phòng ban chưa có `ApprovalRoutePolicy` active, khi nhân viên phòng đó nộp đơn nghỉ phép, hệ thống **CHẶN TẠO ĐƠN** và trả thông báo lỗi:

     > `"Approval route is not configured for this department. Please assign an approver before submitting leave request."`

3. **Module UI Mới Hoàn Toàn — Approval Routing (`/approval-routing`)**:

   - Chốt thiết kế module UI hoàn toàn mới để quản lý quy trình duyệt.

   - Màn hình cũ `LeaveApproverAssignment` (`/leave-approver-assignment`) chính thức **DEPRECATED / LEGACY**, không sử dụng làm giao diện vận hành chính nữa.

4. **Nguyên Tắc Routing Thuần Cấu Hình (Strict Config-Driven Routing)**:

   - Hệ thống **chỉ leo cấp theo đúng thứ tự ưu tiên trong bảng candidate (`ApprovalRouteRuleCandidate.PriorityOrder`)**.

   - Hệ thống **TUYỆT ĐỐI KHÔNG TỰ SUY LUẬN** hay tự ý chèn các Position có sẵn ngoài thực tế nếu Position đó không nằm trong cấu hình Candidate.

5. **Xử Lý Specific Approver Không Hợp Lệ (Specific Approver Invalid Handling)**:

   - Nếu `SpecificApproverEmployeeId` bị inactive/disabled/unlinked User/không có quyền, hệ thống **KHÔNG tự động lặng lẽ fallback**.

   - Tại thời điểm submit hoặc khi Admin/HR thao tác, hệ thống quét các candidate hợp lệ trong rule/policy. Nếu có candidate hợp lệ -> Hiển thị Modal cho Admin/HR xác nhận/gán; Nếu không có -> Chặn nộp đơn/thao tác.

6. **Bảo Vệ Lifecycle Khi Inactive/Xóa Approver (Impact Analysis)**:

   - Trước khi Inactive/Delete/Unassign Approver B, Application layer thực hiện query kiểm tra tác động (Impact Analysis).

   - Nếu có đơn `Pending` hoặc Level Slot bị ảnh hưởng: Tạm dừng thao tác, trả về DTO tóm tắt tác động cho UI và bắt buộc Admin/HR chọn phương án xử lý trước khi cho phép hoàn tất.



---



## 2. Thiết Kế Domain Model V5 & Ràng Buộc Dữ Liệu (Constraints)



### 2.1. Các Domain Entities V5



1. **`ApprovalRoutePolicy` (Aggregate Root)**: Cấu hình quy trình duyệt phòng ban.

   - `Id` (Guid)

   - `DepartmentId` (Guid, Unique Index per Active Policy)

   - `Name` (string, e.g. "Chính sách duyệt phòng IT")

   - `IsActive` (bool)



2. **`ApprovalRouteLevel` (Entity thuộc Policy)**: Các cấp duyệt trong phòng ban.

   - `Id` (Guid)

   - `PolicyId` (Guid, FK)

   - `LevelName` (string, e.g. "Direct Leader", "Department Head")

   - `LevelRank` (int: 1, 2, 3...)

   - `CanApproveLeave` (bool, mặc định `true`)

   - `IsActive` (bool)



3. **`ApprovalRouteLevelAssignment` (Entity lưu vết người đảm nhận Cấp duyệt)**:

   - `Id` (Guid)

   - `ApprovalRouteLevelId` (Guid, FK)

   - `AssignedEmployeeId` (Guid, FK)

   - `EffectiveFrom` (DateOnly)

   - `EffectiveTo` (DateOnly?)

   - `IsActive` (bool)

   - `Reason` (string)

   - `CreatedByUserId` (Guid)

   - `CreatedAt` (DateTime)

   - **Ràng buộc (Constraint)**:

     - 1 `ApprovalRouteLevelId` tại một thời điểm **chỉ có tối đa 1 Assignment Active**.

     - Khung thời gian (`EffectiveFrom` -> `EffectiveTo`) của các assignment thuộc cùng 1 Level **không được phép chồng lấn nhau (No Date Range Overlap)**.



4. **`ApprovalRouteRule` (Entity thuộc Policy)**: Rule phân giải theo vị trí nhân viên nộp đơn.

   - `Id` (Guid)

   - `PolicyId` (Guid, FK)

   - `RequesterPositionId` (Guid?, null = Mọi vị trí trong phòng)

   - `SpecificApproverEmployeeId` (Guid?, Override duyệt đích danh)

   - `IsActive` (bool)



5. **`ApprovalRouteRuleCandidate` (Entity Bảng Con danh sách Cấp duyệt ưu tiên)**:

   - `Id` (Guid)

   - `ApprovalRouteRuleId` (Guid, FK)

   - `ApprovalRouteLevelId` (Guid, FK)

   - `PriorityOrder` (int: 1, 2, 3...)

   - `IsActive` (bool)



6. **`LeaveRequestApprovalAssignment` (Entity 1:1 với LeaveRequest - Snapshot Metadata)**:

   - `Id` (Guid)

   - `LeaveRequestId` (Guid, Unique Index)

   - `AssignedApproverEmployeeId` (Guid?, Người duyệt hiện tại)

   - `AssignmentStatus` (Enum: `Assigned`, `NeedsAdminAttention`, `Reassigned`)

   - `AssignmentReason` (Enum: `DirectLevelMatch`, `SuperiorLevelEscalated`, `SpecificEmployeeOverride`, `AdminManualReassigned`)

   - `SnapshotPolicyId` (Guid, Snapshot thông tin policy tại thời điểm gán)

   - `SnapshotRuleId` (Guid, Snapshot rule áp dụng)

   - `SnapshotCandidateId` (Guid?, Snapshot candidate áp dụng)

   - `SnapshotLevelAssignmentId` (Guid?, Snapshot assignment level)

   - `AssignedAt` (DateTime)



7. **`ApprovalRouteAuditLog` (Entity Audit Đầy Đủ)**:

   - `Id` (Guid)

   - `LeaveRequestId` (Guid, FK)

   - `PreviousApproverEmployeeId` (Guid?)

   - `NewApproverEmployeeId` (Guid?)

   - `ActionType` (string: `Created`, `Reassigned`, `Escalated`, `NeedsAttention`, `OverrideApplied`)

   - `OldAssignmentStatus` (string?)

   - `NewAssignmentStatus` (string)

   - `ReasonCode` (string)

   - `Note` (string?)

   - `CreatedByUserId` (Guid)

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

                                        | SnapshotPolicyId / RuleId...      |

                                        +-----------------------------------+

```



---



## 3. Case Test Nghiệp Vụ Bắt Buộc: IT Department Routing (Pure Config-Driven)



### Bối cảnh Thực tế của Phòng IT:

Phòng IT có 4 vị trí nhân sự thực tế:

- `Position = Header` (Trưởng phòng IT)

- `Position = Manager` (Quản lý IT)

- `Position = Leader` (Trưởng nhóm IT)

- `Position = Employee` (Lập trình viên IT)



### Cấu hình Approval Policy Phòng IT:

Rule dành cho `RequesterPosition = Employee` chỉ được cấu hình 2 Candidate:

- `Priority 1`: Candidate `ApprovalRouteLevel = Leader`

- `Priority 2`: Candidate `ApprovalRouteLevel = Header`

*(Chú ý: Position `Manager` hoàn toàn KHÔNG được đưa vào danh sách Candidate của Rule này).*



### Kịch bản Thử nghiệm khi Leader nghỉ việc / chưa có người thay:

1. Lập trình viên A (`Position = Employee`) nộp đơn nghỉ phép.

2. Engine kiểm tra Candidate `Priority 1` (`Leader`) -> Leader bị inactive / không có người giữ slot.

3. Engine kiểm tra Candidate `Priority 2` (`Header`). Header active & hợp lệ -> **Engine gán đơn trực tiếp cho Header**.

4. **KẾT QUẢ QUAN TRỌNG**: **Hệ thống TUYỆT ĐỐI KHÔNG tự ý route qua Position `Manager`**, mặc dù `Manager` đang tồn tại và Active trong phòng IT.

5. **Ý nghĩa**: Bằng chứng chứng minh Engine hoàn toàn hoạt động **Strict Config-Driven**, không tự suy luận vị trí ngoài cấu hình.



---



## 4. Thuật Toán Phân Giải Approver (Approval Resolve Algorithm)



Khi Nhân viên A nộp đơn nghỉ phép (`CreateLeaveRequestCommand`):



```

1. Query ApprovalRoutePolicy active của Department của Nhân viên A:

   - IF NULL -> CHẶN TẠO ĐƠN & Trả lỗi:

     "Approval route is not configured for this department. Please assign an approver before submitting leave request."

2. Tìm ApprovalRouteRule active trong Policy khớp với RequesterPositionId của Nhân viên A.

3. [SPECIFIC APPROVER OVERRIDE CHECK]:

   - Nếu Rule có SpecificApproverEmployeeId:

     - Check: IsActive == true AND Linked User active AND Has APPROVE_LEAVE_REQUEST permission AND SpecificApprover != Requester.

     - IF PASS -> Assign Specific Approver. REASON = SpecificEmployeeOverride. Snapshot Metadata. RETURN SUCCESS.

     - IF FAIL -> Log Warning Audit ("Specific approver invalid, proceeding to Rule Candidates").

4. [RULE CANDIDATES LEVEL CHECK]:

   - Query danh sách ApprovalRouteRuleCandidate thuộc Rule, ORDER BY PriorityOrder ASC:

     - Với mỗi Candidate Level:

       - Tìm Record Active trong ApprovalRouteLevelAssignment (EffectiveFrom <= Today <= EffectiveTo).

       - Check Employee hợp lệ (IsActive == true, Linked User active, Has APPROVE_LEAVE_REQUEST, Approver != Requester).

       - IF PASS -> Assign Approver. REASON = DirectLevelMatch (nếu Candidate 1) hoặc SuperiorLevelEscalated (nếu Candidate > 1). Snapshot Metadata. RETURN SUCCESS.

5. [NO VALID APPROVER RESOLVED]:

   - System CHẶN TẠO ĐƠN. Throw Business Error:

     "Approval route is not configured for this department. Please assign an approver before submitting leave request."

```



---



## 5. Module UI Mới — Approval Routing (`/approval-routing`)



Màn hình cũ `LeaveApproverAssignment` (`/leave-approver-assignment`) được đánh dấu **DEPRECATED**. Hệ thống xây dựng module UI mới gồm các màn hình chính:



1. **Màn hình Danh Sách Policy Theo Phòng Ban (`/approval-routing`)**:

   - Hiển thị danh sách các Department kèm trạng thái `Configured` / `Not Configured`.

   - Nút "Create Policy" / "Edit Policy" cho từng phòng ban.

2. **Màn hình Cấu Hình Cấp Duyệt & Candidate (`/approval-routing/detail/{departmentId}`)**:

   - Quản lý các `ApprovalRouteLevel` (Direct Leader, Department Head...).

   - Drag-and-drop / Sắp xếp thứ tự ưu tiên `PriorityOrder` cho các `ApprovalRouteRuleCandidate`.

   - Cấu hình `SpecificApproverEmployeeId` override nếu cần.

3. **Màn hình Gán Nhân Viên Cho Cấp Duyệt (`/approval-routing/levels`)**:

   - Gán nhân viên Active nắm giữ từng slot `ApprovalRouteLevel` theo khoảng thời gian (`EffectiveFrom` -> `EffectiveTo`).

   - Kiểm tra validation chống trùng lặp / chồng lấn khung thời gian (Date Range Overlap).

4. **Modal Tác Động Khi Inactive/Xóa Approver (Impact Analysis Modal)**:

   - Hiển thị danh sách đơn `Pending` bị ảnh hưởng và đề xuất Candidate thay thế để HR/Admin xác nhận chuyển giao trước khi cho phép Inactive/Xóa nhân viên.

5. **Màn hình Công Cụ Mapping Chuyển Đổi Dữ Liệu Cũ (`/approval-routing/migration-tool`)**:

   - Hỗ trợ HR/Admin xem dữ liệu `LeaveApproverAssignment` cũ và nhấn "Generate Draft Policy" sang cấu trúc mới.



---



## 6. Phân Quyền & Tác Động Tới Dashboard W4/W5



1. **Đề Xuất Permission Mới**:

   - `VIEW_APPROVAL_ROUTING`: Quyền xem danh sách chính sách duyệt.

   - `UPDATE_APPROVAL_ROUTING`: Quyền tạo/sửa policy và gán nhân viên vào Cấp duyệt.

   - *(Lưu ý: Permission sẽ được tạo qua UI / Permission Management, KHÔNG seed DB).*

2. **Nguyên Tắc Duyệt Đơn**:

   - Admin/HR **KHÔNG ĐƯỢC DUYỆT THAY** đơn của nhân viên khác nếu không phải là Assigned Approver hợp lệ.

3. **Dashboard W4/W5 Admin View**:

   - Phân chia 2 Queue độc lập:

     - **Queue 1 (Pending Approvals)**: Đơn đang có Assigned Approver (chỉ hiển thị theo dõi).

     - **Queue 2 (Needs Attention Queue)**: Đơn bị mất Approver giữa chừng (`AssignmentStatus == NeedsAdminAttention`) để HR/Admin gán lại người duyệt.



---



## 7. Migration & Deprecation Strategy Cho `LeaveApproverAssignment`



- **KHÔNG DÙNG Script Auto-Convert Ngây Thơ**: Dữ liệu `LeaveApproverAssignment` cũ là Filter tĩnh, không mang ngữ nghĩa hierarchy.

- **Quy Trình 3 Giai Đoạn**:

  1. *Giai đoạn 1*: Giữ `LeaveApproverAssignment` cũ ở trạng thái Read-only fallback.

  2. *Giai đoạn 2*: Cung cấp màn hình Migration Mapping Tool để HR/Admin preview và duyệt chuyển đổi từng phòng ban sang `ApprovalRoutePolicy` mới.

  3. *Giai đoạn 3*: Ẩn/Deprecate hoàn toàn menu `LeaveApproverAssignment` cũ sau khi UAT module mới đạt 100% PASS.



---



## 8. Tự Kiểm Tra & Phản Biện Cuối Cùng (Critical Self-Review)



- ✅ **Strict Config Routing**: Trường hợp IT Department (bỏ qua Manager) đã chứng minh engine chạy thuần policy-driven.

- ✅ **No Global Runtime Fallback**: Chặn tạo đơn nếu phòng ban chưa có policy riêng, nâng cao tính kỷ luật cấu hình dữ liệu.

- ✅ **Module UI Mới**: Màn hình `LeaveApproverAssignment` cũ được deprecate rõ ràng.

- ✅ **Rich Audit & Overlap Protection**: Entity `ApprovalRouteLevelAssignment` lưu vết lịch sử đầy đủ và chống chồng lấn thời gian.

- ✅ **Không Code Runtime / Không Migration / Không Seed DB**: Bảo toàn 100% ranh giới task proposal.



---



## 9. Lệnh Kiểm Tra UTF-8 BOM & Mojibake



- **File**: `MD_memory/plans/2026-07-22_1600_phase-dynamic-approval-routing-design_v5_proposal.md`

- **Lệnh**: `python MD_memory/debug/2026-06-26_1430_scan-mojibake.py MD_memory/plans/2026-07-22_1600_phase-dynamic-approval-routing-design_v5_proposal.md --require-bom`
