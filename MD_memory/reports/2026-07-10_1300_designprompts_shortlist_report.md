﻿# BÁO CÁO ĐÁNH GIÁ RÚT GỌN DESIGN PROMPTS - HRM LEAVE MANAGEMENT UI
*Ngày báo cáo: 2026-07-10*
*Trạng thái: Chờ duyệt phong cách thiết kế từ User*
*Tác giả: Senior .NET Fullstack Engineer & Technical Reviewer*

Báo cáo này cô đọng kết quả audit phong cách thiết kế từ `designprompts.dev`, tiến hành đánh giá chi tiết và xây dựng danh sách rút gọn (**Shortlist 4 Styles**) được tối ưu hóa cho bảng màu **Slate Cyan Data Console** để áp dụng vào màn hình đại diện `Leave Request List`.

---

## I. TỔNG QUAN HƯỚNG TIẾP CẬN VÀ BẢO VỆ CHẤT LƯỢNG (ANTI-SLOP GUARD)

> [!IMPORTANT]
> **Vai trò của Taste-Skill trong dự án HRM:**
> Chúng tôi khẳng định **taste-skill không phải là công cụ chính tạo layout tự động** cho các bảng dữ liệu hay dashboard quản trị phức tạp. Thay vào đó, nó đóng vai trò là một **Anti-Slop Guard** - bộ tiêu chuẩn kiểm soát chất lượng frontend nhằm:
> 1. Ngăn chặn việc sử dụng các thuộc tính CSS mặc định thô kệch hoặc các thư viện ngoài luồng.
> 2. Đảm bảo tỷ lệ padding/margin chặt chẽ, chống chồng chéo phần tử (overlap) trên giao diện desktop/mobile.
> 3. Kiểm soát độ tương phản (accessibility) đạt chuẩn WCAG AA đối với các bảng dữ liệu lớn.
> 4. Loại bỏ các hiệu ứng bóng đổ rẻ tiền, bo góc cẩu thả hoặc các gradient lòe loẹt làm giảm trải nghiệm người dùng.

---

## II. BẢNG DANH SÁCH RÚT GỌN (SHORTLIST) & PHẠM VI ÁP DỤNG

Dưới đây là bảng phân loại và đánh giá phạm vi áp dụng của 4 phong cách thiết kế rút gọn dành cho hệ thống HRM:

| Tên Style Đề Xuất | Gốc Từ DesignPrompts | Use as full app? | Use as component treatment? | Do not use? | Lý do kỹ thuật & Phạm vi áp dụng |
|---|---|:---:|:---:|:---:|---|
| **Corporate Slate-Cyan** | `Corporate Trust` (/enterprise) | **YES** (Ứng viên chính) | **YES** (Sidebar, layouts, cards) | **NO** | Layout thẻ có chiều sâu mịn, cấu trúc phân tầng rõ ràng. Phải loại bỏ hoàn toàn các ứng dụng gradient chói, background blobs, và 3D card rotation (isometric) để giữ tính chuyên nghiệp, phẳng. |
| **Minimalist Tech-Cyan** | `Minimalist Modern` (/saas) | **NO** (Quá loãng) | **YES** (Spacing, typography rules) | **NO** | Chỉ kế thừa hệ thống spacing rộng rãi, thoáng đãng và phân vùng dữ liệu. Tuyệt đối không dùng font tiêu đề Calistoga (serif) hay electric-blue marketing mood chói mắt của SaaS gốc. |
| **Structured Grid Swiss** | `Swiss International` (/swiss-minimalist) | **NO** (Gây căng mắt) | **YES** (Table & Data Grid treatment) | **NO** | Sử dụng lưới hình học chặt chẽ và viền Slate mỏng 1px để hiển thị các bảng dữ liệu Leave Request phức tạp. Không áp dụng cho toàn bộ app để tránh thô cứng và mỏi mắt do góc nhọn 0px và tương phản cao. |
| **Console Flat** | `Flat Design` (Synthesized) | **NO** (Dễ bị phẳng nhạt) | **YES** (Zebra row striping, inputs flat) | **NO** | Phương án an toàn kỹ thuật cho Razor views. Chỉ sử dụng cho việc phẳng hóa các ô nhập liệu và phân dòng bảng xen kẽ. Bắt buộc phải thiết lập hierarchy chữ (Geist/Mono) rõ ràng để tránh UI phẳng nhạt, thiếu affordance. |

