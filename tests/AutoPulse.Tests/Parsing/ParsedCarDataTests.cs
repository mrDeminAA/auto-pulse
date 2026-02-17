using AutoPulse.Application.Parsing;
using FluentAssertions;
using Xunit;

namespace AutoPulse.Tests.Parsing;

public class ParsedCarDataTests
{
    [Fact]
    public void ParsedCarData_WithDefaultValues_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var car = new ParsedCarData();

        // Assert
        car.BrandName.Should().BeEmpty();
        car.ModelName.Should().BeEmpty();
        car.Currency.Should().Be("CNY");
        car.SourceUrl.Should().BeEmpty();
        car.Year.Should().Be(0);
        car.Price.Should().Be(0);
        car.Mileage.Should().Be(0);
    }

    [Fact]
    public void ParsedCarData_WithValidData_ShouldHaveCorrectValues()
    {
        // Arrange
        var car = new ParsedCarData
        {
            BrandName = "Toyota",
            ModelName = "Camry",
            Year = 2024,
            Price = 250000,
            Currency = "CNY",
            Mileage = 15000,
            SourceUrl = "https://www.autohome.com.cn/car/123"
        };

        // Assert
        car.BrandName.Should().Be("Toyota");
        car.ModelName.Should().Be("Camry");
        car.Year.Should().Be(2024);
        car.Price.Should().Be(250000);
        car.Currency.Should().Be("CNY");
        car.Mileage.Should().Be(15000);
        car.SourceUrl.Should().Be("https://www.autohome.com.cn/car/123");
    }
}
