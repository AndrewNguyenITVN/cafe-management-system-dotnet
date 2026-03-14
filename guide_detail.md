PHÂN CÔNG NHIỆM VỤ — DỰ ÁN QUẢN LÝ CHUỖI CAFE
Kiến trúc: ASP.NET Core 8 MVC + Service Layer
Cấu trúc dự án:
CafeManagement/
├── Controllers/
├── Services/
│ ├── Interfaces/ ← TV1 định nghĩa
│ └── Implementations/ ← TV3, TV5 implement
├── Models/
│ ├── Domain/ ← Entity classes ánh xạ DB
│ └── ViewModels/ ← ViewModel riêng cho từng View
├── Data/
│ ├── AppDbContext.cs
│ └── Migrations/
├── Views/ ← Shared Layout, Login + Views theo Controller
└── wwwroot/
TV1: Team Leader — Architect & Master Data Manager
Vai trò: Khởi tạo hệ thống, thiết lập nền tảng chung và quản lý các danh mục lõi.
Khởi tạo dự án
Tạo project ASP.NET Core 8 MVC với cấu trúc thư mục như trên.
Cài đặt NuGet packages: Microsoft.EntityFrameworkCore.SqlServer, Microsoft.EntityFrameworkCore.Tools, Microsoft.AspNetCore.Identity.EntityFrameworkCore.
Tạo toàn bộ Domain Models cho tất cả các bảng (tất cả thành viên dùng chung):
Nhóm Master Data: Store, Category, MenuItem, Topping, PaymentMethod, Supplier
Nhóm Users: AppUser, Role, JobPosition, PositionRate
Nhóm POS: Order, OrderDetail, OrderDetailTopping, OrderDailySequence, Transaction
Nhóm Kho: Ingredient, Recipe, InventoryStock, InventoryLog, PurchaseOrder, PurchaseOrderDetail
Nhóm CRM: Customer, PointHistory
Nhóm HRM: Shift, Schedule, Timekeeping
Cấu hình AppDbContext, đăng ký toàn bộ DbSet<>, cấu hình ASP.NET Core Identity.
Thêm connection string vào appsettings.json.
Chạy Add-Migration InitialCreate và Update-Database.
Seed data ban đầu: Roles (Admin, Manager, Staff), tài khoản Admin mặc định, 3 Shifts cố định, PaymentMethods.
Viết BaseController với các helper method dùng chung (GetCurrentUserId, GetCurrentStoreId).
Viết Global Exception Handling Middleware.
Định nghĩa các Interface trong Services/Interfaces/ (các thành viên implement sau):
// Services/Interfaces/IInventoryService.cs — TV3 implement
public interface IInventoryService
{
Task DeductStockAsync(int orderId);
}
// Services/Interfaces/IPointService.cs — TV5 implement
public interface IPointService
{
// orderId: để tự truy vấn TotalAmount từ DB
// pointsUsed: số điểm khách muốn dùng
Task ProcessOrderPointsAsync(int orderId, int pointsUsed);
}
// Services/Interfaces/ITransactionService.cs — TV5 implement
public interface ITransactionService
{
Task RecordPaymentAsync(int orderId, int paymentMethodId,
decimal amount, decimal amountTendered);
}
Back-end (Controllers + Services)
Auth:
AccountController: Login (kiểm tra username/password, tạo session), Logout.
Cấu hình phân quyền theo Role: [Authorize(Roles = "Admin")], [Authorize(Roles = "Manager,Admin")].
Master Data (CRUD đầy đủ):
StoreController: Thêm/sửa/xóa chi nhánh.
CategoryController: Thêm/sửa/xóa danh mục.
MenuItemController: Thêm/sửa/xóa món, upload ảnh (lưu vào wwwroot/images/).
ToppingController: Thêm/sửa/xóa Topping (Trân châu, Thạch, Kem...), mỗi Topping có giá riêng.
PaymentMethodController: Thêm/sửa/xóa phương thức thanh toán.
SupplierController: Thêm/sửa/xóa nhà cung cấp (TV3 dùng khi nhập kho).
UserController: Thêm/sửa/xóa nhân viên, gán Role và JobPosition.
JobPositionController: Quản lý chức vụ và bảng lương giờ (PositionRate với EffectiveDate).
Front-end (Views)
Views/Account/Login.cshtml: Trang đăng nhập chung cho toàn hệ thống.
Layout Admin (Views/Shared/\_AdminLayout.cshtml): Sidebar navigation, header hiển thị tên user và chi nhánh.
Views/Store/, Views/MenuItem/, Views/Topping/: Các trang quản lý danh mục (dạng bảng + form).
Views/User/, Views/JobPosition/: Quản lý nhân sự và bảng lương.
TV2: POS Specialist & Real-time Flow
Vai trò: Chịu trách nhiệm luồng bán hàng trực tiếp và hiển thị trạng thái real-time.> Lưu ý kiến trúc: Màn hình POS là trang Razor View bình thường trong MVC. Từ màn hình mặc định, nhân viên nhập PIN để vào trang Order.

