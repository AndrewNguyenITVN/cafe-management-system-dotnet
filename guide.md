
MVC + Service Layer
TV1: Team Leader - Architect & Master Data Manager NHỰT
Vai trò: Khởi tạo, thiết kế hệ thống và quản lý các danh mục lõi.
Khởi tạo dự án (Core):
Tạo Solution .NET (Clean Architecture: Domain, Application, Infrastructure, WebAPI).
Viết Base Controller, Base Service, Global Exception Handling và cấu hình Swagger.
Back-end (API):
Auth Service: Identity Server/JWT (Login, Logout, Phân quyền).
Master Data API (CRUD): Stores (Chi nhánh), Categories (Danh mục), MenuItems (Món ăn - bao gồm Upload ảnh), PaymentMethods (Phương thức thanh toán), Suppliers (Nhà cung cấp).
Interface Definition: Định nghĩa các Interface IInventoryService, IPointService để TV3 và TV5 "đắp" code vào.
Front-end (Admin UI):
Layout tổng cho trang Admin.
Màn hình quản lý Chi nhánh, Thực đơn (Menu), Nhà cung cấp và Phương thức thanh toán.
TV2: POS Specialist & Real-time Flow(Từ màng hình defaul nhân viên nhập PIN code để vào trang order) GIANG
Vai trò: Chịu trách nhiệm luồng bán hàng trực tiếp và hiển thị trạng thái.
1. PHẦN BACK-END (API & REAL-TIME)
1.1. Order API
Đây là API quan trọng nhất, xử lý luồng tạo đơn hàng.
Input (Request Body): StoreId, UserId, CustomerPhone, CustomerName (nếu khách mới), List<OrderItem> (Id món, Topping, Số lượng, Giá), PointsUsed (số điểm muốn đổi), OrderType.
Logic xử lý (Step-by-Step):
Nhận diện khách hàng: * Dựa trên CustomerPhone, kiểm tra DB (bảng Customers). Nếu có: Load CustomerId và số điểm hiện tại.
Nếu chưa có: Tự động tạo bản ghi Customer mới với Phone và CustomerName cung cấp từ quầy.
Sinh số thứ tự (QueueNumber):
Truy vấn bảng Orders lọc theo StoreId và OrderDate (chỉ tính ngày hôm nay).
Công thức: QueueNumber = Max(QueueNumber) + 1. Nếu là đơn đầu tiên trong ngày, trả về 1.
Tính toán tiền & Điểm:
TotalAmount = Sum(Quantity * UnitPrice).
Nếu PointsUsed > 0: Kiểm tra xem khách có đủ điểm không. Tính DiscountAmount = PointsUsed * 1 (Giả sử 1 điểm = 1 VNĐ).
FinalAmount = TotalAmount - DiscountAmount.
Tính điểm tích lũy mới cho đơn này: PointsEarned = Amount * 1%.
Lưu Database: Tạo bản ghi vào Orders và OrderDetails. Trạng thái mặc định là 0 (Pending).
Tích hợp (Gọi Service):
Gọi IInventoryService.DeductStock(orderId) để trừ kho (TV3 viết).
Gọi IPointService.UpdatePoints(customerId, -PointsUsed + PointsEarned) (TV5 viết).
1.2. KDS API 
Chức năng: Thay đổi trạng thái đơn hàng.
Các mức trạng thái: 0 (Pending) → 1 (Processing) → 2 (Ready).
Logic: Khi chuyển sang Ready, API này phải đồng thời kích hoạt SignalR để báo cho màn hình khách.
1.3. SignalR Hub (OrderHub.cs)
Cấu trúc Group: Mỗi chi nhánh (StoreId) là một Group riêng để tránh việc Bếp ở Quận 1 nhận đơn của Quận 3.
JoinGroup(string storeId): Gọi khi giao diện POS/KDS khởi động.
Events:
NewOrderReceived: Gửi từ Server → KDS (khi khách vừa thanh toán xong).
OrderReady: Gửi từ Server → Customer Display (khi Barista nhấn "Xong đơn").
2. PHẦN FRONT-END (UI/UX)
2.1. Giao diện Order tại quầy (POS Screen)
Thiết kế chia làm 3 cột chính:
Cột Trái (Danh mục): List các Categories (Cà phê, Trà trái cây, Đá xay...).
Cột Giữa (Danh sách món): Các Card món ăn có hình ảnh, tên, giá.
Action: Click vào món → Hiện Pop-up chọn Topping (Trân châu, Thạch...) và Note.
Cột Phải (Giỏ hàng & Khách hàng):
Ô nhập SĐT: Khi nhập đủ số, gọi API lấy thông tin khách. Nếu không có, hiện thêm ô nhập "Tên khách".
Thông tin điểm: Hiển thị "Điểm hiện có: 15,000". Bên cạnh có ô nhập số điểm cần dùng và nút "Use". Khi bấm, giá trị FinalAmount phải được trừ đi ngay lập tức trên UI.
Nút Thanh toán: In hóa đơn và đẩy đơn đi.
2.2. Kitchen Display System (KDS)
Giao diện dạng danh sách các thẻ (Cards) là các đơn hàng phải làm:
Mỗi thẻ đại diện cho 1 đơn hàng, bao gồm: QueueNumber, CustomerName, Danh sách món + Note.
Nút chức năng: "Bắt đầu làm" (chuyển sang Processing) và "Xong đơn" (chuyển sang Ready).
2.3. Customer Display (Màn hình TV)
Giao diện tĩnh, không cần tương tác, chia làm 2 vùng lớn:
Vùng 1 (Preparing): Danh sách các QueueNumber đang pha chế (dạng text lớn).
Vùng 2 (Ready): Danh sách các QueueNumber đã xong.
3. CÁC ĐIỂM CẦN LƯU Ý KHI CODE
Concurrency: Đảm bảo 2 máy POS cùng bấm thanh toán một lúc không bị trùng QueueNumber (Sử dụng lock hoặc Transaction trong SQL).

