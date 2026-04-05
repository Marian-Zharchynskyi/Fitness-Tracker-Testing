using Domain.Users;
using Domain.Workouts;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests.Database;

public class WorkoutDateRangeQueryTests : BaseDatabaseTest
{
    [Fact]
    public async Task GetWorkoutsByDateRange_ShouldReturnOnlyWorkoutsInRange()
    {
        var userId = new UserId(Guid.NewGuid());
        var user = User.New(userId, "test@example.com", "Test User");
        Context.Users.Add(user);

        var workout1 = CreateWorkout(userId, "Workout 1", DateTime.SpecifyKind(new DateTime(2024, 1, 15), DateTimeKind.Utc));
        var workout2 = CreateWorkout(userId, "Workout 2", DateTime.SpecifyKind(new DateTime(2024, 2, 15), DateTimeKind.Utc));
        var workout3 = CreateWorkout(userId, "Workout 3", DateTime.SpecifyKind(new DateTime(2024, 3, 15), DateTimeKind.Utc));

        Context.Workouts.AddRange(workout1, workout2, workout3);
        await SaveChangesAsync();

        var startDate = DateTime.SpecifyKind(new DateTime(2024, 2, 1), DateTimeKind.Utc);
        var endDate = DateTime.SpecifyKind(new DateTime(2024, 2, 28), DateTimeKind.Utc);

        var results = await Context.Workouts
            .Where(w => w.UserId == userId && w.Date >= startDate && w.Date <= endDate)
            .ToListAsync();

        results.Should().HaveCount(1);
        results[0].Name.Should().Be("Workout 2");
    }

    [Fact]
    public async Task GetWorkoutsByDateRange_WithStartDateOnly_ShouldReturnWorkoutsAfterDate()
    {
        var userId = new UserId(Guid.NewGuid());
        var user = User.New(userId, "test@example.com", "Test User");
        Context.Users.Add(user);

        var workout1 = CreateWorkout(userId, "Workout 1", DateTime.SpecifyKind(new DateTime(2024, 1, 15), DateTimeKind.Utc));
        var workout2 = CreateWorkout(userId, "Workout 2", DateTime.SpecifyKind(new DateTime(2024, 2, 15), DateTimeKind.Utc));
        var workout3 = CreateWorkout(userId, "Workout 3", DateTime.SpecifyKind(new DateTime(2024, 3, 15), DateTimeKind.Utc));

        Context.Workouts.AddRange(workout1, workout2, workout3);
        await SaveChangesAsync();

        var startDate = DateTime.SpecifyKind(new DateTime(2024, 2, 1), DateTimeKind.Utc);

        var results = await Context.Workouts
            .Where(w => w.UserId == userId && w.Date >= startDate)
            .OrderBy(w => w.Date)
            .ToListAsync();

        results.Should().HaveCount(2);
        results[0].Name.Should().Be("Workout 2");
        results[1].Name.Should().Be("Workout 3");
    }

    [Fact]
    public async Task GetWorkoutsByDateRange_WithEndDateOnly_ShouldReturnWorkoutsBeforeDate()
    {
        var userId = new UserId(Guid.NewGuid());
        var user = User.New(userId, "test@example.com", "Test User");
        Context.Users.Add(user);

        var workout1 = CreateWorkout(userId, "Workout 1", DateTime.SpecifyKind(new DateTime(2024, 1, 15), DateTimeKind.Utc));
        var workout2 = CreateWorkout(userId, "Workout 2", DateTime.SpecifyKind(new DateTime(2024, 2, 15), DateTimeKind.Utc));
        var workout3 = CreateWorkout(userId, "Workout 3", DateTime.SpecifyKind(new DateTime(2024, 3, 15), DateTimeKind.Utc));

        Context.Workouts.AddRange(workout1, workout2, workout3);
        await SaveChangesAsync();

        var endDate = DateTime.SpecifyKind(new DateTime(2024, 2, 28), DateTimeKind.Utc);

        var results = await Context.Workouts
            .Where(w => w.UserId == userId && w.Date <= endDate)
            .OrderBy(w => w.Date)
            .ToListAsync();

        results.Should().HaveCount(2);
        results[0].Name.Should().Be("Workout 1");
        results[1].Name.Should().Be("Workout 2");
    }

    [Fact]
    public async Task GetWorkoutsByDateRange_WithNoWorkoutsInRange_ShouldReturnEmpty()
    {
        var userId = new UserId(Guid.NewGuid());
        var user = User.New(userId, "test@example.com", "Test User");
        Context.Users.Add(user);

        var workout1 = CreateWorkout(userId, "Workout 1", DateTime.SpecifyKind(new DateTime(2024, 1, 15), DateTimeKind.Utc));
        var workout2 = CreateWorkout(userId, "Workout 2", DateTime.SpecifyKind(new DateTime(2024, 3, 15), DateTimeKind.Utc));

        Context.Workouts.AddRange(workout1, workout2);
        await SaveChangesAsync();

        var startDate = DateTime.SpecifyKind(new DateTime(2024, 2, 1), DateTimeKind.Utc);
        var endDate = DateTime.SpecifyKind(new DateTime(2024, 2, 28), DateTimeKind.Utc);

        var results = await Context.Workouts
            .Where(w => w.UserId == userId && w.Date >= startDate && w.Date <= endDate)
            .ToListAsync();

        results.Should().BeEmpty();
    }

    private static Workout CreateWorkout(UserId userId, string name, DateTime date)
    {
        var workout = Workout.New(new WorkoutId(Guid.NewGuid()), userId, name);
        workout.SetDate(date);
        workout.SetMetrics(30, 250);
        return workout;
    }
}
