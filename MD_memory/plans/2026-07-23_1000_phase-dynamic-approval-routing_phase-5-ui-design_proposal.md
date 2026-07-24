# Phase 5 UI Design Proposal: Dynamic Approval Routing Module (`/approval-routing`)



**Date**: 2026-07-23

**Author**: Technical Reviewer & Senior .NET Fullstack Engineer (Anti)

**Status**: PROPOSAL — WAITING FOR USER / CODEX APPROVAL

**Target Module**: `HRM_Leave_Management/WebUI/Controllers/ApprovalRoutingController.cs` & `/Views/ApprovalRouting/`



---



## 1. Architecture Boundary & Refactor Guard



The following Clean Architecture layer dependencies are strictly preserved:



```text

Web.Backend -> Application -> Domain

Infrastructure -> Application/Domain

```



- **Domain**: Entity definitions (`ApprovalRoutePolicy`, `ApprovalRouteLevel`, `ApprovalRouteLevelAssignment`, `ApprovalRouteRule`, `ApprovalRouteRuleCandidate`, `LeaveRequestApprovalAssignment`). No UI or infrastructure dependencies.

- **Application**: CQRS Commands/Queries (`ReassignPendingLeaveRequestsCommand`, `UnassignApprovalLevelCommand`, `GetEmployeeDeactivationImpactQuery`, `ApprovalRouteResolverService`).

- **Infrastructure**: Persistence, EF Core query projections, audit log mapping.

- **Web.Backend**: ASP.NET Core MVC Controllers, Razor Views, ViewModels, AJAX endpoints, and Swiss International HR UI layout.



---



## 2. Stitch Canvas Design Deliverables



All Phase 5 UI screens have been designed and rendered on **Stitch Canvas** under Project ID `17479353588209716186` utilizing the **Swiss International HR** design system (`assets/f4fbeeb3791c4c52991dd52c4fb92635`).



| Screen # | Module Screen Name | Viewport | Stitch Screen ID | Description & State Coverage |

| :--- | :--- | :--- | :--- | :--- |

| **1a** | Policy List Screen | Desktop (1440px) | `ffb3498505444a46ad87b1c37125e939` | High-density policy ledger table, active/inactive badges, filter bar, creation CTA. |

| **1b** | Policy List Screen | Mobile (390px) | `23a89f4ff9514549b6d7664b42e813a3` | Stacked policy cards with 1px hairline borders, mobile search bar, touch actions. |

| **2a** | Policy Detail Screen | Desktop (1440px) | `47ffe8565a154cd19fbd03f0b5eec574` | 2-column header (Overview + Level Slots), status chips, full rule breakdown. |

| **2b** | Policy Detail Screen | Mobile (390px) | `ac64ecf42c9947c484c4a8cdb662c428` | Stacked mobile overview card, full-width outline actions, fixed simulation footer. |

| **3a** | Rule / Candidate Config | Desktop (1440px) | `47ffe8565a154cd19fbd03f0b5eec574` | Position rule list showing priority candidate sequences & specific approver overrides. |

| **3b** | Rule / Candidate Config | Mobile (390px) | `ac64ecf42c9947c484c4a8cdb662c428` | Mobile rule cards distinguishing sequence vs override mode with delete warnings. |

| **4a** | Level Slot Assignments | Desktop (1440px) | `07e8ed1983434404825cfcc4076bef44` | High-density spreadsheet grid for level slot approvers, effective dates, vacant slots. |

| **4b** | Level Slot Assignments | Mobile (390px) | `e2792249f5204a4fab1aab4b4d1df5d5` | Stacked level slot cards, vacant slot badges (Red), warning callout border. |

| **5a** | Impact Preview Overlay | Desktop (1440px) | `22c0b7c5599a40c6bcdfa43c69857c00` | 80% black backdrop modal, 3-metric summary, affected requests table, red warning badge. |

| **5b** | Impact Preview Overlay | Mobile (390px) | `79edd9cacfd04445aaa04638c5a29d20` | Full-screen mobile modal, stacked request impact cards, NeedsAttention dropdown. |

| **6a** | Reassignment Decision | Desktop (1440px) | `22c0b7c5599a40c6bcdfa43c69857c00` | Radio strategy box (Auto Re-route vs Manual Reassign), single-click atomic commit. |

| **6b** | Reassignment Decision | Mobile (390px) | `79edd9cacfd04445aaa04638c5a29d20` | Mobile radio strategy selector, sticky footer with execute CTA. |

| **7a** | Legacy Migration Console | Desktop (1440px) | `3fb828b034af45e8bf660339482cf2f7` | Legacy `LeaveApproverAssignment` read-only audit log, 100% policy mapping verification. |

| **7b** | Legacy Migration Console | Mobile (390px) | `a737a59f68824511985e572f5f9d0d39` | Mobile legacy record audit cards, read-only system lock notice. |



---



## 3. UI System & Visual Aesthetics Specification



Strict adherence to **Swiss International HR Design System** (`.stitch/DESIGN.md`):



1. **Geometry & Shape Language**:

   - **0px border radius** on ALL components (buttons, input fields, cards, tables, badges, modals).

   - No rounded pills, no soft cards, no shadows, no floating cards.

   - All spatial boundaries defined by thin **1px solid hairline borders** (`#D1D1D1` or `#000000`).



