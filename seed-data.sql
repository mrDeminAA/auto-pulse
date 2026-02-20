-- Seed данные для AutoPulse

-- Бренды
INSERT INTO "Brands" ("Name", "Country", "LogoUrl", "CreatedAt") VALUES
('Audi', 'Germany', '/logos/audi.png', NOW()),
('BMW', 'Germany', '/logos/bmw.png', NOW()),
('Mercedes-Benz', 'Germany', '/logos/mercedes.png', NOW()),
('Volkswagen', 'Germany', '/logos/vw.png', NOW()),
('Toyota', 'Japan', '/logos/toyota.png', NOW()),
('Honda', 'Japan', '/logos/honda.png', NOW()),
('Nissan', 'Japan', '/logos/nissan.png', NOW()),
('Mazda', 'Japan', '/logos/mazda.png', NOW()),
('Lexus', 'Japan', '/logos/lexus.png', NOW()),
('Porsche', 'Germany', '/logos/porsche.png', NOW())
ON CONFLICT DO NOTHING;

-- Модели (Audi)
INSERT INTO "Models" ("BrandId", "Name", "Category", "CreatedAt") VALUES
(1, 'A3', 'Compact', NOW()),
(1, 'A4', 'Mid-size', NOW()),
(1, 'A4L', 'Mid-size LWB', NOW()),
(1, 'A6', 'Executive', NOW()),
(1, 'A6L', 'Executive LWB', NOW()),
(1, 'Q5', 'Compact SUV', NOW()),
(1, 'Q7', 'Full-size SUV', NOW()),
(1, 'A5', 'Compact Executive', NOW()),
(1, 'Q3', 'Subcompact SUV', NOW()),
(1, 'e-tron', 'Electric SUV', NOW())
ON CONFLICT DO NOTHING;

-- Модели (BMW)
INSERT INTO "Models" ("BrandId", "Name", "Category", "CreatedAt") VALUES
(2, '3 Series', 'Compact Executive', NOW()),
(2, '5 Series', 'Executive', NOW()),
(2, '7 Series', 'Full-size Luxury', NOW()),
(2, 'X3', 'Compact SUV', NOW()),
(2, 'X5', 'Mid-size SUV', NOW()),
(2, 'X7', 'Full-size SUV', NOW())
ON CONFLICT DO NOTHING;

-- Модели (Mercedes-Benz)
INSERT INTO "Models" ("BrandId", "Name", "Category", "CreatedAt") VALUES
(3, 'C-Class', 'Compact Executive', NOW()),
(3, 'E-Class', 'Executive', NOW()),
(3, 'S-Class', 'Full-size Luxury', NOW()),
(3, 'GLC', 'Compact SUV', NOW()),
(3, 'GLE', 'Mid-size SUV', NOW()),
(3, 'GLS', 'Full-size SUV', NOW())
ON CONFLICT DO NOTHING;

-- Модели (Toyota)
INSERT INTO "Models" ("BrandId", "Name", "Category", "CreatedAt") VALUES
(5, 'Camry', 'Mid-size', NOW()),
(5, 'Corolla', 'Compact', NOW()),
(5, 'RAV4', 'Compact SUV', NOW()),
(5, 'Land Cruiser', 'Full-size SUV', NOW()),
(5, 'Highlander', 'Mid-size SUV', NOW())
ON CONFLICT DO NOTHING;

-- Markets
INSERT INTO "Markets" ("Name", "Code", "Country", "Currency", "IsActive", "CreatedAt") VALUES
('Китай', 'china', 'China', 'CNY', true, NOW()),
('Европа', 'europe', 'Europe', 'EUR', true, NOW()),
('США', 'usa', 'United States', 'USD', true, NOW()),
('Корея', 'korea', 'South Korea', 'KRW', true, NOW())
ON CONFLICT DO NOTHING;

-- Dealers (пример)
INSERT INTO "Dealers" ("Name", "MarketId", "City", "Rating", "CreatedAt") VALUES
('Beijing Audi Center', 1, 'Beijing', 4.8, NOW()),
('Shanghai BMW Center', 1, 'Shanghai', 4.7, NOW()),
('Guangzhou Mercedes Center', 1, 'Guangzhou', 4.6, NOW())
ON CONFLICT DO NOTHING;
