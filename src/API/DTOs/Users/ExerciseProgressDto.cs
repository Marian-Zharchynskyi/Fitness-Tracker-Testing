namespace API.DTOs.Users;

public record ExerciseProgressDto(
    DateTime? Date,
    int? Sets,
    int? Reps,
    decimal? WeightKg,
    int? DurationSeconds);
