# Dynamic Approval Routing — Phase 5B Policy CRUD UI & Commands Implementation Report



**Date**: 2026-07-23

**Author**: Technical Reviewer & Fullstack Engineer (Anti)

**Architecture Boundary**:

- `Web.Backend -> Application -> Domain`

- `Infrastructure -> Application/Domain`



---



## 1. Overview & Objectives Completed



Phase 5B connects the full UI workflow for creating, configuring, and assigning Approval Routing Policies without manual DB seeds.



Admin users with `UPDATE_LEAVE_APPROVER_ASSIGNMENT` permission can now:

1. Navigate to `/approval-routing/policies/create` and create a Department Approval Policy.

2. Configure **Level Slots** (e.g. `Department Manager`, LevelRank = 1) from Policy Detail.

3. Configure **Position Rules** (e.g. Requester Position = `Employee`, Target Candidate Level = `Department Manager`).

4. Assign active eligible employees (e.g. `uat.provision81` / EMP04) to Level Slots with validation.



---



## 2. Files Created & Modified



### A. Domain Layer (`Domain/ApprovalRouting/`)

- `[NEW]` [ApprovalRouteErrors.cs](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Domain/ApprovalRouting/ApprovalRouteErrors.cs): Domain errors for policy creation, duplicates, level ranks, and approver validation.

- `[MODIFY]` [ApprovalRouteLevel.cs](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Domain/ApprovalRouting/ApprovalRouteLevel.cs): Added `AddAssignment(ApprovalRouteLevelAssignment assignment)` aggregate method.



### B. Application Layer (`Application/ApprovalRouting/Commands/`)

- `[NEW]` [CreateApprovalRoutePolicyCommand.cs](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Application/ApprovalRouting/Commands/CreateApprovalRoutePolicy/CreateApprovalRoutePolicyCommand.cs) & [CreateApprovalRoutePolicyCommandHandler.cs](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Application/ApprovalRouting/Commands/CreateApprovalRoutePolicy/CreateApprovalRoutePolicyCommandHandler.cs)

- `[NEW]` [AddApprovalRouteLevelCommand.cs](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Application/ApprovalRouting/Commands/AddApprovalRouteLevel/AddApprovalRouteLevelCommand.cs) & [AddApprovalRouteLevelCommandHandler.cs](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Application/ApprovalRouting/Commands/AddApprovalRouteLevel/AddApprovalRouteLevelCommandHandler.cs)

- `[NEW]` [AddApprovalRouteRuleCommand.cs](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Application/ApprovalRouting/Commands/AddApprovalRouteRule/AddApprovalRouteRuleCommand.cs) & [AddApprovalRouteRuleCommandHandler.cs](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Application/ApprovalRouting/Commands/AddApprovalRouteRule/AddApprovalRouteRuleCommandHandler.cs)

- `[NEW]` [AssignApprovalRouteLevelCommand.cs](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Application/ApprovalRouting/Commands/AssignApprovalRouteLevel/AssignApprovalRouteLevelCommand.cs) & [AssignApprovalRouteLevelCommandHandler.cs](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Application/ApprovalRouting/Commands/AssignApprovalRouteLevel/AssignApprovalRouteLevelCommandHandler.cs)



### C. Web Backend Layer (`Web.Backend/`)

- `[MODIFY]` [ApprovalRoutingController.cs](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Web.Backend/Controllers/ApprovalRoutingController.cs): Added `GET/POST /approval-routing/policies/create`, `POST /approval-routing/levels/add`, `POST /approval-routing/rules/add`, `POST /approval-routing/levels/assign`.

- `[MODIFY]` [PolicyDetailViewModel.cs](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Web.Backend/Models/ApprovalRouting/PolicyDetailViewModel.cs): Added `Positions` and `Employees` properties.

- `[NEW]` [CreatePolicy.cshtml](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Web.Backend/Views/ApprovalRouting/CreatePolicy.cshtml): Swiss HR UI form for creating a department approval routing policy.

