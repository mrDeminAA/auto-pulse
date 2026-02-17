using AutoPulse.Domain;
using FluentAssertions;
using Xunit;

namespace AutoPulse.Tests.Domain;

public class BrandTests
{
    [Fact]
    public void Brand_WithName_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var brand = new Brand("Toyota", "Japan");

        // Assert
        brand.Name.Should().Be("Toyota");
        brand.Country.Should().Be("Japan");
        brand.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Brand_WithEmptyName_ShouldThrowException(string? name)
    {
        // Arrange & Act
        var act = () => new Brand(name!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Название марки не может быть пустым*");
    }

    [Fact]
    public void Brand_SetName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var brand = new Brand("Toyota");

        // Act
        brand.SetName("Honda");

        // Assert
        brand.Name.Should().Be("Honda");
        brand.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Brand_SetName_WithEmptyName_ShouldThrowException()
    {
        // Arrange
        var brand = new Brand("Toyota");

        // Act
        var act = () => brand.SetName("");

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
