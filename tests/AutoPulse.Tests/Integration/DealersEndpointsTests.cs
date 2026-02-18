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

public class DealersEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public DealersEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDealers_ShouldReturnAllDealers()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        dbContext.Markets.RemoveRange(dbContext.Markets);
        dbContext.Dealers.RemoveRange(dbContext.Dealers);
        await dbContext.SaveChangesAsync();

        var market = new Market("USA", "North America", "USD");
        dbContext.Markets.Add(market);
        await dbContext.SaveChangesAsync();

        var dealer1 = new Dealer("Dealer 1", market.Id, "contact1@test.com");
        var dealer2 = new Dealer("Dealer 2", market.Id, "contact2@test.com");
        dealer1.SetRating(4.5m);
        dealer2.SetRating(3.8m);
        dbContext.Dealers.AddRange(dealer1, dealer2);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/dealers");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json);

        result.GetProperty("items").GetArrayLength().Should().Be(2);
        result.GetProperty("totalCount").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task GetDealerById_WithValidId_ShouldReturnDealer()
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

        var dealer = new Dealer("Test Dealer", market.Id, "contact@test.com", "Test Address");
        dealer.SetRating(4.8m);
        dbContext.Dealers.Add(dealer);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/dealers/{dealer.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json);

        result.GetProperty("name").GetString().Should().Be("Test Dealer");
        result.GetProperty("rating").GetDecimal().Should().Be(4.8m);
        result.GetProperty("contactInfo").GetString().Should().Be("contact@test.com");
        result.GetProperty("carsCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task GetDealerById_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/dealers/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDealersByMarket_ShouldReturnDealersForMarket()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        dbContext.Markets.RemoveRange(dbContext.Markets);
        dbContext.Dealers.RemoveRange(dbContext.Dealers);
        await dbContext.SaveChangesAsync();

        var market1 = new Market("USA", "North America", "USD");
        var market2 = new Market("Europe", "Europe", "EUR");
        dbContext.Markets.AddRange(market1, market2);
        await dbContext.SaveChangesAsync();

        var dealer1 = new Dealer("USA Dealer 1", market1.Id);
        var dealer2 = new Dealer("USA Dealer 2", market1.Id);
        var dealer3 = new Dealer("Europe Dealer", market2.Id);
        dbContext.Dealers.AddRange(dealer1, dealer2, dealer3);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/dealers/market/{market1.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var dealers = JsonSerializer.Deserialize<List<JsonElement>>(json);

        dealers.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetDealersStats_ShouldReturnStatistics()
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

        var dealer1 = new Dealer("Dealer 1", market.Id);
        var dealer2 = new Dealer("Dealer 2", market.Id);
        dealer1.SetRating(5.0m);
        dealer2.SetRating(4.0m);
        dbContext.Dealers.AddRange(dealer1, dealer2);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/dealers/stats/summary");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json);

        result.GetProperty("totalDealers").GetInt32().Should().Be(2);
        result.GetProperty("averageRating").GetDecimal().Should().Be(4.5m);
    }

    [Fact]
    public async Task GetDealers_WithMinRatingFilter_ShouldFilterDealers()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        dbContext.Markets.RemoveRange(dbContext.Markets);
        dbContext.Dealers.RemoveRange(dbContext.Dealers);
        await dbContext.SaveChangesAsync();

        var market = new Market("USA", "North America", "USD");
        dbContext.Markets.Add(market);
        await dbContext.SaveChangesAsync();

        var dealer1 = new Dealer("Good Dealer", market.Id);
        var dealer2 = new Dealer("Bad Dealer", market.Id);
        dealer1.SetRating(4.8m);
        dealer2.SetRating(2.0m);
        dbContext.Dealers.AddRange(dealer1, dealer2);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/dealers?minRating=4.0");

        // Assert
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json);

        result.GetProperty("items").GetArrayLength().Should().Be(1);
        result.GetProperty("items")[0].GetProperty("name").GetString().Should().Be("Good Dealer");
    }
}
