# BÁO CÁO KẾT QUẢ XỬ LÝ TRÙNG LẶP QUYỀN POSITION (ACTUAL CLEANUP REPORT)

- **Thời gian lập**: 2026-07-20 15:45
- **Tác giả**: Senior .NET Fullstack Engineer & Database Architect
- **Trạng thái**: Đã thực thi và COMMIT thành công trên cơ sở dữ liệu `hrm_baseline_db`

---

## 1. DỮ LIỆU CHỨNG MINH TRÙNG LẶP (EVIDENCE)
Kết quả truy vấn trực tiếp từ cơ sở dữ liệu `hrm_baseline_db` trước khi xử lý ghi nhận các bản ghi trùng lặp sau:

### A. Danh sách các permission bị trùng lặp trong bảng `permission`
| ID | Resource Name | Display Name | Is Default | Created Date (Local) | Vai Trò |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **`9a8b7c6d-5e4d-3c2b-1a0f-9e8d7c6b5a4f`** | `VIEW_POSITION` | View Position | True | 2026-06-30 15:15:23 | **Official (Giữ lại)** |
| `cf0b0ef2-ef1e-4501-8b9a-4c28470aefc1` | `VIEW_POSITION` | View Position | False | 2026-06-30 13:52:10 | **Duplicate (Xóa)** |
| **`8a7b6c5d-4e3d-2b1a-0f9e-8d7c6b5a4f3e`** | `UPDATE_POSITION` | Update Position | True | 2026-06-30 15:15:23 | **Official (Giữ lại)** |
| `cf0b0ef2-ef1e-4501-8b9a-4c28470aefc2` | `UPDATE_POSITION` | Update Position | False | 2026-06-30 13:52:10 | **Duplicate (Xóa)** |

### B. Ánh xạ vai trò trước cleanup trong bảng `role_to_permission`
- **Role ADMIN** (`11111111-1111-1111-1111-111111111111`):
    - Đang liên kết thừa với duplicate ID `cf0b0ef2-ef1e-4501-8b9a-4c28470aefc1` (`VIEW_POSITION`).
    - Đang liên kết chính thức với official ID `9a8b7c6d-5e4d-3c2b-1a0f-9e8d7c6b5a4f` (`VIEW_POSITION`).
    - Đang liên kết thừa với duplicate ID `cf0b0ef2-ef1e-4501-8b9a-4c28470aefc2` (`UPDATE_POSITION`).
    - Đang liên kết chính thức với official ID `8a7b6c5d-4e3d-2b1a-0f9e-8d7c6b5a4f3e` (`UPDATE_POSITION`).
- **Role UAT** (`40533dcd-8720-492d-9e18-7017b7ae23a4`):
    - Đang liên kết thừa với duplicate ID `cf0b0ef2-ef1e-4501-8b9a-4c28470aefc1` (`VIEW_POSITION`).
    - Đang liên kết chính thức với official ID `9a8b7c6d-5e4d-3c2b-1a0f-9e8d7c6b5a4f` (`VIEW_POSITION`).

---

## 2. KẾT QUẢ SAU CLEANUP (ACTUAL RESULT)
Sau khi thực thi script SQL và COMMIT thành công, hệ thống đã đạt trạng thái sạch:
- **Bảng `permission`**:
    - Chỉ còn đúng **2 bản ghi** liên quan đến Position (các GUID Official). Các GUID Duplicate đã bị xóa hoàn toàn khỏi DB.
- **Bảng `role_to_permission`**:
    - Các liên kết thừa trỏ đến các duplicate ID (`cf0b...`) đã bị xóa bỏ.
    - **ADMIN** vẫn giữ nguyên quyền `VIEW_POSITION` và `UPDATE_POSITION` (trỏ về Official IDs).
    - **UAT** vẫn giữ nguyên quyền `VIEW_POSITION` (trỏ về Official ID).

---

## 3. PHÂN TÍCH NGUỒN GỐC TRÙNG LẶP (ROOT CAUSE VERIFICATION)
- **Xác nhận trùng lặp**: **DB duplicate confirmed**. Sự tồn tại của các bản ghi trùng lặp trong DB đã được chứng minh và làm sạch.
- **Nguồn gốc GUID `cf0b...`**: **Origin not fully proven**. Rà soát toàn bộ dự án khẳng định không có mã nguồn (.cs, .py, .sql) nào định nghĩa hay sinh các GUID này. Khả năng cao chúng phát sinh từ các đợt seed thử nghiệm thủ công ở các phiên làm việc rất cũ trước đây.

---

