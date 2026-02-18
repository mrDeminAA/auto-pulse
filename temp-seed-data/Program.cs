using Npgsql;

var connectionString = "User ID=postgres;Password=adminsgesYfdkjnfk;Host=10.23.3.172;Port=5432;Database=autopulse;";

await using var conn = new NpgsqlConnection(connectionString);
await conn.OpenAsync();

Console.WriteLine("✅ Подключение к БД успешно");

// Создаем тестовые данные
await using var tx = await conn.BeginTransactionAsync();

try
{
    // Brands
    await using var brandCmd = new NpgsqlCommand(@"
        INSERT INTO ""Brands"" (""Name"", ""Country"", ""CreatedAt"") 
        VALUES 
            ('Audi', 'Germany', NOW()),
            ('BMW', 'Germany', NOW()),
            ('Mercedes-Benz', 'Germany', NOW())
        ON CONFLICT DO NOTHING
        RETURNING ""Id"", ""Name"";
    ", conn, tx.Transaction);
    
    await using var brandReader = await brandCmd.ExecuteReaderAsync();
    var brands = new Dictionary<string, int>();
    while (await brandReader.ReadAsync())
    {
        brands[brandReader.GetString(1)] = brandReader.GetInt32(0);
    }
    Console.WriteLine($"✅ Добавлено брендов: {brands.Count}");

    // Models
    await using var modelCmd = new NpgsqlCommand(@"
        INSERT INTO ""Models"" (""BrandId"", ""Name"", ""CreatedAt"") 
        VALUES 
            ((SELECT ""Id"" FROM ""Brands"" WHERE ""Name"" = 'Audi'), 'A6L', NOW()),
            ((SELECT ""Id"" FROM ""Brands"" WHERE ""Name"" = 'BMW'), '5 Series', NOW()),
            ((SELECT ""Id"" FROM ""Brands"" WHERE ""Name"" = 'Mercedes-Benz'), 'E-Class', NOW())
        ON CONFLICT DO NOTHING
        RETURNING ""Id"", ""Name"";
    ", conn, tx.Transaction);
    
    await using var modelReader = await modelCmd.ExecuteReaderAsync();
    var models = new List<string>();
    while (await modelReader.ReadAsync())
    {
        models.Add(modelReader.GetString(1));
    }
    Console.WriteLine($"✅ Добавлено моделей: {models.Count}");

    // Markets
    await using var marketCmd = new NpgsqlCommand(@"
        INSERT INTO ""Markets"" (""Name"", ""Region"", ""Currency"", ""CreatedAt"") 
        VALUES 
            ('China', 'Asia', 'CNY', NOW())
        ON CONFLICT DO NOTHING
        RETURNING ""Id"";
    ", conn, tx.Transaction);
    
    var marketId = await marketCmd.ExecuteScalarAsync();
    Console.WriteLine($"✅ Добавлен рынок: ID={marketId}");

    // Dealers
    await using var dealerCmd = new NpgsqlCommand(@"
        INSERT INTO ""Dealers"" (""Name"", ""ContactInfo"", ""Address"", ""Rating"", ""MarketId"", ""CreatedAt"") 
        VALUES 
            ('Beijing Audi Dealer', '+86-10-12345678', 'Beijing, Chaoyang District', 4.5, @MarketId, NOW()),
            ('BMW Premium Dealer', '+86-10-87654321', 'Beijing, Haidian District', 4.8, @MarketId, NOW())
        ON CONFLICT DO NOTHING
        RETURNING ""Id"";
    ", conn, tx.Transaction);
    
    dealerCmd.Parameters.AddWithValue("MarketId", (int)marketId!);
    await using var dealerReader = await dealerCmd.ExecuteReaderAsync();
    var dealerIds = new List<int>();
    while (await dealerReader.ReadAsync())
    {
        dealerIds.Add(dealerReader.GetInt32(0));
    }
    Console.WriteLine($"✅ Добавлено дилеров: {dealerIds.Count}");

    // Cars
    await using var carCmd = new NpgsqlCommand(@"
        INSERT INTO ""Cars"" (
            ""BrandId"", ""ModelId"", ""MarketId"", ""DealerId"", 
            ""Year"", ""Price"", ""Currency"", ""Vin"", ""Mileage"", 
            ""Transmission"", ""Engine"", ""FuelType"", ""Color"", 
            ""Location"", ""Country"", ""SourceUrl"", ""ImageUrl"", 
            ""IsAvailable"", ""CreatedAt""
        ) VALUES 
            (
                (SELECT ""Id"" FROM ""Brands"" WHERE ""Name"" = 'Audi'),
                (SELECT ""Id"" FROM ""Models"" WHERE ""Name"" = 'A6L'),
                @MarketId, @DealerId1,
                2021, 250000.00, 'CNY', 'WAUZZZ4G0EN123456', 50000,
                'Automatic', '2.0L Turbo', 'Gasoline', 'Black',
                'Beijing', 'China', 'https://www.che168.com/dealer/114575/55590802.html', '',
                TRUE, NOW()
            ),
            (
                (SELECT ""Id"" FROM ""Brands"" WHERE ""Name"" = 'BMW'),
                (SELECT ""Id"" FROM ""Models"" WHERE ""Name"" = '5 Series'),
                @MarketId, @DealerId2,
                2022, 380000.00, 'CNY', 'WBA5A5C50ED123456', 30000,
                'Automatic', '3.0L Turbo', 'Gasoline', 'White',
                'Beijing', 'China', 'https://www.che168.com/dealer/114575/55590802.html', '',
                TRUE, NOW()
            ),
            (
                (SELECT ""Id"" FROM ""Brands"" WHERE ""Name"" = 'Mercedes-Benz'),
                (SELECT ""Id"" FROM ""Models"" WHERE ""Name"" = 'E-Class'),
                @MarketId, @DealerId1,
                2023, 420000.00, 'CNY', 'WDD2130001A123456', 15000,
                'Automatic', '2.0L Turbo', 'Gasoline', 'Silver',
                'Beijing', 'China', 'https://www.che168.com/dealer/114575/55590802.html', '',
                TRUE, NOW()
            )
        ON CONFLICT DO NOTHING;
    ", conn, tx.Transaction);
    
    carCmd.Parameters.AddWithValue("MarketId", (int)marketId!);
    carCmd.Parameters.AddWithValue("DealerId1", dealerIds[0]);
    carCmd.Parameters.AddWithValue("DealerId2", dealerIds.Count > 1 ? dealerIds[1] : dealerIds[0]);
    
    var carsAdded = await carCmd.ExecuteNonQueryAsync();
    Console.WriteLine($"✅ Добавлено автомобилей: {carsAdded}");

    await tx.CommitAsync();
    Console.WriteLine("\n🎉 Тестовые данные успешно созданы!");
}
catch (Exception ex)
{
    await tx.RollbackAsync();
    Console.WriteLine($"❌ Ошибка: {ex.Message}");
    throw;
}
