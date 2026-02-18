-- Быстрое наполнение базы тестовыми данными с реальными изображениями
-- Вставляем бренды, рынки, модели, дилеров и автомобили

-- Рынок (Китай)
INSERT INTO "Markets" ("Id", "Name", "Region", "Currency", "CreatedAt")
VALUES (1, 'China', 'China', 'CNY', NOW())
ON CONFLICT ("Id") DO NOTHING;

-- Бренды
INSERT INTO "Brands" ("Id", "Name", "Country", "CreatedAt") VALUES
(1, 'Audi', 'Germany', NOW()),
(2, 'BMW', 'Germany', NOW()),
(3, 'Mercedes-Benz', 'Germany', NOW()),
(4, 'Volkswagen', 'Germany', NOW()),
(5, 'Toyota', 'Japan', NOW())
ON CONFLICT ("Id") DO NOTHING;

-- Модели
INSERT INTO "Models" ("Id", "BrandId", "Name", "CreatedAt") VALUES
(1, 1, 'A8', NOW()),
(2, 1, 'A6L', NOW()),
(3, 2, 'X5', NOW()),
(4, 3, 'E-Class', NOW()),
(5, 4, 'Touareg', NOW()),
(6, 5, 'Camry', NOW())
ON CONFLICT ("Id") DO NOTHING;

-- Дилеры
INSERT INTO "Dealers" ("Id", "Name", "Rating", "ContactInfo", "Address", "MarketId", "CreatedAt") VALUES
(1, 'Beijing Audi Dealer', 4.5, '+86 10 1234 5678', 'Beijing, Chaoyang District', 1, NOW()),
(2, 'Shanghai BMW Center', 4.3, '+86 21 8765 4321', 'Shanghai, Pudong', 1, NOW()),
(3, 'Guangzhou Mercedes', 4.7, '+86 20 1111 2222', 'Guangzhou, Tianhe', 1, NOW())
ON CONFLICT ("Id") DO NOTHING;

-- Источник данных
INSERT INTO "DataSources" ("Id", "Name", "Country", "BaseUrl", "Language", "IsActive", "CreatedAt")
VALUES (1, 'Che168', 'China', 'https://www.che168.com', 'zh-CN', true, NOW())
ON CONFLICT ("Id") DO NOTHING;

-- Автомобили с изображениями (используем placeholder с названиями)
INSERT INTO "Cars" (
    "Id", "BrandId", "ModelId", "MarketId", "DealerId", "DataSourceId",
    "Year", "Price", "Currency", "Vin", "Mileage",
    "Transmission", "Engine", "FuelType", "Color",
    "Location", "Country", "SourceUrl", "ImageUrl",
    "IsAvailable", "CreatedAt"
) VALUES
(1, 1, 1, 1, 1, 1, 2023, 668000, 'CNY', 'WAUZZZ4G0EN123456', 18000,
 'Automatic', '3.0T V6', 'Petrol', 'Black',
 'Beijing', 'China', 'https://www.che168.com/home/123456',
 'https://cdn.pixabay.com/photo/2020/09/06/07/37/car-5548243_1280.jpg',
 true, NOW()),

(2, 1, 1, 1, 1, 1, 2024, 720000, 'CNY', 'WAUZZZ4G1FN654321', 5000,
 'Automatic', '3.0T V6', 'Petrol', 'White',
 'Beijing', 'China', 'https://www.che168.com/home/234567',
 'https://cdn.pixabay.com/photo/2020/09/06/07/37/car-5548243_1280.jpg',
 true, NOW()),

(3, 1, 2, 1, 1, 1, 2023, 420000, 'CNY', 'WAUZZZ4G2DN789012', 12000,
 'Automatic', '2.0T I4', 'Petrol', 'Silver',
 'Beijing', 'China', 'https://www.che168.com/home/345678',
 'https://cdn.pixabay.com/photo/2016/08/25/12/09/car-1619561_1280.jpg',
 true, NOW()),

(4, 2, 3, 1, 2, 1, 2023, 750000, 'CNY', 'WBAKB8C50DF345678', 10000,
 'Automatic', '3.0T I6', 'Petrol', 'Gray',
 'Shanghai', 'China', 'https://www.che168.com/home/456789',
 'https://cdn.pixabay.com/photo/2014/07/09/06/47/car-387882_1280.jpg',
 true, NOW()),

(5, 3, 4, 1, 3, 1, 2023, 980000, 'CNY', 'WDD2130001A123456', 6000,
 'Automatic', '2.0T I4', 'Petrol', 'Black',
 'Shenzhen', 'China', 'https://www.che168.com/home/567890',
 'https://cdn.pixabay.com/photo/2016/11/23/06/57/island-1853345_1280.jpg',
 true, NOW()),

(6, 4, 5, 1, 1, 1, 2023, 680000, 'CNY', 'WVGZZZ7LZLD123456', 15000,
 'Automatic', '3.0T V6', 'Petrol', 'Blue',
 'Beijing', 'China', 'https://www.che168.com/home/678901',
 'https://cdn.pixabay.com/photo/2012/05/29/00/43/car-49278_1280.jpg',
 true, NOW()),

(7, 5, 6, 1, 1, 1, 2023, 250000, 'CNY', 'JTNBF1HK5L3123456', 20000,
 'Automatic', '2.5L I4', 'Petrol', 'White',
 'Guangzhou', 'China', 'https://www.che168.com/home/789012',
 'https://cdn.pixabay.com/photo/2016/05/06/16/32/car-1376190_1280.jpg',
 true, NOW())
ON CONFLICT ("Id") DO UPDATE SET "ImageUrl" = EXCLUDED."ImageUrl";

-- Сброс счетчиков
SELECT setval('"Cars_Id_seq"', (SELECT MAX("Id") FROM "Cars") + 1);
SELECT setval('"Brands_Id_seq"', (SELECT MAX("Id") FROM "Brands") + 1);
SELECT setval('"Models_Id_seq"', (SELECT MAX("Id") FROM "Models") + 1);
SELECT setval('"Markets_Id_seq"', (SELECT MAX("Id") FROM "Markets") + 1);
SELECT setval('"Dealers_Id_seq"', (SELECT MAX("Id") FROM "Dealers") + 1);

-- Проверяем результат
SELECT 
    c."Id",
    b."Name" as "Brand",
    m."Name" as "Model",
    c."Year",
    c."Price",
    c."ImageUrl"
FROM "Cars" c
JOIN "Brands" b ON c."BrandId" = b."Id"
JOIN "Models" m ON c."ModelId" = m."Id"
ORDER BY c."Id";
