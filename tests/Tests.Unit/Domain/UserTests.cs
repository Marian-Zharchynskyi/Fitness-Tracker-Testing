using Domain.Users;
using FluentAssertions;
using Xunit;

namespace Tests.Unit.Domain;

public class UserTests
{
    [Fact]
    public void SetMetrics_WithPositiveValues_ShouldSetHeightAndWeight()
    {
        // Arrange
        var user = User.New(new UserId(Guid.NewGuid()), "test@example.com", "Test User");

        // Act
        user.SetMetrics(180, 75);

        // Assert
        user.HeightCm.Should().Be(180);
        user.WeightKg.Should().Be(75);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void SetMetrics_WithNonPositiveHeight_ShouldThrowArgumentException(decimal height)
    {
        // Arrange
        var user = User.New(new UserId(Guid.NewGuid()), "test@example.com", "Test User");

        // Act
        var act = () => user.SetMetrics(height, 75);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Height must be positive*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void SetMetrics_WithNonPositiveWeight_ShouldThrowArgumentException(decimal weight)
    {
        // Arrange
        var user = User.New(new UserId(Guid.NewGuid()), "test@example.com", "Test User");

        // Act
        var act = () => user.SetMetrics(180, weight);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Weight must be positive*");
    }

    [Fact]
    public void UpdateUser_ShouldUpdateEmailAndName()
    {
        // Arrange
        var user = User.New(new UserId(Guid.NewGuid()), "old@example.com", "Old Name");

        // Act
        user.UpdateUser("new@example.com", "New Name");

        // Assert
        user.Email.Should().Be("new@example.com");
        user.Name.Should().Be("New Name");
    }
}
