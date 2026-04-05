using Domain.Users;
using FluentAssertions;
using Xunit;

namespace Tests.Unit.Domain;

public class UserTests
{
    [Fact]
    public void SetMetrics_WithPositiveValues_ShouldSetHeightAndWeight()
    {
        var user = User.New(new UserId(Guid.NewGuid()), "test@example.com", "Test User");

        user.SetMetrics(180, 75);

        user.HeightCm.Should().Be(180);
        user.WeightKg.Should().Be(75);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void SetMetrics_WithNonPositiveHeight_ShouldThrowArgumentException(decimal height)
    {
        var user = User.New(new UserId(Guid.NewGuid()), "test@example.com", "Test User");

        var act = () => user.SetMetrics(height, 75);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Height must be positive*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void SetMetrics_WithNonPositiveWeight_ShouldThrowArgumentException(decimal weight)
    {
        var user = User.New(new UserId(Guid.NewGuid()), "test@example.com", "Test User");

        var act = () => user.SetMetrics(180, weight);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Weight must be positive*");
    }

    [Fact]
    public void UpdateUser_ShouldUpdateEmailAndName()
    {
        var user = User.New(new UserId(Guid.NewGuid()), "old@example.com", "Old Name");

        user.UpdateUser("new@example.com", "New Name");

        user.Email.Should().Be("new@example.com");
        user.Name.Should().Be("New Name");
    }
}