2. **Color Palette**:

   - **Canvas Background**: `#FAF9F9` / `#F4F3F3`

   - **Pure Surface**: `#FFFFFF`

   - **Primary Ink / Accent**: Pure Black `#000000` / Charcoal `#111111`

   - **Secondary Text & Metadata**: Muted Gray `#4C4546` / `#777587`

   - **Borders & Dividers**: Hairline Gray `#D1D1D1` / `#E2E2E2`

   - **Swiss Red Accent (`#E62429`)**: Used **SURGICALLY AND EXCLUSIVELY** for:

     - Inactive policy badges (`INACTIVE`)

     - Unassigned / Vacant level slot warnings (`UNASSIGNED`)

     - Unresolvable routing conflicts (`NEEDS_ADMIN_ATTENTION`)

     - Destructive action triggers ("Delete Rule", "Unassign Slot")

   - **Banned Colors**: Blue, Green, Amber, Yellow, Purple, Pastel fills, Neon glows.



3. **Typography & Hierarchy**:

   - **UI Font**: Geist Sans (`font-sans` / `font-family: Geist, sans-serif`).

   - **Technical Data & Codes**: JetBrains Mono (`font-mono`) for Policy IDs (`POL-ENG-01`), Employee Codes (`EMP-012`), Leave Request IDs (`LR-2026-089`), and timestamps.

   - **Scale**: Large bold uppercase page headers (`24px - 32px`), uppercase table header signposts (`10px - 12px`, letter spacing `0.05em`), body text (`14px - 16px`).



4. **Icons & Emojis**:

   - **Zero Emojis**: Strictly prohibited.

   - **Icons**: Minimalist functional SVG icons only (search lens, chevron dropdown, close X).



---



## 4. Razor MVC Architecture & Route Breakdown



### 4.1 Controllers & Route Structure



`HRM_Leave_Management/WebUI/Controllers/ApprovalRoutingController.cs`



- `GET /approval-routing/policies`: Displays Policy List screen.

- `GET /approval-routing/policies/detail/{id}`: Displays Policy Detail & Position Rules screen.

- `GET /approval-routing/levels/assignments`: Displays Level Slot Assignments screen.

- `GET /approval-routing/legacy-migration`: Displays Legacy Mapping & Migration Audit Console.

- `POST /approval-routing/impact-preview`: AJAX endpoint to execute dry-run impact calculation (`GetEmployeeDeactivationImpactQuery`) and return modal partial HTML.

- `POST /approval-routing/execute-reassignment`: AJAX endpoint to process `UnassignApprovalLevelCommand` or `ReassignPendingLeaveRequestsCommand`.



### 4.2 ViewModels & Data Transfer Objects



```csharp

namespace HRM.Web.Models.ApprovalRouting

{

    public class PolicyListViewModel

    {

        public List<PolicySummaryDto> Policies { get; set; } = new();

        public string? DepartmentFilter { get; set; }

        public string? SearchTerm { get; set; }

    }



    public class PolicyDetailViewModel

    {

        public Guid PolicyId { get; set; }

        public string PolicyName { get; set; } = default!;

        public string DepartmentName { get; set; } = default!;

        public bool IsActive { get; set; }

        public List<LevelSlotDto> LevelSlots { get; set; } = new();

        public List<PositionRuleDto> PositionRules { get; set; } = new();

    }



    public class LevelAssignmentViewModel

    {

        public Guid PolicyId { get; set; }

        public string PolicyName { get; set; } = default!;

        public List<LevelAssignmentRowDto> Assignments { get; set; } = new();

    }



    public class ImpactPreviewModalViewModel

    {

        public Guid LevelAssignmentId { get; set; }

        public string TargetSlotName { get; set; } = default!;

        public string AssignedEmployeeName { get; set; } = default!;

        public int TotalImpactedCount { get; set; }

        public int AutoReroutableCount { get; set; }

        public int NeedsAdminAttentionCount { get; set; }

        public List<ImpactedRequestRowDto> AffectedRequests { get; set; } = new();

        public List<EmployeeOptionDto> AvailableApprovers { get; set; } = new();

    }

}

```



---



## 5. Verification & Quality Assurance Plan



Before submitting Phase 5 for final code review:



1. **Compilation Check**:

   - Run `dotnet build` on `HRM_Leave_Management/HRM_Leave_Management.sln`.

   - Verify **0 Errors, 0 Warnings**.



2. **Encoding & Mojibake Check**:

   - Run `python MD_memory/debug/2026-06-26_1430_scan-mojibake.py MD_memory/plans/2026-07-23_1000_phase-dynamic-approval-routing_phase-5-ui-design_proposal.md --require-bom`.



3. **UI Fidelity Checklist**:

   - Confirm 0px border radius across all new Razor views and CSS components.

   - Confirm monochrome palette with Swiss Red `#E62429` reserved exclusively for errors/unassigned states.

   - Confirm absence of emojis and rounded SaaS pills.



---



## 6. Stop & Approval Request



> [!IMPORTANT]

> **Anti has completed the Stitch Canvas designs and Phase 5 UI Design Proposal.**

> Anti is now **STOPPING** in planning mode and waiting for explicit User / Codex review and approval before writing any C# Controller or Razor View code.