TV3: Inventory & Recipe Engine HOA
Vai trò: Xử lý logic tính toán định mức và quản lý hàng hóa.
1. PHẦN BACK-END (LOGIC & API)
1.1. Recipe
Quản lý "Công thức biến hình" từ nguyên liệu thô sang món hoàn chỉnh.
Input: MenuItemId, List<IngredientRequirement> (bao gồm IngredientId và Quantity).
Logic: * Xóa các công thức cũ của MenuItemId đó (nếu có) và ghi đè mới.
Đơn vị tính (Unit) phải khớp với định nghĩa trong bảng Ingredients (ví dụ: Cafe dùng gram, Sữa dùng ml).
1.2. Inventory Engine (Hàm DeductStock)
Đây là hàm Core, được TV2 gọi thông qua Interface IInventoryService khi đơn hàng hoàn tất.
Quy trình xử lý (Logic luồng):
Lấy danh sách OrderDetails từ orderId.
Với mỗi món trong đơn hàng, tìm trong bảng Recipes để biết cần bao nhiêu nguyên liệu.
Truy cập bảng InventoryStocks theo StoreId của đơn hàng đó.
Cập nhật: CurrentQuantity = CurrentQuantity - (Số_lượng_món_bán * Định_mức_nguyên_liệu).
Ghi log vào bảng InventoryLog với Type = 'Sale'.
1.3. Purchase 
Xử lý khi có hàng về từ nhà cung cấp.
Input: StoreId, SupplierId, List<PurchaseItem> (IngredientId, Quantity, UnitPrice).
Logic: * Cập nhật tăng CurrentQuantity trong bảng InventoryStocks.
Lưu thông tin vào bảng PurchaseOrders để phục vụ báo cáo chi phí của TV5.
Tính toán lại giá vốn trung bình (nếu cần nâng cao).
1.4. Stocktake API (POST /api/stocktake)
Xử lý khi nhân viên kiểm kho thực tế.
Input: StoreId, List<ActualStock> (IngredientId, ActualQuantity).
Logic:
Lấy TheoreticalQuantity (Tồn lý thuyết) hiện tại từ DB.
Công thức tính chênh lệch:
Delta = Q_actual - Q_theoretical
Nếu Delta khác 0: Tạo một bản ghi InventoryLog loại Adjustment để ép số dư trong máy khớp với thực tế.
Cảnh báo nếu Delta (Hao hụt) vượt quá mức cho phép (ví dụ > 5%).
2. PHẦN FRONT-END (UI/UX)
2.1. Màn hình Thiết lập Công thức (Recipe Management)
Giao diện: Chọn một món bên trái (từ danh mục của TV1) -> Hiện danh sách nguyên liệu bên phải.
Chức năng: * Thanh tìm kiếm nguyên liệu nhanh.
Ô nhập số lượng (có hỗ trợ số thập phân, ví dụ: 0.5 túi trà).
Nút "Save" để lưu bộ định mức.
2.2. Màn hình Nhập kho & Tồn kho
Tồn kho: Hiển thị dạng bảng (Tên nguyên liệu | Đơn vị | Tồn hiện tại | Mức cảnh báo).
Highlight: Cần tô màu đỏ những nguyên liệu có CurrentQuantity < MinStockLevel.
Nhập kho: Giao diện chọn Nhà cung cấp -> Thêm các dòng nguyên liệu -> Nhập số lượng và giá.
2.3. Màn hình Kiểm kê (Stocktake Screen)
Thiết kế: Dạng bảng đối chiếu 3 cột:
Tồn máy tính: (Hệ thống tự điền, không cho sửa).
Tồn thực tế: (Ô Input để nhân viên nhập sau khi đếm).
Chênh lệch: (Tự động tính và hiển thị ngay khi nhập cột 2).
Nút "Chốt kiểm kê": Khi bấm sẽ cập nhật lại toàn bộ kho. Nút này có nhiệm vụ "Đưa thực tế vào hệ thống". Nó xác nhận rằng: "Kể từ giây phút này, mọi tính toán tiếp theo phải dựa trên số lượng tôi vừa đếm được, không phải số lượng cũ trên máy." Đồng thời 1 bảng thống kê gồm 3 cột trên sẽ được lưu lại.

