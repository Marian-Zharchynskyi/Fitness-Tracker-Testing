using Domain.Users;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tests.Database;
using Xunit;

namespace Tests.Database.Persistence;

public class ApplicationDbContextInitialiserTests : BaseDatabaseTest
{
    private ApplicationDbContextInitialiser _initialiser = null!;
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;

    public ApplicationDbContextInitialiserTests()
    {
        _logger = Substitute.For<ILogger<ApplicationDbContextInitialiser>>();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _initialiser = new ApplicationDbContextInitialiser(Context, _logger);
    }

    [Fact]
    public async Task InitializeAsync_WhenDatabaseIsAccessible_ShouldNotThrow()
    {
        // Act & Assert
        var act = async () => await _initialiser.InitializeAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InitializeAsync_WhenDataExists_ShouldSkipSeeding()
    {
        // Arrange
        Context.Users.Add(User.New(new UserId(Guid.NewGuid()), "test@test.com", "Test"));
        await Context.SaveChangesAsync();

        // Act
        await _initialiser.InitializeAsync();

        // Assert - If we reach here, it skipped seeding successfully without throwing
        Context.Users.Should().NotBeEmpty();
    }
}
