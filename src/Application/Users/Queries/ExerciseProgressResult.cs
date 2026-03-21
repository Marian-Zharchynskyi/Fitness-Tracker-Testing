namespace Application.Users.Queries;

public record ExerciseProgressResult(
    DateTime? Date,
    int? Sets,
    int? Reps,
    decimal? WeightKg,
    int? DurationSeconds);
