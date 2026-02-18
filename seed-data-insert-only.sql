-- ========================================
-- AutoPulse Seed Data (только INSERT)
-- ========================================

-- Markets
INSERT INTO "Markets" ("Name", "Region", "Currency", "CreatedAt")
VALUES 
('China', 'Asia', 'CNY', NOW()),
('USA', 'North America', 'USD', NOW()),
('Europe', 'Europe', 'EUR', NOW());

-- Brands
INSERT INTO "Brands" ("Name", "Country", "CreatedAt")
VALUES 
('Audi', 'Germany', NOW()),
('BMW', 'Germany', NOW()),
('Mercedes-Benz', 'Germany', NOW()),
('Toyota', 'Japan', NOW()),
('Honda', 'Japan', NOW()),
('Volkswagen', 'Germany', NOW()),
('BYD', 'China', NOW()),
('Geely', 'China', NOW());

-- Models
INSERT INTO "Models" ("BrandId", "Name", "CreatedAt")
VALUES 
(1, 'A8', NOW()),
(1, 'A6', NOW()),
(1, 'Q7', NOW()),
(1, 'A4', NOW()),
(2, 'X5', NOW()),
(2, '3 Series', NOW()),
(3, 'S-Class', NOW()),
(3, 'E-Class', NOW()),
(4, 'Camry', NOW()),
(5, 'Civic', NOW());

-- Dealers
INSERT INTO "Dealers" ("Name", "Rating", "ContactInfo", "Address", "MarketId", "CreatedAt")
VALUES 
('北京世纪双合旧机动车经纪有限公司', 4.8, '电话：+86-10-12345678', '北京丰台区南四环西路 123 号西厅 D 道', 1, NOW()),
('北京奥迪官方经销商', 4.9, '电话：+86-10-87654321', '北京市朝阳区建国路 88 号', 1, NOW()),
('上海二手车交易中心', 4.5, '电话：+86-21-11223344', '上海市浦东新区沪南路 500 号', 1, NOW()),
('广州宝马授权经销商', 4.7, '电话：+86-20-99887766', '广州市天河区天河路 200 号', 1, NOW()),
('深圳奔驰体验中心', 4.6, '电话：+86-755-66778899', '深圳市福田区深南大道 1000 号', 1, NOW());

-- Cars (без Description так как колонки нет)
INSERT INTO "Cars" ("DealerId", "MarketId", "BrandId", "ModelId", "Year", "Price", "Currency", "Mileage", "IsAvailable", "CreatedAt")
VALUES 
-- Audi A8 из HTML
(1, 1, 1, 1, 2023, 668000, 'CNY', 18000, TRUE, NOW()),

-- Дополнительные Audi
(2, 1, 1, 1, 2024, 720000, 'CNY', 5000, TRUE, NOW()),
(1, 1, 1, 1, 2022, 580000, 'CNY', 35000, TRUE, NOW()),
(2, 1, 1, 2, 2023, 420000, 'CNY', 12000, TRUE, NOW()),
(3, 1, 1, 2, 2022, 380000, 'CNY', 28000, FALSE, NOW()),
(1, 1, 1, 3, 2023, 680000, 'CNY', 15000, TRUE, NOW()),

-- BMW
(4, 1, 2, 5, 2023, 750000, 'CNY', 10000, TRUE, NOW()),
(2, 1, 2, 5, 2022, 680000, 'CNY', 22000, TRUE, NOW()),
(4, 1, 2, 6, 2023, 350000, 'CNY', 8000, TRUE, NOW()),

-- Mercedes-Benz
(5, 1, 3, 7, 2023, 980000, 'CNY', 6000, TRUE, NOW()),
(3, 1, 3, 7, 2022, 850000, 'CNY', 18000, TRUE, NOW()),
(5, 1, 3, 8, 2023, 520000, 'CNY', 11000, TRUE, NOW()),

-- Toyota
(3, 1, 4, 9, 2023, 220000, 'CNY', 15000, TRUE, NOW()),
(1, 1, 4, 9, 2022, 195000, 'CNY', 32000, TRUE, NOW()),

-- Honda
(3, 1, 5, 10, 2023, 180000, 'CNY', 9000, TRUE, NOW());

-- DataSources
INSERT INTO "DataSources" ("Name", "Country", "BaseUrl", "IsActive", "CreatedAt")
VALUES 
('Autohome', 'China', 'https://www.autohome.com.cn', TRUE, NOW()),
('Che168', 'China', 'https://www.che168.com', TRUE, NOW()),
('Cars.com', 'USA', 'https://www.cars.com', FALSE, NOW());

-- ========================================
-- Проверка данных
-- ========================================
SELECT 'Markets' as TableName, COUNT(*) as RecordCount FROM "Markets"
UNION ALL
SELECT 'Brands', COUNT(*) FROM "Brands"
UNION ALL
SELECT 'Models', COUNT(*) FROM "Models"
UNION ALL
SELECT 'Dealers', COUNT(*) FROM "Dealers"
UNION ALL
SELECT 'Cars', COUNT(*) FROM "Cars"
UNION ALL
SELECT 'DataSources', COUNT(*) FROM "DataSources";
