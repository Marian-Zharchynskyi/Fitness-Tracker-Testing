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
        var workout = Workout.New(
            new WorkoutId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            "Morning Run");

        var pastDate = DateTime.UtcNow.AddDays(-1);
        workout.SetDate(pastDate);

        workout.Date.Should().Be(pastDate);
    }

    [Fact]
    public void SetDate_WithFutureDate_ShouldThrowArgumentException()
    {
        var workout = Workout.New(
            new WorkoutId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            "Morning Run");

        var futureDate = DateTime.UtcNow.AddDays(1);
        var act = () => workout.SetDate(futureDate);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Workout date cannot be in the future*");
    }

    [Fact]
    public void SetMetrics_WithPositiveValues_ShouldSetDurationAndCalories()
    {
        var workout = Workout.New(
            new WorkoutId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            "Morning Run");

        workout.SetMetrics(45, 350);

        workout.DurationMinutes.Should().Be(45);
        workout.CaloriesBurned.Should().Be(350);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void SetMetrics_WithNonPositiveDuration_ShouldThrowArgumentException(int duration)
    {
        var workout = Workout.New(
            new WorkoutId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            "Morning Run");

        var act = () => workout.SetMetrics(duration, 350);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Duration must be positive*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void SetMetrics_WithNonPositiveCalories_ShouldThrowArgumentException(int calories)
    {
        var workout = Workout.New(
            new WorkoutId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            "Morning Run");

        var act = () => workout.SetMetrics(45, calories);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Calories burned must be positive*");
    }

    [Fact]
    public void AddExercise_ShouldAddExerciseToWorkout()
    {
        var workout = Workout.New(
            new WorkoutId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            "Strength Training");

        var exercise = Exercise.New(
            new ExerciseId(Guid.NewGuid()),
            workout.Id,
            "Bench Press");

        workout.AddExercise(exercise);

        workout.Exercises.Should().ContainSingle();
        workout.Exercises.First().Should().Be(exercise);
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateNameAndNotes()
    {
        var workout = Workout.New(
            new WorkoutId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            "Old Name",
            "Old Notes");

        workout.UpdateDetails("New Name", "New Notes");

        workout.Name.Should().Be("New Name");
        workout.Notes.Should().Be("New Notes");
    }
}
