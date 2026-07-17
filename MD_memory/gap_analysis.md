# Phase 1.5 - UI Shell Parity: Gap Analysis

> Muc tieu: Giao dien HRM giong layout tong the project goc (sidebar, header, login style, mau sac, spacing).
> Noi dung nghiep vu phai doi sang HRM, khong giu menu/data nha hang.

---

## 1. Bang So Sanh Chi Tiet

| File Goc | File HRM Hien Tai | Muc Do Giong | Asset/Script/Route Phu Thuoc | Hanh Dong De Xuat | Rui Ro Runtime | Can GitNexus Impact? |
|:---|:---|:---|:---|:---|:---|:---|
| `Web.Backend/Views/Shared/_Layout.cshtml` (253 dong) | `HRM.Web/Views/Shared/_Layout.cshtml` (27 dong) | 0% - Hoan toan khac | CDN: Tailwind(`styles.css`), Flowbite, Chart.js, DataTables, Toastify. JS: `site.js`, `jquery.sumoselect.js`. Route: `GET /Auth/GetUserLogin` (can them). Logout: goc dung GET, HRM giu POST. Cookie: HRM dung `jwt_token` (giu nguyen). | Copy khung HTML, doi menu JS sang HRM. Them endpoint `GetUserLogin`. Sua logout trong Layout tu link `<a>` (GET) thanh `<form method="post">` (POST) — **khong doi backend Logout**. | **CAO** - Nhieu dependency route/cookie | **CO** - Phai kiem tra AuthController (chi Login POST + them GetUserLogin) |
| `Web.Backend/Views/Login/LoginScreen.cshtml` (172 dong) | `HRM.Web/Views/Auth/Login.cshtml` (23 dong) | 0% - Hoan toan khac | CSS: `styles.css`, `site.css`, `sumoselect.css`. JS: jQuery, Flowbite, `site.js`. Route: `POST /auth/login` (AJAX, tra `Ok()`). Toast partial: `SuccessToastPartial`, `FailToastPartial`. | Copy style HTML. **Phai doi auth flow**: goc dung AJAX + `Ok()` response, HRM dung form POST + redirect. Chon 1 trong 2 cach. | **CAO** - Login flow khac nhau hoan toan | **CO** - AuthController, LoginController |
| `Web.Backend/Views/Dashboard/Index.cshtml` (241 dong) | `HRM.Web/Views/Home/Index.cshtml` (~5 dong) | 0% - Hoan toan khac | ViewModel: `DashboardViewModel` (goc), khong co (HRM). Chart.js. Route: `/dashboard` (goc) vs `/Home/Index` (HRM). | Tao `DashboardController` + `DashboardViewModel` cho HRM voi du lieu nghi phep. Hoac giu `HomeController` va them ViewModel. | **TRUNG BINH** - Can tao ViewModel moi | **CO** - Neu tao controller moi |
| `Web.Backend/wwwroot/css/styles.css` (64KB compiled Tailwind) | **KHONG CO** | Thieu 100% | La file CSS chinh, tat ca Tailwind utility class. | Copy nguyen van. Day la file CSS da compile, khong can build lai. | **THAP** - File tinh | Khong |
| `Web.Backend/wwwroot/css/site.css` (2KB) | `HRM.Web/wwwroot/css/site.css` (362B, default .NET) | 0% | Chua Tailwind directives + custom scrollbar + animation. | Copy nguyen van thay the file hien tai. | **THAP** - File tinh | Khong |
| `Web.Backend/wwwroot/css/sumoselect.css` (9.7KB) | **KHONG CO** | Thieu 100% | Styling cho SumoSelect dropdown. | Copy nguyen van. | **THAP** - File tinh | Khong |
| `Web.Backend/wwwroot/js/site.js` (6.8KB) | `HRM.Web/wwwroot/js/site.js` (231B, default) | 0% | Chua: `parseDate`, `debounce`, `deleteAction`, `showToast`, `formatDate`. Phu thuoc: Flowbite (`Modal`), Toastify. | Copy nguyen van. Loc bo `getDeleteModal` neu chua can. | **THAP** - File tinh | Khong |
| `Web.Backend/wwwroot/js/jquery.sumoselect.js` (31KB) | **KHONG CO** | Thieu 100% | Thu vien dropdown. | Copy nguyen van. | **THAP** - File tinh | Khong |
| `Web.Backend/wwwroot/js/jquery.min.js` (87KB) | **KHONG RO** | Can kiem tra | jQuery core. | Kiem tra thu muc `lib/` cua HRM. | **THAP** | Khong |

---

## 2. Phan Tich Route/Cookie Khac Biet

