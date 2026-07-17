# Báo cáo Phân tích Nguyên nhân Gốc rễ (RCA) - Lỗi Cuộn Sidebar Layout HRM
**Thời gian:** 2026-07-15 04:00  
**Phase:** UAT / Refactor Global Layout  
**Người thực hiện:** Senior .NET Fullstack Engineer & Technical Reviewer  

---

## 1. Mô tả Hiện tượng (Issue Description)
Trong quá trình UAT hệ thống HRM Portal với giao diện chuẩn **Swiss International HR**, người dùng phát hiện lỗi nghiêm trọng liên quan đến khả năng tương tác của Sidebar (Thanh điều hướng bên trái):
- **Hiện tượng:** Khi danh sách menu động phình to (bao gồm nhiều cấu phần từ General, HRM Management, Leave Management đến System), sidebar bị cắt cụt nội dung ở phía dưới.
- **Hậu quả:** Người dùng hoàn toàn không thể cuộn (scroll) sidebar lên/xuống để tiếp cận và nhấn vào các mục menu nằm ở dưới cùng (như `Users`, `Roles & Permissions`, hoặc nút logout/user profile) trên cả màn hình Desktop độ phân giải trung bình/thấp và màn hình Mobile.

---

## 2. Bằng chứng Kỹ thuật & Hiện trạng Layout (Physical Evidence)
Qua phân tích tĩnh file layout chính `HRM_Leave_Management/Web.Backend/Views/Shared/_Layout.cshtml`, các đoạn mã chịu trách nhiệm chính gây ra lỗi này bao gồm:

### A. Khóa Cuộn Toàn Trang (Global Scroll Lock)
Tại dòng 82 của `_Layout.cshtml`:
```html
<body class="font-body-md h-screen flex flex-row bg-swiss-light overflow-hidden">
```
- **Phân tích:** Lớp `overflow-hidden` và `h-screen` (100vh) khóa cứng khả năng cuộn của toàn bộ trang web. Đây là thiết kế dạng **Single-Pane** (App Shell cố định, các vùng con tự quản lý cuộn). Do đó, việc cuộn sidebar phụ thuộc hoàn toàn vào hành vi scroll nội bộ của container sidebar.

### B. Cấu hình Cấm Cuộn trên Sidebar Container
Tại dòng 34-39 trong thẻ `<style>`:
```css
#sidebarMenu {
    width: 320px;
    min-width: 320px;
    max-width: 320px;
    overflow: hidden; /* Cấm cuộn thô trên sidebar */
}
```
Và tại dòng 43-54 (thiết lập cho Desktop màn hình `>= 1024px`):
```css
@@media (min-width: 1024px) {
    #sidebarMenu {
        position: relative !important;
        top: 0 !important;
        height: 100vh !important; /* Khóa cứng chiều cao bằng viewport */
        transform: none !important;
        display: flex !important;
        overflow: hidden !important; /* Ghi đè cứng cấm cuộn */
    }
    #sidebarMenu nav {
        overflow: visible !important; /* Ép menu con hiển thị tràn, không tự cuộn */
    }
}
```

---

## 3. Phân tích Nguyên nhân Gốc rễ (Root Cause Analysis)

### Xung đột Kích thước (Viewport Height vs. Dynamic Menu Height)
1. **Chiều cao Menu thực tế:** Danh sách menu động được tạo ra từ script có tổng cộng 13 dòng (bao gồm các thẻ phân mục `GENERAL`, `HRM MANAGEMENT`, `LEAVE MANAGEMENT`, `SYSTEM` và các menu con). Khi render ra HTML, cộng thêm khối Logo phía trên (khoảng 80px) và khối User Profile phía dưới (khoảng 70px), chiều cao thực tế của Sidebar thường vượt quá **800px**.
2. **Cơ chế Cắt cụt (Clipping):** 
   - Trên các màn hình máy tính thông thường (ví dụ chiều cao màn hình 768px, 900px) hoặc khi thu nhỏ cửa sổ trình duyệt, chiều cao viewport khả dụng nhỏ hơn chiều cao thực tế của menu.
   - Do `#sidebarMenu` có `height: 100vh !important` và `overflow: hidden !important`, trình duyệt bắt buộc phải cắt bỏ (clip) toàn bộ phần nội dung vượt quá chiều cao màn hình.
   - Do thẻ `<nav>` chứa danh sách menu bị áp thuộc tính `overflow: visible !important`, nó không tự sinh ra thanh cuộn nội bộ cho chính nó. Sự kết hợp này triệt tiêu hoàn toàn khả năng scroll của sidebar, tạo thành một khối tĩnh bị cắt cụt.

---

## 4. Đề xuất Giải pháp Khắc phục (Technical Proposals)

Để giải quyết triệt để lỗi này mà vẫn giữ nguyên được thẩm mỹ tối giản, sạch sẽ của hệ thống (**Enterprise Calm / Swiss International**), chúng tôi đề xuất phương án tối ưu sau:

### Phương án: Cuộn Độc lập Phân khu Menu Giữa (Khuyên dùng)
*Giữ cố định Logo ở đầu và User Profile ở chân Sidebar, chỉ cho phép phần danh sách menu `<nav>` ở giữa cuộn tự động khi quá dài.*

