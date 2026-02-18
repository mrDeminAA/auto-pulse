-- Очистка базы данных перед новым парсингом
-- Важно: порядок обратный вставке из-за FOREIGN KEY

-- Очищаем Cars (зависит от Brands, Models, Markets, Dealers, DataSources)
DELETE FROM "Cars";

-- Сбрасываем счетчики ID
ALTER SEQUENCE "Cars_Id_seq" RESTART WITH 1;

-- Очищаем справочники (если нужно полное обновление)
DELETE FROM "Dealers";
ALTER SEQUENCE "Dealers_Id_seq" RESTART WITH 1;

DELETE FROM "Models";
ALTER SEQUENCE "Models_Id_seq" RESTART WITH 1;

DELETE FROM "Brands";
ALTER SEQUENCE "Brands_Id_seq" RESTART WITH 1;

DELETE FROM "Markets";
ALTER SEQUENCE "Markets_Id_seq" RESTART WITH 1;

DELETE FROM "DataSources";
ALTER SEQUENCE "DataSources_Id_seq" RESTART WITH 1;

-- Проверяем что очистилось
SELECT 'Cars' as table_name, COUNT(*) as row_count FROM "Cars"
UNION ALL
SELECT 'Brands', COUNT(*) FROM "Brands"
UNION ALL
SELECT 'Models', COUNT(*) FROM "Models"
UNION ALL
SELECT 'Markets', COUNT(*) FROM "Markets"
UNION ALL
SELECT 'Dealers', COUNT(*) FROM "Dealers"
UNION ALL
SELECT 'DataSources', COUNT(*) FROM "DataSources";
