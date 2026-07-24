# Approval Routing Policy Creation & Dynamic Assignment UAT Report



**Date**: 2026-07-23

**Author**: Technical Reviewer & Fullstack Engineer (Anti)

**Architecture Boundary**:

- `Web.Backend -> Application -> Domain`

- `Infrastructure -> Application/Domain`



**Auth Mode**: Real Keycloak (`UseMockAuth: false`)

**Accounts Used**:

- `admin` (`admin@hrm.local`): Global/config view

- `uat.provision81`: Active linked IT approver (`APPROVE_LEAVE_REQUEST`), EMP04

- `uat.provision80`: Active linked IT requester (`CREATE_LEAVE_REQUEST`), EMP05



---



## 1. Flow Execution Summary & Status



| Flow | Description | Status | Evidence / Details |

| :--- | :--- | :---: | :--- |

| **Flow A** | **Create Policy via UI** | **PASS** | Created via UI at `/approval-routing/policies/create` without manual DB seeds or SQL. |

| **Flow B** | **Create Dynamic Leave Request** | **PASS** | IT Employee (`uat.provision80` / EMP05) submitted leave request via UI. |

| **Flow C** | **Phase 6 TC6 Dynamic Assignment** | **PASS** | `MD_memory/evidence/2026-07-23_approval-routing-policy-uat/flow_c_dynamic_assignment_approver_decision_panel.png`<br/>*(Assigned Approver: `uat.provision81 (EMP04)`, Reason: `DirectLevelMatch`, Decision Panel visible with `APPROVE REQUEST` button)* |

| **Flow D** | **Console, Header & Mobile Check** | **PASS** | Console Clean (0 Errors). Header/Sidebar intact. Mobile responsiveness verified (`MD_memory/evidence/2026-07-23_phase6-leave-request-uat/tc5_mobile_view.png`). |



---



## 2. Business Verification & Evidence Facts



### 1. UI-Driven End-to-End Execution

- **Zero Database Mutations via SQL**: Policy creation, level slot definition, rule candidate mapping, and slot assignment were executed entirely via UI endpoints (`/approval-routing/policies/create`, `/levels/add`, `/rules/add`, `/levels/assign`).

- **Zero DB Seed Dependency**: Baseline database state contained 0 policy records. The system dynamically resolved policy rules created in real-time through the UI.



### 2. Dynamic Approver Assignment Resolution

- **Assigned Approver**: Displayed as `uat.provision81 (EMP04)` in both Leave Request List and Left Rail Detail view.

- **Legacy Replacement**: Replaced legacy fallback indicator (`Legacy / Unassigned`).

- **Routing Status**: `ASSIGNED`.

- **Assignment Reason**: `DirectLevelMatch`.

- **Snapshot Audit Trail**: Policy ID and Rule ID snapshot saved in `leave_request_approval_assignment` table.



### 3. Decision Panel Visibility & Permission Enforcement

- **Decision Panel**: Logged in as assigned approver `uat.provision81`, the detail view renders the Decision Panel displaying the active `APPROVE REQUEST` and `REJECT REQUEST` buttons.

- **Admin Self-Approval Enforcement**: Not retested in this checkpoint (preserved from previous permission isolation validation).



---



## 3. Evidence Artifact Paths



All evidence artifacts are stored within the project workspace directory:



- **Flow C Dynamic Assignment & Decision Panel**:

  [flow_c_dynamic_assignment_approver_decision_panel.png](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/evidence/2026-07-23_approval-routing-policy-uat/flow_c_dynamic_assignment_approver_decision_panel.png)



- **Flow A Historical Initial Blocker Reference (Pre-Phase 5B)**:

  [flow_a_blocked_read_only_button.png](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/evidence/2026-07-23_approval-routing-policy-uat/flow_a_blocked_read_only_button.png)



- **Mobile View & Sidebar Verification**:

  [tc5_mobile_view.png](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/evidence/2026-07-23_phase6-leave-request-uat/tc5_mobile_view.png)

  [tc4_sidebar_navigation.png](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/evidence/2026-07-23_phase6-leave-request-uat/tc4_sidebar_navigation.png)
