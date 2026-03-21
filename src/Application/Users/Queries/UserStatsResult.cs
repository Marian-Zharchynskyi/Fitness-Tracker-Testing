namespace Application.Users.Queries;

public record UserStatsResult(
    int TotalWorkouts,
    double AverageDurationMinutes,
    int TotalCaloriesBurned);
