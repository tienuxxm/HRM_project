# Phase 2A.1 — Department CRUD UAT Report (Keycloak thật)

> **Ngày UAT**: 2026-06-25
> **Trạng thái**: ✅ PASS — Keycloak thật
> **Môi trường**: Local Development (`http://localhost:5300`)
> **Keycloak**: Container `keycloak-hrm` trên `localhost:8080`

---

## 1. Cấu hình xác thực

```json
// Web.Backend/appsettings.json
{
  "UseMockAuth": false,
  "Authentication": {
    "Audience": "account",
    "Issuer": "http://localhost:8080/realms/hrm",
    "MetadataUrl": "http://localhost:8080/realms/hrm/.well-known/openid-configuration",
    "RequireHttpsMetadata": false
  },
  "Keycloak": {
    "TokenUrl": "http://localhost:8080/realms/hrm/protocol/openid-connect/token",
    "AuthClientId": "hrm-web",
    "AuthClientSecret": "s22WAn7hsBZ3zyIV7W38AGLx6nlDQL2N"
  }
}
```

- `UseMockAuth: false` — runtime đi qua Keycloak thật, nhánh mock không chạy.
- Keycloak well-known endpoint: **HTTP 200 OK** (đã kiểm tra trước khi khởi chạy ứng dụng).

---

## 2. Kết quả xác thực

### 2.1 Sai password — bị Keycloak từ chối

| Trường | Giá trị |
|--------|---------|
| Username | `admin` |
| Password | `SaiPassword@123` |
| Kết quả | ❌ Đăng nhập thất bại |
| Thông báo | "Incorrect Username or Password" |

> Đây là bằng chứng Keycloak thật đang hoạt động. Nếu mock auth đang bật, bất kỳ password nào cũng sẽ thành công vì mock không kiểm tra password.

### 2.2 Đúng password — đăng nhập thành công

| Trường | Giá trị |
|--------|---------|
| Username | `admin` |
| Password | `Admin@123456` |
| Kết quả | ✅ Đăng nhập thành công |
| Redirect | Dashboard (`/dashboard`) |

---

## 3. Kết quả CRUD Department

| Chức năng | Dữ liệu test | Kết quả | Chi tiết |
|-----------|--------------|---------|----------|
| **Create** | Code: `FIN_DEPT`, Name: `Finance Department`, Desc: `Finance Dept for UAT` | ✅ Thành công | Modal đóng, dòng mới xuất hiện trong bảng |
| **Edit** | Đổi Name → `Finance Department Edited` | ✅ Thành công | Bảng cập nhật tên mới |
| **Delete** | Xóa `FIN_DEPT`, nhấn Đồng ý | ✅ Thành công | Dòng biến mất khỏi bảng |
| **List** | Truy cập `/department` | ✅ Thành công | Bảng hiển thị danh sách phòng ban |

---

## 4. Route đã test

| Route | Kết quả |
|-------|---------|
| `/auth/login-screen` | ✅ Trang login hiển thị |
| `/dashboard` | ✅ Dashboard hiển thị sau đăng nhập |
| `/department` | ✅ Danh sách phòng ban hiển thị |

> Lưu ý: Các route nghiệp vụ cũ (VD: `/booking`, `/voucher`, `/member`) chưa được test trong đợt này.

---

## 5. Danh sách file auth liên quan

| # | File | Có sửa so với LUC gốc? | Nội dung thay đổi | Trạng thái hiện tại |
|---|------|------------------------|-------------------|---------------------|
| 1 | `Web.Backend/appsettings.json` | ✅ Có | Thêm cờ `"UseMockAuth": false` | An toàn — cờ đã tắt |
| 2 | `Infrastructure/Authentication/JwtService.cs` | ✅ Có | Thêm nhánh `if (useMockAuth)` trong `GetAccessTokenAsync` (dòng 47-86) và `GetAccessAndRefreshTokenAsync` (dòng 124-137). Nhánh mock **không kiểm tra password**. | Mock code tồn tại nhưng **không chạy** khi `UseMockAuth: false` |
| 3 | `Infrastructure/Authentication/JwtBearerOptionsSetup.cs` | ✅ Có | Thêm nhánh `if (useMockAuth)` (dòng 24-38) để set `TokenValidationParameters` tĩnh với HMAC key hardcode, `ValidateLifetime: false`. | Mock code tồn tại nhưng **không chạy** khi `UseMockAuth: false` |
| 4 | `Infrastructure/Authentication/UserContext.cs` | ❌ Không | Không thay đổi — đọc claim `ClaimTypes.NameIdentifier` từ JWT, hoạt động giống nhau cho cả Keycloak và mock. | Nguyên bản |

---

## 6. Phân tích rủi ro bảo mật

### 6.1 GitNexus Static Impact

| Symbol | Risk Level | Direct Callers |
|--------|-----------|---------------|
| `JwtService` (class) | LOW | 0 (inject qua interface) |
| `IJwtService` (interface) | LOW | 1 (JwtService) |
| `JwtBearerOptionsSetup` | LOW | 0 (đăng ký qua ConfigureOptions) |

### 6.2 Security Runtime Risk — nếu `UseMockAuth` bật lại

> **⚠️ SECURITY RISK: HIGH**

| Tiêu chí | Mock Auth ON | Keycloak thật (hiện tại) |
|----------|-------------|------------------------|
| Kiểm tra password | ❌ Không — bất kỳ password nào cũng pass | ✅ Keycloak kiểm tra |
| Token signing | HMAC key hardcode trong source | ✅ RS256 từ Keycloak |
| Token lifetime | ❌ `ValidateLifetime: false` — token không hết hạn | ✅ Keycloak enforce expiry |
| Rủi ro production | 🔴 CRITICAL — bypass xác thực hoàn toàn | ✅ An toàn |

### 6.3 Phân biệt quan trọng

- **GitNexus static impact LOW** nghĩa là: ít caller trực tiếp, blast radius code nhỏ.
- **Security runtime risk HIGH** nghĩa là: nếu cờ `UseMockAuth` vô tình bật `true` trên production, toàn bộ hệ thống bị bypass xác thực.
- Hai loại risk này **khác bản chất** — không nên nhầm lẫn.

### 6.4 Khuyến nghị dọn dẹp

- **Hiện tại**: Mock code còn tồn tại nhưng không chạy. Chấp nhận được cho giai đoạn phát triển.
- **Phase Cleanup**: Xóa toàn bộ nhánh `if (useMockAuth)`, xóa cờ `UseMockAuth`, xóa HMAC key hardcode.
- **Trước khi deploy**: Bắt buộc xác nhận `UseMockAuth: false` hoặc đã xóa mock code.

---

## 7. Kết luận

Phase 2A.1 Department CRUD đã hoàn tất với xác thực Keycloak thật:
- ✅ `UseMockAuth: false` — Keycloak thật đang hoạt động
- ✅ Sai password bị từ chối — chứng minh không phải mock
- ✅ Đúng password đăng nhập thành công
- ✅ Department Create / Edit / Delete — tất cả pass
- ⚠️ Mock code còn tồn tại nhưng không chạy — cần dọn dẹp trong Phase Cleanup
- ⚠️ Security risk của mock là HIGH nếu bật lại — không bao giờ bật trên production

**Điều kiện tiên quyết để bắt đầu Phase 2A.2 (Employee)**: Báo cáo này đã được xác nhận.
