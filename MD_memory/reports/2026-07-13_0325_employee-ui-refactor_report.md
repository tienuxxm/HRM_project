# BÃ¡o CÃ¡o XÃ¡c Minh UAT: Refactor Giao Diá»‡n Employee Directory (Swiss International)

## 1. ThÃ´ng Tin Há»‡ Thá»‘ng & RÃ ng Buá»™c Ká»¹ Thuáº­t
* **Cháº¿ Ä‘á»™ Auth**: Keycloak thá»±c táº¿ (Docker container `keycloak-hrm` cháº¡y táº¡i `http://localhost:8080`)
* **Tráº¡ng thÃ¡i `UseMockAuth`**: `false`
* **TÃ i khoáº£n UAT**: `admin` (hoáº·c `admin@hrm.local`) / Máº­t kháº©u: `Admin@123456`
* **Quyá»n háº¡n Ä‘Ã£ cáº¥u hÃ¬nh (Permissions)**: `VIEW_EMPLOYEE`, `UPDATE_EMPLOYEE`
* **ÄÆ°á»ng dáº«n kiá»ƒm thá»­ (Test URL)**: `/employee`

---

## 2. Nháº­t KÃ½ PhÃ¢n TÃ­ch Lá»—i & Kháº¯c Phá»¥c (Hygiene & Build)

### 2.1 NguyÃªn nhÃ¢n lá»—i (Root Cause)
* **Váº¥n Ä‘á»**: Giao diá»‡n Employee vÃ  Layout tá»•ng thá»ƒ bá»‹ máº¥t Sidebar vÃ  Header, chá»‰ hiá»ƒn thá»‹ ná»™i dung tráº§n.
* **NguyÃªn nhÃ¢n cá»‘t lÃµi**: TrÃ¬nh biÃªn dá»‹ch Tailwind CSS JIT (Just-In-Time) bá»‹ stale hoáº·c khÃ´ng nháº­n diá»‡n chÃ­nh xÃ¡c cÃ¡c lá»›p responsive vÃ  cÃ¡c token thiáº¿t káº¿ Swiss International má»›i (nhÆ° `lg:flex`, `lg:translate-x-0`, `w-260`, `bg-swiss-light`). Äiá»u nÃ y xáº£y ra do cáº¥u hÃ¬nh glob pattern trong `tailwind.config.js` khÃ´ng Ä‘Æ°á»£c giáº£i quyáº¿t Ä‘Ãºng trÃªn mÃ´i trÆ°á»ng Windows PowerShell khi cháº¡y lá»‡nh build CSS ban Ä‘áº§u.

### 2.2 Giáº£i phÃ¡p kháº¯c phá»¥c (Fix Implementation)
* ÄÃ£ cáº¥u hÃ¬nh má»Ÿ rá»™ng (extend) cÃ¡c token mÃ u sáº¯c (`swiss-light`, `swiss-border`, `swiss-red`, `swiss-accent-red`) vÃ  kÃ­ch thÆ°á»›c (`260`) vÃ o [tailwind.config.js](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Web.Backend/tailwind.config.js).
* Thá»±c hiá»‡n dá»n dáº¹p cÃ¡c tá»‡p táº¡m khÃ´ng cáº§n thiáº¿t:
  * XÃ³a `HRM_Leave_Management/Web.Backend/tailwind-test.config.js`
  * XÃ³a `HRM_Leave_Management/Web.Backend/wwwroot/css/styles-test.css`
* BiÃªn dá»‹ch láº¡i Tailwind CSS báº±ng lá»‡nh: `npm run css:build`.

---

## 3. Káº¿t Quáº£ XÃ¡c Minh Ká»¹ Thuáº­t (Technical Build Verification)

### 3.1 BiÃªn dá»‹ch giáº£i phÃ¡p C# (dotnet build)
* **Lá»‡nh cháº¡y**: `dotnet build HRM_Leave_Management/LUC.sln`
* **Káº¿t quáº£**: **PASS** (0 Error(s), 30 Warning(s))

### 3.2 Kiá»ƒm tra sá»± tá»“n táº¡i cá»§a cÃ¡c lá»›p CSS trong styles.css
* ÄÃ£ cháº¡y lá»‡nh kiá»ƒm chá»©ng PowerShell (`Select-String`) trÃªn tá»‡p [styles.css](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/HRM_Leave_Management/Web.Backend/wwwroot/css/styles.css) vÃ  xÃ¡c nháº­n sá»± tá»“n táº¡i cá»§a cÃ¡c lá»›p quan trá»ng sau:
  * **`.w-260`** (Äá»™ rá»™ng Sidebar): **PASS** (Tá»“n táº¡i á»Ÿ dÃ²ng 1573)
  * **`.border-swiss-border`** (MÃ u viá»n tÃ³c máº£nh): **PASS** (Tá»“n táº¡i á»Ÿ dÃ²ng 2197)
  * **`.bg-swiss-light`** (MÃ u ná»n xÃ¡m nháº¡t): **PASS** (Tá»“n táº¡i á»Ÿ dÃ²ng 2406)
  * **`.bg-swiss-red`** (MÃ u Ä‘á» chá»§ Ä‘áº¡o Swiss): **PASS** (Tá»“n táº¡i á»Ÿ dÃ²ng 2411)
  * **`.lg\:flex`** (Flexbox trÃªn mÃ n hÃ¬nh lá»›n): **PASS** (Tá»“n táº¡i á»Ÿ dÃ²ng 4204)
  * **`.lg\:translate-x-0`** (Hiá»‡n Sidebar trÃªn mÃ n hÃ¬nh lá»›n): **PASS** (Tá»“n táº¡i á»Ÿ dÃ²ng 4212)

