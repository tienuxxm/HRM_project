# RCA: Work Calendar Import Excel → NotFound
> **Thời gian:** 2026-07-21 08:55
> **Phase:** phase-design-work-calendar-runtime (RCA độc lập)
> **Trạng thái:** 🔴 BUG XÁC NHẬN — Chưa patch, chờ User duyệt

---

## 1. Root Cause Chính

**Preview.cshtml form `asp-action` trỏ sai tên action.**

| Thành phần | Giá trị hiện tại | Giá trị đúng |
|---|---|---|
| Preview.cshtml L55 | `asp-action="ConfirmImport"` | `asp-action="Confirm"` |
| Controller L103-104 | `[HttpPost("confirm")] Confirm(...)` | — (đây là source of truth) |

### Cơ chế lỗi
1. User nhấn **"Apply Import"** trên trang Preview.
2. ASP.NET Tag Helper `asp-action="ConfirmImport"` sinh ra URL: `POST /work-calendar/ConfirmImport`.
3. Controller **không có** action tên `ConfirmImport` — chỉ có `Confirm` với route `[HttpPost("confirm")]`.
4. ASP.NET routing không match → trả **404 NotFound**.

---

## 2. Evidence Tĩnh (Code)

### Evidence A — Preview.cshtml (L55)
```html
<form asp-action="ConfirmImport" method="post" id="confirmImportForm">
```
→ Tag Helper sẽ sinh URL `/work-calendar/ConfirmImport` (convention-based, không có route attribute match).

### Evidence B — WorkCalendarController.cs (L103-104)
```csharp
[HttpPost("confirm")]
public async Task<IActionResult> Confirm([FromForm] Guid batchId, CancellationToken cancellationToken)
```
→ Route thật là `POST /work-calendar/confirm`. Method name là `Confirm`, không phải `ConfirmImport`.

### Evidence C — Confirm trả JSON, không redirect
```csharp
// L116-119
return Json(new { success = false, message = result.Error.Name });
// ...
return Json(new { success = true });
```
→ Ngay cả khi fix `asp-action`, form submit sẽ nhận **JSON response** trong browser thay vì redirect sang Summary page. Đây là **bug thứ 2**: form POST truyền thống không có JS handler để xử lý JSON response.

### Evidence D — Summary route tồn tại nhưng chưa được gọi
```csharp
// L122-123
[HttpGet("summary/{batchId:guid}")]
public async Task<IActionResult> Summary([FromRoute] Guid batchId, ...)
```
→ Route `/work-calendar/summary/{batchId}` tồn tại và trả View. Nhưng **không có code nào redirect** từ Confirm sang Summary — action Confirm chỉ trả JSON.

---

## 3. Chuỗi Bug (2 lỗi liên tiếp)

| # | Bug | Hậu quả |
|---|---|---|
| **Bug 1** | `asp-action="ConfirmImport"` — action name sai | Form POST → 404 NotFound |
| **Bug 2** | Confirm action trả JSON, form submit không có JS handler | Nếu fix Bug 1, user sẽ thấy raw JSON `{"success":true}` thay vì redirect sang Summary |

---

## 4. Đề xuất Patch Tối thiểu

### Option A: Sửa chỉ Preview.cshtml (ưu tiên — không chạm C#)

Thay form submit truyền thống bằng AJAX + JS redirect, giống pattern đã dùng cho Upload Excel trong Index.cshtml:

```diff
- <form asp-action="ConfirmImport" method="post" id="confirmImportForm">
-     <input type="hidden" name="batchId" value="@Model.BatchId" />
-     ...
-     <button type="submit" id="confirmImportBtn" ...>Apply Import</button>
- </form>
+ <form id="confirmImportForm">
+     <input type="hidden" id="confirmBatchId" value="@Model.BatchId" />
+     ...
+     <button type="button" onclick="confirmImport()" id="confirmImportBtn" ...>Apply Import</button>
+ </form>
```

Thêm JS ở cuối file:
```javascript
function confirmImport() {
    const batchId = document.getElementById('confirmBatchId').value;
    const btn = document.getElementById('confirmImportBtn');
    btn.disabled = true;
    btn.textContent = 'Processing...';

    $.ajax({
        url: '/work-calendar/confirm',
        type: 'POST',
        contentType: 'application/x-www-form-urlencoded',
        data: { batchId: batchId },
        success: function(response) {
            if (response.success) {
                window.location.href = '/work-calendar/summary/' + batchId;
            } else {
                btn.disabled = false;
                btn.textContent = 'Apply Import';
                alert('Error: ' + (response.message || 'Import failed.'));
            }
        },
        error: function() {
            btn.disabled = false;
            btn.textContent = 'Apply Import';
            alert('An error occurred during import confirmation.');
        }
    });
}
```

**Ưu điểm:** Không chạm Controller/C#. Chỉ sửa 1 file `.cshtml`. Đồng bộ pattern AJAX đã dùng cho Upload.

### Option B: Sửa Controller (nếu muốn giữ form submit truyền thống)

Đổi action `Confirm` trả `RedirectToAction("Summary", new { batchId })` thay vì JSON. **Nhưng điều này phá vỡ scope** vì phải sửa C# — không khuyến nghị cho phase này.

---

## 5. File Excel có phải nguyên nhân không?

**KHÔNG.** File Excel format không liên quan đến lỗi NotFound:
- Upload thành công → AJAX trả `{ success: true, batchId: "..." }` → redirect sang Preview page.
- Preview page hiển thị validation table bình thường (nếu file có lỗi, sẽ thấy dòng đỏ Invalid).
- Lỗi 404 xảy ra **sau** khi nhấn "Apply Import", tức là ở bước form submit, hoàn toàn độc lập với nội dung file Excel.

---

## 6. Tóm tắt

| Câu hỏi RCA | Trả lời |
|---|---|
| Preview.cshtml `asp-action` là gì? | `ConfirmImport` (SAI) |
| Controller POST thật là route nào? | `[HttpPost("confirm")]` method `Confirm` |
| Có action `ConfirmImport` không? | **KHÔNG** — không tồn tại |
| Confirm trả JSON hay redirect? | **JSON** `{ success: true/false }` |
| Summary route được gọi ở đâu? | **Chưa có code nào gọi** — route tồn tại nhưng chưa wired |

**Kết luận:** Cần sửa **chỉ Preview.cshtml** — đổi form submit sang AJAX POST `/work-calendar/confirm` + JS redirect sang `/work-calendar/summary/{batchId}`.

---

*Chờ User duyệt trước khi patch.*
