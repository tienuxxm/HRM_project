﻿# GitNexus Change Detection Results & Untracked Scope Manifest — Dynamic Approval Routing (Phase 2)



Date: 2026-07-22

Repository: `HRM_project`

Scope: Uncommitted Changes & Untracked Scope Verification



---



## 1. GitNexus `detect_changes` Output (Tracked Modifications Only)



> **Technical Limitation Note**:

> Lệnh `detect_changes` của GitNexus chỉ phân tích các chỉnh sửa trên những file đã được Git track. Do đó, các file mới được tạo hoàn toàn (untracked files) sẽ chưa được `detect_changes` bao phủ. Do đó, bằng chứng phạm vi Phase 2 bắt buộc phải kết hợp giữa `detect_changes` và danh sách `git status --short` bổ sung ở Mục 2 bên dưới.



```json

{

  "summary": {

    "changed_count": 22,

    "affected_count": 0,

    "changed_files": 16,

    "risk_level": "low"

  },

  "changed_symbols": [

    {

      "id": "Class:HRM_Leave_Management/Infrastructure/Configurations/DepartmentConfiguration.cs:DepartmentConfiguration",

      "name": "DepartmentConfiguration",

      "filePath": "HRM_Leave_Management/Infrastructure/Configurations/DepartmentConfiguration.cs",

      "change_type": "touched"

    },

    {

      "id": "Method:HRM_Leave_Management/Infrastructure/Configurations/DepartmentConfiguration.cs:DepartmentConfiguration.Configure#1",

      "name": "Configure",

      "filePath": "HRM_Leave_Management/Infrastructure/Configurations/DepartmentConfiguration.cs",

      "change_type": "touched"

    },

    {

      "id": "Class:HRM_Leave_Management/Infrastructure/Configurations/EmployeeConfiguration.cs:EmployeeConfiguration",

      "name": "EmployeeConfiguration",

      "filePath": "HRM_Leave_Management/Infrastructure/Configurations/EmployeeConfiguration.cs",

      "change_type": "touched"

    },

    {

      "id": "Method:HRM_Leave_Management/Infrastructure/Configurations/EmployeeConfiguration.cs:EmployeeConfiguration.Configure#1",

      "name": "Configure",

      "filePath": "HRM_Leave_Management/Infrastructure/Configurations/EmployeeConfiguration.cs",

      "change_type": "touched"

    },

    {

      "id": "Class:HRM_Leave_Management/Infrastructure/Configurations/LeaveApproverAssignmentConfiguration.cs:LeaveApproverAssignmentConfiguration",

      "name": "LeaveApproverAssignmentConfiguration",

      "filePath": "HRM_Leave_Management/Infrastructure/Configurations/LeaveApproverAssignmentConfiguration.cs",

      "change_type": "touched"

    },

    {

      "id": "Method:HRM_Leave_Management/Infrastructure/Configurations/LeaveApproverAssignmentConfiguration.cs:LeaveApproverAssignmentConfiguration.Configure#1",

      "name": "Configure",

      "filePath": "HRM_Leave_Management/Infrastructure/Configurations/LeaveApproverAssignmentConfiguration.cs",

      "change_type": "touched"

    },

    {

      "id": "Class:HRM_Leave_Management/Infrastructure/Configurations/LeaveRequestConfiguration.cs:LeaveRequestConfiguration",

      "name": "LeaveRequestConfiguration",

      "filePath": "HRM_Leave_Management/Infrastructure/Configurations/LeaveRequestConfiguration.cs",

      "change_type": "touched"

    },

    {

      "id": "Method:HRM_Leave_Management/Infrastructure/Configurations/LeaveRequestConfiguration.cs:LeaveRequestConfiguration.Configure#1",

      "name": "Configure",

      "filePath": "HRM_Leave_Management/Infrastructure/Configurations/LeaveRequestConfiguration.cs",

      "change_type": "touched"

    },

    {

      "id": "Class:HRM_Leave_Management/Infrastructure/DependencyInjection.cs:DependencyInjection",

      "name": "DependencyInjection",

      "filePath": "HRM_Leave_Management/Infrastructure/DependencyInjection.cs",

      "change_type": "touched"

    },

    {

      "id": "Method:HRM_Leave_Management/Infrastructure/DependencyInjection.cs:DependencyInjection.AddInfrastructure#1",

      "name": "AddInfrastructure",

      "filePath": "HRM_Leave_Management/Infrastructure/DependencyInjection.cs",

      "change_type": "touched"

    }

  ]

}

```



---



## 2. Phase 2 Untracked New Files Scope (`git status --short`)



Xác minh qua `git status --short`, danh sách các untracked new files thuộc phạm vi Phase 2 bao gồm:



1. **Domain Layer (`HRM_Leave_Management/Domain/ApprovalRouting/`)**:

   - `ApprovalRoutePolicy.cs`, `ApprovalRoutePolicyId.cs`

   - `ApprovalRouteLevel.cs`, `ApprovalRouteLevelId.cs`

   - `ApprovalRouteLevelAssignment.cs`, `ApprovalRouteLevelAssignmentId.cs`

   - `ApprovalRouteRule.cs`, `ApprovalRouteRuleId.cs`

   - `ApprovalRouteRuleCandidate.cs`, `ApprovalRouteRuleCandidateId.cs`

   - `LeaveRequestApprovalAssignment.cs`, `LeaveRequestApprovalAssignmentId.cs`

   - `ApprovalRouteAuditLog.cs`, `ApprovalRouteAuditLogId.cs`

   - `IApprovalRoutePolicyRepository.cs`, `ILeaveRequestApprovalAssignmentRepository.cs`



2. **Infrastructure Configurations (`HRM_Leave_Management/Infrastructure/Configurations/`)**:

   - `ApprovalRoutePolicyConfiguration.cs`

   - `ApprovalRouteLevelConfiguration.cs`

   - `ApprovalRouteRuleConfiguration.cs`

   - `ApprovalRouteRuleCandidateConfiguration.cs`

   - `ApprovalRouteLevelAssignmentConfiguration.cs`

   - `LeaveRequestApprovalAssignmentConfiguration.cs`

   - `ApprovalRouteAuditLogConfiguration.cs`



3. **Infrastructure Repositories (`HRM_Leave_Management/Infrastructure/Repositories/`)**:

   - `ApprovalRoutePolicyRepository.cs`

   - `LeaveRequestApprovalAssignmentRepository.cs`



4. **EF Core Migrations (`HRM_Leave_Management/Infrastructure/Migrations/`)**:

   - `20260722100601_AddApprovalRouting.cs`

   - `20260722100601_AddApprovalRouting.Designer.cs`



---



## Conclusion

`detect_changes` xác nhận 22 tracked symbols có **risk_level = low**, và danh sách `git status --short` xác nhận toàn bộ 25 untracked files mới tạo đều thuộc chuẩn phạm vi của Phase 2 (`Domain/ApprovalRouting`, `Infrastructure/Configurations`, `Repositories` & `Migrations`).
