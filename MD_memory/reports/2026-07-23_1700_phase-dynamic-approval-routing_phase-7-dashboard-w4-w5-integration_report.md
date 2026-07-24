# Phase 7 — Dashboard W4/W5 Integration & W3 Layout Bounding-Box Verification Report



**Date**: 2026-07-24

**Author**: Technical Reviewer & Fullstack Engineer (Anti)

**Architecture Boundary**:

- `Web.Backend -> Application -> Domain`

- `Infrastructure -> Application/Domain`



---



## 1. Previous PASS Invalid Clarification



> [!CAUTION]

> **Clarification on Previous Status**:

> The previous PASS status reported for Widget W3 was **INVALID** because initial verification relied solely on high-level build checks and cropped screenshots without measuring exact DOM element coordinates (`getBoundingClientRect()`) or validating single-image full-boundary spatial isolation between Widget W3 and Widget W4.



---



## 2. Root Cause Layer: Web.Backend Presentation



The root cause was strictly localized to the **Web.Backend Presentation Layer** (`Views/Dashboard/Index.cshtml` & `wwwroot/css/dashboard.css`). The Application Layer, Domain Layer, and Database Layer were 100% clean and uninvolved in this visual issue.



### Specific Presentation Deficiencies:

1. **Unpadded SVG Y-Mapping**: Zero-value data points generated `y = 100` within SVG viewBox `0 0 400 100`. The rendered circle (`cy = 100`, `r = 3`) extended from `y = 97` to `y = 103`, causing lower circle arcs to bleed outside the coordinate boundary.

2. **Fixed Container Height**: `.hrm-trend-line-container` had fixed `height: 140px` with `overflow: hidden`, which pushed the x-axis month labels row below the container frame.



---



## 3. Applied Patch (Presentation Layer Only)



1. **`Index.cshtml`**:

   - Updated Y-mapping formula in viewBox `0 0 400 90`:

     `var y = 72 - (int)Math.Round((Model.MonthlyTrend[i].RequestCount / (double)chartMax) * 54);`

   - Zero values map to `y = 72` (circle spans `y = 69..75`), guaranteeing **15px clear SVG space** inside the bottom viewBox boundary.

   - Maximum values map to `y = 18` (circle spans `y = 15..21`), guaranteeing **15px clear SVG space** inside the top viewBox boundary.

   - Wrapped SVG in `div class="w-full h-[95px] relative overflow-hidden"`.

   - Placed month labels in a dedicated flex row with top border divider (`border-t border-gray-100 shrink-0`).

2. **`dashboard.css`**:

   - Updated `.hrm-trend-line-container` to `min-height: 150px; padding: 0.75rem 0.75rem 0.5rem 0.75rem; display: flex; flex-direction: column; justify-content: space-between; box-sizing: border-box;`.

3. **Swiss Palette Compliance**: Monochromatic Black (`#111111`), White, Gray (`text-gray-500`). Zero emojis, zero blue/green/yellow accents.



---



## 4. Official Evidence Artifacts & Obsolete Artifact Audit



### Primary Official Evidence Set:

- [w3_desktop_before_fail.png](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/evidence/2026-07-23_dashboard-phase7-uat/w3_desktop_before_fail.png): Before patch screenshot (showing coordinate overflow).

- [w3_desktop_after_pass_full_boundary.png](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/evidence/2026-07-23_dashboard-phase7-uat/w3_desktop_after_pass_full_boundary.png): **Primary Desktop Evidence** — single unfragmented picture displaying W3 Card, X-axis month labels, 24px vertical gap, and W4 header.

- [w3_mobile_after_pass.png](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/evidence/2026-07-23_dashboard-phase7-uat/w3_mobile_after_pass.png): **Primary Mobile Evidence** — viewport `390x844` showing clean stacked vertical layout with zero horizontal overflow.

- [w3_bounding_box_check.txt](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/evidence/2026-07-23_dashboard-phase7-uat/w3_bounding_box_check.txt): **Primary Log Evidence** — full raw mathematical bounding-box measurement log.



### Obsolete / Superseded Artifacts:

- `w3_desktop_before_or_verified.png`: Superseded by `w3_desktop_after_pass_full_boundary.png`.

- `uat_provision81_dashboard_w4_w5.png`: Obsolete early UAT screenshot (retained for historic log only).



---



## 5. Bounding-Box Proof



Exact raw pixel values extracted from active DOM runtime instance (`http://localhost:5300/dashboard`, user `uat.provision81`):



- **DOM Selectors Used**:

  - `w3Card`: `document.querySelector('.col-span-8')`

  - `w4Card`: `document.querySelector('.col-span-6')`

  - `chartContainer`: `document.querySelector('.hrm-trend-line-container')`



- **Raw Measured Values (Desktop Viewport `1536x730`)**:

  - `w3Card.bottom`: **`471.0 px`**

  - `w4Card.top`: **`495.0 px`**

  - `gap = w4Card.top - w3Card.bottom`: **`24.0 px`** (Matches Tailwind `gap-6`)

  - `chartContainer.bottom`: **`447.0 px`**



- **Verification Evaluation**:

  - `w3Card.bottom < w4Card.top` (`471.0px < 495.0px`): **TRUE** (`24.0px` clean vertical margin, zero overlap).

  - `chartContainer.bottom <= w3Card.bottom` (`447.0px <= 471.0px`): **TRUE** (`chartContainer` sits 24.0px inside W3 Card).

  - X-Axis Month Labels: 100% visible, 0% clipped.



---



## 6. Residual Technical Note & Recommendation



> [!NOTE]

> **Residual Note for Future E2E Test Automation**:

> The current DOM selectors (`.col-span-8` / `.col-span-6`) are fully sufficient and verified for this manual/subagent UAT because the single full-boundary screenshot visually confirms exact element mapping. However, for future automated regression testing (Playwright/Cypress), adding explicit, stable DOM attributes such as `data-widget="w3"` and `data-widget="w4"` to the widget card containers is recommended.
