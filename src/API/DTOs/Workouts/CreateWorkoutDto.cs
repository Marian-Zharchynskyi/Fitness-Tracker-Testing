namespace API.DTOs.Workouts;

public record CreateWorkoutDto(
    Guid UserId,
    string Name,
    string? Notes,
    DateTime? Date,
    int? DurationMinutes,
    int? CaloriesBurned);
