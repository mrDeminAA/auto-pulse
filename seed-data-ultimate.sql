-- ========================================
-- AutoPulse Seed Data - Финальная версия с правильными ID
-- ========================================

-- Cars (DealerId=10-14, MarketId=2, BrandId: Audi=2/BMW=3/Mercedes=4/Toyota=5/Honda=6, ModelId: A8=17/A6=18/Q7=19/X5=15/S-Class=16/Camry=22/Civic=23)
INSERT INTO "Cars" ("DealerId", "MarketId", "BrandId", "ModelId", "Year", "Price", "Currency", "Mileage", "IsAvailable", "SourceUrl", "CreatedAt") VALUES
-- Audi A8 из HTML (основной автомобиль) - Dealer: 北京世纪双合 (Id=10), Model: A8 (Id=17)
(10, 2, 2, 17, 2023, 668000, 'CNY', 18000, TRUE, 'https://www.che168.com/beijing/aodi/aodia8/s58708/', NOW()),

-- Дополнительные Audi A8 - Dealer: 北京奥迪官方 (Id=11)
(11, 2, 2, 17, 2024, 720000, 'CNY', 5000, TRUE, 'https://www.che168.com/beijing/aodi/aodia8/2024/', NOW()),
(10, 2, 2, 17, 2022, 580000, 'CNY', 35000, TRUE, 'https://www.che168.com/beijing/aodi/aodia8/used2022/', NOW()),

-- Audi A6 (ModelId=18)
(11, 2, 2, 18, 2023, 420000, 'CNY', 12000, TRUE, 'https://www.che168.com/beijing/aodi/aodia6/2023/', NOW()),
(12, 2, 2, 18, 2022, 380000, 'CNY', 28000, FALSE, 'https://www.che168.com/shanghai/aodi/aodia6/used2022/', NOW()),

-- Audi Q7 (ModelId=19)
(10, 2, 2, 19, 2023, 680000, 'CNY', 15000, TRUE, 'https://www.che168.com/beijing/aodi/aodiq7/2023/', NOW()),

-- BMW X5 (DealerId=13, BrandId=3, ModelId=15)
(13, 2, 3, 15, 2023, 750000, 'CNY', 10000, TRUE, 'https://www.che168.com/guangzhou/bmw/bmwx5/2023/', NOW()),
(11, 2, 3, 15, 2022, 680000, 'CNY', 22000, TRUE, 'https://www.che168.com/beijing/bmw/bmwx5/used2022/', NOW()),

-- Mercedes S-Class (DealerId=14, BrandId=4, ModelId=16)
(14, 2, 4, 16, 2023, 980000, 'CNY', 6000, TRUE, 'https://www.che168.com/shenzhen/mercedes/mercedess/2023/', NOW()),
(12, 2, 4, 16, 2022, 850000, 'CNY', 18000, TRUE, 'https://www.che168.com/shanghai/mercedes/mercedess/used2022/', NOW()),

-- Toyota Camry (BrandId=5, ModelId=22)
(12, 2, 5, 22, 2023, 220000, 'CNY', 15000, TRUE, 'https://www.che168.com/shanghai/toyota/toyotacamry/2023/', NOW()),
(10, 2, 5, 22, 2022, 195000, 'CNY', 32000, TRUE, 'https://www.che168.com/beijing/toyota/toyotacamry/used2022/', NOW()),

-- Honda Civic (BrandId=6, ModelId=23)
(12, 2, 6, 23, 2023, 180000, 'CNY', 9000, TRUE, 'https://www.che168.com/shanghai/honda/hondacivic/2023/', NOW());

-- ========================================
-- Проверка данных
-- ========================================
SELECT 'Cars' as TableName, COUNT(*) as RecordCount FROM "Cars";

-- ========================================
-- Показать все автомобили с деталями
-- ========================================
SELECT 
    c."Id" as CarId,
    c."Year",
    b."Name" as Brand,
    m."Name" as Model,
    c."Price",
    c."Currency",
    c."Mileage",
    d."Name" as Dealer,
    c."IsAvailable",
    c."SourceUrl"
FROM "Cars" c
JOIN "Brands" b ON c."BrandId" = b."Id"
JOIN "Models" m ON c."ModelId" = m."Id"
JOIN "Dealers" d ON c."DealerId" = d."Id"
ORDER BY c."Id";
