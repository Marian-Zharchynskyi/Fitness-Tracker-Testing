using System.Net.Http.Json;
using FluentAssertions;
using Tests.Common;
using Tests.Data;

namespace Api.Tests.Integration.Users;

using System.Net;
using API.DTOs.Users;
using Domain.Users;
using Xunit;

public class UsersControllerTests : BaseIntegrationTest, IAsyncLifetime
{
    private readonly User _mainUser = UsersData.MainUser;
    private readonly User _secondUser = UsersData.SecondUser;

    public UsersControllerTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ShouldGetAllUsers()
    {
        // Arrange
        // Act
        var response = await Client.GetAsync("users/get-all");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var users = await response.ToResponseModel<List<UserDto>>();
        users.Should().NotBeNull();
        users.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task ShouldGetUserById()
    {
        // Arrange
        // Act
        var response = await Client.GetAsync($"users/get-by-id/{_mainUser.Id.Value}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var user = await response.ToResponseModel<UserDto>();
        user.Id.Should().Be(_mainUser.Id.Value);
        user.Email.Should().Be(_mainUser.Email);
    }

    [Fact]
    public async Task ShouldReturnNotFoundForUnknownUser()
    {
        // Arrange
        var unknownId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"users/get-by-id/{unknownId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldCreateUser()
    {
        // Arrange
        var request = new CreateUserDto(
            Email: "new.user@fitness.com",
            Password: "secure123",
            Name: "New");

        // Act
        var response = await Client.PostAsJsonAsync("users/create", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var user = await response.ToResponseModel<UserDto>();
        user.Email.Should().Be(request.Email);
        user.Name.Should().Be(request.Name);

        Context.Users.Any(u => u.Id == new UserId(user.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task ShouldNotCreateUserWithDuplicateEmail()
    {
        // Arrange
        var request = new CreateUserDto(
            Email: _mainUser.Email,
            Password: "password123",
            Name: "Duplicate");

        // Act
        var response = await Client.PostAsJsonAsync("users/create", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ShouldUpdateUser()
    {
        // Arrange
        var request = new UpdateUserDto(
            Email: "updated@fitness.com",
            Name: "Updated",
            HeightCm: 180,
            WeightKg: 80);

        // Act
        var response = await Client.PutAsJsonAsync($"users/update/{_secondUser.Id.Value}", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var user = await response.ToResponseModel<UserDto>();
        user.Email.Should().Be(request.Email);
        user.Name.Should().Be(request.Name);
        user.HeightCm.Should().Be(request.HeightCm);
        user.WeightKg.Should().Be(request.WeightKg);
    }

    [Fact]
    public async Task ShouldDeleteUser()
    {
        // Arrange
        var request = new CreateUserDto(
            Email: "delete.me@fitness.com",
            Password: "password123",
            Name: "Delete");

        var createResponse = await Client.PostAsJsonAsync("users/create", request);
        createResponse.IsSuccessStatusCode.Should().BeTrue();
        var created = await createResponse.ToResponseModel<UserDto>();

        // Act
        var deleteResponse = await Client.DeleteAsync($"users/delete/{created.Id}");

        // Assert
        deleteResponse.IsSuccessStatusCode.Should().BeTrue();
        Context.Users.Any(u => u.Id == new UserId(created.Id)).Should().BeFalse();
    }

    public async Task InitializeAsync()
    {
        Context.Users.AddRange(_mainUser, _secondUser);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.Users.RemoveRange(Context.Users);
        await SaveChangesAsync();
    }
}
