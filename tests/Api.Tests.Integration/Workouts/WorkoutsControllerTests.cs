using System.Net;
using System.Net.Http.Json;
using API.DTOs.Workouts;
using Domain.Users;
using Domain.Workouts;
using FluentAssertions;
using Tests.Common;
using Tests.Data;
using Xunit;

namespace Api.Tests.Integration.Workouts;

public class WorkoutsControllerTests : BaseIntegrationTest, IAsyncLifetime
{
    private readonly User _testUser = UsersData.MainUser;

    public WorkoutsControllerTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateWorkout_WithValidData_ShouldCreateWorkout()
    {
        var request = new CreateWorkoutDto(
            UserId: _testUser.Id.Value,
            Name: "Morning Run",
            Notes: "5km run in the park",
            Date: DateTime.UtcNow.AddDays(-1),
            DurationMinutes: 30,
            CaloriesBurned: 250);

        var response = await Client.PostAsJsonAsync("/api/workouts", request);

        response.IsSuccessStatusCode.Should().BeTrue();
        var workout = await response.ToResponseModel<WorkoutDto>();
        workout.Name.Should().Be(request.Name);
        workout.Notes.Should().Be(request.Notes);
        workout.DurationMinutes.Should().Be(request.DurationMinutes);
        workout.CaloriesBurned.Should().Be(request.CaloriesBurned);

        Context.Workouts.Any(w => w.Id == new WorkoutId(workout.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task CreateWorkout_WithFutureDate_ShouldReturnBadRequest()
    {
        var request = new CreateWorkoutDto(
            UserId: _testUser.Id.Value,
            Name: "Future Workout",
            Notes: null,
            Date: DateTime.UtcNow.AddDays(1),
            DurationMinutes: 30,
            CaloriesBurned: 250);

        var response = await Client.PostAsJsonAsync("/api/workouts", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateWorkout_WithNegativeCalories_ShouldReturnBadRequest()
    {
        var request = new CreateWorkoutDto(
            UserId: _testUser.Id.Value,
            Name: "Invalid Workout",
            Notes: null,
            Date: DateTime.UtcNow.AddDays(-1),
            DurationMinutes: 30,
            CaloriesBurned: -100);

        var response = await Client.PostAsJsonAsync("/api/workouts", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetWorkout_WithValidId_ShouldReturnWorkoutWithExercises()
    {
        var workout = await CreateTestWorkoutWithExercises();

        var response = await Client.GetAsync($"/api/workouts/{workout.Id.Value}");

        response.IsSuccessStatusCode.Should().BeTrue();
        var result = await response.ToResponseModel<WorkoutDto>();
        result.Id.Should().Be(workout.Id.Value);
        result.Name.Should().Be(workout.Name);
        result.Exercises.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetWorkout_WithInvalidId_ShouldReturnNotFound()
    {
        var invalidId = Guid.NewGuid();

        var response = await Client.GetAsync($"/api/workouts/{invalidId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateWorkout_WithValidData_ShouldUpdateWorkout()
    {
        var workout = await CreateTestWorkout();

        var updateRequest = new UpdateWorkoutDto(
            Name: "Updated Workout Name",
            Notes: "Updated notes",
            Date: DateTime.UtcNow.AddDays(-2),
            DurationMinutes: 45,
            CaloriesBurned: 350);

        var response = await Client.PutAsJsonAsync($"/api/workouts/{workout.Id.Value}", updateRequest);

        response.IsSuccessStatusCode.Should().BeTrue();
        var updated = await response.ToResponseModel<WorkoutDto>();
        updated.Name.Should().Be(updateRequest.Name);
        updated.Notes.Should().Be(updateRequest.Notes);
        updated.DurationMinutes.Should().Be(updateRequest.DurationMinutes);
        updated.CaloriesBurned.Should().Be(updateRequest.CaloriesBurned);
    }

    [Fact]
    public async Task DeleteWorkout_WithValidId_ShouldDeleteWorkout()
    {
        var workout = await CreateTestWorkout();

        var response = await Client.DeleteAsync($"/api/workouts/{workout.Id.Value}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        Context.Workouts.Any(w => w.Id == workout.Id).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteWorkout_ShouldCascadeDeleteExercises()
    {
        var workout = await CreateTestWorkoutWithExercises();
        var exerciseIds = workout.Exercises.Select(e => e.Id).ToList();

        var response = await Client.DeleteAsync($"/api/workouts/{workout.Id.Value}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        Context.Workouts.Any(w => w.Id == workout.Id).Should().BeFalse();
        
        foreach (var exerciseId in exerciseIds)
        {
            Context.Exercises.Any(e => e.Id == exerciseId).Should().BeFalse();
        }
    }

    [Fact]
    public async Task AddExercise_WithValidData_ShouldAddExerciseToWorkout()
    {
        var workout = await CreateTestWorkout();

        var exerciseRequest = new AddExerciseDto(
            Name: "Bench Press",
            Sets: 3,
            Reps: 10,
            WeightKg: 80,
            DurationSeconds: null);

        var response = await Client.PostAsJsonAsync($"/api/workouts/{workout.Id.Value}/exercises", exerciseRequest);

        response.IsSuccessStatusCode.Should().BeTrue();
        var result = await response.ToResponseModel<WorkoutDto>();
        result.Exercises.Should().ContainSingle();
        result.Exercises.First().Name.Should().Be(exerciseRequest.Name);
        result.Exercises.First().Sets.Should().Be(exerciseRequest.Sets);
        result.Exercises.First().Reps.Should().Be(exerciseRequest.Reps);
        result.Exercises.First().WeightKg.Should().Be(exerciseRequest.WeightKg);
    }

    [Fact]
    public async Task AddExercise_WithInvalidSets_ShouldReturnBadRequest()
    {
        var workout = await CreateTestWorkout();

        var exerciseRequest = new AddExerciseDto(
            Name: "Push-ups",
            Sets: 0,
            Reps: 10,
            WeightKg: null,
            DurationSeconds: null);

        var response = await Client.PostAsJsonAsync($"/api/workouts/{workout.Id.Value}/exercises", exerciseRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<Workout> CreateTestWorkout()
    {
        var workout = Workout.New(new WorkoutId(Guid.NewGuid()), _testUser.Id, "Test Workout");
        workout.SetDate(DateTime.UtcNow.AddDays(-1));
        workout.SetMetrics(30, 250);

        Context.Workouts.Add(workout);
        await SaveChangesAsync();

        return workout;
    }

    private async Task<Workout> CreateTestWorkoutWithExercises()
    {
        var workout = Workout.New(new WorkoutId(Guid.NewGuid()), _testUser.Id, "Strength Training");
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

        return workout;
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
