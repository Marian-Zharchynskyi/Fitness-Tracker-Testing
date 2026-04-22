using Domain.Users;
using FluentAssertions;
using Infrastructure.Persistence.Repositories;
using Tests.Database;
using Xunit;

namespace Tests.Database.Repositories;

public class UserRepositoryTests : BaseDatabaseTest
{
    private UserRepository _repository = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _repository = new UserRepository(Context);
    }

    [Fact]
    public async Task Create_ShouldAddUserToDatabase()
    {
        // Arrange
        var user = User.New(new UserId(Guid.NewGuid()), "test@test.com", "Test User");

        // Act
        var result = await _repository.Create(user, CancellationToken.None);

        // Assert
        result.Should().Be(user);
        var savedUser = await Context.Users.FindAsync(user.Id);
        savedUser.Should().NotBeNull();
        savedUser!.Email.Should().Be("test@test.com");
    }

    [Fact]
    public async Task Delete_ShouldRemoveUserFromDatabase()
    {
        // Arrange
        var user = User.New(new UserId(Guid.NewGuid()), "delete@test.com", "Delete User");
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        // Act
        await _repository.Delete(user, CancellationToken.None);

        // Assert
        var savedUser = await Context.Users.FindAsync(user.Id);
        savedUser.Should().BeNull();
    }

    [Fact]
    public async Task SearchById_ShouldReturnUser_WhenExists()
    {
        // Arrange
        var user = User.New(new UserId(Guid.NewGuid()), "search@test.com", "Search User");
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetById(user.Id, CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(u => u.Should().Be(user));
    }

    [Fact]
    public async Task SearchById_ShouldReturnNone_WhenDoesNotExist()
    {
        // Arrange
        // Act
        var result = await _repository.GetById(new UserId(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task SearchByEmail_ShouldReturnUser_WhenExists()
    {
        // Arrange
        var user = User.New(new UserId(Guid.NewGuid()), "email@test.com", "Email User");
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchByEmail("email@test.com", CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(u => u.Should().Be(user));
    }

    [Fact]
    public async Task SearchByEmailForUpdate_ShouldReturnNone_WhenSearchingOwnEmail()
    {
        // Arrange
        var user = User.New(new UserId(Guid.NewGuid()), "update@test.com", "Update User");
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchByEmailForUpdate(user.Id, "update@test.com", CancellationToken.None);

        // Assert
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task GetPaged_ShouldReturnPagedUsers()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            Context.Users.Add(User.New(new UserId(Guid.NewGuid()), $"paged{i}@test.com", $"User {i}"));
        }
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPaged(1, 3, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(5);
    }
}
