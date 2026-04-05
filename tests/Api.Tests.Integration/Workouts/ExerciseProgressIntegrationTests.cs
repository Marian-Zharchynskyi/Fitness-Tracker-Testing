using API.DTOs.Users;
using Domain.Users;
using Domain.Workouts;
using FluentAssertions;
using Tests.Common;
using Tests.Data;
using Xunit;

namespace Api.Tests.Integration.Workouts;

public class ExerciseProgressIntegrationTests : BaseIntegrationTest, IAsyncLifetime
{
    private readonly User _testUser = UsersData.MainUser;

    public ExerciseProgressIntegrationTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetProgress_WithMultipleWorkouts_ShouldReturnProgressOverTime()
    {
        var exerciseName = "Bench Press";
        
        await CreateWorkoutWithExercise(DateTime.UtcNow.AddDays(-10), exerciseName, 3, 8, 60);
        await CreateWorkoutWithExercise(DateTime.UtcNow.AddDays(-5), exerciseName, 3, 10, 70);
        await CreateWorkoutWithExercise(DateTime.UtcNow.AddDays(-1), exerciseName, 4, 10, 75);

        var response = await Client.GetAsync($"/users/{_testUser.Id.Value}/progress?exercise={exerciseName}");

        response.IsSuccessStatusCode.Should().BeTrue();
        var progress = await response.ToResponseModel<List<ExerciseProgressDto>>();
        
        progress.Should().HaveCount(3);
        progress[0].WeightKg.Should().Be(60);
        progress[1].WeightKg.Should().Be(70);
        progress[2].WeightKg.Should().Be(75);
    }

    [Fact]
    public async Task GetProgress_WithCaseInsensitiveExerciseName_ShouldReturnResults()
    {
        var exerciseName = "Squat";
        
        await CreateWorkoutWithExercise(DateTime.UtcNow.AddDays(-1), exerciseName, 3, 10, 100);

        var response = await Client.GetAsync($"/users/{_testUser.Id.Value}/progress?exercise=SQUAT");

        response.IsSuccessStatusCode.Should().BeTrue();
        var progress = await response.ToResponseModel<List<ExerciseProgressDto>>();
        
        progress.Should().HaveCount(1);
        progress[0].WeightKg.Should().Be(100);
    }

    [Fact]
    public async Task GetProgress_WithNoMatchingExercise_ShouldReturnEmptyList()
    {
        await CreateWorkoutWithExercise(DateTime.UtcNow.AddDays(-1), "Squat", 3, 10, 100);

        var response = await Client.GetAsync($"/users/{_testUser.Id.Value}/progress?exercise=Bench Press");

        response.IsSuccessStatusCode.Should().BeTrue();
        var progress = await response.ToResponseModel<List<ExerciseProgressDto>>();
        
        progress.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProgress_ShouldOrderByDate()
    {
        var exerciseName = "Deadlift";
        
        await CreateWorkoutWithExercise(DateTime.UtcNow.AddDays(-1), exerciseName, 3, 5, 140);
        await CreateWorkoutWithExercise(DateTime.UtcNow.AddDays(-10), exerciseName, 3, 5, 120);
        await CreateWorkoutWithExercise(DateTime.UtcNow.AddDays(-5), exerciseName, 3, 5, 130);

        var response = await Client.GetAsync($"/users/{_testUser.Id.Value}/progress?exercise={exerciseName}");

        response.IsSuccessStatusCode.Should().BeTrue();
        var progress = await response.ToResponseModel<List<ExerciseProgressDto>>();
        
        progress.Should().HaveCount(3);
        progress[0].WeightKg.Should().Be(120);
        progress[1].WeightKg.Should().Be(130);
        progress[2].WeightKg.Should().Be(140);
    }

    [Fact]
    public async Task GetProgress_WithMultipleExercisesInWorkout_ShouldReturnOnlyMatching()
    {
        var workout = Workout.New(new WorkoutId(Guid.NewGuid()), _testUser.Id, "Full Body");
        workout.SetDate(DateTime.UtcNow.AddDays(-1));
        workout.SetMetrics(90, 600);

        var benchPress = Exercise.New(new ExerciseId(Guid.NewGuid()), workout.Id, "Bench Press");
        benchPress.SetMetrics(3, 10, 80);
        workout.AddExercise(benchPress);

        var squat = Exercise.New(new ExerciseId(Guid.NewGuid()), workout.Id, "Squat");
        squat.SetMetrics(3, 10, 100);
        workout.AddExercise(squat);

        Context.Workouts.Add(workout);
        await SaveChangesAsync();

        var response = await Client.GetAsync($"/users/{_testUser.Id.Value}/progress?exercise=Bench Press");

        response.IsSuccessStatusCode.Should().BeTrue();
        var progress = await response.ToResponseModel<List<ExerciseProgressDto>>();
        
        progress.Should().HaveCount(1);
        progress[0].WeightKg.Should().Be(80);
    }

    private async Task CreateWorkoutWithExercise(DateTime date, string exerciseName, int sets, int reps, decimal weight)
    {
        var workout = Workout.New(new WorkoutId(Guid.NewGuid()), _testUser.Id, "Strength Training");
        workout.SetDate(date);
        workout.SetMetrics(60, 400);

        var exercise = Exercise.New(new ExerciseId(Guid.NewGuid()), workout.Id, exerciseName);
        exercise.SetMetrics(sets, reps, weight);
        workout.AddExercise(exercise);

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
