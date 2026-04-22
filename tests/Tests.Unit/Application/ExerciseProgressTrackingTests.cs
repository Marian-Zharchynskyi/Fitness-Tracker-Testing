using Application.Common.Interfaces.Queries;
using Application.Users.Queries;
using Domain.Users;
using Domain.Workouts;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Tests.Unit.Application;

public class ExerciseProgressTrackingTests
{
    private readonly IWorkoutQueries _workoutQueriesMock;
    private readonly GetExerciseProgressQueryHandler _handler;

    public ExerciseProgressTrackingTests()
    {
        _workoutQueriesMock = Substitute.For<IWorkoutQueries>();
        _handler = new GetExerciseProgressQueryHandler(_workoutQueriesMock);
    }

    [Fact]
    public async Task Handle_WithMatchingExercises_ShouldReturnProgressOverTime()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var exerciseName = "Bench Press";

        var workout1 = CreateWorkoutWithExercise(userId, DateTime.UtcNow.AddDays(-10), exerciseName, 3, 8, 60);
        var workout2 = CreateWorkoutWithExercise(userId, DateTime.UtcNow.AddDays(-5), exerciseName, 3, 10, 70);
        var workout3 = CreateWorkoutWithExercise(userId, DateTime.UtcNow.AddDays(-1), exerciseName, 4, 10, 75);

        var workouts = new List<Workout> { workout1, workout2, workout3 };

        _workoutQueriesMock
            .GetAllByUserId(userId, Arg.Any<CancellationToken>())
            .Returns(workouts);

        // Act
        var query = new GetExerciseProgressQuery(userId.Value, exerciseName);
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result[0].Sets.Should().Be(3);
        result[0].Reps.Should().Be(8);
        result[0].WeightKg.Should().Be(60);
        
        result[1].Sets.Should().Be(3);
        result[1].Reps.Should().Be(10);
        result[1].WeightKg.Should().Be(70);
        
        result[2].Sets.Should().Be(4);
        result[2].Reps.Should().Be(10);
        result[2].WeightKg.Should().Be(75);
    }

    [Fact]
    public async Task Handle_WithCaseInsensitiveMatch_ShouldReturnResults()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var workout = CreateWorkoutWithExercise(userId, DateTime.UtcNow.AddDays(-1), "Bench Press", 3, 10, 80);

        _workoutQueriesMock
            .GetAllByUserId(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Workout> { workout });

        // Act
        var query = new GetExerciseProgressQuery(userId.Value, "BENCH PRESS");
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithNoMatchingExercises_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var workout = CreateWorkoutWithExercise(userId, DateTime.UtcNow.AddDays(-1), "Squat", 3, 10, 100);

        _workoutQueriesMock
            .GetAllByUserId(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Workout> { workout });

        // Act
        var query = new GetExerciseProgressQuery(userId.Value, "Bench Press");
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldOrderResultsByDate()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var exerciseName = "Squat";

        var workout1 = CreateWorkoutWithExercise(userId, DateTime.UtcNow.AddDays(-1), exerciseName, 3, 10, 100);
        var workout2 = CreateWorkoutWithExercise(userId, DateTime.UtcNow.AddDays(-10), exerciseName, 3, 8, 90);
        var workout3 = CreateWorkoutWithExercise(userId, DateTime.UtcNow.AddDays(-5), exerciseName, 3, 10, 95);

        var workouts = new List<Workout> { workout1, workout2, workout3 };

        _workoutQueriesMock
            .GetAllByUserId(userId, Arg.Any<CancellationToken>())
            .Returns(workouts);

        // Act
        var query = new GetExerciseProgressQuery(userId.Value, exerciseName);
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result[0].WeightKg.Should().Be(90);
        result[1].WeightKg.Should().Be(95);
        result[2].WeightKg.Should().Be(100);
    }

    [Fact]
    public async Task Handle_WithMultipleExercisesInWorkout_ShouldReturnOnlyMatchingOnes()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var workout = Workout.New(new WorkoutId(Guid.NewGuid()), userId, "Full Body");
        workout.SetDate(DateTime.UtcNow.AddDays(-1));

        var benchPress = Exercise.New(new ExerciseId(Guid.NewGuid()), workout.Id, "Bench Press");
        benchPress.SetMetrics(3, 10, 80);
        workout.AddExercise(benchPress);

        var squat = Exercise.New(new ExerciseId(Guid.NewGuid()), workout.Id, "Squat");
        squat.SetMetrics(3, 10, 100);
        workout.AddExercise(squat);

        _workoutQueriesMock
            .GetAllByUserId(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Workout> { workout });

        // Act
        var query = new GetExerciseProgressQuery(userId.Value, "Bench Press");
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].WeightKg.Should().Be(80);
    }

    private static Workout CreateWorkoutWithExercise(
        UserId userId, 
        DateTime date, 
        string exerciseName, 
        int sets, 
        int reps, 
        decimal weight)
    {
        var workout = Workout.New(new WorkoutId(Guid.NewGuid()), userId, "Strength Training");
        workout.SetDate(date);
        workout.SetMetrics(60, 400);

        var exercise = Exercise.New(new ExerciseId(Guid.NewGuid()), workout.Id, exerciseName);
        exercise.SetMetrics(sets, reps, weight);
        workout.AddExercise(exercise);

        return workout;
    }
}