TV4: HRM & Scheduling BÌNH
Vai trò: Quản lý con người, thời gian làm việc và chấm công.
1. PHẦN BACK-END (API & LOGIC)
1.1. Shift & Schedule 
Quản lý việc phân bổ nhân sự vào các khung giờ cố định.
Cấu hình ca cố định (Hardcoded logic):  
Ca 1: 06:00 - 12:00
Ca 2: 12:00 - 18:00
Ca 3: 18:00 - 24:00
Input: StoreId, Date, ShiftId (1, 2, hoặc 3), List<UserId>.
Logic:
Xóa các phân công cũ của ca đó trong ngày đó (nếu quản lý muốn chỉnh sửa).
Lưu danh sách nhân viên vào bảng Schedules. Một ca có thể có nhiều nhân viên (ví dụ: 2 pha chế, 1 thu ngân).
1.2. Timekeeping(Làm thủ công)
Xử lý việc nhân viên nhập mã PIN 6 số trên máy POS.
Input: PinCode (6 chữ số).
Quy trình xử lý:
Xác thực: Tìm User dựa trên PinCode. Nếu không thấy -> Trả về lỗi "Sai mã PIN".
Nhận diện luồng: * Nếu User chưa có bản ghi CheckIn trong ngày hôm nay (hoặc bản ghi gần nhất đã CheckOut) -> Thực hiện Check-in.
Nếu User đã có bản ghi CheckIn nhưng chưa CheckOut -> Thực hiện Check-out.
Logic Chấm công:
Khi Check-in: Tìm ca làm việc (Schedule) gần nhất của User. So sánh giờ hiện tại với giờ bắt đầu ca. Nếu trễ > 15 phút -> Đánh dấu IsLate = true.
Khi Check-out: Tính TotalHours = (Giờ_hiện_tại - Giờ_CheckIn).
Lưu ý quan trọng: Ngay lúc Check-out, phải lấy HourlyRate từ bảng chức vụ của User đó và lưu vào cột HourlyRateAtTime trong bảng Timekeepings.
1.3. Salary Service
Input: Month, Year, StoreId.
Logic: * Truy vấn tất cả bản ghi Timekeepings của tất cả nhân viên thuộc StoreId trong tháng đó.
Công thức tính lương cho từng nhân viên:

Trả về danh sách gồm: Tên nhân viên, Tổng giờ làm, Mức lương trung bình, Tổng tiền nhận.
2. PHẦN FRONT-END (UI/UX)
2.1. Màn hình Chốt lịch làm việc (Weekly Schedule)
Đây là màn hình dành cho Quản lý trên trang Admin Web.
Giao diện: Một bảng lưới (Grid) 7 cột (thứ 2 - chủ nhật) và 3 hàng (Ca 1, Ca 2, Ca 3).
Thao tác: * Quản lý click vào một ô (ví dụ: Thứ 3 - Ca 2).
Một Pop-up hiện ra danh sách nhân viên của chi nhánh đó.
Quản lý tích chọn những người làm ca đó -> Bấm "Xác nhận".
Hiển thị: Các ô đã xếp lịch sẽ hiện tên các nhân viên bên trong.
2.2. Giao diện PIN-pad (Tại máy POS)(Làm thủ công)
Giao diện này xuất hiện khi nhân viên muốn chấm công nhanh.
Thiết kế: 10 nút số (0-9) kích thước lớn, dễ bấm trên màn hình cảm ứng. 1 nút "Xóa" (Clear) và 1 nút "Xác nhận" (Enter).
Thao tác: Nhân viên nhập 6 số -> Bấm Enter -> Hệ thống hiện thông báo: "Chào [Tên NV], bạn đã Check-in/out thành công lúc 07:05".
2.3. Bảng báo cáo lương (Payroll Report)
Giao diện: Một bảng danh sách sạch sẽ với các bộ lọc theo Tháng và theo Chi nhánh.
Các cột thông tin: STT | Tên Nhân Viên | Chức vụ | Tổng giờ công | Mức lương/h | Thành tiền | Trạng thái.
3. CÁC QUY TẮC NGHIỆP VỤ THỰC TẾ 
Bảo mật mã PIN: Tuyệt đối không hiển thị 6 số nhân viên đang nhập lên màn hình. Hãy dùng ký tự ******.
Xử lý ca xuyên đêm (Nếu có): Mặc dù ca cuối là 24h, nhưng nhân viên có thể Check-out lúc 00:15 ngày hôm sau. Logic code của bạn phải hiểu đó vẫn là thuộc ca làm của ngày hôm trước.
HourlyRateAtTime: Đây là "chìa khóa" để giải quyết vấn đề nhân viên được tăng lương. Lương của ngày nào phải tính theo giá của ngày đó.

