using Domain.Workouts;

namespace API.DTOs.Workouts;

public record ExerciseDto(
    Guid Id,
    Guid WorkoutId,
    string Name,
    int? Sets,
    int? Reps,
    decimal? WeightKg,
    int? DurationSeconds)
{
    public static ExerciseDto FromDomainModel(Exercise exercise) => new(
        exercise.Id.Value,
        exercise.WorkoutId.Value,
        exercise.Name,
        exercise.Sets,
        exercise.Reps,
        exercise.WeightKg,
        exercise.DurationSeconds);
}
