using System.Diagnostics;
using Bogus;
using Domain.Users;
using Domain.Workouts;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Tests.Performance;

public class BulkWorkoutInsertPerformanceTests : PerformanceTestBase
{
    private readonly ITestOutputHelper _output;
    private readonly Faker _faker;

    public BulkWorkoutInsertPerformanceTests(ITestOutputHelper output)
    {
        _output = output;
        _faker = new Faker();
    }

    [Fact]
    public async Task BulkInsert_100Workouts_ShouldCompleteInUnder5Seconds()
    {
        // Arrange
        var user = await Context.Users.FirstAsync();
        var workouts = GenerateWorkouts(user.Id, 100);

        var stopwatch = Stopwatch.StartNew();

        Context.Workouts.AddRange(workouts);
        await Context.SaveChangesAsync();

        stopwatch.Stop();
        _output.WriteLine($"Bulk insert of 100 workouts took {stopwatch.ElapsedMilliseconds}ms");

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000);
    }

    [Fact]
    public async Task BulkInsert_500Workouts_ShouldCompleteInUnder20Seconds()
    {
        // Arrange
        var user = await Context.Users.FirstAsync();
        var workouts = GenerateWorkouts(user.Id, 500);

        var stopwatch = Stopwatch.StartNew();

        // Act
        Context.Workouts.AddRange(workouts);
        await Context.SaveChangesAsync();

        stopwatch.Stop();
        _output.WriteLine($"Bulk insert of 500 workouts took {stopwatch.ElapsedMilliseconds}ms");

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(20000);
    }

    [Fact]
    public async Task BulkInsert_WithExercises_ShouldCompleteEfficiently()
    {
        // Arrange
        var user = await Context.Users.FirstAsync();
        var workouts = GenerateWorkoutsWithExercises(user.Id, 100, 5);

        var stopwatch = Stopwatch.StartNew();

        // Act
        Context.Workouts.AddRange(workouts);
        await Context.SaveChangesAsync();

        stopwatch.Stop();
        _output.WriteLine($"Bulk insert of 100 workouts with 5 exercises each took {stopwatch.ElapsedMilliseconds}ms");

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000);
    }

    [Fact]
    public async Task QueryWorkouts_AfterBulkInsert_ShouldBeEfficient()
    {
        // Arrange
        var user = await Context.Users.FirstAsync();
        var workouts = GenerateWorkouts(user.Id, 200);

        // Act
        Context.Workouts.AddRange(workouts);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var stopwatch = Stopwatch.StartNew();

        // Act
        var results = await Context.Workouts
            .Where(w => w.UserId == user.Id)
            .OrderByDescending(w => w.Date)
            .Take(50)
            .ToListAsync();

        stopwatch.Stop();
        _output.WriteLine($"Query for 50 most recent workouts took {stopwatch.ElapsedMilliseconds}ms");

        results.Should().HaveCount(50);
        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    private List<Workout> GenerateWorkouts(UserId userId, int count)
    {
        var workouts = new List<Workout>();
        for (int i = 0; i < count; i++)
        {
            var workout = Workout.New(
                new WorkoutId(Guid.NewGuid()),
                userId,
                _faker.Lorem.Word());

            workout.SetDate(DateTime.UtcNow.AddDays(-_faker.Random.Int(1, 365)));
            workout.SetMetrics(
                _faker.Random.Int(15, 120),
                _faker.Random.Int(100, 800));

            workouts.Add(workout);
        }
        return workouts;
    }

    private List<Workout> GenerateWorkoutsWithExercises(UserId userId, int workoutCount, int exercisesPerWorkout)
    {
        var workouts = new List<Workout>();
        var exerciseNames = new[] { "Bench Press", "Squat", "Deadlift", "Pull-ups", "Push-ups" };

        for (int i = 0; i < workoutCount; i++)
        {
            var workout = Workout.New(
                new WorkoutId(Guid.NewGuid()),
                userId,
                _faker.Lorem.Word());

            workout.SetDate(DateTime.UtcNow.AddDays(-_faker.Random.Int(1, 365)));
            workout.SetMetrics(
                _faker.Random.Int(15, 120),
                _faker.Random.Int(100, 800));

            for (int j = 0; j < exercisesPerWorkout; j++)
            {
                var exercise = Exercise.New(
                    new ExerciseId(Guid.NewGuid()),
                    workout.Id,
                    _faker.PickRandom(exerciseNames));

                exercise.SetMetrics(
                    _faker.Random.Int(2, 5),
                    _faker.Random.Int(6, 15),
                    _faker.Random.Decimal(5, 150));

                workout.AddExercise(exercise);
            }

            workouts.Add(workout);
        }
        return workouts;
    }
}