TV5: CRM, Finance & Reporting ĐẠT
Vai trò: Quản lý khách hàng, dòng tiền và báo cáo kết quả kinh doanh.
1. PHẦN BACK-END (API & LOGIC)
1.1. Customer & Loyalty API (IPointService)
Triển khai logic tích điểm và tiêu điểm dựa trên Interface mà Leader (TV1) đã định nghĩa.
Hàm ProcessOrderPointsAsync(orderId, pointsUsed):
Truy vấn thông tin Order từ orderId.
Khấu trừ điểm: Nếu pointsUsed > 0, trừ số điểm này vào bảng Customers.TotalPoints và ghi log vào PointHistory (Type: 'Redeem').
Tích điểm mới:
PointsEarned = Amount * 1\%
(Ví dụ: Bill 100,000đ được 1,000 điểm).
Cộng PointsEarned vào Customers.TotalPoints
1.2. Transaction API 
Lưu trữ mọi vết tích thanh toán để đối soát.
Input: OrderId, PaymentMethodId, Amount, TransactionDate.
Logic: Mỗi khi TV2 gọi "Hoàn tất đơn hàng", một bản ghi Transaction phải được tạo ngay. Đây là dữ liệu nguồn cho báo cáo tài chính.
1.3. End-of-Shift Logic 
Xác định số tiền mặt nhân viên thu ngân phải bàn giao lại cho quản lý/chủ quán.
Input: StoreId, ShiftId (lấy từ TV4), ActualCashCounted (Số tiền mặt nhân viên đếm được thực tế).
Logic:
Hệ thống xác định khoảng thời gian của ca đó (Ví dụ: 06:00 - 12:00).
Tổng hợp tất cả giao dịch bằng Tiền mặt (PaymentMethod = Cash) phát sinh trong khoảng thời gian đó tại chi nhánh.
Số dư lý thuyết: ExpectedCash = Tiền mặt đầu ca + Tổng tiền mặt thu được.
Chênh lệch: Difference = ActualCashCounted - ExpectedCash.
1.4. Reporting API (Business Intelligence)
Tổng hợp dữ liệu từ tất cả các thành viên khác.
Revenue API: SUM(FinalAmount) từ bảng Orders.
Cost API: Tổng hợp từ 2 nguồn:
Giá vốn (COGS): SUM(Số lượng món bán * Đơn giá nhập nguyên liệu) (Phối hợp với TV3).
Chi phí nhân sự: SUM(TotalHours * HourlyRateAtTime) (Phối hợp với TV4).
Profit API: Profit = Revenue - (COGS + StaffCost).
2. PHẦN FRONT-END (UI/UX)
2.1. Màn hình Tra cứu & Đăng ký Thành viên (Tích hợp POS)
Chức năng: Một Widget nhỏ nằm trong màn hình POS của TV2.
Giao diện: * Ô Search SĐT nhanh.
Nếu tìm thấy: Hiện tên khách, hạng thẻ, số điểm hiện có.
Nếu không: Hiện nút "Đăng ký nhanh" (Chỉ cần nhập Tên + SĐT).
2.2. Màn hình "Kết ca" (Shift Handover)
Giao diện: * Hiển thị: Tổng doanh thu trong ca, Tổng tiền mặt theo máy tính.
Ô Input: "Nhập số tiền mặt thực tế trong két".
Ô Note: "Lý do chênh lệch" (Nếu có).
Action: Bấm "Xác nhận kết ca" -> In biên bản bàn giao tiền.
2.3. Hệ thống báo cáo BI (Business Intelligence) Thực hiện trước, các chức năng phía trên sẽ thực hiện sau khi các thành viên khác hoàn thành
Trang dành riêng cho Chủ chuỗi (Admin Web).
Biểu đồ đường (Line Chart): Theo dõi doanh thu theo ngày/tuần/tháng.
Biểu đồ tròn (Pie Chart): Cơ cấu doanh thu theo chi nhánh hoặc theo loại món ăn.
Bảng báo cáo P&L (Lợi nhuận & Lỗ): * Doanh thu: ...
Chi phí nguyên liệu: ...
Chi phí nhân sự: ...
Lợi nhuận ròng: ...



