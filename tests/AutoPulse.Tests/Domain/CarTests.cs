using AutoPulse.Domain;
using FluentAssertions;
using Xunit;

namespace AutoPulse.Tests.Domain;

public class CarTests
{
    [Fact]
    public void Car_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var car = new Car(
            brandId: 1,
            modelId: 1,
            marketId: 1,
            year: 2024,
            price: 250000,
            currency: "CNY",
            sourceUrl: "https://www.autohome.com.cn/car/123"
        );

        // Assert
        car.BrandId.Should().Be(1);
        car.ModelId.Should().Be(1);
        car.MarketId.Should().Be(1);
        car.Year.Should().Be(2024);
        car.Price.Should().Be(250000);
        car.Currency.Should().Be("CNY");
        car.SourceUrl.Should().Be("https://www.autohome.com.cn/car/123");
        car.IsAvailable.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Car_WithInvalidBrandId_ShouldThrowException(int brandId)
    {
        // Arrange & Act
        var act = () => new Car(
            brandId: brandId,
            modelId: 1,
            marketId: 1,
            year: 2024,
            price: 250000,
            currency: "CNY",
            sourceUrl: "https://www.autohome.com.cn/car/123"
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*ID марки должен быть положительным*");
    }

    [Fact]
    public void Car_WithInvalidYear_ShouldThrowException()
    {
        // Arrange & Act
        var act = () => new Car(
            brandId: 1,
            modelId: 1,
            marketId: 1,
            year: 1800,
            price: 250000,
            currency: "CNY",
            sourceUrl: "https://www.autohome.com.cn/car/123"
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Некорректный год*");
    }

    [Fact]
    public void Car_WithNegativePrice_ShouldThrowException()
    {
        // Arrange & Act
        var act = () => new Car(
            brandId: 1,
            modelId: 1,
            marketId: 1,
            year: 2024,
            price: -100,
            currency: "CNY",
            sourceUrl: "https://www.autohome.com.cn/car/123"
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Цена должна быть положительной*");
    }

    [Fact]
    public void Car_UpdatePrice_WithValidPrice_ShouldUpdate()
    {
        // Arrange
        var car = new Car(1, 1, 1, 2024, 250000, "CNY", "https://url.com");

        // Act
        car.UpdatePrice(300000);

        // Assert
        car.Price.Should().Be(300000);
        car.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Car_MarkAsSold_ShouldMarkUnavailable()
    {
        // Arrange
        var car = new Car(1, 1, 1, 2024, 250000, "CNY", "https://url.com");

        // Act
        car.MarkAsSold();

        // Assert
        car.IsAvailable.Should().BeFalse();
        car.SoldAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