## 4. SQL CLEANUP ĐÃ THỰC THI (CÓ FAIL-FAST GUARD VÀ COMMIT)
Tệp SQL thực tế đã chạy được lưu tại [2026-07-20_1545_cleanup_duplicate_permissions_proposal.sql](file:///d:/Customer_Management_System-Cao_Thanh_Huy_01212407665/MD_memory/debug/2026-07-20_1545_cleanup_duplicate_permissions_proposal.sql):

```sql
-- Bắt đầu giao dịch an toàn
BEGIN;

-- ============================================================================
-- 1. GUARD FAIL-FAST TRƯỚC CLEANUP (Nếu guard fail thì không chạy cleanup)
-- ============================================================================
SELECT '--- 1. RUNNING PRE-CHECK GUARD ---' AS step;

DO $$
DECLARE
    official_count integer;
    duplicate_count integer;
    view_count integer;
    update_count integer;
BEGIN
    SELECT COUNT(*) INTO official_count
    FROM permission
    WHERE id IN (
        '9a8b7c6d-5e4d-3c2b-1a0f-9e8d7c6b5a4f',
        '8a7b6c5d-4e3d-2b1a-0f9e-8d7c6b5a4f3e'
    );

    SELECT COUNT(*) INTO duplicate_count
    FROM permission
    WHERE id IN (
        'cf0b0ef2-ef1e-4501-8b9a-4c28470aefc1',
        'cf0b0ef2-ef1e-4501-8b9a-4c28470aefc2'
    );

    SELECT COUNT(*) INTO view_count
    FROM permission
    WHERE resource_name = 'VIEW_POSITION';

    SELECT COUNT(*) INTO update_count
    FROM permission
    WHERE resource_name = 'UPDATE_POSITION';

    IF official_count <> 2 THEN
        RAISE EXCEPTION 'Abort cleanup: official permission IDs count expected 2, got %', official_count;
    END IF;

    IF duplicate_count <> 2 THEN
        RAISE EXCEPTION 'Abort cleanup: duplicate permission IDs count expected 2, got %', duplicate_count;
    END IF;

    IF view_count <> 2 THEN
        RAISE EXCEPTION 'Abort cleanup: VIEW_POSITION count expected 2, got %', view_count;
    END IF;

    IF update_count <> 2 THEN
        RAISE EXCEPTION 'Abort cleanup: UPDATE_POSITION count expected 2, got %', update_count;
    END IF;
END $$;

-- ============================================================================
-- 2. THỰC HIỆN CLEANUP
-- ============================================================================
SELECT '--- 2. EXECUTE CLEANUP ---' AS step;

-- A. Remap VIEW_POSITION từ duplicate sang official nếu vai trò đó chưa liên kết với official
UPDATE role_to_permission rp
SET permission_id = '9a8b7c6d-5e4d-3c2b-1a0f-9e8d7c6b5a4f'
WHERE rp.permission_id = 'cf0b0ef2-ef1e-4501-8b9a-4c28470aefc1'
  AND NOT EXISTS (
      SELECT 1 
      FROM role_to_permission sub 
      WHERE sub.role_id = rp.role_id 
        AND sub.permission_id = '9a8b7c6d-5e4d-3c2b-1a0f-9e8d7c6b5a4f'
  );

-- B. Remap UPDATE_POSITION từ duplicate sang official nếu vai trò đó chưa liên kết với official
UPDATE role_to_permission rp
SET permission_id = '8a7b6c5d-4e3d-2b1a-0f9e-8d7c6b5a4f3e'
WHERE rp.permission_id = 'cf0b0ef2-ef1e-4501-8b9a-4c28470aefc2'
  AND NOT EXISTS (
      SELECT 1 
      FROM role_to_permission sub 
      WHERE sub.role_id = rp.role_id 
        AND sub.permission_id = '8a7b6c5d-4e3d-2b1a-0f9e-8d7c6b5a4f3e'
  );

-- C. Xóa các liên kết thừa trỏ tới các duplicate ID trong bảng role_to_permission
DELETE FROM role_to_permission
WHERE permission_id IN ('cf0b0ef2-ef1e-4501-8b9a-4c28470aefc1', 'cf0b0ef2-ef1e-4501-8b9a-4c28470aefc2');

-- D. Xóa các bản ghi duplicate permission trong bảng permission
DELETE FROM permission
WHERE id IN ('cf0b0ef2-ef1e-4501-8b9a-4c28470aefc1', 'cf0b0ef2-ef1e-4501-8b9a-4c28470aefc2');

-- ============================================================================
-- 3. VERIFY SAU CLEANUP TRONG TRANSACTION
-- ============================================================================
SELECT '--- 3. VERIFY SAU CLEANUP ---' AS step;

-- A. Kiểm tra bảng permission chỉ còn official IDs (Kỳ vọng: Đúng 2 dòng)
SELECT id, resource_name, display_name, is_default, created_date 
FROM permission 
WHERE resource_name IN ('VIEW_POSITION', 'UPDATE_POSITION')
ORDER BY resource_name;

-- B. Kiểm tra bảng role_to_permission không còn các duplicate permission IDs (Kỳ vọng: Trả về rỗng)
SELECT id, role_id, permission_id 
FROM role_to_permission
WHERE permission_id IN ('cf0b0ef2-ef1e-4501-8b9a-4c28470aefc1', 'cf0b0ef2-ef1e-4501-8b9a-4c28470aefc2');

-- C. Kiểm tra quyền của ADMIN và UAT đối với các Official IDs (Kỳ vọng: ADMIN có cả VIEW và UPDATE, UAT có VIEW)
SELECT r.resource_name AS role_name, p.resource_name AS permission_name, p.id AS permission_id
FROM role_to_permission rp
JOIN role r ON rp.role_id = r.id
JOIN permission p ON rp.permission_id = p.id
WHERE r.resource_name IN ('ADMIN', 'UAT')
  AND p.resource_name IN ('VIEW_POSITION', 'UPDATE_POSITION')
ORDER BY r.resource_name, p.resource_name;

-- Thực thi lưu thay đổi vĩnh viễn vào DB
COMMIT;
```

### Kết quả chạy thật (Actual Execution Output)
- **Pre-check guard**: Hoạt động bình thường và vượt qua (Official count = 2, Duplicate count = 2, VIEW count = 2, UPDATE count = 2).
- **Remap & Delete mappings**: Xóa thành công 3 bản ghi thừa trong `role_to_permission`.
- **Delete permissions**: Xóa thành công 2 bản ghi trùng lặp trong `permission`.
- **Verify**: Bảng `permission` chỉ còn lại 2 dòng Official IDs chính xác. Mọi truy cập phân quyền của vai trò ADMIN/UAT hoạt động đúng chuẩn.
- **Transaction**: COMMIT thành công.
