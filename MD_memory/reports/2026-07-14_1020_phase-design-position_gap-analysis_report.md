# Design Gap Analysis: Position Management Interface

**Date:** 2026-07-14
**Phase:** Position Management Design Alignment
**Stitch Project ID:** `17479353588209716186`

---

## 1. Boundary & Context Preservation
*   **Active Boundary:** `Web.Backend -> Application -> Domain` & `Infrastructure -> Application/Domain`
*   **Design Direction:** Swiss International HR (0px border-radius, Geist/JetBrains Mono typography, high-contrast, black accents, selective Swiss red accent).
*   **Key Constraints:**
    *   No runtime C# or Razor changes in this step.
    *   No database, schema, or Keycloak modifications.
    *   Stitch-first design review before coding.

---

## 2. Entity & Database Schema vs. Stitch Design Review

### Current Runtime Entity Definition
According to the C# entity class `Domain.Positions.Position`:
*   `Code` (string): Unique identifier code of the position.
*   `Name` (string): Position name.
*   `Level` (int): Seniority or organization level of the position.
*   `IsActive` (bool): Active status of the position.
*   `CreatedDate` (DateTime): Timestamp of position creation.

### Initial Stitch Design (Desktop `9714fc9d811e4b3e829eeeceed9195bd`)
*   **Columns present:** `Code`, `Position Name`, `Department`, `Count`, `Description`, `Actions`.
*   **Mismatches identified:**
    1.  `Department` is not a property of the `Position` entity (the relationship is established via the `Employee` entity, which references both a Department and a Position).
    2.  `Count` (number of active employees in this position) is not a direct property of the `Position` entity.
    3.  `Description` is not stored in the database for positions.
    4.  `Level` (a critical database property) was missing from the desktop view.

---

## 3. Completed Design Refinement on Stitch

To align the design system assets with the actual schema without proposing unauthorized database changes, we updated the Stitch screens to match the exact properties:

### Refined Desktop Position List (Screen ID: `9714fc9d811e4b3e829eeeceed9195bd`)
*   **Action taken:** Executed a surgical update via Stitch MCP `edit_screens`.
*   **Columns now present:** `Code`, `Position Name`, `Level`, `Actions` (Edit / Delete).
*   **Visual changes:**
    *   Removed `Department`, `Count`, and `Description` columns.
    *   Inserted the `Level` column (numerical values like `1`, `2`, `3`) between `Position Name` and `Actions`, styled with JetBrains Mono font to match data-density standards.
    *   Preserved flat layout, black header accents, 0px border-radius, and bottom pagination.

### Mobile Position List (Screen ID: `e7f3855fe06442768c5faec12a4fdfec`)
*   **Viewport configuration:** Standard `780px` width.
*   **Content mapping:** Displays cards with `Code`, `Position Name`, and `LEVEL` tags, along with flat actions (Edit/Delete).
*   **Status:** Aligns perfectly with the refined desktop view and C# properties out-of-the-box.

---

## 4. Current Site Map & Registry Verification

### Metadata Registry (`.stitch/metadata.json`)
*   **Desktop:** `position_list_swiss_international_desktop` -> ID: `9714fc9d811e4b3e829eeeceed9195bd` (`GENERATED_FOR_REVIEW`)
*   **Mobile:** `position_list_swiss_international_mobile` -> ID: `e7f3855fe06442768c5faec12a4fdfec` (`GENERATED_FOR_REVIEW`)

### Sitemap (`.stitch/SITE.md`)
*   Updated sitemap documentation to reflect the columns alignment.

---

## 5. Technical Log & Environmental Notes
*   **Git Status Check:** Proposing Git commands via shell execution returned `unexpected user interaction type: not permission`. This is a system permission restriction and does not represent a code issue. No files have been staged, committed, or pushed.
*   **Next Phase Entry Checklist:**
    1.  User review and approval of the refined Desktop (`9714fc9d811e4b3e829eeeceed9195bd`) and Mobile (`e7f3855fe06442768c5faec12a4fdfec`) screens on Stitch.
    2.  Transition to Phase 3A Position Management MVC views refactoring.
