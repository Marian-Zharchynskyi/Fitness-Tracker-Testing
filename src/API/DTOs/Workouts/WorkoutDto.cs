using Domain.Workouts;

namespace API.DTOs.Workouts;

public record WorkoutDto(
    Guid Id,
    Guid UserId,
    string Name,
    DateTime? Date,
    int? DurationMinutes,
    int? CaloriesBurned,
    string? Notes,
    IReadOnlyCollection<ExerciseDto> Exercises)
{
    public static WorkoutDto FromDomainModel(Workout workout) => new(
        workout.Id.Value,
        workout.UserId.Value,
        workout.Name,
        workout.Date,
        workout.DurationMinutes,
        workout.CaloriesBurned,
        workout.Notes,
        workout.Exercises.Select(ExerciseDto.FromDomainModel).ToList()
    );
}
