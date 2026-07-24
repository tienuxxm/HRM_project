# Phase 6 UAT Report: Leave Request List & Detail Integration (Baseline & Legacy Verification)



**Date**: 2026-07-23

**Author**: Technical Reviewer & Fullstack Engineer (Anti)

**Auth Mode**: Real Keycloak (`UseMockAuth: false`)

**Verified Accounts**:

- `admin` (`admin@hrm.local`): Global/config view

- `ceo.test`: Active linked approver (`APPROVE_LEAVE_REQUEST`)

- `uat.provision81`: Active linked IT approver (`APPROVE_LEAVE_REQUEST`)

- `uat.provision86`: Active linked IT employee/requester (`CREATE_LEAVE_REQUEST`, no approve permission)



---



## 1. Database State & Scope Bounds



- **`approval_route_policy` count**: `0`

- **`leave_request_approval_assignment` count**: `0`

- **Dynamic Approval Routing**: Not yet configured in DB.

- **Existing Leave Requests**: All are legacy requests created prior to Phase 5.

- **Legacy Approver Assignments**:

  1. `ceo.test` -> All Departments / Department Manager

  2. `uat.provision81` -> Information Technology / Employee



---



## 2. Test Execution Summary



| Test Case | Description | Result | Details & Evidence |

| :--- | :--- | :---: | :--- |

| **TC1** | **List Page Rendering** | **PASS** | Table column "Process Info / Approver" renders cleanly.<br/>`MD_memory/evidence/2026-07-23_phase6-leave-request-uat/tc1_existing_list.png` |

| **TC2** | **Detail Page Routing Panel (Legacy Fallback)** | **PASS** | Routing card renders `Legacy / Unassigned` fallback without crash.<br/>`MD_memory/evidence/2026-07-23_phase6-leave-request-uat/tc2_tc3_existing_detail.png` |

| **TC3** | **Permission & Decision Visibility** | **PASS** | Non-assigned users (`admin`) see details but NO in-place Approve/Reject buttons.<br/>`MD_memory/evidence/2026-07-23_phase6-leave-request-uat/tc2_tc3_existing_detail.png` |

| **TC4** | **Sidebar & Navigation Scope Lock** | **PASS** | Sidebar active highlight correct; zero redirect errors.<br/>`MD_memory/evidence/2026-07-23_phase6-leave-request-uat/tc4_sidebar_navigation.png` |

| **TC5** | **Mobile Responsive Layout (390x844)** | **PASS** | Mobile stacked card view renders cleanly without layout overflow.<br/>`MD_memory/evidence/2026-07-23_phase6-leave-request-uat/tc5_mobile_view.png` |

| **TC6** | **Dynamic Route Assignment UAT** | **DEFERRED** | Requires UI configuration of Approval Routing Policy. No direct DB seeds permitted. |



---



## 3. Findings & Security Verifications



1. **Legacy Fallback Robustness**:

   Because `approval_route_policy` and `leave_request_approval_assignment` are currently 0, the system gracefully falls back to legacy behavior. Detail card displays `Current Approver: Legacy / Unassigned` without any 500 server error or JavaScript exception.

2. **In-place Decision Guard**:

   Users accessing a leave request for which they are not the active assigned approver cannot see or execute in-place Approve or Reject actions.

3. **Console Health**:

   Console remains **100% Clean (0 Errors)**.



---



## 4. Next Step Directive



To test active Dynamic Approval Routing assignments (`Assigned Approver: <Name>` and red `NEEDS ROUTING ATTENTION` badge):

- The User or Admin must configure an active **Approval Routing Policy** via `/approval-routing/policies` in the UI.

- No direct DB SQL seeds or mock inserts will be performed, respecting DB read-only baseline rules.
