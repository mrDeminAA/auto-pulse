using System.Net.Http;

var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

var html = await httpClient.GetStringAsync("https://www.che168.com/beijing/aodi/a3/");

// Сохраняем HTML для анализа
await File.WriteAllTextAsync("che168-page.html", html);

Console.WriteLine($"HTML сохранен! Размер: {html.Length} байт");

// Ищем простые паттерны
var homeLinks = System.Text.RegularExpressions.Regex.Matches(html, @"/home/\d+");
Console.WriteLine($"Найдено ссылок /home/: {homeLinks.Count}");

foreach (Match m in homeLinks.Take(5))
{
    Console.WriteLine($"  - {m.Value}");
}
