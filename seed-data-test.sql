-- Тестовые данные для AutoPulse
-- Очистка таблиц (если есть данные)
TRUNCATE "Cars", "Models", "Dealers", "Brands", "Markets", "DataSources" RESTART IDENTITY CASCADE;

-- Создаём рынок (Китай)
INSERT INTO "Markets" ("Id", "Name", "Region", "Currency", "CreatedAt")
VALUES (1, 'China', 'Asia', 'CNY', NOW())
ON CONFLICT ("Id") DO NOTHING;

-- Создаём источник данных
INSERT INTO "DataSources" ("Id", "Name", "Country", "BaseUrl", "Language", "IsActive", "CreatedAt")
VALUES (1, 'Che168', 'China', 'https://www.che168.com', 'zh-CN', TRUE, NOW())
ON CONFLICT ("Id") DO NOTHING;

-- Создаём бренды
INSERT INTO "Brands" ("Id", "Name", "Country", "CreatedAt", "UpdatedAt")
VALUES 
    (1, 'Audi', 'Germany', NOW(), NULL),
    (2, 'BMW', 'Germany', NOW(), NULL),
    (3, 'Mercedes-Benz', 'Germany', NOW(), NULL),
    (4, 'Toyota', 'Japan', NOW(), NULL),
    (5, 'Honda', 'Japan', NOW(), NULL)
ON CONFLICT ("Id") DO NOTHING;

-- Создаём модели
INSERT INTO "Models" ("Id", "BrandId", "Name", "Category", "CreatedAt", "UpdatedAt")
VALUES 
    (1, 1, 'A6L', 'Sedan', NOW(), NULL),
    (2, 1, 'A8', 'Sedan', NOW(), NULL),
    (3, 1, 'Q7', 'SUV', NOW(), NULL),
    (4, 2, '5 Series', 'Sedan', NOW(), NULL),
    (5, 2, 'X5', 'SUV', NOW(), NULL),
    (6, 3, 'E-Class', 'Sedan', NOW(), NULL),
    (7, 4, 'Camry', 'Sedan', NOW(), NULL),
    (8, 5, 'Civic', 'Sedan', NOW(), NULL)
ON CONFLICT ("Id") DO NOTHING;

-- Создаём дилеров
INSERT INTO "Dealers" ("Id", "Name", "ContactInfo", "Address", "Rating", "MarketId", "CreatedAt", "UpdatedAt")
VALUES 
    (1, 'Beijing Audi Dealer', 'info@beijingaudi.cn', 'Beijing, Chaoyang District', 4.5, 1, NOW(), NULL),
    (2, 'Shanghai BMW Center', 'sales@shanghaibmw.cn', 'Shanghai, Pudong', 4.7, 1, NOW(), NULL),
    (3, 'Guangzhou Mercedes', 'contact@gzmercedes.cn', 'Guangzhou, Tianhe', 4.3, 1, NOW(), NULL)
ON CONFLICT ("Id") DO NOTHING;

-- Создаём автомобили
INSERT INTO "Cars" ("Id", "BrandId", "ModelId", "MarketId", "DealerId", "DataSourceId", "Year", "Price", "Currency", "Vin", "Mileage", "Transmission", "Engine", "FuelType", "Color", "Location", "Country", "SourceUrl", "ImageUrl", "IsAvailable", "CreatedAt", "UpdatedAt")
VALUES 
    (1, 1, 2, 1, 1, 1, 2023, 668000, 'CNY', 'WAUZZZ4G0EN123456', 18000, 'Automatic', '3.0T V6', 'Petrol', 'Black', 'Beijing', 'China', 'https://www.che168.com/beijing/aodi/aodia8/s58708/', 'https://example.com/audi-a8.jpg', TRUE, NOW(), NULL),
    (2, 1, 2, 1, 1, 1, 2024, 720000, 'CNY', 'WAUZZZ4G1FN654321', 5000, 'Automatic', '3.0T V6', 'Petrol', 'White', 'Beijing', 'China', 'https://www.che168.com/beijing/aodi/aodia8/2024/', 'https://example.com/audi-a8-2024.jpg', TRUE, NOW(), NULL),
    (3, 1, 1, 1, 1, 1, 2023, 420000, 'CNY', 'WAUZZZ4G2DN789012', 12000, 'Automatic', '2.0T I4', 'Petrol', 'Silver', 'Beijing', 'China', 'https://www.che168.com/beijing/aodi/aodia6/2023/', 'https://example.com/audi-a6l.jpg', TRUE, NOW(), NULL),
    (4, 2, 5, 1, 2, 1, 2023, 750000, 'CNY', 'WBAKB8C50DF345678', 10000, 'Automatic', '3.0T I6', 'Petrol', 'Gray', 'Shanghai', 'China', 'https://www.che168.com/shanghai/bmw/bmwx5/2023/', 'https://example.com/bmw-x5.jpg', TRUE, NOW(), NULL),
    (5, 3, 6, 1, 3, 1, 2023, 980000, 'CNY', 'WDD2130001A123456', 6000, 'Automatic', '2.0T I4', 'Petrol', 'Black', 'Shenzhen', 'China', 'https://www.che168.com/shenzhen/mercedes/mercedess/2023/', 'https://example.com/mercedes-e.jpg', TRUE, NOW(), NULL),
    (6, 4, 7, 1, 1, 1, 2023, 220000, 'CNY', '4T1BF1FK5DU123456', 15000, 'Automatic', '2.5L I4', 'Hybrid', 'White', 'Shanghai', 'China', 'https://www.che168.com/shanghai/toyota/toyotacamry/2023/', 'https://example.com/toyota-camry.jpg', TRUE, NOW(), NULL),
    (7, 5, 8, 1, 1, 1, 2023, 180000, 'CNY', '19XFC2F59DE123456', 9000, 'CVT', '1.5T I4', 'Petrol', 'Blue', 'Shanghai', 'China', 'https://www.che168.com/shanghai/honda/hondacivic/2023/', 'https://example.com/honda-civic.jpg', TRUE, NOW(), NULL)
ON CONFLICT ("Id") DO NOTHING;

-- Обновляем индексы
SELECT setval('"Brands_Id_seq"', (SELECT MAX("Id") FROM "Brands"));
SELECT setval('"Models_Id_seq"', (SELECT MAX("Id") FROM "Models"));
SELECT setval('"Dealers_Id_seq"', (SELECT MAX("Id") FROM "Dealers"));
SELECT setval('"Cars_Id_seq"', (SELECT MAX("Id") FROM "Cars"));
