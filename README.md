# CafeManagement

Hệ thống quản lý chuỗi quán cafe — ASP.NET Core 8 MVC + Service Layer + SQL Server.

---


## Yêu cầu môi trường

| Công cụ | Phiên bản |
|---|---|
| .NET SDK | 8.x |
| SQL Server | 2019+ (hoặc Docker) |
| Visual Studio / VS Code / Rider | Bất kỳ |
| Git | Bất kỳ |

---

## Hướng dẫn cài đặt

### Bước 1 — Clone dự án

```bash
git clone https://github.com/AndrewNguyenITVN/cafe-management-system-dotnet.git
cd CafeManagement
```

### Bước 2 — Cấu hình Connection String

Mở file `CafeManagement/appsettings.json` và chỉnh lại thông tin kết nối SQL Server cho phù hợp với máy của bạn:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=CafeManagement;User Id=sa;Password=StrongPass!123;TrustServerCertificate=True;"
}
```

> Nếu dùng **SQL Server Docker**, giữ nguyên mặc định.
> Nếu dùng **SQL Server local (Windows Auth)**, đổi thành:
> `Server=.\SQLEXPRESS;Database=CafeManagement;Trusted_Connection=True;TrustServerCertificate=True;`

### Bước 3 — Tạo Database

Mở **SSMS** kết nối vào SQL Server và chạy lần lượt 2 file sau (theo thứ tự):

```
1. CafeManagement_Schema.sql    ← Tạo database + toàn bộ bảng
2. CafeManagement_SeedData.sql  ← Nạp dữ liệu mẫu (ca làm, chức vụ, món ăn...)
```

### Bước 4 — Chạy ứng dụng

Nhấn **Run** hoặc **F5** trong Visual Studio.

### Bước 5 — Đăng nhập

Truy cập `http://localhost:7190`.

| Tài khoản | Mật khẩu |
|---|---|
| `admin@cafe.com` | `Admin@123` |

---

## Cấu trúc dự án

```
CafeManagement/
├── Controllers/            # MVC Controllers (mỗi thành viên thêm controller của mình vào đây)
├── Services/
│   ├── Interfaces/         # Interface định nghĩa bởi Leader (IInventoryService, IPointService...)
│   └── Implementations/    # Các thành implement vào đây
├── Models/
│   ├── Domain/             # Entity classes (map với database)
│   └── ViewModels/         # ViewModel cho từng màn hình
├── Views/                  # Razor Views
│   └── Shared/
│       └── _AdminLayout.cshtml   # Layout chung cho trang Admin
├── Data/
│   └── AppDbContext.cs     # EF Core DbContext
├── wwwroot/                # CSS, JS, hình ảnh tĩnh dùng Bootstrap và Jquery.
├── appsettings.json        # Cấu hình (connection string...)
└── Program.cs              # Entry point, DI registration

CafeManagement_Schema.sql   # Script tạo database (chạy thủ công)
CafeManagement_SeedData.sql # Script nạp dữ liệu mẫu
```

---

## Quy tắc code chung

### 1. Đặt tên

```
Controller:  <Feature>Controller.cs         VD: OrderController.cs
Service:     I<Feature>Service.cs (interface)
             <Feature>Service.cs (implementation)
ViewModel:   <Feature>ViewModel.cs          VD: OrderViewModel.cs
View:        Views/<Controller>/<Action>.cshtml
```

### 2. Thêm Service mới

Sau khi tạo interface và implementation, đăng ký trong `Program.cs`:

```csharp
builder.Services.AddScoped<IMyService, MyService>();
```

### 3. Thêm bảng mới vào database

1. Tạo model class trong `Models/Domain/`
2. Thêm `DbSet<T>` vào `Data/AppDbContext.cs`
3. Viết câu `ALTER TABLE` hoặc `CREATE TABLE` vào `CafeManagement_Schema.sql` tất cả thay đổi DB làm thủ công qua SQL

### 4. Git workflow

```bash
# Trước khi bắt đầu làm việc
git pull origin main

# Sau khi hoàn thành 1 tính năng
git add .
git commit -m "feat: <mô tả ngắn>"
git push origin <tên-branch-của-bạn>
```

> Không commit thẳng vào `main`. Tạo branch riêng theo tên: `tv2/pos-order`, `tv3/inventory`, v.v.

---

## Interfaces cần implement

Thành viên 3 và Thành viên 5 implement các service đã được chỉ định.

---

## Tài khoản admin mặc định

App tự động tạo khi khởi động lần đầu (xem `Program.cs → SeedAdminAsync`):

```
Email:    admin@cafe.com
Password: Admin@123
Role:     Admin
```

---

## Lưu ý

- File `appsettings.json` chứa connection string — **không commit password thật** lên GitHub nếu deploy production.
- Thư mục `obj/` và `bin/` đã được bỏ qua trong `.gitignore`, không cần quan tâm.