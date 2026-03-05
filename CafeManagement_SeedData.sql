-- =============================================================
-- CafeManagement - Seed Data
-- Chạy sau khi đã thực hiện: dotnet ef database update
-- =============================================================

-- Shifts (Ca làm cố định)
INSERT INTO Shifts (ShiftName, StartTime, EndTime) VALUES
    (N'Ca Sáng',  '06:00', '12:00'),
    (N'Ca Chiều', '12:00', '18:00'),
    (N'Ca Tối',   '18:00', '00:00');

-- PaymentMethods
INSERT INTO PaymentMethods (MethodName, IsActive) VALUES
    (N'Tiền mặt',      1),
    (N'Chuyển khoản',  1),
    (N'Momo',          1),
    (N'VNPay',         1);

-- JobPositions (tên + lương/giờ)
INSERT INTO JobPositions (PositionName, HourlyRate, IsActive) VALUES
    (N'Quản lý',   35000, 1),
    (N'Pha chế',   25000, 1),
    (N'Thu ngân',  22000, 1),
    (N'Rửa chén',  18000, 1);

-- Stores (Chi nhánh mẫu)
INSERT INTO Stores (Name, Address, Phone, OpenTime, CloseTime, IsActive) VALUES
    (N'Chi nhánh Quận 1', N'123 Nguyễn Huệ, Q.1, TP.HCM',  N'028-1234-5678', '06:00', '23:00', 1),
    (N'Chi nhánh Quận 3', N'45 Lê Văn Sỹ, Q.3, TP.HCM',    N'028-8765-4321', '06:00', '23:00', 1);

-- Categories
INSERT INTO Categories (Name, Description, IsActive) VALUES
    (N'Cà phê',        N'Các loại cà phê truyền thống và hiện đại', 1),
    (N'Trà trái cây',  N'Trà tươi kết hợp trái cây tươi',           1),
    (N'Đá xay',        N'Thức uống đá xay mát lạnh',                 1),
    (N'Bánh & Snack',  N'Bánh ngọt và đồ ăn nhẹ',                   1);

-- MenuItems (CategoryId: 1=Cafe, 2=Tra, 3=Da xay, 4=Banh)
INSERT INTO MenuItems (CategoryId, Name, Description, BasePrice, IsActive) VALUES
    (1, N'Cà phê sữa đá',   N'Cà phê phin truyền thống với sữa đặc',    35000, 1),
    (1, N'Bạc xỉu',         N'Cà phê sữa tươi kiểu Sài Gòn',            40000, 1),
    (1, N'Cà phê đen đá',   N'Cà phê phin đậm đặc không đường',         30000, 1),
    (1, N'Cappuccino',       N'Espresso kết hợp sữa bọt mịn',            55000, 1),
    (2, N'Trà đào cam sả',  N'Trà xanh đào cam sả thơm mát',            45000, 1),
    (2, N'Trà vải',         N'Trà vải tươi ngọt thanh',                  45000, 1),
    (3, N'Đá xay socola',   N'Socola Bỉ xay đá mịn',                    55000, 1),
    (3, N'Đá xay matcha',   N'Matcha Nhật xay đá',                       55000, 1);

-- Toppings
INSERT INTO Toppings (Name, Price, IsActive) VALUES
    (N'Trân châu trắng',  10000, 1),
    (N'Trân châu đen',    10000, 1),
    (N'Thạch cà phê',     8000,  1),
    (N'Thạch trái cây',   8000,  1),
    (N'Pudding trứng',    15000, 1),
    (N'Kem cheese',       15000, 1);

-- Ingredients
INSERT INTO Ingredients (Name, Unit, MinStockLevel, IsActive) VALUES
    (N'Cà phê hạt rang',     N'gram',  500,  1),
    (N'Sữa đặc có đường',    N'gram',  1000, 1),
    (N'Sữa tươi không đường',N'ml',    2000, 1),
    (N'Đường',               N'gram',  500,  1),
    (N'Đá viên',             N'gram',  5000, 1),
    (N'Trà xanh',            N'gram',  200,  1),
    (N'Đào tươi',            N'gram',  1000, 1),
    (N'Bột matcha',          N'gram',  200,  1),
    (N'Bột socola',          N'gram',  300,  1),
    (N'Kem tươi',            N'ml',    500,  1);

-- Recipes (Công thức cho từng món - lượng cho 1 ly)
-- MenuItem 1: Cà phê sữa đá
INSERT INTO Recipes (MenuItemId, IngredientId, Quantity, WastePercent) VALUES
    (1, 1, 25, 5),    -- 25g cà phê hạt
    (1, 2, 40, 0),    -- 40g sữa đặc
    (1, 5, 200, 0);   -- 200g đá

-- MenuItem 2: Bạc xỉu
INSERT INTO Recipes (MenuItemId, IngredientId, Quantity, WastePercent) VALUES
    (2, 1, 15, 5),    -- 15g cà phê
    (2, 2, 30, 0),    -- 30g sữa đặc
    (2, 3, 100, 0),   -- 100ml sữa tươi
    (2, 5, 200, 0);   -- 200g đá

-- MenuItem 3: Cà phê đen đá
INSERT INTO Recipes (MenuItemId, IngredientId, Quantity, WastePercent) VALUES
    (3, 1, 25, 5),    -- 25g cà phê
    (3, 5, 200, 0);   -- 200g đá

-- MenuItem 5: Trà đào cam sả
INSERT INTO Recipes (MenuItemId, IngredientId, Quantity, WastePercent) VALUES
    (5, 6, 5,   5),   -- 5g trà xanh
    (5, 7, 80,  2),   -- 80g đào
    (5, 4, 20,  0),   -- 20g đường
    (5, 5, 200, 0);   -- 200g đá

-- InventoryStocks (Tồn kho ban đầu cho Chi nhánh 1)
INSERT INTO InventoryStocks (StoreId, IngredientId, CurrentQuantity) VALUES
    (1, 1, 2000),   -- 2kg cà phê
    (1, 2, 5000),   -- 5kg sữa đặc
    (1, 3, 10000),  -- 10 lít sữa tươi
    (1, 4, 3000),   -- 3kg đường
    (1, 5, 20000),  -- 20kg đá
    (1, 6, 500),    -- 500g trà xanh
    (1, 7, 3000),   -- 3kg đào
    (1, 8, 500),    -- 500g bột matcha
    (1, 9, 800),    -- 800g bột socola
    (1, 10, 2000);  -- 2 lít kem tươi
