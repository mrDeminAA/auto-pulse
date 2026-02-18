#r "nuget: MassTransit.RabbitMQ, 8.1.3"
#r "nuget: System.Text.Json, 8.0.0"

using System;
using System.Text;
using System.Text.Json;
using MassTransit;

var command = new
{
    url = "https://www.che168.com/home?infoid=56640428",
    sourceName = "Che168",
    country = "China"
};

Console.WriteLine("=== Отправка команды на парсинг ===");
Console.WriteLine($"URL: {command.url}");
Console.WriteLine($"Source: {command.sourceName}");
Console.WriteLine($"Country: {command.country}");

// Создаём JSON сообщение в формате MassTransit
var message = new
{
    message = command,
    messageType = new[] { "parse-cars-command" }
};

var json = JsonSerializer.Serialize(message, new JsonSerializerOptions 
{ 
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = false
});

Console.WriteLine($"\nMessage JSON: {json}");
Console.WriteLine("\n✓ Сообщение готово к отправке через RabbitMQ UI");
Console.WriteLine("\nИнструкция:");
Console.WriteLine("1. Откройте http://localhost:15672");
Console.WriteLine("2. Login: guest / Password: guest");
Console.WriteLine("3. Перейдите в 'Queues' → 'parse-cars-queue'");
Console.WriteLine("4. В разделе 'Publish message':");
Console.WriteLine("   - Message properties: content_type = application/vnd.masstransit+json");
Console.WriteLine("   - Payload encoding: string");
Console.WriteLine("   - Payload: " + json);
Console.WriteLine("5. Нажмите 'Publish message'");
