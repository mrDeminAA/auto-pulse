-- ========================================
-- AutoPulse Seed Data - Версия с правильными ID
-- ========================================

-- Models (BrandId: Audi=2, BMW=3, Mercedes=4, Toyota=5, Honda=6)
INSERT INTO "Models" ("BrandId", "Name", "CreatedAt") VALUES
(2, 'A8', NOW()),
(2, 'A6', NOW()),
(2, 'Q7', NOW()),
(3, 'X5', NOW()),
(4, 'S-Class', NOW()),
(5, 'Camry', NOW()),
(6, 'Civic', NOW());

-- Dealers (MarketId: China=2)
INSERT INTO "Dealers" ("Name", "Rating", "ContactInfo", "Address", "MarketId", "CreatedAt") VALUES
('北京世纪双合旧机动车经纪有限公司', 4.8, '电话：+86-10-12345678', '北京丰台区南四环西路 123 号西厅 D 道', 2, NOW()),
('北京奥迪官方经销商', 4.9, '电话：+86-10-87654321', '北京市朝阳区建国路 88 号', 2, NOW()),
('上海二手车交易中心', 4.5, '电话：+86-21-11223344', '上海市浦东新区沪南路 500 号', 2, NOW()),
('广州宝马授权经销商', 4.7, '电话：+86-20-99887766', '广州市天河区天河路 200 号', 2, NOW()),
('深圳奔驰体验中心', 4.6, '电话：+86-755-66778899', '深圳市福田区深南大道 1000 号', 2, NOW());

-- Cars (с правильными ID: DealerId=1-5, MarketId=2, BrandId=2-6, ModelId=1-7, DataSourceId=2)
INSERT INTO "Cars" ("DealerId", "MarketId", "BrandId", "ModelId", "Year", "Price", "Currency", "Mileage", "IsAvailable", "SourceUrl", "CreatedAt") VALUES
-- Audi A8 из HTML (основной автомобиль)
(1, 2, 2, 1, 2023, 668000, 'CNY', 18000, TRUE, 'https://www.che168.com/beijing/aodi/aodia8/s58708/', NOW()),

-- Дополнительные Audi A8
(2, 2, 2, 1, 2024, 720000, 'CNY', 5000, TRUE, 'https://www.che168.com/beijing/aodi/aodia8/2024/', NOW()),
(1, 2, 2, 1, 2022, 580000, 'CNY', 35000, TRUE, 'https://www.che168.com/beijing/aodi/aodia8/used2022/', NOW()),

-- Audi A6
(2, 2, 2, 2, 2023, 420000, 'CNY', 12000, TRUE, 'https://www.che168.com/beijing/aodi/aodia6/2023/', NOW()),
(3, 2, 2, 2, 2022, 380000, 'CNY', 28000, FALSE, 'https://www.che168.com/shanghai/aodi/aodia6/used2022/', NOW()),

-- Audi Q7
(1, 2, 2, 3, 2023, 680000, 'CNY', 15000, TRUE, 'https://www.che168.com/beijing/aodi/aodiq7/2023/', NOW()),

-- BMW X5
(4, 2, 3, 4, 2023, 750000, 'CNY', 10000, TRUE, 'https://www.che168.com/guangzhou/bmw/bmwx5/2023/', NOW()),
(2, 2, 3, 4, 2022, 680000, 'CNY', 22000, TRUE, 'https://www.che168.com/beijing/bmw/bmwx5/used2022/', NOW()),

-- Mercedes S-Class
(5, 2, 4, 5, 2023, 980000, 'CNY', 6000, TRUE, 'https://www.che168.com/shenzhen/mercedes/mercedess/2023/', NOW()),
(3, 2, 4, 5, 2022, 850000, 'CNY', 18000, TRUE, 'https://www.che168.com/shanghai/mercedes/mercedess/used2022/', NOW()),

-- Toyota Camry
(3, 2, 5, 6, 2023, 220000, 'CNY', 15000, TRUE, 'https://www.che168.com/shanghai/toyota/toyotacamry/2023/', NOW()),
(1, 2, 5, 6, 2022, 195000, 'CNY', 32000, TRUE, 'https://www.che168.com/beijing/toyota/toyotacamry/used2022/', NOW()),

-- Honda Civic
(3, 2, 6, 7, 2023, 180000, 'CNY', 9000, TRUE, 'https://www.che168.com/shanghai/honda/hondacivic/2023/', NOW());

-- ========================================
-- Проверка данных
-- ========================================
SELECT 'Models' as TableName, COUNT(*) as RecordCount FROM "Models"
UNION ALL
SELECT 'Dealers', COUNT(*) FROM "Dealers"
UNION ALL
SELECT 'Cars', COUNT(*) FROM "Cars";