| Yeu to | Project Goc | HRM Hien Tai | Van de |
|:---|:---|:---|:---|
| Login GET | `GET /auth/login-screen` (LoginController) | `GET /Auth/Login` (AuthController) | **Route khac** - `_Layout` goc redirect ve `/Auth/Login` nhung LoginScreen render o `/auth/login-screen` |
| Login POST | `POST /auth/login` (LoginController, AJAX, tra Ok()) | `POST /Auth/Login` (AuthController, form POST, tra Redirect) | **Auth flow khac** - Goc: AJAX call -> Ok() -> JS redirect. HRM: form submit -> server redirect |
| GetUserLogin | `GET /Auth/GetUserLogin` (AuthController goc) | **KHONG CO** | `_Layout` goc goi endpoint nay de hien thi username tren header. HRM se bi 404 |
| Logout | `GET /Auth/Logout` (xoa `X-Access-Token`) | `POST /Auth/Logout` (xoa `jwt_token`) | **Verb khac** - `_Layout` goc render link `<a>` (GET). HRM dung form POST |
| Cookie name | `X-Access-Token` | `jwt_token` | **Ten khac** - Middleware doc JWT tu cookie name. Phai nhat quan |
| After login | Redirect `/dashboard` | Redirect `/Home/Index` | **Route khac** - Menu sidebar url `dashboard` se tro ve DashboardController |

---

## 3. Quyet Dinh Thiet Ke Cho Phase 1.5

> **Luu y:** GitNexus hien chua index thu muc `HRM_Leave_Management`.
> Impact analysis bang GitNexus chi ap dung cho project goc (`Web.Backend`).
> Voi HRM, dang dung **manual code analysis** tam thoi.
> Neu can re-index, chay: `node .gitnexus/run.cjs analyze` sau khi cau hinh them path.

### Login Flow
**Chon huong:** Doi HRM AuthController.Login(POST) sang AJAX flow (tra `Ok()`/`BadRequest()` thay vi redirect).
- Ly do: De copy nguyen `LoginScreen.cshtml` voi it thay doi nhat, giu duoc spinner/toast UX.
- Rui ro: Phai sua `AuthController.Login(POST)`. Manual analysis: chi Login.cshtml goi action nay, khong co caller C# nao khac.

### Logout
**Quyet dinh: GIU POST /Auth/Logout.** Khong doi sang GET.
- Ly do security: GET logout de bi CSRF qua `<img>` hoac link. POST an toan hon.
- Hanh dong: Chi sua `_Layout.cshtml` — doi logout tu `<a href>` (goc dung GET) thanh `<form method="post" action="/Auth/Logout"><button>Logout</button></form>`.
- **AuthController.Logout KHONG SUA** — hien tai da la POST va xoa `jwt_token` dung.

### GetUserLogin
- Them action moi `GET /Auth/GetUserLogin` vao HRM AuthController.
- Tra JSON `{ username: "..." }` de `_Layout.cshtml` hien thi username tren header.
- Day la **them code moi**, khong sua/xoa code cu.

### Layout
- Copy khung `_Layout.cshtml` tu goc, doi menu JS sang HRM.
- Sua logout HTML thanh form POST (xem muc Logout).
- Giu endpoint `/Auth/GetUserLogin` giong goc.

### Dashboard
- Tao `DashboardController` moi (khong sua HomeController cu).
- Tao `DashboardViewModel` don gian voi placeholder data HRM.
- Route: `/dashboard` de menu sidebar hoat dong.

### Cookie
- Giu ten cookie `jwt_token` (da duoc doc boi `Program.cs` middleware va `JwtBearerOptionsSetup.cs`).
- Khong doi sang `X-Access-Token`.

### Assets
- Copy thang cac file tinh: `styles.css`, `site.css`, `sumoselect.css`, `site.js`, `jquery.sumoselect.js`.
- CDN links giu nguyen (Flowbite, Chart.js, Font Awesome, DataTables, Toastify).

---

## 4. Menu HRM (Thay The Menu Nha Hang)

```
Tab: HRM MANAGEMENT
- Dashboard (url: dashboard)
- Employees (url: employees)  
- Departments (url: departments)

Tab: LEAVE MANAGEMENT
- Leave Requests (url: leave-requests)
- Leave Types (url: leave-types)
- Approvals (url: approvals)
- Reports (url: reports)

Tab: SYSTEM
- Roles/Permissions (url: roles)
```

---

## 5. Scope Phase 1.5

### Se lam:
- Copy assets tinh (CSS, JS)
- Copy + refactor `_Layout.cshtml` (menu HRM, logout form POST, route fix)
- Copy + refactor `LoginScreen.cshtml`
- Sua `AuthController.Login(POST)` — doi tra `Ok()`/`BadRequest()` thay vi redirect
- Them `AuthController.GetUserLogin(GET)` — endpoint moi, khong sua code cu
- AuthController.Logout(POST) — **KHONG SUA**, chi sua View
- Tao `DashboardController` + `DashboardViewModel` + View voi placeholder cards
- Browser verification (login, dashboard, menu)

### CHUA lam trong Phase 1.5:
- CRUD Employee, Department
- LeaveRequest/LeaveType entity va UI
- Chart voi du lieu that (chi dung placeholder)
- Approval flow
- Reports
