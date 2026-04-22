using Domain.Users;
using Domain.Workouts;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests.Database;

public class CascadeDeleteTests : BaseDatabaseTest
{
    [Fact]
    public async Task DeleteWorkout_ShouldCascadeDeleteExercises()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var user = User.New(userId, "test@example.com", "Test User");
        Context.Users.Add(user);

        var workout = Workout.New(new WorkoutId(Guid.NewGuid()), userId, "Strength Training");
        workout.SetDate(DateTime.UtcNow.AddDays(-1));
        workout.SetMetrics(60, 400);

        var exercise1 = Exercise.New(new ExerciseId(Guid.NewGuid()), workout.Id, "Bench Press");
        exercise1.SetMetrics(3, 10, 80);
        workout.AddExercise(exercise1);

        var exercise2 = Exercise.New(new ExerciseId(Guid.NewGuid()), workout.Id, "Squat");
        exercise2.SetMetrics(4, 8, 100);
        workout.AddExercise(exercise2);

        Context.Workouts.Add(workout);
        await SaveChangesAsync();

        var exerciseIds = workout.Exercises.Select(e => e.Id).ToList();

        // Act
        Context.Workouts.Remove(workout);
        await SaveChangesAsync();
        // Assert
        var workoutExists = await Context.Workouts.AnyAsync(w => w.Id == workout.Id);
        workoutExists.Should().BeFalse();

        // Assert
        foreach (var exerciseId in exerciseIds)
        {
            var exerciseExists = await Context.Exercises.AnyAsync(e => e.Id == exerciseId);
            exerciseExists.Should().BeFalse();
        }
    }

    [Fact]
    public async Task DeleteUser_ShouldCascadeDeleteWorkoutsAndExercises()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var user = User.New(userId, "test@example.com", "Test User");
        Context.Users.Add(user);

        var workout1 = Workout.New(new WorkoutId(Guid.NewGuid()), userId, "Workout 1");
        workout1.SetDate(DateTime.UtcNow.AddDays(-1));
        workout1.SetMetrics(30, 250);

        var exercise1 = Exercise.New(new ExerciseId(Guid.NewGuid()), workout1.Id, "Push-ups");
        exercise1.SetMetrics(3, 15);
        workout1.AddExercise(exercise1);

        var workout2 = Workout.New(new WorkoutId(Guid.NewGuid()), userId, "Workout 2");
        workout2.SetDate(DateTime.UtcNow.AddDays(-2));
        workout2.SetMetrics(45, 350);

        var exercise2 = Exercise.New(new ExerciseId(Guid.NewGuid()), workout2.Id, "Squats");
        exercise2.SetMetrics(4, 10, 60);
        workout2.AddExercise(exercise2);

        Context.Workouts.AddRange(workout1, workout2);
        await SaveChangesAsync();

        var workoutIds = new[] { workout1.Id, workout2.Id };
        var exerciseIds = new[] { exercise1.Id, exercise2.Id };

        // Act
        Context.Users.Remove(user);
        await SaveChangesAsync();

        // Assert
        var userExists = await Context.Users.AnyAsync(u => u.Id == userId);
        userExists.Should().BeFalse();

        foreach (var workoutId in workoutIds)
        {
        // Assert
        var workoutExists = await Context.Workouts.AnyAsync(w => w.Id == workoutId);
            workoutExists.Should().BeFalse();
        }

        foreach (var exerciseId in exerciseIds)
        {
            var exerciseExists = await Context.Exercises.AnyAsync(e => e.Id == exerciseId);
            exerciseExists.Should().BeFalse();
        }
    }

    [Fact]
    public async Task DeleteWorkout_WithNoExercises_ShouldDeleteSuccessfully()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var user = User.New(userId, "test@example.com", "Test User");
        Context.Users.Add(user);

        var workout = Workout.New(new WorkoutId(Guid.NewGuid()), userId, "Simple Workout");
        workout.SetDate(DateTime.UtcNow.AddDays(-1));
        workout.SetMetrics(30, 250);

        Context.Workouts.Add(workout);
        await SaveChangesAsync();

        // Act
        Context.Workouts.Remove(workout);
        await SaveChangesAsync();
        // Assert
        var workoutExists = await Context.Workouts.AnyAsync(w => w.Id == workout.Id);
        workoutExists.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteWorkout_ShouldNotAffectOtherWorkouts()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var user = User.New(userId, "test@example.com", "Test User");
        Context.Users.Add(user);

        var workout1 = Workout.New(new WorkoutId(Guid.NewGuid()), userId, "Workout 1");
        workout1.SetDate(DateTime.UtcNow.AddDays(-1));
        workout1.SetMetrics(30, 250);

        var exercise1 = Exercise.New(new ExerciseId(Guid.NewGuid()), workout1.Id, "Exercise 1");
        exercise1.SetMetrics(3, 10);
        workout1.AddExercise(exercise1);

        var workout2 = Workout.New(new WorkoutId(Guid.NewGuid()), userId, "Workout 2");
        workout2.SetDate(DateTime.UtcNow.AddDays(-2));
        workout2.SetMetrics(45, 350);

        var exercise2 = Exercise.New(new ExerciseId(Guid.NewGuid()), workout2.Id, "Exercise 2");
        exercise2.SetMetrics(3, 10);
        workout2.AddExercise(exercise2);

        Context.Workouts.AddRange(workout1, workout2);
        await SaveChangesAsync();

        // Act
        Context.Workouts.Remove(workout1);
        await SaveChangesAsync();

        //Assert
        var workout1Exists = await Context.Workouts.AnyAsync(w => w.Id == workout1.Id);
        var workout2Exists = await Context.Workouts.AnyAsync(w => w.Id == workout2.Id);
        var exercise1Exists = await Context.Exercises.AnyAsync(e => e.Id == exercise1.Id);
        var exercise2Exists = await Context.Exercises.AnyAsync(e => e.Id == exercise2.Id);

        workout1Exists.Should().BeFalse();
        workout2Exists.Should().BeTrue();
        exercise1Exists.Should().BeFalse();
        exercise2Exists.Should().BeTrue();
    }
}