* **Tráº¡ng thÃ¡i XÃ¡c Minh Ká»¹ Thuáº­t**: **PASS**

---

## 4. Tráº¡ng ThÃ¡i UAT Trá»±c Quan (Visual UAT Status)
* **Tráº¡ng thÃ¡i hiá»‡n táº¡i**: **PENDING**
* **LÃ½ do**: ChÆ°a tiáº¿n hÃ nh UAT báº±ng trÃ¬nh duyá»‡t tá»± Ä‘á»™ng (browser subagent) Ä‘á»ƒ chá»¥p áº£nh mÃ n hÃ¬nh vÃ  Ä‘á»‘i chiáº¿u trá»±c quan 100% vá»›i Stitch canvas. Viá»‡c xÃ¡c nháº­n hiá»ƒn thá»‹ giao diá»‡n thá»±c táº¿ cáº§n Ä‘Æ°á»£c ngÆ°á»i dÃ¹ng kiá»ƒm tra trá»±c quan trÃªn trÃ¬nh duyá»‡t local.

---

## 5. Ká»‹ch Báº£n UAT Thá»§ CÃ´ng Cho NgÆ°á»i DÃ¹ng (Manual UAT Steps)

### TC-01: Kiá»ƒm tra cáº¥u trÃºc Global App Shell (Swiss International Layout)
* **CÃ¡c bÆ°á»›c**:
  1. ÄÄƒng nháº­p vá»›i tÃ i khoáº£n Admin Keycloak.
  2. Truy cáº­p `/employee`.
  3. Kiá»ƒm tra Sidebar, Header, vÃ  Footer.
* **Káº¿t quáº£ ká»³ vá»ng**:
  * Sidebar hiá»ƒn thá»‹ nhÃ£n **HRM PORTAL** cÃ³ gáº¡ch chÃ¢n Ä‘á» (`swiss-underline`).
  * Header Desktop hiá»ƒn thá»‹ breadcrumb `SYS / DIRECTORY / EMPLOYEES`.
  * PhÃ­a bÃªn pháº£i cá»§a Header hiá»ƒn thá»‹ trá»±c tiáº¿p: `REALM: HRM | USER: admin | LOGOUT` (khÃ´ng bá»‹ áº©n trong dropdown).

### TC-02: Kiá»ƒm tra báº£ng thÃ´ng tin Employee Directory (Desktop Layout)
* **CÃ¡c bÆ°á»›c**:
  1. DÃ¹ng trÃ¬nh duyá»‡t trÃªn Desktop.
  2. Quan sÃ¡t báº£ng dá»¯ liá»‡u danh sÃ¡ch nhÃ¢n viÃªn.
* **Káº¿t quáº£ ká»³ vá»ng**:
  * CÃ¡c gÃ³c cá»§a báº£ng Ä‘á»u vuÃ´ng vá»©c (`0px` border-radius).
  * TiÃªu Ä‘á» báº£ng (`thead`) cÃ³ ná»n xÃ¡m nháº¡t (`#F5F5F5`), chá»¯ in hoa Ä‘áº­m.
  * Cá»™t mÃ£ nhÃ¢n viÃªn hiá»ƒn thá»‹ dáº¡ng font monospace (`JetBrains Mono`), chá»¯ in hoa.
  * Tráº¡ng thÃ¡i hoáº¡t Ä‘á»™ng hiá»ƒn thá»‹ dÆ°á»›i dáº¡ng badge chá»¯ nháº­t khÃ´ng bo gÃ³c.

### TC-03: Kiá»ƒm tra giao diá»‡n trÃªn Mobile Responsive Layout
* **CÃ¡c bÆ°á»›c**:
  1. Chuyá»ƒn trÃ¬nh duyá»‡t sang Mobile view (viewport < 768px).
  2. Kiá»ƒm tra Sidebar vÃ  danh sÃ¡ch nhÃ¢n viÃªn.
* **Káº¿t quáº£ ká»³ vá»ng**:
  * Sidebar áº©n Ä‘i vÃ  cÃ³ nÃºt hamburger Ä‘á»ƒ kÃ­ch hoáº¡t.
  * Báº£ng dá»¯ liá»‡u chuyá»ƒn thÃ nh dáº¡ng tháº» xáº¿p chá»“ng (stacked cards) vuÃ´ng vá»©c.
  * ChÃ¢n tháº» chia Ä‘á»u 3 nÃºt hÃ nh Ä‘á»™ng ngÄƒn cÃ¡ch báº±ng Ä‘Æ°á»ng viá»n máº£nh: `[PROVISION]`, `[EDIT]`, `[DELETE]`.