- `[MODIFY]` [Policies.cshtml](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Web.Backend/Views/ApprovalRouting/Policies.cshtml): Replaced disabled read-only button with active link to `/approval-routing/policies/create`.

- `[MODIFY]` [PolicyDetail.cshtml](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Web.Backend/Views/ApprovalRouting/PolicyDetail.cshtml): Integrated inline modal forms for Add Level Slot, Add Position Rule, and Assign Level Slot.



---



## 3. Routes Added



| Route | Method | Description | Permission Required |

| :--- | :---: | :--- | :--- |

| `/approval-routing/policies/create` | `GET` | Renders policy creation form | `UPDATE_LEAVE_APPROVER_ASSIGNMENT` |

| `/approval-routing/policies/create` | `POST` | Processes policy creation command | `UPDATE_LEAVE_APPROVER_ASSIGNMENT` |

| `/approval-routing/levels/add` | `POST` | Adds a new level slot to policy | `UPDATE_LEAVE_APPROVER_ASSIGNMENT` |

| `/approval-routing/rules/add` | `POST` | Adds position rule & candidate level | `UPDATE_LEAVE_APPROVER_ASSIGNMENT` |

| `/approval-routing/levels/assign` | `POST` | Assigns an employee to level slot | `UPDATE_LEAVE_APPROVER_ASSIGNMENT` |



---



## 4. Validation Rules Implemented



1. **Policy Level**:

   - Department is required for department policy.

   - Prevents duplicate active policies for the same department.

   - Policy name must not be empty.



2. **Level Slot Level**:

   - Level name is required.

   - Level rank must be >= 1.

   - Prevents duplicate active rank within the same policy.



3. **Approver Employee Validation**:

   - Employee must be active (`IsActive == true`).

   - Must have a linked user account (`UserId != null`).

   - Linked user must not be deleted (`IsDeleted == false`).

   - Linked user must possess permission `APPROVE_LEAVE_REQUEST`.

   - Employee's department must match policy's department for department policies.



---



## 5. Residual Risks



- **Keycloak Permission Seed**: UAT accounts used for approver assignments (`uat.provision81`) must possess `APPROVE_LEAVE_REQUEST` permission in database/Keycloak role map. If missing, UI returns error `ApprovalRoute.ApproverNoApprovePermission`.

- **Existing Active Requests**: Creating a new policy affects future leave request creation; pending leave requests created prior to policy setup require manual re-assignment if unassignment trigger occurs.



---



## 6. Manual UAT Execution Steps



### Step 1: Create IT Policy & Assignment via UI

1. Login as `admin` (`admin@hrm.local` / `Admin@123456`).

2. Navigate to `http://localhost:5300/approval-routing/policies`.

3. Click `+ CREATE DEPARTMENT POLICY`.

4. Select Department: **Information Technology**.

5. Input Policy Name: `Information Technology Approval Policy`.

6. Click `+ CREATE POLICY`. Verify redirect to Policy Detail.

7. Click `+ Add Level Slot`:

   - Name: `Department Manager`

   - Rank: `1`

   - Check `Can Approve Leave Requests`.

8. Click `+ Add Position Rule`:

   - Requester Position: `Employee`

   - Candidate Level Slot: `Level 1: Department Manager`

   - Priority: `1`

9. Click `Assign` on Level Slot `Level 1: Department Manager`:

   - Select Approver Employee: `uat.provision81` (EMP04)

   - Reason: `UAT Phase 5B Initial Setup`

   - Submit assignment. Verify slot status changes to `ASSIGNED`.



### Step 2: Rerun Phase 6 TC6 Dynamic Assignment Verification

1. Logout `admin`. Login as `uat.provision86` (IT Employee).

2. Navigate to `/leave-request/create`.

3. Create a leave request with Start Date `2026-07-27` and End Date `2026-07-27`. Submit request.

4. Logout `uat.provision86`. Login as `uat.provision81` (IT Department Manager).

5. Navigate to `/leave-request`.

6. Verify the newly created leave request displays:

   - **Assigned Approver**: `uat.provision81 (EMP04)`.

   - In-place **Approve** and **Reject** decision buttons visible to `uat.provision81`.
