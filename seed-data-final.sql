-- ========================================
-- AutoPulse Seed Data - Правильная версия
-- ========================================

-- Brands (пропускаем если уже есть)
INSERT INTO "Brands" ("Name", "Country", "CreatedAt")
SELECT 'Audi', 'Germany', NOW() WHERE NOT EXISTS (SELECT 1 FROM "Brands" WHERE "Name" = 'Audi');
INSERT INTO "Brands" ("Name", "Country", "CreatedAt")
SELECT 'BMW', 'Germany', NOW() WHERE NOT EXISTS (SELECT 1 FROM "Brands" WHERE "Name" = 'BMW');
INSERT INTO "Brands" ("Name", "Country", "CreatedAt")
SELECT 'Mercedes-Benz', 'Germany', NOW() WHERE NOT EXISTS (SELECT 1 FROM "Brands" WHERE "Name" = 'Mercedes-Benz');
INSERT INTO "Brands" ("Name", "Country", "CreatedAt")
SELECT 'Toyota', 'Japan', NOW() WHERE NOT EXISTS (SELECT 1 FROM "Brands" WHERE "Name" = 'Toyota');
INSERT INTO "Brands" ("Name", "Country", "CreatedAt")
SELECT 'Honda', 'Japan', NOW() WHERE NOT EXISTS (SELECT 1 FROM "Brands" WHERE "Name" = 'Honda');

-- Models
INSERT INTO "Models" ("BrandId", "Name", "CreatedAt")
SELECT 1, 'A8', NOW() WHERE NOT EXISTS (SELECT 1 FROM "Models" WHERE "Name" = 'A8');
INSERT INTO "Models" ("BrandId", "Name", "CreatedAt")
SELECT 1, 'A6', NOW() WHERE NOT EXISTS (SELECT 1 FROM "Models" WHERE "Name" = 'A6');
INSERT INTO "Models" ("BrandId", "Name", "CreatedAt")
SELECT 1, 'Q7', NOW() WHERE NOT EXISTS (SELECT 1 FROM "Models" WHERE "Name" = 'Q7');
INSERT INTO "Models" ("BrandId", "Name", "CreatedAt")
SELECT 2, 'X5', NOW() WHERE NOT EXISTS (SELECT 1 FROM "Models" WHERE "Name" = 'X5');
INSERT INTO "Models" ("BrandId", "Name", "CreatedAt")
SELECT 3, 'S-Class', NOW() WHERE NOT EXISTS (SELECT 1 FROM "Models" WHERE "Name" = 'S-Class');

-- Dealers
INSERT INTO "Dealers" ("Name", "Rating", "ContactInfo", "Address", "MarketId", "CreatedAt")
SELECT '北京世纪双合旧机动车经纪有限公司', 4.8, '电话：+86-10-12345678', '北京丰台区南四环西路 123 号西厅 D 道', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "Dealers" WHERE "Name" = '北京世纪双合旧机动车经纪有限公司');

INSERT INTO "Dealers" ("Name", "Rating", "ContactInfo", "Address", "MarketId", "CreatedAt")
SELECT '北京奥迪官方经销商', 4.9, '电话：+86-10-87654321', '北京市朝阳区建国路 88 号', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "Dealers" WHERE "Name" = '北京奥迪官方经销商');

INSERT INTO "Dealers" ("Name", "Rating", "ContactInfo", "Address", "MarketId", "CreatedAt")
SELECT '上海二手车交易中心', 4.5, '电话：+86-21-11223344', '上海市浦东新区沪南路 500 号', 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "Dealers" WHERE "Name" = '上海二手车交易中心');

-- Cars (с обязательным SourceUrl)
INSERT INTO "Cars" ("DealerId", "MarketId", "BrandId", "ModelId", "Year", "Price", "Currency", "Mileage", "IsAvailable", "SourceUrl", "CreatedAt")
SELECT 1, 1, 1, 1, 2023, 668000, 'CNY', 18000, TRUE, 'https://www.che168.com/beijing/aodi/aodia8/s58708/', NOW()
WHERE NOT EXISTS (SELECT 1 FROM "Cars" WHERE "SourceUrl" = 'https://www.che168.com/beijing/aodi/aodia8/s58708/');

INSERT INTO "Cars" ("DealerId", "MarketId", "BrandId", "ModelId", "Year", "Price", "Currency", "Mileage", "IsAvailable", "SourceUrl", "CreatedAt")
SELECT 2, 1, 1, 1, 2024, 720000, 'CNY', 5000, TRUE, 'https://www.che168.com/beijing/aodi/aodia8/2024/', NOW()
WHERE NOT EXISTS (SELECT 1 FROM "Cars" WHERE "SourceUrl" = 'https://www.che168.com/beijing/aodi/aodia8/2024/');

INSERT INTO "Cars" ("DealerId", "MarketId", "BrandId", "ModelId", "Year", "Price", "Currency", "Mileage", "IsAvailable", "SourceUrl", "CreatedAt")
SELECT 1, 1, 1, 2, 2023, 420000, 'CNY', 12000, TRUE, 'https://www.che168.com/beijing/aodi/aodia6/2023/', NOW()
WHERE NOT EXISTS (SELECT 1 FROM "Cars" WHERE "SourceUrl" = 'https://www.che168.com/beijing/aodi/aodia6/2023/');

INSERT INTO "Cars" ("DealerId", "MarketId", "BrandId", "ModelId", "Year", "Price", "Currency", "Mileage", "IsAvailable", "SourceUrl", "CreatedAt")
SELECT 3, 1, 2, 5, 2023, 750000, 'CNY', 10000, TRUE, 'https://www.che168.com/shanghai/bmw/bmwx5/2023/', NOW()
WHERE NOT EXISTS (SELECT 1 FROM "Cars" WHERE "SourceUrl" = 'https://www.che168.com/shanghai/bmw/bmwx5/2023/');

INSERT INTO "Cars" ("DealerId", "MarketId", "BrandId", "ModelId", "Year", "Price", "Currency", "Mileage", "IsAvailable", "SourceUrl", "CreatedAt")
SELECT 2, 1, 3, 5, 2023, 980000, 'CNY', 6000, TRUE, 'https://www.che168.com/beijing/mercedes/mercedess/2023/', NOW()
WHERE NOT EXISTS (SELECT 1 FROM "Cars" WHERE "SourceUrl" = 'https://www.che168.com/beijing/mercedes/mercedess/2023/');

-- Проверка
SELECT 'Brands' as TableName, COUNT(*) as RecordCount FROM "Brands"
UNION ALL
SELECT 'Models', COUNT(*) FROM "Models"
UNION ALL
SELECT 'Dealers', COUNT(*) FROM "Dealers"
UNION ALL
SELECT 'Cars', COUNT(*) FROM "Cars";
