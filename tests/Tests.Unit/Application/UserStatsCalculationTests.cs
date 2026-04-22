using Application.Common.Interfaces.Queries;
using Application.Users.Queries;
using Domain.Users;
using Domain.Workouts;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Tests.Unit.Application;

public class UserStatsCalculationTests
{
    private readonly IWorkoutQueries _workoutQueriesMock;
    private readonly GetUserStatsQueryHandler _handler;

    public UserStatsCalculationTests()
    {
        _workoutQueriesMock = Substitute.For<IWorkoutQueries>();
        _handler = new GetUserStatsQueryHandler(_workoutQueriesMock);
    }

    [Fact]
    public async Task Handle_WithMultipleWorkouts_ShouldCalculateCorrectStats()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var workouts = new List<Workout>
        {
            CreateWorkout(userId, 30, 200),
            CreateWorkout(userId, 45, 350),
            CreateWorkout(userId, 60, 500)
        };

        _workoutQueriesMock
            .GetByUserId(userId, null, null, Arg.Any<CancellationToken>())
            .Returns(workouts);

        // Act
        var query = new GetUserStatsQuery(userId.Value, null, null);
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalWorkouts.Should().Be(3);
        result.TotalCaloriesBurned.Should().Be(1050);
        result.AverageDurationMinutes.Should().Be(45);
    }

    [Fact]
    public async Task Handle_WithNoWorkouts_ShouldReturnZeroStats()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var workouts = new List<Workout>();

        _workoutQueriesMock
            .GetByUserId(userId, null, null, Arg.Any<CancellationToken>())
            .Returns(workouts);

        // Act
        var query = new GetUserStatsQuery(userId.Value, null, null);
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalWorkouts.Should().Be(0);
        result.TotalCaloriesBurned.Should().Be(0);
        result.AverageDurationMinutes.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithDateRange_ShouldPassDatesToQuery()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);

        _workoutQueriesMock
            .GetByUserId(userId, startDate, endDate, Arg.Any<CancellationToken>())
            .Returns(new List<Workout>());

        // Act
        var query = new GetUserStatsQuery(userId.Value, startDate, endDate);
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _workoutQueriesMock.Received(1).GetByUserId(userId, startDate, endDate, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithWorkoutsHavingNullMetrics_ShouldTreatAsZero()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var workout1 = Workout.New(new WorkoutId(Guid.NewGuid()), userId, "Workout 1");
        workout1.SetMetrics(30, 200);
        
        var workout2 = Workout.New(new WorkoutId(Guid.NewGuid()), userId, "Workout 2");

        var workouts = new List<Workout> { workout1, workout2 };

        _workoutQueriesMock
            .GetByUserId(userId, null, null, Arg.Any<CancellationToken>())
            .Returns(workouts);

        // Act
        var query = new GetUserStatsQuery(userId.Value, null, null);
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalWorkouts.Should().Be(2);
        result.TotalCaloriesBurned.Should().Be(200);
        result.AverageDurationMinutes.Should().Be(15);
    }

    private static Workout CreateWorkout(UserId userId, int duration, int calories)
    {
        var workout = Workout.New(new WorkoutId(Guid.NewGuid()), userId, "Test Workout");
        workout.SetDate(DateTime.UtcNow.AddDays(-1));
        workout.SetMetrics(duration, calories);
        return workout;
    }
}
