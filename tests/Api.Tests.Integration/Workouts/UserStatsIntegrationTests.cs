using System.Net.Http.Json;
using API.DTOs.Users;
using API.DTOs.Workouts;
using Domain.Users;
using Domain.Workouts;
using FluentAssertions;
using Tests.Common;
using Tests.Data;
using Xunit;

namespace Api.Tests.Integration.Workouts;

public class UserStatsIntegrationTests : BaseIntegrationTest, IAsyncLifetime
{
    private readonly User _testUser = UsersData.MainUser;

    public UserStatsIntegrationTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetStats_WithMultipleWorkouts_ShouldReturnCorrectStatistics()
    {
        // Arrange
        await CreateWorkout(30, 200, DateTime.UtcNow.AddDays(-10));
        await CreateWorkout(45, 350, DateTime.UtcNow.AddDays(-5));
        await CreateWorkout(60, 500, DateTime.UtcNow.AddDays(-1));

        // Act
        var response = await Client.GetAsync($"/users/{_testUser.Id.Value}/stats");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var stats = await response.ToResponseModel<UserStatsDto>();
        stats.TotalWorkouts.Should().Be(3);
        stats.TotalCaloriesBurned.Should().Be(1050);
        stats.AverageDurationMinutes.Should().Be(45);
    }

    [Fact]
    public async Task GetStats_WithDateRange_ShouldFilterWorkouts()
    {
        // Arrange
        await CreateWorkout(30, 200, DateTime.SpecifyKind(new DateTime(2024, 1, 15), DateTimeKind.Utc));
        await CreateWorkout(45, 350, DateTime.SpecifyKind(new DateTime(2024, 2, 15), DateTimeKind.Utc));
        await CreateWorkout(60, 500, DateTime.SpecifyKind(new DateTime(2024, 3, 15), DateTimeKind.Utc));

        var startDate = DateTime.SpecifyKind(new DateTime(2024, 2, 1), DateTimeKind.Utc);
        var endDate = DateTime.SpecifyKind(new DateTime(2024, 3, 31), DateTimeKind.Utc);

        // Act
        var response = await Client.GetAsync(
            $"/users/{_testUser.Id.Value}/stats?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var stats = await response.ToResponseModel<UserStatsDto>();
        stats.TotalWorkouts.Should().Be(2);
        stats.TotalCaloriesBurned.Should().Be(850);
        stats.AverageDurationMinutes.Should().Be(52.5);
    }

    [Fact]
    public async Task GetStats_WithNoWorkouts_ShouldReturnZeroStats()
    {
        // Arrange
        // Act
        var response = await Client.GetAsync($"/users/{_testUser.Id.Value}/stats");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var stats = await response.ToResponseModel<UserStatsDto>();
        stats.TotalWorkouts.Should().Be(0);
        stats.TotalCaloriesBurned.Should().Be(0);
        stats.AverageDurationMinutes.Should().Be(0);
    }

    [Fact]
    public async Task GetWorkouts_WithDateRangeFilter_ShouldReturnFilteredWorkouts()
    {
        // Arrange
        await CreateWorkout(30, 200, DateTime.SpecifyKind(new DateTime(2024, 1, 15), DateTimeKind.Utc));
        await CreateWorkout(45, 350, DateTime.SpecifyKind(new DateTime(2024, 2, 15), DateTimeKind.Utc));
        await CreateWorkout(60, 500, DateTime.SpecifyKind(new DateTime(2024, 3, 15), DateTimeKind.Utc));

        var startDate = DateTime.SpecifyKind(new DateTime(2024, 2, 1), DateTimeKind.Utc);
        var endDate = DateTime.SpecifyKind(new DateTime(2024, 2, 28), DateTimeKind.Utc);

        // Act
        var response = await Client.GetAsync(
            $"/users/{_testUser.Id.Value}/workouts?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var workouts = await response.ToResponseModel<List<WorkoutDto>>();
        workouts.Should().HaveCount(1);
        workouts[0].DurationMinutes.Should().Be(45);
    }

    private async Task CreateWorkout(int duration, int calories, DateTime date)
    {
        var workout = Workout.New(new WorkoutId(Guid.NewGuid()), _testUser.Id, $"Workout {date:yyyy-MM-dd}");
        workout.SetDate(date);
        workout.SetMetrics(duration, calories);

        Context.Workouts.Add(workout);
        await SaveChangesAsync();
    }

    public async Task InitializeAsync()
    {
        Context.Users.Add(_testUser);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.Workouts.RemoveRange(Context.Workouts);
        Context.Users.RemoveRange(Context.Users);
        await SaveChangesAsync();
    }
}
