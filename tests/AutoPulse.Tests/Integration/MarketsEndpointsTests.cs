using System.Net;
using System.Text.Json;
using AutoPulse.Domain;
using AutoPulse.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AutoPulse.Tests.Integration;

public class MarketsEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public MarketsEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetMarkets_ShouldReturnAllMarkets()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Очистка и добавление тестовых данных
        dbContext.Markets.RemoveRange(dbContext.Markets);
        await dbContext.SaveChangesAsync();

        var market1 = new Market("USA", "North America", "USD");
        var market2 = new Market("China", "Asia", "CNY");
        dbContext.Markets.AddRange(market1, market2);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/markets");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var markets = JsonSerializer.Deserialize<List<JsonElement>>(json);

        markets.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMarketById_WithValidId_ShouldReturnMarket()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        dbContext.Markets.RemoveRange(dbContext.Markets);
        await dbContext.SaveChangesAsync();

        var market = new Market("Europe", "Europe", "EUR");
        dbContext.Markets.Add(market);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/markets/{market.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json);

        result.GetProperty("name").GetString().Should().Be("Europe");
        result.GetProperty("region").GetString().Should().Be("Europe");
        result.GetProperty("currency").GetString().Should().Be("EUR");
    }

    [Fact]
    public async Task GetMarketById_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/markets/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMarketDealers_ShouldReturnDealersForMarket()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        dbContext.Markets.RemoveRange(dbContext.Markets);
        dbContext.Dealers.RemoveRange(dbContext.Dealers);
        await dbContext.SaveChangesAsync();

        var market = new Market("China", "Asia", "CNY");
        dbContext.Markets.Add(market);
        await dbContext.SaveChangesAsync();

        var dealer1 = new Dealer("Dealer 1", market.Id, "contact@test.com");
        var dealer2 = new Dealer("Dealer 2", market.Id);
        dbContext.Dealers.AddRange(dealer1, dealer2);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/markets/{market.Id}/dealers");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var dealers = JsonSerializer.Deserialize<List<JsonElement>>(json);

        dealers.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMarketCarsCount_ShouldReturnCounts()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        dbContext.Markets.RemoveRange(dbContext.Markets);
        dbContext.Cars.RemoveRange(dbContext.Cars);
        await dbContext.SaveChangesAsync();

        var market = new Market("USA", "North America", "USD");
        dbContext.Markets.Add(market);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/markets/{market.Id}/cars/count");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json);

        result.GetProperty("totalCount").GetInt32().Should().Be(0);
        result.GetProperty("availableCount").GetInt32().Should().Be(0);
    }
}
