-- Seed Data для AutoPulse
-- Данные из китайского сайта che168.com

-- ========================================
-- 1. Рынки (Markets)
-- ========================================
INSERT INTO "Markets" ("Id", "Name", "Region", "Currency", "CreatedAt")
VALUES 
(1, 'China', 'Asia', 'CNY', NOW()),
(2, 'USA', 'North America', 'USD', NOW()),
(3, 'Europe', 'Europe', 'EUR', NOW());

-- ========================================
-- 2. Бренды (Brands)
-- ========================================
INSERT INTO "Brands" ("Id", "Name", "Country", "CreatedAt")
VALUES 
(1, 'Audi', 'Germany', NOW()),
(2, 'BMW', 'Germany', NOW()),
(3, 'Mercedes-Benz', 'Germany', NOW()),
(4, 'Toyota', 'Japan', NOW()),
(5, 'Honda', 'Japan', NOW()),
(6, 'Volkswagen', 'Germany', NOW()),
(7, 'BYD', 'China', NOW()),
(8, 'Geely', 'China', NOW());

-- ========================================
-- 3. Модели (Models)
-- ========================================
INSERT INTO "Models" ("Id", "BrandId", "Name", "CreatedAt")
VALUES 
(1, 1, 'A8', NOW()),
(2, 1, 'A6', NOW()),
(3, 1, 'Q7', NOW()),
(4, 1, 'A4', NOW()),
(5, 2, 'X5', NOW()),
(6, 2, '3 Series', NOW()),
(7, 3, 'S-Class', NOW()),
(8, 3, 'E-Class', NOW()),
(9, 4, 'Camry', NOW()),
(10, 5, 'Civic', NOW());

-- ========================================
-- 4. Дилеры (Dealers)
-- ========================================
INSERT INTO "Dealers" ("Id", "Name", "Rating", "ContactInfo", "Address", "MarketId", "CreatedAt", "UpdatedAt")
VALUES 
(1, '北京世纪双合旧机动车经纪有限公司', 4.8, '电话：+86-10-12345678', '北京丰台区南四环西路123号西厅D道', 1, NOW(), NULL),
(2, '北京奥迪官方经销商', 4.9, '电话：+86-10-87654321', '北京市朝阳区建国路88号', 1, NOW(), NULL),
(3, '上海二手车交易中心', 4.5, '电话：+86-21-11223344', '上海市浦东新区沪南路500号', 1, NOW(), NULL),
(4, '广州宝马授权经销商', 4.7, '电话：+86-20-99887766', '广州市天河区天河路200号', 1, NOW(), NULL),
(5, '深圳奔驰体验中心', 4.6, '电话：+86-755-66778899', '深圳市福田区深南大道1000号', 1, NOW(), NULL);

-- ========================================
-- 5. Автомобили (Cars)
-- ========================================
INSERT INTO "Cars" (
    "Id", "DealerId", "MarketId", "BrandId", "ModelId", 
    "Year", "Price", "Currency", "Mileage", "IsAvailable", 
    "Description", "CreatedAt", "UpdatedAt"
)
VALUES 
-- Audi A8 от основного дилера
(1, 1, 1, 1, 1, 2023, 668000, 'CNY', 18000, TRUE, 
 'Audi A8L 55 TFSI quattro Flagship. Белый цвет, 3.0L V6, автоматическая коробка передач. 
  Особенности: адаптивный круиз-контроль, пневмоподвеска, матричные LED фары, 
  панорамная крыша, кожаный салон с вентиляцией.', NOW(), NULL),

-- Дополнительные Audi A8
(2, 2, 1, 1, 1, 2024, 720000, 'CNY', 5000, TRUE, 
 'Audi A8L 60 TFSI e quattro. Гибрид, черный цвет, максимальная комплектация.', NOW(), NULL),

(3, 1, 1, 1, 1, 2022, 580000, 'CNY', 35000, TRUE, 
 'Audi A8L 50 TDI quattro. Дизель, серый цвет, отличное состояние.', NOW(), NULL),

-- Audi A6
(4, 2, 1, 1, 2, 2023, 420000, 'CNY', 12000, TRUE, 
 'Audi A6L 45 TFSI quattro. Белый цвет, бизнес-класс.', NOW(), NULL),

(5, 3, 1, 1, 2, 2022, 380000, 'CNY', 28000, FALSE, 
 'Audi A6L 40 TFSI. Черный цвет, продан.', NOW(), NULL),

-- Audi Q7
(6, 1, 1, 1, 3, 2023, 680000, 'CNY', 15000, TRUE, 
 'Audi Q7 55 TFSI quattro. Внедорожник, 7 мест, черный цвет.', NOW(), NULL),

-- BMW X5
(7, 4, 1, 2, 5, 2023, 750000, 'CNY', 10000, TRUE, 
 'BMW X5 xDrive40i. Белый цвет, M-пакет, панорамная крыша.', NOW(), NULL),

(8, 2, 1, 2, 5, 2022, 680000, 'CNY', 22000, TRUE, 
 'BMW X5 xDrive30d. Дизель, серый цвет, 7 мест.', NOW(), NULL),

-- BMW 3 Series
(9, 4, 1, 2, 6, 2023, 350000, 'CNY', 8000, TRUE, 
 'BMW 330i M Sport. Красный цвет, спортивная версия.', NOW(), NULL),

-- Mercedes S-Class
(10, 5, 1, 3, 7, 2023, 980000, 'CNY', 6000, TRUE, 
 'Mercedes-Benz S500L 4MATIC. Черный цвет, максимальная комплектация, массаж сидений.', NOW(), NULL),

(11, 3, 1, 3, 7, 2022, 850000, 'CNY', 18000, TRUE, 
 'Mercedes-Benz S450L. Белый цвет, бизнес-класс.', NOW(), NULL),

-- Mercedes E-Class
(12, 5, 1, 3, 8, 2023, 520000, 'CNY', 11000, TRUE, 
 'Mercedes-Benz E300L. Серебристый цвет, AMG-пакет.', NOW(), NULL),

-- Toyota Camry
(13, 3, 1, 4, 9, 2023, 220000, 'CNY', 15000, TRUE, 
 'Toyota Camry 2.5G. Белый цвет, гибрид, надежный и экономичный.', NOW(), NULL),

(14, 1, 1, 4, 9, 2022, 195000, 'CNY', 32000, TRUE, 
 'Toyota Camry 2.0G. Черный цвет, отличное состояние.', NOW(), NULL),

-- Honda Civic
(15, 3, 1, 5, 10, 2023, 180000, 'CNY', 9000, TRUE, 
 'Honda Civic 220TURBO. Синий цвет, спортивный дизайн.', NOW(), NULL);

-- ========================================
-- 6. Источники данных (DataSources)
-- ========================================
INSERT INTO "DataSources" ("Id", "Name", "Country", "BaseUrl", "IsActive", "CreatedAt")
VALUES 
(1, 'Autohome', 'China', 'https://www.autohome.com.cn', TRUE, NOW()),
(2, 'Che168', 'China', 'https://www.che168.com', TRUE, NOW()),
(3, 'Cars.com', 'USA', 'https://www.cars.com', FALSE, NOW());

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
