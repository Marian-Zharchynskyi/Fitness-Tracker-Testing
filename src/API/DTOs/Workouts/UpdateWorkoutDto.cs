namespace API.DTOs.Workouts;

public record UpdateWorkoutDto(
    string Name,
    string? Notes,
    DateTime? Date,
    int? DurationMinutes,
    int? CaloriesBurned);
