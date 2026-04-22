using Domain.Users;
using Domain.Workouts;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests.Database;

public class StatsAggregationTests : BaseDatabaseTest
{
    [Fact]
    public async Task AggregateStats_ShouldCalculateTotalWorkoutsCorrectly()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var user = User.New(userId, "test@example.com", "Test User");
        Context.Users.Add(user);

        // Act
        var workout1 = CreateWorkout(userId, 30, 200);
        var workout2 = CreateWorkout(userId, 45, 350);
        var workout3 = CreateWorkout(userId, 60, 500);

        Context.Workouts.AddRange(workout1, workout2, workout3);
        await SaveChangesAsync();

        var totalWorkouts = await Context.Workouts
            .Where(w => w.UserId == userId)
            .CountAsync();

        // Assert
        totalWorkouts.Should().Be(3);
    }

    [Fact]
    public async Task AggregateStats_ShouldCalculateTotalCaloriesCorrectly()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var user = User.New(userId, "test@example.com", "Test User");
        Context.Users.Add(user);

        var workout1 = CreateWorkout(userId, 30, 200);
        var workout2 = CreateWorkout(userId, 45, 350);
        var workout3 = CreateWorkout(userId, 60, 500);
        Context.Workouts.AddRange(workout1, workout2, workout3);
        await SaveChangesAsync();

        var totalCalories = await Context.Workouts
            .Where(w => w.UserId == userId)
            .SumAsync(w => w.CaloriesBurned ?? 0);

        totalCalories.Should().Be(1050);
    }

    [Fact]
    public async Task AggregateStats_ShouldCalculateAverageDurationCorrectly()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var user = User.New(userId, "test@example.com", "Test User");
        Context.Users.Add(user);

        var workout1 = CreateWorkout(userId, 30, 200);
        var workout2 = CreateWorkout(userId, 45, 350);
        var workout3 = CreateWorkout(userId, 60, 500);
        Context.Workouts.AddRange(workout1, workout2, workout3);
        await SaveChangesAsync();

        var avgDuration = await Context.Workouts
            .Where(w => w.UserId == userId)
            .AverageAsync(w => w.DurationMinutes ?? 0);

        avgDuration.Should().Be(45);
    }

    [Fact]
    public async Task AggregateStats_WithDateRange_ShouldFilterCorrectly()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var user = User.New(userId, "test@example.com", "Test User");
        Context.Users.Add(user);

        var workout1 = CreateWorkoutWithDate(userId, 30, 200, DateTime.SpecifyKind(new DateTime(2024, 1, 15), DateTimeKind.Utc));
        var workout2 = CreateWorkoutWithDate(userId, 45, 350, DateTime.SpecifyKind(new DateTime(2024, 2, 15), DateTimeKind.Utc));
        var workout3 = CreateWorkoutWithDate(userId, 60, 500, DateTime.SpecifyKind(new DateTime(2024, 3, 15), DateTimeKind.Utc));
        Context.Workouts.AddRange(workout1, workout2, workout3);
        await SaveChangesAsync();

        var startDate = DateTime.SpecifyKind(new DateTime(2024, 2, 1), DateTimeKind.Utc);
        var endDate = DateTime.SpecifyKind(new DateTime(2024, 3, 31), DateTimeKind.Utc);
        // Act
        var stats = await Context.Workouts
            .Where(w => w.UserId == userId && w.Date >= startDate && w.Date <= endDate)
            .GroupBy(w => w.UserId)
            .Select(g => new
            {
                TotalWorkouts = g.Count(),
                TotalCalories = g.Sum(w => w.CaloriesBurned ?? 0),
                AvgDuration = g.Average(w => w.DurationMinutes ?? 0)
            })
            .FirstOrDefaultAsync();

        // Assert
        stats.Should().NotBeNull();
        stats!.TotalWorkouts.Should().Be(2);
        stats.TotalCalories.Should().Be(850);
        stats.AvgDuration.Should().Be(52.5);
    }

    [Fact]
    public async Task AggregateStats_WithMultipleUsers_ShouldIsolateByUser()
    {
        // Arrange
        var user1Id = new UserId(Guid.NewGuid());
        var user2Id = new UserId(Guid.NewGuid());

        var user1 = User.New(user1Id, "user1@example.com", "User 1");
        var user2 = User.New(user2Id, "user2@example.com", "User 2");
        Context.Users.AddRange(user1, user2);

        var user1Workout1 = CreateWorkout(user1Id, 30, 200);
        var user1Workout2 = CreateWorkout(user1Id, 45, 350);
        var user2Workout1 = CreateWorkout(user2Id, 60, 500);
        Context.Workouts.AddRange(user1Workout1, user1Workout2, user2Workout1);
        await SaveChangesAsync();

        var user1TotalCalories = await Context.Workouts
            .Where(w => w.UserId == user1Id)
            .SumAsync(w => w.CaloriesBurned ?? 0);

        var user2TotalCalories = await Context.Workouts
            .Where(w => w.UserId == user2Id)
            .SumAsync(w => w.CaloriesBurned ?? 0);

        user1TotalCalories.Should().Be(550);
        user2TotalCalories.Should().Be(500);
    }

    [Fact]
    public async Task AggregateStats_WithNullValues_ShouldHandleGracefully()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var user = User.New(userId, "test@example.com", "Test User");
        Context.Users.Add(user);

        var workout1 = Workout.New(new WorkoutId(Guid.NewGuid()), userId, "Workout 1");
        workout1.SetDate(DateTime.UtcNow.AddDays(-1));
        workout1.SetMetrics(30, 200);

        var workout2 = Workout.New(new WorkoutId(Guid.NewGuid()), userId, "Workout 2");
        workout2.SetDate(DateTime.UtcNow.AddDays(-2));
        Context.Workouts.AddRange(workout1, workout2);
        await SaveChangesAsync();

        var totalCalories = await Context.Workouts
            .Where(w => w.UserId == userId)
            .SumAsync(w => w.CaloriesBurned ?? 0);

        var avgDuration = await Context.Workouts
            .Where(w => w.UserId == userId)
            .AverageAsync(w => w.DurationMinutes ?? 0);

        totalCalories.Should().Be(200);
        avgDuration.Should().Be(15);
    }

    private static Workout CreateWorkout(UserId userId, int duration, int calories)
    {
        var workout = Workout.New(new WorkoutId(Guid.NewGuid()), userId, "Test Workout");
        workout.SetDate(DateTime.UtcNow.AddDays(-1));
        workout.SetMetrics(duration, calories);
        return workout;
    }

    private static Workout CreateWorkoutWithDate(UserId userId, int duration, int calories, DateTime date)
    {
        var workout = Workout.New(new WorkoutId(Guid.NewGuid()), userId, "Test Workout");
        workout.SetDate(date);
        workout.SetMetrics(duration, calories);
        return workout;
    }
}
