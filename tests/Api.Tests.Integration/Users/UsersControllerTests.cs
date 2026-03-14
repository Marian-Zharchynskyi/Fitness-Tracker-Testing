using System.Net;
using System.Net.Http.Json;
using API.DTOs.Users;
using Domain.Users;
using FluentAssertions;
using Tests.Common;
using Tests.Data;
using Xunit;

namespace Api.Tests.Integration.Users;

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
            Name: "New",
            Surname: "User");

        // Act
        var response = await Client.PostAsJsonAsync("users/create", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var user = await response.ToResponseModel<UserDto>();
        user.Email.Should().Be(request.Email);
        user.Name.Should().Be(request.Name);
        user.Surname.Should().Be(request.Surname);
        user.HasPassword.Should().BeTrue();

        // Verify in database
        Context.Users.Any(u => u.Id == new UserId(user.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task ShouldNotCreateUserWithDuplicateEmail()
    {
        // Arrange
        var request = new CreateUserDto(
            Email: _mainUser.Email,
            Password: "password123",
            Name: "Duplicate",
            Surname: "User");

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
            Surname: "Name",
            PhoneNumber: "+380501234567");

        // Act
        var response = await Client.PutAsJsonAsync($"users/update/{_secondUser.Id.Value}", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var user = await response.ToResponseModel<UserDto>();
        user.Email.Should().Be(request.Email);
        user.Name.Should().Be(request.Name);
        user.PhoneNumber.Should().Be(request.PhoneNumber);
    }

    [Fact]
    public async Task ShouldDeleteUser()
    {
        // Arrange — create a throwaway user to delete
        var request = new CreateUserDto(
            Email: "delete.me@fitness.com",
            Password: "password123",
            Name: "Delete",
            Surname: "Me");

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
        // Seed test users into the database
        Context.Users.AddRange(_mainUser, _secondUser);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        // Clean up all users after each test class run
        Context.Users.RemoveRange(Context.Users);
        await SaveChangesAsync();
    }
}