---

## III. CHI TIẾT CÁC PHONG CÁCH & BẰNG CHỨNG NGUỒN (PROVENANCE)

### 1. Style 01: Corporate Slate-Cyan (Phái sinh từ Corporate Trust)
*   **Provenance (Bằng chứng nguồn):**
    *   *URL/Slug:* [designprompts.dev/enterprise](https://www.designprompts.dev/enterprise)
    *   *File Evidence:* `MD_memory/debug/2026-07-10_0443_extracted_data.txt` (Dòng 954-1000) và `MD_memory/debug/2026-07-10_0442_extracted-styles.json` (Key: "Corporate Trust")
    *   *Timestamp trích xuất:* 2026-07-10T11:43:00+07:00
    *   *Raw prompt (Summary extracted from raw prompt):*
        ```markdown
        Modern enterprise SaaS aesthetic - professional yet approachable. Palette: Background #F8FAFC, Surface #FFFFFF, Border #E2E8F0. Card shadow: 0 4px 20px -2px rgba(79, 70, 229, 0.1). Bo góc 12px cho cards.
        ```
*   **Điều chỉnh áp dụng HRM:** Thay đổi màu chính Indigo `#4F46E5` sang màu Cyan `#0891B2` của Slate Cyan palette. Loại bỏ hiệu ứng xoay 3D (isometric) trên các thẻ và dọn dẹp các đốm màu nền (background blob) để giữ giao diện phẳng và tập trung.

### 2. Style 02: Minimalist Tech-Cyan (Phái sinh từ Minimalist Modern)
*   **Provenance (Bằng chứng nguồn):**
    *   *URL/Slug:* [designprompts.dev/saas](https://www.designprompts.dev/saas)
    *   *File Evidence:* `MD_memory/debug/2026-07-10_0443_extracted_data.txt` (Dòng 2270-2315) và `MD_memory/debug/2026-07-10_0442_extracted-styles.json` (Key: "Minimalist Modern")
    *   *Timestamp trích xuất:* 2026-07-10T11:43:00+07:00
    *   *Raw prompt (Summary extracted from raw prompt):*
        ```markdown
        Minimalist Modern. Background #FAFAFA, Primary #0052FF (Electric Blue). Headlines: Calistoga (Serif), Body/UI: Inter. Spacing py-24 to py-32.
        ```
*   **Điều chỉnh áp dụng HRM:** Không dùng font Calistoga cho tiêu đề (thay bằng Geist). Thay thế màu Electric Blue `#0052FF` sang màu Accent Cyan `#0891B2` dịu mắt hơn. Chỉ kế thừa hệ thống spacing thoáng đãng để người dùng không cảm thấy ngột ngạt khi làm việc lâu.

### 3. Style 03: Structured Grid Swiss (Phái sinh từ Swiss International)
*   **Provenance (Bằng chứng nguồn):**
    *   *URL/Slug:* [designprompts.dev/swiss-minimalist](https://www.designprompts.dev/swiss-minimalist)
    *   *File Evidence:* `MD_memory/debug/2026-07-10_0443_extracted_data.txt` (Dòng 2073-2115) và `MD_memory/debug/2026-07-10_0442_extracted-styles.json` (Key: "Swiss International (International Typographic Style)")
    *   *Timestamp trích xuất:* 2026-07-10T11:43:00+07:00
    *   *Raw prompt (Summary extracted from raw prompt):*
        ```markdown
        International Typographic Style. 0px border-radius, thick borders (border-2 or border-4 solid #000000), no shadows. Helvetica/Inter typography. Background #FFFFFF, Accent #FF3000 (Swiss Red).
        ```
*   **Điều chỉnh áp dụng HRM:** Chỉ dùng triết lý lưới kẻ viền mỏng 1px Slate `#E2E8F0` cho các bảng dữ liệu phức tạp (như Leave Request List) để căn hàng thẳng cột rõ ràng. Tuyệt đối không dùng viền đen dày, góc nhọn 0px trên toàn app hay màu đỏ Swiss Red gắt, tránh gây ức chế thị giác.

### 4. Style 04: Console Flat (Phái sinh từ Flat Design)
*   **Provenance (Bằng chứng nguồn):**
    *   *URL/Slug:* Tổng hợp dựa trên các thuộc tính thiết kế phẳng (Flat) từ các style trên designprompts.dev
    *   *File Evidence:* `MD_memory/debug/2026-07-10_0500_designprompts-audit.txt` (Mục 6 - Flat Design)
    *   *Timestamp trích xuất:* 2026-07-10T11:43:00+07:00
    *   *Raw prompt (Summary extracted from raw prompt):*
        ```markdown
        Flat Design. Clean white surface (#FFFFFF), borders rounded-md (6px), no box shadows (shadow-none). Focus on fast rendering and clean layouts.
        ```
*   **Điều chỉnh áp dụng HRM:** Dùng làm giải pháp an toàn cho Razor View khi tích hợp. Sử dụng màu nền xen kẽ (Zebra striping) màu Slate nhạt và thiết lập phân cấp chữ Geist / JetBrains Mono (cho dữ liệu ngày tháng/số lượng) cực kỳ rõ nét để bù đắp sự thiếu hụt của bóng đổ, giữ cho UI có chiều sâu thông tin.

---

## IV. TRẠNG THÁI ĐỒNG BỘ DESIGN SYSTEM (STITCH SYNC STATUS)

Hiện tại, cấu hình Design System đang có sự lệch pha (mất đồng bộ) giữa các thành phần cục bộ và trên Stitch canvas:

1.  **Local `.stitch/DESIGN.md`:** 
    *   *Trạng thái:* **Đã cập nhật đúng.** 
    *   *Chi tiết:* Đã định hình chính xác theme **Slate Cyan Data Console** (Accent Cyan `#0891B2`, Background Slate `#F8FAFC`, và font chữ Geist / JetBrains Mono).
2.  **Local `.stitch/metadata.json`:**
    *   *Trạng thái:* **Lệch pha (Stale).**
    *   *Chi tiết:* Cấu hình `primaryColor` tại dòng 8 vẫn đang để màu mặc định cũ của LUC project: `"primaryColor": "#4F46E5"` (màu Indigo).
3.  **Stitch Project Asset thật (trên canvas):**
    *   *Trạng thái:* **Chưa đồng bộ (Stale).**
    *   *Chi tiết:* Dự án thật trên Stitch (Project ID: `17479353588209716186`) với Design System Asset ID `919a8a410a4c4e8384f20712e26821ca` vẫn đang hiển thị tông màu tím/indigo cũ.
    *   *Nguyên nhân:* Do chúng ta đang tạm dừng quy trình generate/design screen trên Stitch theo chỉ thị của bạn để hoàn thiện giai đoạn audit này, nên các thao tác upload/apply design system chưa được thực hiện.

---

## V. CAM KẾT BẢO VỆ DỰ ÁN (GUARDRAILS)

Chúng tôi cam kết tuân thủ tuyệt đối các nguyên tắc an toàn:
1.  **Không thực hiện bất kỳ lệnh generate screen nào trên Stitch** trong bước này.
2.  **Không sửa đổi bất kỳ mã nguồn .NET** của hệ thống HRM hiện tại.
3.  **Không chạy lệnh Git add / commit / push** lên repository.
4.  **Tất cả các tài liệu đánh giá được ghi trực tiếp dưới định dạng UTF-8 BOM** để đảm bảo không bị mojibake trên môi trường Windows.
