namespace API.DTOs.Users;

public record UserStatsDto(
    int TotalWorkouts,
    double AverageDurationMinutes,
    int TotalCaloriesBurned);
