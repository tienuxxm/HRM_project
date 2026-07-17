---
page: position_list
---
A high-density administrative Position List screen for an enterprise HRM application, implementing the Swiss International HR typographic design language.

**DESIGN SYSTEM (REQUIRED):**
- Platform: Web, Desktop-first
- Theme: Light, Minimalist Swiss International
- Background: Canvas Warm Light (#FAF9F9)
- Primary Accent: Solid Black Ink (#000000) for active borders/headers and grid borders
- Text Primary: Dark Gray (#1A1C1C)
- Text Secondary: Muted Gray (#4C4546)
- Borders: Hairline Gray (#D1D1D1), 1px solid
- Swiss Red Accent: Swiss Red (#E62429) for actions like delete or critical indicators
- Corners: Strictly 0px (completely squared, border-radius: 0px)
- Shadows: None (flat, structural layout only)
- Gradients: None (solid color fills only)
- Typography: Geist for UI labels, titles, and text. JetBrains Mono for numbers, IDs, breadcrumbs, status indicators, and metadata.

**Page Structure & Layout:**
1. **Sidebar Navigation (Left Panel):**
   - Width: 260px, height: 100vh. Background: Solid White (#FFFFFF), right border: 1px solid (#D1D1D1).
   - Brand Section at top: "HRM PORTAL" in bold Geist, 20px, uppercase, black (#000000). A 2px horizontal Swiss Red (#E62429) accent line sits directly beneath the brand title.
   - Menu Items: Stacked vertically with comfortable density. Menu links in Geist, 11px, bold, uppercase, letter-spacing: 0.05em, text color: Muted Gray (#4C4546).
   - Active Menu Item: "POSITIONS" (highlighted in black #000000, with a 3px vertical black line on the left edge).
   - Navigation links to include: "DASHBOARD", "EMPLOYEES", "DEPARTMENTS", "POSITIONS", "LEAVE REQUESTS", "LEAVE BALANCES", "WORK CALENDAR", "SETTINGS".
2. **Top Header Bar:**
   - Height: 64px. Background: Solid White (#FFFFFF), bottom border: 1px solid (#D1D1D1).
   - Breadcrumb navigation: "SYS / STRUCTURE / POSITIONS" in JetBrains Mono, 10px, bold uppercase, light gray (#7F8C8D).
   - Right Side: User session metadata in JetBrains Mono, 10px: "REALM: HRM" | "USER: ADMIN@HRM.LOCAL". A minimal flat "LOGOUT" button (1px border, 0px border-radius, Geist, 10px, bold uppercase, text-color: #E62429).
3. **Main Content Workspace:**
   - Padding: 32px all sides.
   - **Section 1: Actions & Filter Bar:**
     - A horizontal bar with flex-row layout, 1px solid border (#D1D1D1) at the bottom, padding-bottom: 20px.
     - Left: Search input (1px solid border, 0px border-radius, "SEARCH POSITIONS..." placeholder in Geist 11px, height 36px, width 240px) and a dropdown select for Department ("ALL DEPARTMENTS" default, Geist 11px, 1px solid border, 0px border-radius, height 36px, width 180px, margin-left: 12px).
     - Right: A solid black button "+ CREATE POSITION" (Background: #000000, Text: #FFFFFF, Geist 11px bold uppercase, letter-spacing: 0.05em, padding: 0 16px, height 36px, 0px border-radius).
   - **Section 2: High-Density Position Data Table:**
     - Container: White background, 1px solid border (#D1D1D1), 0px border-radius.
     - Table structure:
       - Header row: Column titles in Geist, 10px bold uppercase, text color: Muted Gray (#4C4546), bottom border 1px solid (#D1D1D1), background #F5F5F5.
         - Columns: "CODE", "POSITION TITLE", "DEPARTMENT", "REPORT TO", "GRADE", "TOTAL EMPLOYEES", "STATUS", "ACTIONS".
       - Data Rows (4 rows minimum for demonstration):
         - Row 1:
           - CODE: "POS-ENG-LEAD" (JetBrains Mono, bold)
           - Position Title: "LEAD ENGINEER" (Geist, 12px bold, text: #000000)
           - Department: "ENGINEERING" (Geist, 11px)
           - Report To: "CTO" (Geist, 11px)
           - Grade: "M1" (JetBrains Mono, 11px)
           - Total Employees: "3" (JetBrains Mono, 11px)
           - Status: A rectangular status badge with light-green border/text outline saying "ACTIVE" (Geist, 10px, bold).
           - Actions: Flat text buttons "[EDIT]" (black, bold Geist 10px) | "[DEACTIVATE]" (Swiss Red, bold Geist 10px) | "[VIEW EMPLOYEES]" (black, Geist 10px).
         - Row 2:
           - CODE: "POS-ENG-SWE"
           - Position Title: "SOFTWARE ENGINEER"
           - Department: "ENGINEERING"
           - Report To: "LEAD ENGINEER"
           - Grade: "IC3"
           - Total Employees: "18"
           - Status: "ACTIVE" badge
           - Actions: "[EDIT]" | "[DEACTIVATE]" | "[VIEW EMPLOYEES]"
         - Row 3:
           - CODE: "POS-HR-SPEC"
           - Position Title: "HR SPECIALIST"
           - Department: "HUMAN RESOURCES"
           - Report To: "HR MANAGER"
           - Grade: "IC2"
           - Total Employees: "4"
           - Status: "ACTIVE" badge
           - Actions: "[EDIT]" | "[DEACTIVATE]" | "[VIEW EMPLOYEES]"
         - Row 4:
           - CODE: "POS-FIN-ANL"
           - Position Title: "FINANCIAL ANALYST"
           - Department: "FINANCE"
           - Report To: "CFO"
           - Grade: "IC3"
           - Total Employees: "0"
           - Status: "INACTIVE" badge (light gray border/text)
           - Actions: "[EDIT]" | "[ACTIVATE]" (black, Geist 10px) | "[VIEW EMPLOYEES]"
4. **Footer:**
   - Connectivity status line: "REALM CONNECTIVITY SECURED BY KEYCLOAK IDENTITY PROVIDER. SESSION STATUS: VERIFIED." in JetBrains Mono, 9px, muted gray (#7F8C8D).
