using Domain.Workouts;
using FluentAssertions;
using Xunit;

namespace Tests.Unit.Domain;

public class ExerciseTests
{
    [Fact]
    public void SetMetrics_WithValidValues_ShouldSetAllMetrics()
    {
        var exercise = Exercise.New(
            new ExerciseId(Guid.NewGuid()),
            new WorkoutId(Guid.NewGuid()),
            "Bench Press");

        exercise.SetMetrics(3, 10, 80, 120);

        exercise.Sets.Should().Be(3);
        exercise.Reps.Should().Be(10);
        exercise.WeightKg.Should().Be(80);
        exercise.DurationSeconds.Should().Be(120);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SetMetrics_WithNonPositiveSets_ShouldThrowArgumentException(int sets)
    {
        var exercise = Exercise.New(
            new ExerciseId(Guid.NewGuid()),
            new WorkoutId(Guid.NewGuid()),
            "Bench Press");

        var act = () => exercise.SetMetrics(sets, 10, 80);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Sets must be a positive integer*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SetMetrics_WithNonPositiveReps_ShouldThrowArgumentException(int reps)
    {
        var exercise = Exercise.New(
            new ExerciseId(Guid.NewGuid()),
            new WorkoutId(Guid.NewGuid()),
            "Bench Press");

        var act = () => exercise.SetMetrics(3, reps, 80);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Reps must be a positive integer*");
    }

    [Fact]
    public void SetMetrics_WithNegativeWeight_ShouldThrowArgumentException()
    {
        var exercise = Exercise.New(
            new ExerciseId(Guid.NewGuid()),
            new WorkoutId(Guid.NewGuid()),
            "Bench Press");

        var act = () => exercise.SetMetrics(3, 10, -10);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Weight must be non-negative*");
    }

    [Fact]
    public void SetMetrics_WithNegativeDuration_ShouldThrowArgumentException()
    {
        var exercise = Exercise.New(
            new ExerciseId(Guid.NewGuid()),
            new WorkoutId(Guid.NewGuid()),
            "Plank");

        var act = () => exercise.SetMetrics(3, 10, null, -10);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Duration must be non-negative*");
    }

    [Fact]
    public void SetMetrics_WithNullOptionalValues_ShouldSetOnlyRequiredMetrics()
    {
        var exercise = Exercise.New(
            new ExerciseId(Guid.NewGuid()),
            new WorkoutId(Guid.NewGuid()),
            "Push-ups");

        exercise.SetMetrics(3, 15);

        exercise.Sets.Should().Be(3);
        exercise.Reps.Should().Be(15);
        exercise.WeightKg.Should().BeNull();
        exercise.DurationSeconds.Should().BeNull();
    }
}
