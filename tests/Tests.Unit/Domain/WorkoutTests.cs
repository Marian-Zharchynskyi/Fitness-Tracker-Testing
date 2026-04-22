using Domain.Users;
using Domain.Workouts;
using FluentAssertions;
using Xunit;

namespace Tests.Unit.Domain;

public class WorkoutTests
{
    [Fact]
    public void SetDate_WithPastDate_ShouldSetDate()
    {
        // Arrange
        var workout = Workout.New(
            new WorkoutId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            "Morning Run");

        // Act
        var pastDate = DateTime.UtcNow.AddDays(-1);

        workout.SetDate(pastDate);

        // Assert
        workout.Date.Should().Be(pastDate);
    }

    [Fact]
    public void SetDate_WithFutureDate_ShouldThrowArgumentException()
    {
        // Arrange
        var workout = Workout.New(
            new WorkoutId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            "Morning Run");

        var futureDate = DateTime.UtcNow.AddDays(1);
        
        // Act
        var act = () => workout.SetDate(futureDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Workout date cannot be in the future*");
    }

    [Fact]
    public void SetMetrics_WithPositiveValues_ShouldSetDurationAndCalories()
    {
        // Arrange
        var workout = Workout.New(
            new WorkoutId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            "Morning Run");

        // Act
        workout.SetMetrics(45, 350);

        // Assert
        workout.DurationMinutes.Should().Be(45);
        workout.CaloriesBurned.Should().Be(350);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void SetMetrics_WithNonPositiveDuration_ShouldThrowArgumentException(int duration)
    {
        // Arrange
        var workout = Workout.New(
            new WorkoutId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            "Morning Run");

        // Act
        var act = () => workout.SetMetrics(duration, 350);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Duration must be positive*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void SetMetrics_WithNonPositiveCalories_ShouldThrowArgumentException(int calories)
    {
        // Arrange
        var workout = Workout.New(
            new WorkoutId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            "Morning Run");

        // Act
        var act = () => workout.SetMetrics(45, calories);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Calories burned must be positive*");
    }

    [Fact]
    public void AddExercise_ShouldAddExerciseToWorkout()
    {
        // Arrange
        var workout = Workout.New(
            new WorkoutId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            "Strength Training");

        var exercise = Exercise.New(
            new ExerciseId(Guid.NewGuid()),
            workout.Id,
            "Bench Press");

        // Act
        workout.AddExercise(exercise);

        // Assert
        workout.Exercises.Should().ContainSingle();
        workout.Exercises.First().Should().Be(exercise);
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateNameAndNotes()
    {
        // Arrange
        var workout = Workout.New(
            new WorkoutId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            "Old Name",
            "Old Notes");

        // Act
        workout.UpdateDetails("New Name", "New Notes");

        // Assert
        workout.Name.Should().Be("New Name");
        workout.Notes.Should().Be("New Notes");
    }
}