1. Back-end
   1.1. Order Controller — Action CreateOrder (POST)
   Input (từ Form/AJAX): StoreId, UserId, CustomerPhone, CustomerName (nếu khách mới), List<OrderItemDto> (MenuItemId, List<ToppingId>, Quantity, UnitPrice), PointsUsed, OrderType, PaymentMethodId, AmountTendered.Logic xử lý (theo thứ tự — trong 1 database transaction):Bước 1 — Nhận diện khách hàng:
   Nếu CustomerPhone được nhập: Tìm trong bảng Customers theo phone.
   Tìm thấy → lấy CustomerId và TotalPoints.
   Không thấy → Tự động tạo bản ghi Customer mới với Phone và CustomerName.
   Nếu không nhập phone → CustomerId = null.
   Bước 2 — Sinh QueueNumber (xử lý concurrency an toàn):Không dùng MAX() + 1 vì có race condition khi 2 máy POS bấm đồng thời. Thay vào đó dùng bảng OrderDailySequence(StoreId, Date, LastSeq) với row-level locking:
   // Trong transaction, dùng UPDLOCK để khóa dòng
   var today = DateOnly.FromDateTime(DateTime.Now);
   var seq = await \_db.OrderDailySequences
   .FromSqlRaw(@"SELECT \* FROM OrderDailySequences WITH (UPDLOCK)
   WHERE StoreId = {0} AND Date = {1}", storeId, today)
   .FirstOrDefaultAsync();
   if (seq == null) {
   seq = new OrderDailySequence { StoreId = storeId, Date = today, LastSeq = 1 };
   \_db.OrderDailySequences.Add(seq);
   } else {
   seq.LastSeq += 1;
   }
   await \_db.SaveChangesAsync(); // Lưu sequence trước
   string queueNumber = $"{today:yyyyMMdd}-{storeId}-{seq.LastSeq:D3}";
   // Ví dụ: 20260305-1-007
   Bước 3 — Tính tiền:
   TotalAmount = Sum(Quantity × UnitPrice) + Sum(ToppingQty × ToppingPrice)
   DiscountAmount = PointsUsed (1 điểm = 1 VNĐ, validate PointsUsed ≤ TotalPoints)
   FinalAmount = TotalAmount - DiscountAmount
   Bước 4 — Lưu Order và OrderDetails:
   Tạo bản ghi Order với Status = 0 (Pending).
   Tạo các bản ghi OrderDetail + OrderDetailTopping.
   UnitPrice trong OrderDetail là snapshot giá tại thời điểm bán.
   Bước 5 — Xử lý sau khi lưu Order (gọi các Service):Ba lời gọi dưới đây phải nằm trong cùng một transaction. Nếu bất kỳ bước nào lỗi → rollback toàn bộ:
   await \_inventoryService.DeductStockAsync(order.Id);
   await \_pointService.ProcessOrderPointsAsync(order.Id, pointsUsed);
   await \_transactionService.RecordPaymentAsync(
   order.Id, paymentMethodId, order.FinalAmount, amountTendered);
   Bước 6 — Đẩy SignalR:Sau khi transaction commit thành công → Gọi \_hubContext.Clients.Group(storeId).SendAsync("NewOrderReceived", orderDto) để KDS nhận đơn ngay lập tức.
   1.2. Order Status API — Action UpdateStatus (PATCH)
   Chức năng: Pha chế nhấn "Xong đơn" → Cập nhật trạng thái.Trạng thái: 0 (Pending) → 1 (Processing) → 2 (Ready)Logic khi chuyển sang Ready:Gọi \_hubContext.Clients.Group(storeId).SendAsync("OrderReady", queueNumber) để màn hình TV khách cập nhật.
   1.3. SignalR Hub — OrderHub.cs
   public class OrderHub : Hub
   {
   // Client gọi khi khởi động POS/KDS/TV
   public async Task JoinStoreGroup(string storeId)
   => await Groups.AddToGroupAsync(Context.ConnectionId, storeId);
   // Server gọi khi có đơn mới → KDS nhận
   // SendAsync("NewOrderReceived", orderData)
   // Server gọi khi đơn Ready → TV nhận
   // SendAsync("OrderReady", queueNumber)
   }
   Đăng ký trong Program.cs: app.MapHub<OrderHub>("/orderHub");
2. Front-end
   2.1. Màn hình PIN Entry (Trang mặc định POS)
   Hiển thị bàn phím số 0–9, nút Clear, nút Enter.
   Khi nhân viên nhập PIN → Submit POST lên TimekeepingController.Clock() của TV4.
   Nếu PIN hợp lệ → Redirect sang OrderController.Index() (màn hình Order).
   Nếu PIN sai → Hiện thông báo lỗi ngay trên màn hình.
   2.2. Màn hình Order (POS Screen)
   Thiết kế chia 3 cột:Cột Trái — Danh mục:
   Danh sách Category dạng nút lớn, dễ bấm.
   Click vào Category → Cột giữa lọc theo danh mục đó.
   Cột Giữa — Danh sách món:
   Card cho mỗi MenuItem: Hình ảnh, tên, giá.
   Click vào món → Hiện popup:
   Danh sách Toppings (checkbox, mỗi Topping hiển thị giá thêm).
   Ô ghi chú (ít đường, ít đá...).
   Nút "Thêm vào giỏ".
   Cột Phải — Giỏ hàng & Khách hàng:
   Ô nhập SĐT: Khi nhập đủ 10 số → Gọi AJAX tìm khách, hiện tên + điểm. Nếu không tìm thấy → Hiện thêm ô "Tên khách".
   Thông tin điểm: "Điểm hiện có: 15,000đ". Ô input số điểm muốn dùng + nút "Áp dụng" → FinalAmount cập nhật ngay trên UI.
   Danh sách món trong giỏ: Tên, số lượng, giá, nút xóa.
   Tổng tiền: TotalAmount → DiscountAmount → FinalAmount (in đậm).
   Chọn phương thức thanh toán (Tiền mặt / Chuyển khoản).
   Nếu Tiền mặt: Ô nhập "Tiền khách đưa" → Hiện "Tiền thối: X đồng".
   Nút "Hoàn tất thanh toán" → Submit tạo Order.
   2.3. Kitchen Display System (KDS)
   Danh sách thẻ (card) đơn hàng đang Pending và Processing, sắp xếp theo thời gian.
   Mỗi thẻ: QueueNumber (to, nổi bật), Tên khách, Loại đơn (Tại chỗ/Mang đi), danh sách món + ghi chú, thời gian đặt.
   Nút "Bắt đầu làm" → AJAX PATCH → Status = Processing.
   Nút "Xong đơn" → AJAX PATCH → Status = Ready → SignalR đẩy sang màn hình TV.
   Tự động cập nhật real-time qua SignalR (nhận event NewOrderReceived).
   2.4. Customer Display (Màn hình TV)
   Giao diện tĩnh, tự động kết nối SignalR khi load trang.
   Hai vùng lớn:
   "Đang pha chế": Hiển thị QueueNumber các đơn Pending/Processing.
   "Mời nhận món": Hiển thị QueueNumber các đơn Ready, nền xanh, hiệu ứng nổi bật.
   Khi nhận event OrderReady từ Hub → Tự động chuyển số từ cột trái sang cột phải.
   TV3: Inventory & Recipe Engine
   Vai trò: Xử lý logic tính toán định mức và quản lý hàng hóa.
3. Back-end
   1.1. Recipe Management
   Action SaveRecipe (POST):
   Input: MenuItemId, List<RecipeItem> (IngredientId, Quantity, WastePercent).
   Logic: Xóa các dòng Recipe cũ của MenuItemId đó rồi insert mới (ghi đè toàn bộ).
   Lưu ý: WastePercent (ví dụ: 5 = 5%) là bắt buộc, mặc định 0 nếu không nhập.
   1.2. Implement IInventoryService — Hàm DeductStockAsync
   Đây là hàm core được TV2 gọi sau khi thanh toán thành công.
   public async Task DeductStockAsync(int orderId)
   {
   var order = await \_db.Orders
   .Include(o => o.OrderDetails)
   .FirstAsync(o => o.Id == orderId);
   foreach (var detail in order.OrderDetails)
   {
   var recipes = await \_db.Recipes
   .Where(r => r.MenuItemId == detail.MenuItemId)
   .ToListAsync();
   foreach (var recipe in recipes)
   {
   // Tính lượng trừ thực tế có cộng tỷ lệ hao hụt
   decimal actualDeduct = recipe.Quantity
   _ (1 + recipe.WastePercent / 100m)
   _ detail.Quantity;
   var stock = await \_db.InventoryStocks.FirstAsync(
   s => s.StoreId == order.StoreId
   && s.IngredientId == recipe.IngredientId);
   stock.CurrentQuantity -= actualDeduct;
   stock.LastUpdated = DateTime.Now;
   \_db.InventoryLogs.Add(new InventoryLog {
   StoreId = order.StoreId,
   IngredientId = recipe.IngredientId,
   ChangeQuantity = -actualDeduct,
   Type = "Sale",
   ReferenceId = orderId,
   CreatedAt = DateTime.Now
   });
   }
   }
   await \_db.SaveChangesAsync();
   }
   1.3. Purchase API — Action CreatePurchase (POST)
   Input: StoreId, SupplierId, List<PurchaseItem> (IngredientId, Quantity, UnitPrice).
   Logic:
   Tạo bản ghi PurchaseOrder + PurchaseOrderDetail (TV5 dùng để tính COGS).
   Cộng Quantity vào InventoryStocks.CurrentQuantity.
   Ghi InventoryLog với Type = "Purchase".
   1.4. Stocktake API — Action SubmitStocktake (POST)
   Input: StoreId, List<ActualStockItem> (IngredientId, ActualQuantity).
   Logic:
   Delta = ActualQuantity - CurrentQuantity (lý thuyết trong DB)
   Nếu Delta ≠ 0: Tạo InventoryLog loại Adjustment để hiệu chỉnh kho về đúng thực tế.
   Cập nhật InventoryStocks.CurrentQuantity = ActualQuantity.
   Lưu StocktakeRecord (bảng lưu lịch sử kiểm kê): Date, StoreId, IngredientId, TheoreticalQty, ActualQty, Delta.
   Cảnh báo nếu |Delta / TheoreticalQty| > 5%.
4. Front-end
   2.1. Màn hình Công thức Recipe
   Bên trái: Danh sách MenuItem (lấy từ API của TV1).
   Bên phải (khi chọn món): Danh sách nguyên liệu đang có trong công thức.
   Thêm nguyên liệu: Thanh tìm kiếm Ingredient → Chọn → Nhập Quantity (hỗ trợ số thập phân) và WastePercent.
   Nút "Lưu công thức" → Submit.
   2.2. Màn hình Tồn kho & Nhập kho
   Tab Tồn kho:
   Bảng: Tên nguyên liệu | Đơn vị | Tồn hiện tại | Mức cảnh báo | Trạng thái.
   Hàng có CurrentQuantity < MinStockLevel → Highlight nền đỏ nhạt.
   Tab Nhập kho:
   Dropdown chọn Nhà cung cấp.
   Bảng nhập: Chọn Ingredient → Nhập Số lượng → Nhập Giá vốn/đơn vị.
   Nút "+" để thêm dòng.
   Nút "Xác nhận nhập kho".
   2.3. Màn hình Kiểm kê (Stocktake)
   Bảng 4 cột:
   Nguyên liệu
   Tồn máy tính
   Tồn thực tế (nhập)
   Chênh lệch (tự tính)
   Cafe hạt
   5,200g
   [___]
   —

Cột "Tồn máy tính": Readonly, hệ thống tự điền.
Cột "Tồn thực tế": Input, nhân viên nhập sau khi đếm.
Cột "Chênh lệch": Tự tính và hiển thị ngay khi nhập, tô đỏ nếu âm, tô xanh nếu dương.
Nút "Chốt kiểm kê": Gửi toàn bộ lên server, cập nhật kho, lưu lịch sử kiểm kê.

TV4: HRM & Scheduling
Vai trò: Quản lý lịch làm việc, chấm công và tính lương.

1. Back-end
   1.1. Schedule API — Action AssignSchedule (POST)
   Ca cố định (lưu trong bảng Shifts, seed sẵn):
   Ca 1: 06:00 – 12:00
   Ca 2: 12:00 – 18:00
   Ca 3: 18:00 – 24:00
   Input: StoreId, Date, ShiftId, List<UserId>.Logic:
   Xóa bản ghi Schedule cũ của (StoreId, Date, ShiftId) (cho phép manager sửa lại).
   Insert danh sách nhân viên mới vào bảng Schedules.
   1.2. Timekeeping API — Action Clock (POST)
   Input: PinCode (6 chữ số).Quy trình xử lý:
   // Bước 1: Xác thực PIN
   var user = await \_db.Users.FirstOrDefaultAsync(u => u.PinCode == pinCode);
   if (user == null) return Error("Sai mã PIN");
   // Bước 2: Tìm bản ghi check-in gần nhất chưa check-out
   // KHÔNG dùng DateTime.Today — để xử lý ca xuyên đêm
   var lastRecord = await \_db.Timekeepings
   .Where(t => t.UserId == user.Id && t.CheckOutTime == null)
   .OrderByDescending(t => t.CheckInTime)
   .FirstOrDefaultAsync();
   if (lastRecord == null)
   {
   // ---- CHECK-IN ----
   var now = DateTime.Now;
   // Tìm ca được xếp lịch gần nhất của nhân viên
   var schedule = await \_db.Schedules
   .Include(s => s.Shift)
   .Where(s => s.UserId == user.Id && s.WorkDate == DateOnly.FromDateTime(now))
   .OrderBy(s => s.Shift.StartTime)
   .FirstOrDefaultAsync();
   bool isLate = false;
   if (schedule != null)
   {
   var shiftStart = now.Date + schedule.Shift.StartTime;
   isLate = now > shiftStart.AddMinutes(15);
   }
   \_db.Timekeepings.Add(new Timekeeping {
   UserId = user.Id, StoreId = user.StoreId,
   CheckInTime = now, Date = DateOnly.FromDateTime(now),
   IsLate = isLate
   });
   }
   else
   {
   // ---- CHECK-OUT ----
   var now = DateTime.Now;
   lastRecord.CheckOutTime = now;
   lastRecord.TotalHours = (decimal)(now - lastRecord.CheckInTime).TotalHours;
   // Snapshot mức lương tại thời điểm này
   lastRecord.HourlyRateAtTime = await \_db.PositionRates
   .Where(r => r.PositionId == user.PositionId
   && r.EffectiveDate <= DateOnly.FromDateTime(now))
   .OrderByDescending(r => r.EffectiveDate)
   .Select(r => r.HourlyRate)
   .FirstAsync();
   }
   await \_db.SaveChangesAsync();
   Lưu ý ca xuyên đêm: Logic tìm lastRecord không giới hạn theo ngày hôm nay, do đó nhân viên Ca 3 check-out lúc 00:15 ngày hôm sau vẫn được ghép đúng với bản ghi check-in của tối hôm trước.
   1.3. Salary Report — Action GetPayrollReport (GET)
   Input: Month, Year, StoreId.Logic:
   Salary_NV = SUM(TotalHours × HourlyRateAtTime)
   cho tất cả bản ghi Timekeepings của NV đó trong tháng
   Trả về: Danh sách { FullName, Position, TotalHours, AvgHourlyRate, TotalSalary }.
2. Front-end
   2.1. Màn hình Xếp lịch tuần (Admin/Manager)
   Bảng lưới 7 cột (Thứ 2 → Chủ nhật) × 3 hàng (Ca 1, Ca 2, Ca 3).
   Click vào ô → Popup danh sách nhân viên của chi nhánh (checkbox).
   Chọn nhân viên → Bấm "Xác nhận" → Tên nhân viên hiển thị trong ô.
   Filter theo tuần (có nút chuyển tuần trước/sau).
   2.2. Màn hình PIN Pad (Tại POS)
   10 nút số 0–9 kích thước lớn + nút Clear + nút Enter.
   Ô hiển thị: Khi nhập số → Hiện ●●●●●● (không hiện số thực).
   Sau khi Enter → Hiện thông báo: "Chào Nguyễn Văn A, bạn đã Check-in thành công lúc 07:05".
   2.3. Bảng báo cáo lương (Admin/Manager)
   Bộ lọc: Tháng/Năm + Chi nhánh.
   Bảng: STT | Tên | Chức vụ | Tổng giờ | Lương/h | Thành tiền | Ghi chú.
   Hàng có IsLate → Hiển thị icon cảnh báo.
   Nút "Xuất Excel" (tùy chọn nếu có thời gian).
3. Quy tắc nghiệp vụ quan trọng
   Bảo mật PIN: Không hiển thị số đang nhập, chỉ hiện ●.
   HourlyRateAtTime: Luôn snapshot khi check-out, không tra cứu lại sau này.
   Ca xuyên đêm: Dùng logic lastRecord chưa checkout thay vì lọc theo ngày hôm nay.
   TV5: CRM, Finance & Reporting
   Vai trò: Quản lý khách hàng, dòng tiền và báo cáo kết quả kinh doanh.
4. Back-end
   1.1. Implement IPointService — Hàm ProcessOrderPointsAsync
   public async Task ProcessOrderPointsAsync(int orderId, int pointsUsed)
   {
   var order = await \_db.Orders
   .Include(o => o.Customer)
   .FirstAsync(o => o.Id == orderId);
   // Tích điểm mới = 1% của TotalAmount (TRƯỚC khi trừ điểm)
   int pointsEarned = (int)Math.Floor(order.TotalAmount \* 0.01m);
   if (order.Customer != null)
   {
   // Ghi log dùng điểm (nếu có)
   if (pointsUsed > 0)
   {
   \_db.PointHistories.Add(new PointHistory {
   CustomerId = order.Customer.Id, OrderId = orderId,
   PointsChanged = -pointsUsed, Type = "Redeem",
   CreatedAt = DateTime.Now
   });
   }
   // Ghi log tích điểm
   \_db.PointHistories.Add(new PointHistory {
   CustomerId = order.Customer.Id, OrderId = orderId,
   PointsChanged = +pointsEarned, Type = "Earn",
   CreatedAt = DateTime.Now
   });
   // Cập nhật tổng điểm
   order.Customer.TotalPoints =
   order.Customer.TotalPoints - pointsUsed + pointsEarned;
   }
   await \_db.SaveChangesAsync();
   }
   Lưu ý: PointsEarned tính trên TotalAmount (trước khi trừ điểm) — công bằng cho khách. Mỗi đơn hàng có thể tạo 2 bản ghi PointHistory riêng biệt (1 Redeem + 1 Earn).
   1.2. Implement ITransactionService — Hàm RecordPaymentAsync
   public async Task RecordPaymentAsync(
   int orderId, int paymentMethodId,
   decimal amount, decimal amountTendered)
   {
   \_db.Transactions.Add(new Transaction {
   OrderId = orderId,
   PaymentMethodId = paymentMethodId,
   Amount = amount,
   AmountTendered = amountTendered,
   ChangeAmount = amountTendered - amount, // Tiền thối (0 nếu chuyển khoản)
   TransactionDate = DateTime.Now
   });
   await \_db.SaveChangesAsync();
   }
   1.3. Kết ca — Action CloseShift (POST)
   Input: StoreId, ShiftId, ActualCashCounted, Note.Logic:
   Xác định khoảng thời gian ca: Lấy StartTime–EndTime từ bảng Shifts.
   Tổng hợp tất cả Transaction có PaymentMethod = Cash trong khoảng thời gian đó tại StoreId.
   ExpectedCash = TiềnMặtĐầuCa + SUM(Transaction.Amount WHERE Type=Cash).
   Difference = ActualCashCounted - ExpectedCash.
   Lưu bản ghi ShiftHandover: ShiftId, StoreId, ExpectedCash, ActualCash, Difference, Note, ConfirmedAt.
   1.4. Reporting API
   Revenue: SUM(FinalAmount) từ Orders, group by Date/StoreId/CategoryId.COGS (Giá vốn):
   Với mỗi OrderDetail:
   COGS += Quantity × Recipe.Quantity × GiáVốnNguyênLiệu
   Giá vốn nguyên liệu = Lấy từ PurchaseOrderDetail gần nhất (giá nhập gần nhất). Phối hợp với TV3 để thống nhất query.Staff Cost: SUM(TotalHours × HourlyRateAtTime) từ Timekeepings. Phối hợp với TV4.Profit: Profit = Revenue - COGS - StaffCost.
5. Front-end
   2.1. Widget Tra cứu Khách hàng (Tích hợp vào POS của TV2)
   Partial View được TV2 nhúng vào màn hình Order:
   Ô nhập SĐT → AJAX tìm kiếm.
   Tìm thấy: Hiện "Nguyễn Văn B — 12,500 điểm".
   Không thấy: Hiện nút "Đăng ký nhanh" → Form nhỏ nhập Tên + SĐT.
   2.2. Màn hình Kết ca (Shift Handover)
   Hiển thị tự động: Tổng doanh thu trong ca, Tổng tiền mặt theo hệ thống, Tổng chuyển khoản.
   Ô Input: "Số tiền mặt thực tế đếm được trong két".
   Hệ thống tự tính Difference và hiển thị (đỏ nếu thiếu, xanh nếu thừa).
   Ô Ghi chú lý do (bắt buộc nếu Difference ≠ 0).
   Nút "Xác nhận kết ca" → Lưu biên bản + hiện bản xem trước để in.
   2.3. Hệ thống Báo cáo BI (Làm trước khi các tính năng CRM/Transaction hoàn thiện, dùng data giả)
   Trang dành cho Admin/Chủ chuỗi:
   Biểu đồ đường (Line Chart): Doanh thu theo ngày trong 30 ngày gần nhất, có thể chọn theo chi nhánh.
   Biểu đồ tròn (Pie Chart): Cơ cấu doanh thu theo chi nhánh hoặc theo danh mục món.
   Bảng P&L (Profit & Loss):
   Chỉ số
   Tháng này
   Tháng trước
   Doanh thu
   X
   X
   Giá vốn nguyên liệu
   X
   X
   Chi phí nhân sự
   X
   X
   Lợi nhuận ròng
   X
   X

Bộ lọc: Theo tháng + Theo chi nhánh (hoặc Toàn hệ thống).
