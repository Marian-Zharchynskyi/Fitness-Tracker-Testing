namespace API.DTOs.Workouts;

public record AddExerciseDto(
    string Name,
    int? Sets,
    int? Reps,
    decimal? WeightKg,
    int? DurationSeconds);