#### Bước 1: Điều chỉnh CSS trong thẻ `<style>` của `_Layout.cshtml`
Thay đổi định nghĩa style cho `nav` trên Desktop (dòng 52-54) thành:
```css
        @@media (min-width: 1024px) {
            /* ... các class khác giữ nguyên ... */
            #sidebarMenu nav {
                overflow-y: auto !important; /* Cho phép cuộn dọc nội bộ */
                scrollbar-width: none;       /* Ẩn thanh cuộn thô trên Firefox */
            }
            #sidebarMenu nav::-webkit-scrollbar {
                display: none;               /* Ẩn thanh cuộn thô trên Chrome/Safari */
            }
        }
```

#### Bước 2: Bổ sung các class Tailwind điều phối Flexbox cho các khối con của Sidebar
Cấu trúc HTML của `<aside>` cần điều chỉnh lại để các thành phần co dãn hợp lý:
```html
    <!-- Persistent Sidebar -->
    <aside id="sidebarMenu" class="hidden lg:flex fixed lg:relative inset-y-0 left-0 z-50 w-[320px] h-screen bg-swiss-light border-r border-swiss-border flex-col flex-shrink-0 transform -translate-x-full lg:translate-x-0 transition-transform duration-200 ease-in-out">
        <!-- Khối Logo (Cố định ở trên nhờ flex-shrink-0) -->
        <div class="px-6 pt-8 pb-4 flex-shrink-0">
            <div class="inline-block pb-1 swiss-underline">
                <h1 class="font-bold text-black border-l-4 border-[#bb0015] pl-4 text-2xl tracking-tighter" style="font-family: 'Geist', sans-serif;">HRM PORTAL</h1>
            </div>
            <p class="mt-2 text-[10px] tracking-widest uppercase text-[#4c4546] opacity-60 font-semibold font-mono">Institutional Authority</p>
        </div>
        <!-- Khối Điều Hướng Menu (Thêm flex-1, min-h-0, overflow-y-auto để cuộn độc lập) -->
        <nav class="flex-1 mt-6 overflow-y-auto min-h-0">
            <div class="flex flex-col" id="menuList">
            </div>
        </nav>
        <!-- Khối User Profile (Cố định ở dưới nhờ flex-shrink-0) -->
        <div class="p-6 border-t border-swiss-border flex-shrink-0">
            <div class="flex items-center gap-3">
                <div class="w-8 h-8 bg-[#e2e2e2] border border-swiss-border flex items-center justify-center">
                    <span class="material-symbols-outlined text-black">account_circle</span>
                </div>
                <div>
                    <p class="text-xs font-bold text-black uppercase font-mono headerUsername">...</p>
                    <p class="mono-text text-[9px] text-[#4c4546] uppercase">LEVEL: 00 (ROOT)</p>
                </div>
            </div>
        </div>
    </aside>
```
* **Ý nghĩa:** Việc thêm `flex-shrink-0` vào Khối Logo và Khối User Profile đảm bảo hai khối này luôn giữ đúng kích thước thiết kế, không bị co bóp. Thẻ `<nav>` với `flex-1 min-h-0 overflow-y-auto` sẽ tự động chiếm toàn bộ không gian còn lại ở giữa và tự sinh thanh cuộn khi danh sách menu dài vượt mức.

---

## 5. Kế hoạch Kiểm thử Thủ công (Manual UAT Steps)

Sau khi Codex hoặc User áp dụng sửa đổi theo phương án trên, hãy tiến hành UAT theo các bước sau:

### Kịch bản 1: Kiểm thử trên Desktop (Màn hình máy tính)
1. **Chuẩn bị:** Mở trình duyệt và đăng nhập tài khoản có quyền xem đầy đủ chức năng (ví dụ `admin`).
2. **Thao tác:**
   - Thu nhỏ chiều cao của cửa sổ trình duyệt xuống khoảng `600px` (để mô phỏng màn hình nhỏ).
   - Di chuột vào khu vực Sidebar bên trái.
   - Lăn nút cuộn chuột (scroll wheel) xuống dưới.
3. **Kết quả mong đợi:**
   - Khối tiêu đề `HRM PORTAL` và logo đứng yên.
   - Vùng chứa các menu item (`Dashboard`, `Departments`,..., `Roles & Permissions`) cuộn mượt mà lên xuống.
   - Khối User Profile ở dưới cùng vẫn hiển thị cố định ở chân trang mà không bị cuộn mất hoặc đè lên menu.
   - Không xuất hiện thanh cuộn ngang/dọc thô kệch (đã được ẩn bằng CSS).

### Kịch bản 2: Kiểm thử trên Mobile
1. **Thao tác:**
   - Chuyển sang kích thước thiết bị Mobile (hoặc dùng điện thoại thật).
   - Nhấn nút Hamburger menu ở góc trên bên trái để trượt Sidebar ra.
   - Dùng ngón tay vuốt dọc trên khu vực Sidebar.
2. **Kết quả mong đợi:**
   - Sidebar trượt ra trơn tru.
   - Menu cuộn dọc được bình thường, không bị kẹt cứng hay bị che khuất các mục dưới cùng.
   - Việc vuốt cuộn trên sidebar không làm cuộn phần nội dung chính (`<main>`) đang bị ẩn phía sau.
